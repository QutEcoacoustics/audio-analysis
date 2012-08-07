namespace AudioBrowser
{
    using System.IO;
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            //Application.Run(new AudioBrowser1(System.Environment.GetCommandLineArgs())); 
        }
    }
}
