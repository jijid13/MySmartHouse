using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSHServiceTraffic
{
    internal class INIFile
    {
        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileString(string ApplicationName, string KeyName, string StrValue, string FileName);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string ApplicationName, string KeyName, string DefaultValue, StringBuilder ReturnString, int nSize, string FileName);
        public static void WriteValue(string SectionName , string KeyName, string KeyValue, string FileName)
        {
            WritePrivateProfileString(SectionName , KeyName, KeyValue, FileName);
        }
        public static string ReadValue(string SectionName , string KeyName , string FileName)
        {
            var szStr = new StringBuilder(255);
            GetPrivateProfileString(SectionName, KeyName, string.Empty, szStr, 255, FileName);
            return szStr.ToString().Trim();
        }
    }
}
