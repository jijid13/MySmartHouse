using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Configuration;
using MSHServiceWeather;
using System.Xml;
using System.ComponentModel.Design;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
#if MICRO
using System.Speech.Recognition;
#endif
#if KINECT
using Microsoft.Speech.Recognition;
#endif

namespace MSHService
{
    interface MSHService
    {
        Grammar getGrammar(string pluginDirectory);
        string getTitle();
        string getGroupName();
        Color getBackGroundColor1();
        Color getBackGroundColor2();
        string manageSpeech(Dictionary<string, SemanticValue> P_Semantics);
    }

    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ComVisible(true)]
    [DefaultEvent("Load")]
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [Designer("System.Windows.Forms.Design.UserControlDocumentDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner))]
    [DesignerCategory("UserControl")]
    public partial class MSHServiceFreeBox : UserControl, MSHService
    {
        private string IniFilePath;
        private String freebox_url;
        private Thread thr;
        private Boolean _shouldStop = false;
        private string explore_program_time;
        private string explore_program_command;

        public MSHServiceFreeBox()
        {
            IniFilePath = this.GetType().Assembly.Location.Replace(".dll", ".ini");
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml(INIFile.ReadValue("parameters", "BackgroundColor1", IniFilePath));
            this.freebox_url = INIFile.ReadValue("parameters", "freebox_url", IniFilePath).Replace("%REMOTE_CODE%", INIFile.ReadValue("parameters", "remote_control_code", IniFilePath));
            explore_program_time = INIFile.ReadValue("parameters", "explore_program_time", IniFilePath);
            explore_program_command = INIFile.ReadValue("parameters", "explore_program_command", IniFilePath);
        }

        public Grammar getGrammar(string pluginDirectory)
        {
            return new Grammar(pluginDirectory + @"\" + this.GetType().Name + ".xml", this.GetType().Name);
        }

        public string getTitle()
        {
            return INIFile.ReadValue("parameters", "Title", IniFilePath);
        }

        public string getGroupName()
        {
            return INIFile.ReadValue("parameters", "GroupName", IniFilePath);
        }

        public Color getBackGroundColor1()
        {
            return ColorTranslator.FromHtml(INIFile.ReadValue("parameters", "BackgroundColor1", IniFilePath));
        }

        public Color getBackGroundColor2()
        {
            return ColorTranslator.FromHtml(INIFile.ReadValue("parameters", "BackgroundColor2", IniFilePath));
        }

        public string manageSpeech(Dictionary<string, SemanticValue> P_Semantics)
        {
            var day = string.Empty;

            actionScenario(P_Semantics["program"].Value.ToString());
            
            if (P_Semantics["explore"].Value.ToString() == "explore")
            {
                thr = new Thread(new ThreadStart(exploreTV));
                _shouldStop = false;
                thr.Start();
            }
            else if (P_Semantics["explore"].Value.ToString() == "stop")
            {
                _shouldStop = true;
            }
            else
                actionScenario(P_Semantics["explore"].Value.ToString());
            
            actionScenario(P_Semantics["sound"].Value.ToString());
            
            actionScenario(P_Semantics["functionality"].Value.ToString());
            
            actionScenario(P_Semantics["sleep"].Value.ToString());

            return null;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void exploreTV()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            int ret;
            int.TryParse(explore_program_time, out ret);
            if(ret > 0) 
            {
                while (!_shouldStop)
                {
                    this.actionScenario(explore_program_command);
                    Thread.Sleep(ret);
                }
            }
        }

        private void actionScenario(string P_command)
        {
            if(P_command != "")
            {
                string url;
                WebRequest request;
                String Status;
                Stream dataStream;
                StreamReader reader;
                string responseFromServer;
                WebResponse response;

                foreach (string str in P_command.Split('|'))
                {
                    if (str.Contains("sleep"))
                    {
                        System.Threading.Thread.Sleep(int.Parse(str.Replace("sleep(", "").Replace(")", "")));
                    }
                    else
                    {
                        url = freebox_url + str;
                        request = WebRequest.Create(url);
                        response = request.GetResponse();
                        Status = ((HttpWebResponse)response).StatusDescription;
                        dataStream = response.GetResponseStream();
                        reader = new StreamReader(dataStream);
                        responseFromServer = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();
                        response.Close();
                    }
                }
            }
        }
    }
}
