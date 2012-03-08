using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AudioBrowser
{
    static class AudioBrowser
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AudioBrowser1(args));
            //Application.Run(new AudioBrowser2(args));
        }


    }
}
