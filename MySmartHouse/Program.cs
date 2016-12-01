using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MySmartHouse
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.Run(new StartForm());
        }
    }
}
