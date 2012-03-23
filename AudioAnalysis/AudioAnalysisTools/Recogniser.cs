using System.IO;
using TowseyLib;

namespace AudioAnalysisTools
{
    using Acoustics.Tools.Wav;

    public class Recogniser
	{

        #region Properties
        //public Template_CC Template { get; private set; } // ORIGINAL TEMPLATE DECLARATION
        public BaseTemplate Template { get; private set; }
        public BaseModel Model { get; private set; }
        public WavReader Wav { get; private set; }
        #endregion


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="template"></param>
		//public Recogniser(Template_CC template)  //ORIGINAl CONSTRUCTOR
		public Recogniser(BaseTemplate template)
		{
            Log.WriteIfVerbose("\n\nINITIALISING RECOGNISER:");
            Log.WriteIfVerbose("\tTemplate: "+template.config.GetString(template.key_CALL_NAME));

			Template = template;
            Model    = template.LanguageModel; //the Model was initialised at template init.
            Log.WriteIfVerbose("\tModel: " + Model.modelName);
		}



		public BaseResult Analyse(AudioRecording recording)
		{
            Wav = recording.GetWavReader();
            //Template.GenerateSymbolSequence(Wav);
            //var wav = ar.GetWavReader();
            var avSonogram = new TriAvSonogram(Template.sonogramConfig, Wav);
            Template.AcousticModel.GenerateSymbolSequence(avSonogram, Template);
            Template.AcousticModel.FillGapsInSymbolSequence();

            double frameOffset = Template.sonogramConfig.GetFrameOffset();
            BaseResult result = Template.GetBlankResultCard();

            LanguageModelType type = Template.LanguageModel.ModelType;
            BaseModel.opFolder = Path.GetDirectoryName(Template.DataPath); //this only required when doing unit testing
            if (type == LanguageModelType.UNDEFINED)
            {
                Log.WriteLine("Recogniser.Analysis(): WARNING: The Template MODEL is UNDEFINED.");
                Log.WriteLine("CANNOT PROCEED WITH ANALYSIS");
                return result;
            }
            else
                if (type == LanguageModelType.MM_ERGODIC)
                {
                    Model.ScanSymbolSequenceWithModel(result as Result_MMErgodic, frameOffset);
                    return result;
                }
                else
                    if (type == LanguageModelType.ONE_PERIODIC_SYLLABLE)
                    {
                        Model.ScanSymbolSequenceWithModel(result as Result_1PS, frameOffset);
                        return result as Result_1PS;
                    }
                    else
                        if (type == LanguageModelType.MM_TWO_STATE_PERIODIC) //NOT YET IMPLEMENTED
                        {
                            Model.ScanSymbolSequenceWithModel(result as BaseResult, frameOffset);
                            return result as BaseResult;
                        }
                        else
                {
                    //throw new Exception("Terminating analysis");
                    return null;
                }
        }
		
    } // end class MMRecogniser 

}