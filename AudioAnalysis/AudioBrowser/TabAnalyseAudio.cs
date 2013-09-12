using Acoustics.Shared;
using Acoustics.Tools.Audio;
using AnalysisBase;
using AudioAnalysisTools;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AudioBrowser.Tab
{
    public class TabAnalyseAudio
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TabAnalyseAudio));

        private Helper helper;

        public TabAnalyseAudio(Helper helper)
        {
            this.helper = helper;
        }

    }
}
