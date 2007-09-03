using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QUT
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

            Sensor s = new Sensor();
            s.Start();
                        
        }        

    }


    
}