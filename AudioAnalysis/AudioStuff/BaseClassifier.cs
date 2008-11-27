using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioStuff
{
	public abstract class BaseClassifier
	{
		public abstract BaseResult Analyse(BaseClassifierParameters parameters, AudioRecording recording);
	}
}