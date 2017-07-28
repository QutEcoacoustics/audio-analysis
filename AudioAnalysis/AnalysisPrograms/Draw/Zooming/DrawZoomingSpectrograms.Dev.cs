namespace AnalysisPrograms.Draw.Zooming
{
    using System;
    using System.IO;

    public static partial class DrawZoomingSpectrograms
    {
        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "zoomingSpectrograms"
        /// </summary>
        /// <returns>
        /// The <see cref="Arguments"/>.
        /// </returns>
        public static Arguments Dev()
        {
            // INPUT and OUTPUT DIRECTORIES
            // 2010 Oct 13th
            // string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014May06-100720 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            // string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct13_SpectralIndices";

            // 2010 Oct 14th
            // string ipFileName = "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct14_SpectralIndices";

            // 2010 Oct 15th
            // string ipFileName = "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct15_SpectralIndices";

            // 2010 Oct 16th
            // string ipFileName = "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct16_SpectralIndices";

            // 2010 Oct 17th
            // string ipFileName = "0f2720f2-0caa-460a-8410-df24b9318814_101017-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct17_SpectralIndices";

            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.OneSecondIndices";
            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.200msIndicesKIWI-TEST";

            // KOALA RECORDING AT ST BEES
            //string ipdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016\SpectrogramFocalZoom";
            //string opdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016";

            // TEST recordings
            //string ipdir = @"C:\SensorNetworks\Output\Test\Test\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\TestHiResRidge";

            // BAC
            //string ipdir = @"C:\SensorNetworks\Output\BAC\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\BAC\HiResRidge";

            // BIRD50
            string ipdir = @"C:\SensorNetworks\Output\BIRD50\Towsey.Acoustic";
            string opdir = @"C:\SensorNetworks\Output\BIRD50";

            // ECLIPSE FARMSTAY
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Eclipse\EclipseFarmstay.200ms\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImage";

            //BRISTLE BIRD
            //string ipdir = @"C:\SensorNetworks\Output\BristleBird\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImageBristleBird";

            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImage";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramTileZoom\TiledImages";

            // ################ TEST a colour scheme for the high resolution frame spectrograms.
            //var cch = TowseyLibrary.CubeHelix.GetCubeHelix();
            //cch.TestImage(Path.Combine(opdir, "testImageColorHelixScale.png"));
            //var rsp = new TowseyLibrary.CubeHelix("redscale");
            //rsp.TestImage(Path.Combine(opdir, "testImageRedScale1.png"));
            //var csp = new TowseyLibrary.CubeHelix("cyanscale");
            //csp.TestImage(Path.Combine(opdir, "testImageCyanScale1.png"));
            // ################ TEST a colour scheme for the high resolution frame spectrograms.

            var opDir = new DirectoryInfo(opdir);

            //string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramScalingConfig.json";
            //string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramZoomingConfig.yml";
            string config =
                @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramHiResConfig.yml";


            return new Arguments
            {
                // use the default set of index properties in the AnalysisConfig directory.
                SourceDirectory = ipdir.ToDirectoryInfo(),
                Output = opdir.ToDirectoryInfo(),
                SpectrogramTilingConfig = config.ToFileInfo(),

                // draw a focused multi-resolution pyramid of images
                //ZoomAction = Arguments.ZoomActionType.Tile,
                ZoomAction = Arguments.ZoomActionType.Focused,
                FocusMinute = 1,
                //FocusMinute = 61,
            };
        }
    }
}