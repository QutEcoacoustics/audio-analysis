using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.IO;

namespace TowseyLib
{
    public static class ProcessTools
    {

        public static void RunProcess(string workingDir, string appName, string arguments)
        {
            Process process = new Process();
            process.StartInfo.WorkingDirectory = workingDir;
            process.StartInfo.FileName = appName;
            process.StartInfo.Arguments = arguments;
            process.Start();
            process.WaitForExit();
        }



    }//end class
}
