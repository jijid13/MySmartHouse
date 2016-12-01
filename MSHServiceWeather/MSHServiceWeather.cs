using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Configuration;
using MSHServiceWeather;
using System.Xml;
using System.ComponentModel.Design;
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
    public partial class MSHServiceWeather : UserControl, MSHService
    {
        private string DefaultLocation;
        private string IniFilePath;

        public MSHServiceWeather()
        {
            IniFilePath = this.GetType().Assembly.Location.Replace(".dll", ".ini");
            this.DefaultLocation = INIFile.ReadValue("parameters", "Default_Location", IniFilePath);
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml(INIFile.ReadValue("parameters", "BackgroundColor1", IniFilePath));
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
            var location = this.DefaultLocation;
            var day = string.Empty;

            if (P_Semantics["question"].Value.ToString() != "")
            {
                if (P_Semantics["location"].Value.ToString() != "")
                    location = P_Semantics["location"].Value.ToString();
                
                if (P_Semantics["day"].Value.ToString() != "" && P_Semantics["day"].Value.ToString().ToLower() != "demain")
                {
                    day = P_Semantics["day"].Value.ToString();
                }
                else if (P_Semantics["day"].Value.ToString().ToLower() == "demain")
                {
                    day = System.DateTime.Now.AddDays(1).DayOfWeek.ToString().Substring(0, 3) + "_demain";
                }
                else if (P_Semantics["day"].Value.ToString() == "")
                {
                    day = "_aujourdh'ui";
                }

                return getWeather(location, day);
            }

            return null;
        }

        private String getWeather(string location, string day)
        {
            var xml = new XmlDocument();
            xml.Load(INIFile.ReadValue("parameters", "yahooURL", IniFilePath).Replace("%CITY_NAME%", location));
            string dayCode = day.Split('_')[0];
            string dayName = day.Split('_')[1];

            var xnList = xml.SelectNodes("/query/results/channel/item");
            string low, hight, code, ret;

            low = hight = code = ret = string.Empty;

            foreach (XmlNode xn in xnList)
            {
                foreach (XmlNode xn1 in xn.ChildNodes)
                {
                    if (xn1.Name == "yweather:forecast")
                    {
                        if (dayCode == string.Empty || dayCode == xn1.Attributes["day"].InnerText)
                        {
                            low = xn1.Attributes["low"].InnerText;
                            hight = xn1.Attributes["high"].InnerText;
                            code = xn1.Attributes["code"].InnerText;
                            goto EndOfLoop;
                        }
                    }
                }
            }

            EndOfLoop:
            if (code != string.Empty)
            {
                ret = "la météo à " + location;
                if (dayName != "")
                    ret = ret + " pour " + dayName;
                ret = ret + " " + getcode(code) + " la température minimale est " + low + "°, la maximale est " + hight + "°.";
            }
            else
            {
                ret = "je ne peux pas donner suite à votre requête";
            }

            return ret;
        }

        private string getcode(string code)
        {
            var ret = string.Empty;
            switch (code)
            {
                case "0":
                    ret = "une tornade est prévue";
                    break;
                case "1":
                    ret = "une tempête tropicale est prévue";
                    break;
                case "2":
                    ret = "un ouragan est prévu";
                    break;
                case "3":
                    ret = "des orages violents sont prévus";
                    break;
                case "4":
                    ret = "des orages sont prévus";
                    break;
                case "5":
                    ret = "la pluie et la neige sont prévus";
                    break;
                case "6":
                    ret = "la pluie et la neige fondue sont prévus";
                    break;
                case "7":
                    ret = "neige mêlées et le grésil sont prévus";
                    break;
                case "8":
                    ret = "bruine verglaçante est prévue";
                    break;
                case "9":
                    ret = "bruine est prévue";
                    break;
                case "10":
                    ret = "pluie verglaçante est prévue";
                    break;
                case "11":
                    ret = "des averses sont prévues";
                    break;
                case "12":
                    ret = "des averses sont prévues";
                    break;
                case "13":
                    ret = "des averses de neige sont prévues";
                    break;
                case "14":
                    ret = "des averses de neige sont prévues";
                    break;
                case "15":
                    ret = "poudrerie prévue";
                    break;
                case "16":
                    ret = "neige prévue";
                    break;
                case "17":
                    ret = "grêle prévue";
                    break;
                case "18":
                    ret = "grésil prévu";
                    break;
                case "19":
                    ret = "poussière prévue";
                    break;
                case "20":
                    ret = "brumeux prévu";
                    break;
                case "21":
                    ret = "brume prévue";
                    break;
                case "22":
                    ret = "enfumé prévu";
                    break;
                case "23":
                    ret = "venteux prévu";
                    break;
                case "24":
                    ret = "venteux prévu";
                    break;
                case "25":
                    ret = "froid prévu";
                    break;
                case "26":
                    ret = "temps nuageux";
                    break;
                case "27":
                    ret = "Partiellement nuageux";
                    break;
                case "28":
                    ret = "Partiellement nuageux";
                    break;
                case "29":
                    ret = "Partiellement nuageux";
                    break;
                case "30":
                    ret = "Partiellement nuageux";
                    break;
                case "31":
                    ret = "transparent";
                    break;
                case "32":
                    ret = "ensoleillé";
                    break;
                case "33":
                    ret = "juste";
                    break;
                case "34":
                    ret = "juste";
                    break;
                case "35":
                    ret = "pluie mêlée de grêle";
                    break;
                case "36":
                    ret = "chaude";
                    break;
                case "37":
                    ret = "orages isolés";
                    break;
                case "38":
                    ret = "orages dispersés";
                    break;
                case "39":
                    ret = "orages dispersés";
                    break;
                case "40":
                    ret = "averses dispersées";
                    break;
                case "41":
                    ret = "fortes chutes de neige";
                    break;
                case "42":
                    ret = "Chutes de neige";
                    break;
                case "43":
                    ret = "fortes chutes de neige";
                    break;
                case "44":
                    ret = "éclaircies";
                    break;
                case "45":
                    ret = "averses orageuses";
                    break;
                case "46":
                    ret = "averses de neige";
                    break;
                case "47":
                    ret = "averses orageuses isolées";
                    break;
                case "3200":
                    ret = "n'est pas disponible";
                    break;
            }
            return ret;
        }
    }
}
