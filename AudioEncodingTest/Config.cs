using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AudioEncodingTest
{
    public static class Config
    {
        public static string AnalaysisTechnique
        {
            get
            {
                return ConfigurationManager.AppSettings["AnalysisTechnique"];
            }
        }

        public static string AnalysisTechniqueParameterFile
        {
            get
            {
                return ConfigurationManager.AppSettings["AnalysisTechniqueParameterFile"];
            }
        }

        public static string Encoder
        {
            get
            {
                return (ConfigurationManager.AppSettings["Encoder"]);
            }
        }
        public static string EncoderFormatString
        {
            get
            {
                return (ConfigurationManager.AppSettings["EncoderFormatString"]);
            }
        }

        public static string Decoder
        {
            get
            {
                return (ConfigurationManager.AppSettings["Decoder"]);
            }
        }
        public static string DecoderFormatString
        {
            get
            {
                return (ConfigurationManager.AppSettings["DecoderFormatString"]);
            }
        }

        public static IEnumerable<string> Parameters
        {
            get
            {
                return ConfigurationManager.AppSettings.AllKeys.
                    Where(s => s.StartsWith("Param_")).
                    Select(s => s.Substring("Param_".Length));
            }
        }

        public static IEnumerable<string> GetValues(string param)
        {
            string rawValue = ConfigurationManager.AppSettings["Param_" + param];
            if (rawValue == null || rawValue.Length == 0)
                yield return null;

            string expansion = "";
            if (rawValue.StartsWith("{"))
            {
                int index = rawValue.IndexOf('}');
                if (index == -1)
                    throw new Exception("Invalid parameter value (missing closing brace)");

                expansion = rawValue.Substring(1, index - 1);
                rawValue = rawValue.Substring(index + 1);
            }

            foreach (string element in rawValue.Split(','))
            {
                yield return expansion + element;
            }
        }
    }
}
