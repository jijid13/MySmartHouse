using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
#if MICRO
using System.Speech.Recognition;
#endif

#if KINECT
using Microsoft.Speech.Recognition;
#endif

namespace MySmartHouse
{
    internal class Plugin
    {
        public UserControl _userCtl;
        private static string _pluginDirectory = Directory.GetCurrentDirectory() + @"\plugins";
        public string _Title;
        public string _GroupName;

        public Plugin(UserControl P_userCtl)
        {
            this._userCtl = P_userCtl;
        }

        public static Dictionary<string, Plugin> loadPlugins(SpeechRecognitionEngine P_speechEngine, FlowLayoutPanel P_tileControl)
        {
            var _PluginsList = new Dictionary<string, Plugin>();

            try
            {
                var DirName = string.Empty;
                Plugin _plugin;
                foreach (string d in Directory.GetDirectories(_pluginDirectory))
                {
                    try
                    {
                        DirName = d.Remove(0, _pluginDirectory.Length + 1);
                        if (File.Exists(d + @"\" + DirName + ".dll"))
                        {
                            var myDllAssembly = System.Reflection.Assembly.LoadFile(d + @"\" + DirName + ".dll");
                            var MyDLLFormInstance = (UserControl)myDllAssembly.CreateInstance("MSHService." + DirName);

                            P_speechEngine.LoadGrammar((Grammar)MyDLLFormInstance.GetType().GetMethod("getGrammar").Invoke(MyDLLFormInstance, new object[] { (object)d }));
                            _plugin = new Plugin(MyDLLFormInstance);
                            _plugin._Title = (string)MyDLLFormInstance.GetType().GetMethod("getTitle").Invoke(MyDLLFormInstance, null);
                            _plugin._GroupName = (string)MyDLLFormInstance.GetType().GetMethod("getGroupName").Invoke(MyDLLFormInstance, null);

                            _PluginsList.Add(_plugin._Title, _plugin);

                            Image img = null;
                            if (File.Exists(d + @"\" + DirName + ".png"))
                            {
                                img = Image.FromFile(d + @"\" + DirName + ".png");
                            }
                            initTile(_plugin._Title, _plugin._GroupName, P_tileControl, img);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }

                if (File.Exists(Directory.GetCurrentDirectory() + @"\MySmartHouse.xml"))
                {
                    XmlDocument GrammarXmlDoc = getGrammarXMLFile(Directory.GetCurrentDirectory() + @"\MySmartHouse.xml");
                    var stream = new MemoryStream();
                    GrammarXmlDoc.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    P_speechEngine.LoadGrammar(new Grammar(stream, "MySmartHouse"));
                }
            }
            catch (System.Exception excpt)
            {
                Logger.Error(excpt.Message);
            }

            return _PluginsList;
        }

        private static XmlDocument getGrammarXMLFile(String P_FileName)
        {
            var lFile = P_FileName;
            XmlDocument xd1 = null;

            if (!File.Exists(lFile))
            {
                return null;
            }

            try
            {
                xd1 = new XmlDocument();
                xd1.Load(lFile);
                xd1["grammar"]["rule"]["one-of"]["item"].FirstChild.Value = ConfigurationManager.AppSettings["ServiceName"];
                xd1["grammar"]["rule"]["one-of"]["item"]["tag"].FirstChild.Value = "out = \"" + ConfigurationManager.AppSettings["ServiceName"] + "\";";
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return null;
            }

            return xd1;
        }

        static public Bitmap ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            Bitmap bmp = new Bitmap(newImage);

            return bmp;
        }

        private static void initTile(string P_Title, string P_GroupName, FlowLayoutPanel P_tileControl, Image img)
        {
            var _sefTile = new MetroTile();
            _sefTile.Size = new Size(250, 167);
            _sefTile.TextAlign = ContentAlignment.BottomRight;
            _sefTile.Text = P_Title;
            _sefTile.TileImage = ScaleImage(img, 250, 167);
            _sefTile.TileImageAlign = ContentAlignment.MiddleCenter;
            _sefTile.BackColor = Color.SteelBlue;
            _sefTile.UseCustomBackColor = true;
            _sefTile.UseTileImage = true;
            _sefTile.TileTextFontSize = MetroFramework.MetroTileTextSize.Small;// .Font = new Font("Verdana", 12, FontStyle.Bold);
            _sefTile.TileTextFontWeight = MetroFramework.MetroTileTextWeight.Regular;
            P_tileControl.Controls.Add(_sefTile);
        }
    }
}
