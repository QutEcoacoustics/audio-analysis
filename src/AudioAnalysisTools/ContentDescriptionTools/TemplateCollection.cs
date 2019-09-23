// <copyright file="TemplateCollection.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Schema;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    public class TemplateCollection : Dictionary<string, TemplateManifest>, IConfig
    {
        public static void CreateNewTemplatesManifest(FileInfo manifestFile)
        {
            // Read in all template manifests
            var templateCollection = ConfigFile.Deserialize<TemplateCollection>(manifestFile);
            var oldFile = new FileInfo(Path.Combine(manifestFile.DirectoryName ?? throw new InvalidOperationException(), "ContentDescriptionTemplates.Backup.yml"));
            Yaml.Serialize(oldFile, templateCollection);

            foreach (var kvp in templateCollection)
            {
                var templateManifest = kvp.Value;

                if (templateManifest.Status == TemplateStatus.Locked)
                {
                    continue;
                }

                if (templateManifest.Status == TemplateStatus.CalculateTemplate)
                {
                    var newTemplate = CreateTemplate(templateManifest);
                    templateManifest.Template = newTemplate;
                }

                templateManifest.MostRecentEdit = DateTime.Now;
            }

            Yaml.Serialize(manifestFile, templateCollection);
        }

        /// <summary>
        /// THis method calculates new template based on passed manifest.
        /// </summary>
        public static Dictionary<string, double[]> CreateTemplate(TemplateManifest templateManifest)
        {
            // Read all indices from the complete recording. The path variable is a partial path requiring to be appended.
            var path = new FileInfo(Path.Combine(templateManifest.TemplateSourceSubdirectory, templateManifest.TemplateSourceFileName));
            var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(path.FullName + ContentDescription.AnalysisString);
            var algorithmType = templateManifest.FeatureExtractionAlgorithm;
            Dictionary<string, double[]> newTemplate;

            switch (algorithmType)
            {
                case 1:
                    newTemplate = ContentAlgorithms.CreateFullBandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                case 2:
                    newTemplate = ContentAlgorithms.CreateBroadbandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                case 3:
                    newTemplate = ContentAlgorithms.CreateNarrowBandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                default:
                    //LoggedConsole.WriteWarnLine("Algorithm " + algorithmType + " does not exist.");
                    newTemplate = null;
                    break;
            }

            return newTemplate;
        }

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
