// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestResources.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioEncodingTest
{
    using System;
    using System.IO;

    using AudioAnalysisTools;

    using AudioTools;
    using AudioTools.AudioUtlity;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The test resources.
    /// </summary>
    internal class TestResources
    {
        /*
         * NOTE:   if you change these file names, they also need to be changed 
         * NOTE:   in AnalysisPrograms.Processing.ProcessingUtils input files
        */

        // expected file names
        public const string SettingsFileName = "processing_input_settings.txt";
        public const string AudioFileName = "processing_input_audio.wav";

        // standard out and error
        public const string StderrFileName = "output_stderr.txt";
        public const string StdoutFileName = "output_stdout.txt";

        // analysis program file names
        public const string ProgramOutputFinishedFileName = "output_finishedmessage.txt";
        public const string ProgramOutputResultsFileName = "output_results.xml";
        public const string ProgramOutputErrorFileName = "output_error.txt";

        public static readonly string WorkingDir = new DirectoryInfo(@".\TestOutput\").FullName;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResources"/> class.
        /// </summary>
        /// <param name="audioFileName">
        /// The audio file name.
        /// </param>
        /// <param name="paramFileName">
        /// The param file name.
        /// </param>
        /// <param name="resourceFileName">
        /// The resource file name.
        /// </param>
        public TestResources(string audioFileName, string paramFileName, string resourceFilePartialPath)
        {
            // audio file location
            string audioFile = Path.Combine(new DirectoryInfo(@".\TestData\").FullName, audioFileName);
            this.TestAudioFile = new FileInfo(audioFile);
            Assert.IsTrue(File.Exists(this.TestAudioFile.FullName), "Test data is not available from " + audioFile);

            // find /create output dir
            string testoutput = Path.Combine(WorkingDir, Guid.NewGuid().ToString());
            if (!Directory.Exists(testoutput))
            {
                Directory.CreateDirectory(testoutput);
            }

            this.TestWorkingDir = new DirectoryInfo(testoutput);
            Assert.IsTrue(
                this.TestWorkingDir != null && Directory.Exists(this.TestWorkingDir.FullName),
                "Output dir not found. " + testoutput);

            // check if conversion is required.
            var destFile = Path.ChangeExtension(this.TestAudioFile.FullName, "wav");
            destFile = Path.Combine(this.TestWorkingDir.FullName, Path.GetFileName(destFile));
            SpecificWavAudioUtility.ConvertToWav(this.TestAudioFile.FullName, destFile);

            var success = File.Exists(destFile);

            Assert.IsTrue(success, "Could not get wav pcm file for " + this.TestAudioFile.FullName);
            this.TestAudioFile = new FileInfo(destFile);

            Assert.IsTrue(File.Exists(this.TestAudioFile.FullName), "Test data is not available from " + audioFile);

            // find source dir
            string source1 = new DirectoryInfo(@"..\..\..\Source\AudioAnalysis\").FullName;
            string source2 = new DirectoryInfo(@"..\..\..\AudioAnalysis\").FullName;

            DirectoryInfo sourceDir = null;
            if (Directory.Exists(source1))
            {
                sourceDir = new DirectoryInfo(source1);
            }

            if (Directory.Exists(source2))
            {
                sourceDir = new DirectoryInfo(source2);
            }

            Assert.IsTrue(sourceDir != null && Directory.Exists(sourceDir.FullName), "Source dir does not exist.");

            // get program file
            string programDir = Path.Combine(sourceDir.FullName, @"AnalysisPrograms\bin");
            string release = Path.Combine(programDir, @"Release\AnalysisPrograms.exe");
            string debug = Path.Combine(programDir, @"Debug\AnalysisPrograms.exe");

            if (File.Exists(release))
            {
                this.AnalysisProgramsExeFile = new FileInfo(release);
            }

            if (File.Exists(debug))
            {
                this.AnalysisProgramsExeFile = new FileInfo(debug);
            }

            Assert.IsTrue(
                this.AnalysisProgramsExeFile != null && File.Exists(this.AnalysisProgramsExeFile.FullName),
                "Exe file not found.");

            // get param file
            var paramFile = new FileInfo(Path.Combine(sourceDir.FullName, @"RecogniserParamFiles\" + paramFileName));
            this.ParamFile = paramFile;
            Assert.IsTrue(
                this.ParamFile != null && File.Exists(this.ParamFile.FullName), "Param file not found: " + paramFile);

            // get resources file if given
            if (!string.IsNullOrEmpty(resourceFilePartialPath))
            {
                var resourceFile = Path.Combine(sourceDir.FullName, "RecogniserTemplates");
                resourceFile = Path.Combine(resourceFile, resourceFilePartialPath);
                this.ResourceFile = new FileInfo(resourceFile);

                Assert.IsTrue(
                    this.ResourceFile != null && File.Exists(this.ResourceFile.FullName),
                    "Resource file not found: " + resourceFile);
            }

            // copy audio file, settings file and resource file to working dir
            var copiedAudio = Path.Combine(this.TestWorkingDir.FullName, AudioFileName);
            File.Copy(this.TestAudioFile.FullName, copiedAudio);
            this.TestAudioFile = new FileInfo(copiedAudio);

            var copiedParam = Path.Combine(this.TestWorkingDir.FullName, SettingsFileName);
            File.Copy(this.ParamFile.FullName, copiedParam);
            this.ParamFile = new FileInfo(copiedParam);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets AnalysisProgramsExeFile.
        /// </summary>
        public FileInfo AnalysisProgramsExeFile { get; private set; }

        /// <summary>
        /// Gets ParamFile.
        /// </summary>
        public FileInfo ParamFile { get; private set; }

        /// <summary>
        /// Gets ResourceFile.
        /// </summary>
        public FileInfo ResourceFile { get; private set; }

        /// <summary>
        /// Gets TestAudioFile.
        /// </summary>
        public FileInfo TestAudioFile { get; private set; }

        /// <summary>
        /// Gets TestOutputDir.
        /// </summary>
        public DirectoryInfo TestWorkingDir { get; private set; }

        #endregion
    }
}