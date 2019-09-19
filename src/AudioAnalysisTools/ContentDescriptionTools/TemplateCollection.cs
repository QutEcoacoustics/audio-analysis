using System;
using System.Collections.Generic;

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    //using Zio;

    public class TemplateCollection : Dictionary<string, ContentTemplate>, IConfig
    {
        static TemplateCollection()
        {
            ConfigFile.Defaults.Add(typeof(TemplateCollection), "ContentDescriptionTemplates.yml");
        }

        public TemplateCollection()
        {
            void OnLoaded(IConfig config)
            {
                int i = 0;
                foreach (var kvp in this)
                {
                    // assign the key to the object for consistency
                    kvp.Value.Name = kvp.Key;

                    // HACK: infer order of properties for visualization based on order of for-each
                    kvp.Value.TemplateId = i;
                    i++;
                }
            }

            this.Loaded += OnLoaded;
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

    public abstract class TemplatesConfig : AnalyzerConfig
    {
        protected TemplatesConfig()
        {
            void OnLoaded(IConfig config)
            {
                //var indicesPropertiesConfig = Indices.IndexProperties.Find(this, this.ConfigPath);
                this.TemplateConfig = @"C:\Work\GitHub\audio-analysis\src\AnalysisConfigFiles\ContentDescriptionTemplates.yml";
                this.DictionaryOfTemplates = ConfigFile.Deserialize<TemplateCollection>(this.TemplateConfig);
            }

            this.Loaded += OnLoaded;
        }

        public string TemplateConfig { get; set; }

        public TemplateCollection DictionaryOfTemplates { get; private set; }
    }
}
