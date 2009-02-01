using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioStuff
{
	public abstract class BaseClassifier
	{
		public static BaseClassifier Create(BaseClassifierParameters parameters)
		{
			if (parameters is MMTemplate)
				return new MMRecogniser(parameters as MMTemplate);
			throw new ArgumentException("Unrecognised classifier type.");
		}

		public abstract BaseResult Analyse(AudioRecording recording);
		public abstract BaseResult Analyse(AudioRecording recording, out BaseSonogram sonogram);
	}
}