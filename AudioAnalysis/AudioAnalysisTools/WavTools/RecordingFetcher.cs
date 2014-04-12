using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.WavTools
{
    public class RecordingFetcher
    {
        private const string SERVER = "http://sensor.mquter.qut.edu.au/sensors";

        public static byte[] GetRecordingByFileName(string filename)
        {
            Uri address = ConvertToUri(filename);

            System.Net.WebClient webClient = new System.Net.WebClient();
            return webClient.DownloadData(address);
        }

        private static Uri ConvertToUri(string filename)
        {
            string sensorName, recordingName, extension;

            ParseFilename(filename, out sensorName, out recordingName, out extension);

            string uriString = string.Format("{0}/{1}/{2}.{3}", SERVER, sensorName, recordingName, extension);

            return new Uri(uriString);
            
        }

        private static void ParseFilename(string filename, out string sensorname, out string recordingname, out string extension)
        {
            filename = filename.Replace('/', '_');

            string[] parts = filename.Split('_');
            sensorname = parts[0];
            string fileId = parts[1];
            
            string[] parts2 = fileId.Split('.');
            recordingname = parts2[0];
            extension = parts2[1];
        }



    }//end class
}//end nmaespace
