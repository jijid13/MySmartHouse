using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Kinect;
using System.Speech.Synthesis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections.Generic;

#if MICRO
using System.Speech.Recognition;
#endif

#if KINECT
using Microsoft.Speech;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
#endif


namespace MySmartHouse
{
    public partial class StartForm : Form
    {
        private SpeechSynthesizer speaker = new SpeechSynthesizer();
        private DateTime endtime;
        public bool occupied = false;
        private Dictionary<string, Plugin> _PluginsList;
        private string ServiceName;
        protected bool[] buttons;
        private double ConfidenceThreshold = 0.7;
        private Boolean startListen = false;
        [DllImport("Kernel32.dll")]
        public static extern bool Beep(Int32 dwFreq, Int32 dwDuration);
#if MICRO
        protected SpeechRecognitionEngine speechEngine = null;
#endif
        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        public KinectSensor sensor;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
#if KINECT
        private SpeechRecognitionEngine speechEngine;
#endif

        public StartForm()
        {
            this.ServiceName = ConfigurationManager.AppSettings["ServiceName"];
            this.endtime = DateTime.Now;
            InitializeComponent();
            speaker.SelectVoice(ConfigurationManager.AppSettings["SelectedVoice"]);

            var b = new Bitmap(ConfigurationManager.AppSettings["CursorImage"]);
            this.Cursor = CreateCursor(b, 5, 5);
            this.SystemName.Text = this.ServiceName;

        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "fr-FR".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
#if KINECT
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    this.sensor.Start();
                }
                catch (IOException ex)
                {
                    this.sensor = null;
                    Logger.Error(ex.Message);
                }
            }

            if (null == this.sensor)
            {
                return;
            }
            var ri = GetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
#endif
#if MICRO
            this.speechEngine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("fr-FR"));

            speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            speechEngine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", (int)(this.ConfidenceThreshold * 100));

            speechEngine.MaxAlternates = 10;
            speechEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(0);
            speechEngine.BabbleTimeout = TimeSpan.FromSeconds(0);
            speechEngine.EndSilenceTimeout = TimeSpan.FromSeconds(0.150);
            speechEngine.EndSilenceTimeoutAmbiguous = TimeSpan.FromSeconds(0.500);
            speechEngine.SetInputToDefaultAudioDevice();
#endif
                this._PluginsList = Plugin.loadPlugins(this.speechEngine, this.flowLayoutPanel1);

                speechEngine.RecognizeAsync(RecognizeMode.Multiple);

                speaker.Speak(ConfigurationManager.AppSettings["OperationalSystem"]);
#if KINECT
            }
            else
            {
                speaker.Speak(ConfigurationManager.AppSettings["ErroredSystem"]);
            }

            this.sensor.AudioSource.EchoCancellationMode = EchoCancellationMode.CancellationAndSuppression;
#endif
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            
            
            if (e.Result.Confidence >= ConfidenceThreshold && (e.Result.Audio.StartTime > this.endtime))
            {
                //start listening
                if (e.Result.Text.Equals(ServiceName) || startListen)
                {
                    if (!startListen)
                    {
                        startListen = true;
                        Beep(750, 300);
                        this.endtime = DateTime.Now;
                        System.Threading.Thread.Sleep(1500);
                        return;
                    }
                    else
                    {
                        if ((DateTime.Now - this.endtime).TotalSeconds > 6)
                        {
                            startListen = false;
                            Beep(750, 300);
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
                foreach (Plugin _plugin in _PluginsList.Values)
                {
                    try
                    {
                        if (_plugin._userCtl.GetType().Name == e.Result.Grammar.RuleName || _plugin._userCtl.GetType().Name == e.Result.Grammar.Name)
                        {
                            Logger.Info("Recognized text : " + e.Result.Text + ", Grammar rule name : " + e.Result.Grammar.RuleName);
                            Dictionary<string, SemanticValue> newsemantics = new Dictionary<string, SemanticValue>();
                            foreach (KeyValuePair<string, SemanticValue> sv in e.Result.Semantics)
                            {
                                newsemantics.Add(sv.Key, sv.Value);
                            }
                            var argstopass = new object[] { (object)newsemantics };
#if !LOCAL
                            var lRet = (string)_plugin._userCtl.GetType().GetMethod(ConfigurationManager.AppSettings["ManageSpeechMethod"]).Invoke(_plugin._userCtl, argstopass);

                            if (lRet != null && lRet != string.Empty)
                            {
                                speaker.Speak(lRet);
                            }
#endif
#if LOCAL
                            MessageBox.Show("Recognized text : " + e.Result.Text + ", Grammar rule name : " + e.Result.Grammar.RuleName);
#endif
                            startListen = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }

                this.endtime = DateTime.Now;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        private void timedisplay_Tick(object sender, EventArgs e)
        {
            this.timelabel.Text = DateTime.Now.ToShortTimeString();
        }

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            var ptr = bmp.GetHicon();
            var tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void DoMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void gamePortTimer_Tick(object sender, EventArgs e)
        {
        }

        private void MainTileCtl_Click(object sender, EventArgs e)
        {
        }

        private void btn_shutdown_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Etes vous sûre de vouloir quitter MySmartHouse Project ?", "Fermeture", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
