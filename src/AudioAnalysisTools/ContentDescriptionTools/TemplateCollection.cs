// <copyright file="TemplateCollection.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ConfigFile;

    public class TemplateCollection : Dictionary<string, TemplateManifest>, IConfig
    {
        public event Action<IConfig> Loaded;

        public string ConfigPath { get; set; }

        void IConfig.InvokeLoaded()
        {
            this.Loaded?.Invoke(this);
        }

        //    public static Dictionary<string, double[,]> GetTemplateMatrices(TemplateCollection templates)
        //    {
        //        // init dictionary of matrices
        //        var opTemplate = new Dictionary<string, double[,]>();

        //        foreach (var template in templates)
        //        {
        //            var name = template.Key;
        //            var templateData = template.Value;
        //            var dataDict = templateData.Template;

        //            // init a matrix to contain template values
        //            var matrix = new double[,];
        //            foreach (var kvp in dataDict)
        //            {
        //                var array = kvp.Value;
        //                matrix.AddRow();
        //            }

        //            opTemplate.Add(name, matrix);
        //        }

        //        return opTemplate;
        //    }
    }
}