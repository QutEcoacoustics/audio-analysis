using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Production
{
    using System.Dynamic;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;

    using Acoustics.Shared.Extensions;

    using AnalysisBase;

    using PowerArgs;

    using TowseyLibrary;

    public enum DebugOptions
    {
        No = 0,
        Yes = 1,
        Prompt = 3
    }

    public partial class MainEntryArguments
    {
        public MainEntryArguments()
        {
            this.DebugOption = DebugOptions.Prompt;
        }

           
        [ArgRequired]
        [ArgPosition(0)]
        public string Action { get; set; }

        [DefaultValue(false)]
        public bool Debug { get; set; }

        [DefaultValue(false)]
        public bool NoDebug { get; set; }

        [ArgIgnore]
        public DebugOptions DebugOption
        {
            get
            {
                if (this.NoDebug)
                {
                    return DebugOptions.No;
                }
                else if (this.Debug)
                {
                    return DebugOptions.Yes;
                }
                else
                {
                    return DebugOptions.Prompt;
                }
            }
            set
            {
                switch (value)
                {
                    case DebugOptions.Yes:
                        this.NoDebug = false;
                        this.Debug = true;
                        break;
                    case DebugOptions.No:
                        this.Debug = false;
                        this.NoDebug = true;
                        break;
                    default:
                        this.Debug = this.NoDebug = false;
                        break;
                }
            }
        }
    }

    public class HelpArguments
    {
        [ArgPosition(1)]
        public string ActionName { get; set; }
    }

    
    public class SourceArguments
    {
        
        [ArgDescription("The source audio file to operate on")]
        [Production.ArgExistingFile()]
        [ArgPosition(1)]
        [ArgRequired]
        public FileInfo Source { get; set; }
    }

    public class SourceAndConfigArguments : SourceArguments
    {
        [ArgDescription("The path to the config file")]
        [Production.ArgExistingFile()]
        [ArgRequired]
        [ArgPosition(2)]
        public FileInfo Config { get; set; }
    }

    public class SourceConfigOutputDirArguments : SourceAndConfigArguments
    {
        [ArgDescription("A directory to write output to")]
        [Production.ArgExistingDirectory(createIfNotExists: true)]
        [ArgRequired]
        [ArgPosition(3)]
        public virtual DirectoryInfo Output { get; set; }
    }


    public class AnalyserArguments : SourceConfigOutputDirArguments
    {
        public string TmpWav { get; set; }

        public string Events { get; set; }

        public string Indices { get; set; }

        public string Sgram { get; set; }

        [ArgDescription("The start offset to start analysing from (in seconds)")]
        [ArgRange(0, double.MaxValue)]
        public double? Start { get; set; }

        [ArgDescription("The duration of each segment to analyse (seconds) - a maximum of 10 minutes")]
        [ArgRange(0, 10 * 60)]
        public double? Duration { get; set; }


        public AnalysisSettings ToAnalysisSettings()
        {
            var analysisSettings = new AnalysisSettings();

            // ANT: renabled this line because it just makes sense! this is needed by IAnalyser cmd entry points // this not required at this point
            analysisSettings.SourceFile = this.Source; 
            analysisSettings.ConfigFile = this.Config;
            analysisSettings.AnalysisInstanceOutputDirectory = this.Output;
            analysisSettings.AudioFile = this.Output.CombineFile(this.Source.Name);
            analysisSettings.EventsFile = null;
            analysisSettings.SummaryIndicesFile = null;
            analysisSettings.ImageFile = null;

            var configuration = new ConfigDictionary(this.Config);
            analysisSettings.ConfigDict = configuration.GetTable();

            if (this.TmpWav.NotEmpty())
            {
                analysisSettings.AudioFile = this.Output.CombineFile(this.TmpWav);
            }

            if (this.Events.NotEmpty())
            {
                analysisSettings.EventsFile = this.Output.CombineFile(this.Events);
            }

            if (this.Indices.NotEmpty())
            {
                analysisSettings.SummaryIndicesFile = this.Output.CombineFile(this.Indices);
            }

            if (this.Sgram.NotEmpty())
            {
                analysisSettings.ImageFile = this.Output.CombineFile(this.Sgram);
            }

            return analysisSettings;
        }
    }


}
