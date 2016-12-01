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
    public partial class MSHServiceSonyBravia : UserControl, MSHService
    {
        private string IniFilePath;
        private String sony_url;
        private String xml_command;

        public MSHServiceSonyBravia()
        {
            IniFilePath = this.GetType().Assembly.Location.Replace(".dll", ".ini");
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml(INIFile.ReadValue("parameters", "BackgroundColor1", IniFilePath));
            this.sony_url = INIFile.ReadValue("parameters", "sony_url", IniFilePath);
            this.xml_command = INIFile.ReadValue("parameters", "xml_command1", IniFilePath) + INIFile.ReadValue("parameters", "xml_command2", IniFilePath);
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

            actionScenario(P_Semantics["hdmi"].Value.ToString());
            
            actionScenario(P_Semantics["sleep"].Value.ToString());
            
            actionScenario(P_Semantics["sound"].Value.ToString());
            
            actionScenario(P_Semantics["channel"].Value.ToString());

            return null;
        }

        private void actionScenario(string P_Command)
        {
            if (P_Command != "")
            {
                System.Collections.Specialized.NameValueCollection reqparm;
                string postData;
                byte[] byteArray;
                byte[] responsebytes;
                string result;
                foreach (string str in P_Command.Split('|'))
                {
                    using (var client = new WebClient())
                    {
                        reqparm = new System.Collections.Specialized.NameValueCollection();
                        postData = xml_command.Replace("%COMMAND%", str);
                        byteArray = Encoding.UTF8.GetBytes(postData);
                        responsebytes = client.UploadData(this.sony_url, "POST", byteArray);
                        result = System.Text.Encoding.UTF8.GetString(responsebytes);
                    }
                }
            }
        }
    }
}
