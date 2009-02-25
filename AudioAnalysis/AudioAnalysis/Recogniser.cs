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
        public Template_MFCC Template { get; private set; }
        public BaseModel Model { get; private set; }
        public WavReader Wav { get; private set; }
        #endregion

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="template"></param>
		internal Recogniser(Template_MFCC template)
		{
            Log.WriteIfVerbose("INITIALISING RECOGNISER:");
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
            Results result = new Results(Template);
            //ACCUMULATE OUTPUT SO FAR and put info in Results object 
            result.AcousticMatrix = Template.AcousticModelConfig.AcousticMatrix; //double[,] acousticMatrix
            result.SyllSymbols    = Template.AcousticModelConfig.SyllSymbols;    //string symbolSequence = result.SyllSymbols;
            result.SyllableIDs    = Template.AcousticModelConfig.SyllableIDs;    //int[] integerSequence = result.SyllableIDs;

            ModelType type = Template.Model.ModelType;
            if (type == ModelType.UNDEFINED)
            {
                Log.WriteLine("Recogniser.Analysis(): WARNING: The Recogniser MODEL is UNDERFINED.");
                Log.WriteLine("CANNOT PROCEED WITH ANALYSIS");
                //throw new Exception("Terminating analysis");
                return result;
            }

            BaseModel.opFolder = Path.GetDirectoryName(Template.OPPath); //this only required when doing unit testing
            Model.ScanSymbolSequenceWithModel(result, frameOffset);
            return result;
        }
		
    } // end class MMRecogniser 
}