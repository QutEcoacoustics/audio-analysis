using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;

namespace AudioAnalysis
{
	public class Recogniser
	{

        #region Properties
        public Template_CC Template { get; private set; }
        public BaseModel Model { get; private set; }
        public WavReader Wav { get; private set; }
        #endregion


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="template"></param>
		internal Recogniser(Template_CC template)
		{
            Log.WriteIfVerbose("\n\nINITIALISING RECOGNISER:");
            Log.WriteIfVerbose("\tTemplate: "+template.CallName);

			Template = template;
            Model = template.Model; //the Model was initialised at template init.
            Log.WriteIfVerbose("\tModel: " + Model.modelName);
		}



		public BaseResult Analyse(AudioRecording recording)
		{
            Wav = recording.GetWavData();

            ////STEP THREE: Verify fv extraction by observing output from acoustic model.
            var avSono = new AcousticVectorsSonogram(Template.SonogramConfig, Wav);
            Template.GenerateSymbolSequence( avSono);
            double frameOffset = Template.SonogramConfig.GetFrameOffset();
            BaseResult result = Template.GetBlankResultCard();

            ModelType type = Template.Model.ModelType;
            BaseModel.opFolder = Path.GetDirectoryName(Template.DataPath); //this only required when doing unit testing
            if (type == ModelType.UNDEFINED)
            {
                Log.WriteLine("Recogniser.Analysis(): WARNING: The Template MODEL is UNDEFINED.");
                Log.WriteLine("CANNOT PROCEED WITH ANALYSIS");
                return result;
            }
            else
                if (type == ModelType.MM_ERGODIC)
                {
                    Model.ScanSymbolSequenceWithModel(result as Result_MMErgodic, frameOffset);
                    return result;
                }
                else
                    if (type == ModelType.ONE_PERIODIC_SYLLABLE)
                    {
                        Model.ScanSymbolSequenceWithModel(result as Result_MMErgodic, frameOffset);
                        return result;
                    }
                    else
                        if (type == ModelType.MM_TWO_STATE_PERIODIC)
                        {
                            Model.ScanSymbolSequenceWithModel(result as Result_MMErgodic, frameOffset);
                            return result;
                        }
                        else
                {
                    //throw new Exception("Terminating analysis");
                    return null;
                }
        }
		
    } // end class MMRecogniser 

}