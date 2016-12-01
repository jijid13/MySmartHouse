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
using System.IO;
using System.Data;
using MSHServiceMyFox;
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
    public partial class MSHServiceMyFox : UserControl, MSHService
    {
        private string IniFilePath;
        private String token;
        private String mf_urlToken;
        private String mf_getSecurityUrl;
        private String mf_setSecurityUrl;
        private String mf_ScenarioUrl;
        private String mf_TemperatureUrl;
        private String client_id;
        private String client_key;
        private String login;
        private String password;
        private String site_id;
        private String mirror_url;
        private Dictionary<int, Room> rooms;
        private static string DSPath = Directory.GetCurrentDirectory() + @"\plugins\MSHServiceMyFox\tempHistoryDS.xml";

        public MSHServiceMyFox()
        {
            IniFilePath = this.GetType().Assembly.Location.Replace(".dll", ".ini");
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml(INIFile.ReadValue("parameters", "BackgroundColor1", IniFilePath));
            this.mf_urlToken = INIFile.ReadValue("parameters", "mf_urlToken", IniFilePath);
            this.mf_getSecurityUrl = INIFile.ReadValue("parameters", "mf_getSecurityUrl", IniFilePath);
            this.mf_setSecurityUrl = INIFile.ReadValue("parameters", "mf_setSecurityUrl", IniFilePath);
            this.mf_ScenarioUrl = INIFile.ReadValue("parameters", "mf_ScenarioUrl", IniFilePath);
            this.mf_TemperatureUrl = INIFile.ReadValue("parameters", "mf_TemperatureUrl", IniFilePath);
            this.client_id = INIFile.ReadValue("parameters", "client_id", IniFilePath);
            this.client_key = INIFile.ReadValue("parameters", "client_key", IniFilePath);
            this.login = INIFile.ReadValue("parameters", "login", IniFilePath);
            this.password = INIFile.ReadValue("parameters", "password", IniFilePath);
            this.site_id = INIFile.ReadValue("parameters", "site_id", IniFilePath);
            string temp_rooms = INIFile.ReadValue("parameters", "temperature_rooms", IniFilePath);
            this.mirror_url = INIFile.ReadValue("parameters", "mirror_url", IniFilePath);
            this.rooms = new Dictionary<int, Room>();
            foreach (string str in temp_rooms.Split('|'))
            {
                this.rooms.Add(int.Parse(str.Split(',')[1]), new Room(int.Parse(str.Split(',')[1]), str.Split(',')[0]));
            }

            if (File.Exists(DSPath))
            {
                this.tempHistoryDS1.WriteXml(DSPath);
            }
            getToken();
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
            actionScenario(P_Semantics["lumiere"].Value.ToString());
            
            setSecurity(P_Semantics["alarme"].Value.ToString());

            string ret = getTemperatureCommand(P_Semantics["temperature"].Value.ToString());
            
            actionScenario(P_Semantics["sleep"].Value.ToString());

            return ret;
        }

        private void getToken()
        {
            using (var client = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection();
                reqparm.Add("grant_type", "password");
                reqparm.Add("client_id", this.client_id);
                reqparm.Add("client_secret", this.client_key);
                reqparm.Add("username", this.login);
                reqparm.Add("password", this.password);
                var responsebytes = client.UploadValues(this.mf_urlToken, "POST", reqparm);
                var responsebody = Encoding.UTF8.GetString(responsebytes);
                token = responsebody.Substring(17, 40);
            }
        }

        private string getSecurity()
        {
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString(this.mf_getSecurityUrl.Replace("%site", this.site_id).Replace("%token", this.token));
                var o = JObject.Parse(json);
                var ret = o["payload"]["statusLabel"].ToString();

                switch (ret)
                {
                    case "disarmed":
                        ret = "l'alarme est désactivée";
                        break;

                    case "armed":
                        ret = "l'alarme est activée";
                        break;

                    case "partial":
                        ret = "l'alarme est partiellement activée";
                        break;
                }

                return ret;
            }
        }

        private string actionScenario(String P_Scenario)
        {
            if (P_Scenario != "")
            {
                JObject o;
                string ret = "";

                foreach (string str in P_Scenario.Split('|'))
                {
                    using (var client = new WebClient())
                    {
                        var reqparm = new System.Collections.Specialized.NameValueCollection();
                        reqparm.Add("site", this.site_id);
                        reqparm.Add("scenario", str);
                        reqparm.Add("token", this.token);
                        var responsebytes = client.UploadValues(this.mf_ScenarioUrl.Replace("%site", this.site_id).Replace("%token", this.token).Replace("%scenario", str), "POST", reqparm);
                        o = JObject.Parse(Encoding.UTF8.GetString(responsebytes));
                        ret = o["status"].ToString();

                        switch (ret.ToLower())
                        {
                            case "ok":
                                ret = "c'est fait";
                                break;

                            default:
                                ret = "je ne peux pas le faire";
                                break;
                        }
                    }
                }
                return ret;
            }
                
            return "";
        }

        private string getTemperatureCommand(string P_command)
        {
            string ret = "";
            if (P_command != "")
            {
                setLastTemperature();
                ret = "la température de ";
                foreach (KeyValuePair<int, Room> room in this.rooms)
                {
                    ret += room.Value.Title + " est " + room.Value.Temperature + "degrés et de ";
                }

                ret = ret.Substring(0, ret.Length - 7) + ".";
            }

            return ret;
        }

        private void setLastTemperature()
        {
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString(mf_TemperatureUrl.Replace("%site", site_id).Replace("%token", this.token));
                var o = JObject.Parse(json);
                foreach (KeyValuePair<int, Room> room in this.rooms)
                {
                    room.Value.Temperature = double.Parse(o["payload"]["items"][room.Key]["lastTemperature"].ToString());
                }
            }
        }

        private string setSecurity(string P_Level)
        {
            if (P_Level != "")
            {
                using (var webClient = new System.Net.WebClient())
                {
                    var json = webClient.DownloadString(mf_setSecurityUrl.Replace("%site", site_id).Replace("%token", this.token).Replace("%level", P_Level));
                    var o = JObject.Parse(json);
                    var ret = o["status"].ToString();
                    switch (ret.ToLower())
                    {
                        case "ok":
                            ret = "c'est fait";
                            break;

                        default:
                            ret = "je ne peux pas le faire";
                            break;
                    }

                    return ret;
                }
            }

            return "";
        }

        private void insertNewTemperature()
        {
            setLastTemperature();

            DataRow row;
            foreach (KeyValuePair<int, Room> room in this.rooms)
            {
                row = this.tempHistoryDS1.Tables[0].NewRow();
                row["date"] = DateTime.Now;
                row["piece"] = room.Value.Id;
                row["temperature"] = room.Value.Temperature;
                this.tempHistoryDS1.Tables[0].Rows.Add(row);
            }

            this.tempHistoryDS1.WriteXml(DSPath);
        }

        private void timerToken_Tick(object sender, EventArgs e)
        {
            getToken();
            insertNewTemperature();
        }
    }
}
