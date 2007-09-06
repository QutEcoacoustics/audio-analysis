using System;
using System.Windows.Forms;

namespace CFRecorder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        [MTAThread]
        static void Main()
        {
            //Recorder recorder = new Recorder();

            Settings.SensorDataPath = "\\Storage Card"; 
            Sensor s = new Sensor();
            s.Start();

        }

    }
}
