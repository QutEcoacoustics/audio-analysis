// <copyright file="PulseTrain.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    /// <summary>
    /// This class was an attempt to detect pulse trains as an alternative to using the Oscillation recognition methods.
    /// It did not work effectively so discontinued the idea and have commented out the three methods.
    /// </summary>
    public static class PulseTrain
    {
        /*
        /// <summary>
        /// This method creates a template to recognise two pulses that are possibly part of a pulse train.
        /// The template is designed to detect pulse trains of at least 2 pulses!
        /// The template is bounded either end by silence and then a pulse. i.e.    .:|:. ... .:|:. where .=zero or a negative residual value, := 0.5 and |= 1.0.
        /// Any number of residual values may separate the pulses at either end. In this method, templates are created with 6 non-zero values and the remainder are negative.
        /// The sum of the positive values = 4.0.
        /// The sum of the values in the template should = zero.
        /// Designed this way, the minimum pulse length is about 4 or 5 and the minimum template length is about 10;
        /// </summary>
        /// <param name="pulseLength">length or number of frames between two pulses.</param>
        /// <returns>the template.</returns>
        public static double[] GetPulseTrainTemplate(int pulseLength)
        {
            int templateLength = pulseLength + 5;
            double residual = 4 / (double)(templateLength - 6);

            var template = new double[templateLength];
            template[0] = -residual;
            template[1] = 0.5;
            template[2] = 1.0;
            template[3] = 0.5;
            for (int i = 4; i < templateLength - 4; i++)
            {
                template[i] = -residual;
            }

            template[templateLength - 4] = 0.5;
            template[templateLength - 3] = 1.0;
            template[templateLength - 2] = 0.5;
            template[templateLength - 1] = -residual;
            template = DataTools.normalise2UnitLength(template);
            return template;
        }

        /// <summary>
        /// Only three pulses included in the single template output by this method.
        /// Will generalise if it seems worthwhile.
        /// </summary>
        public static double[] GetPulseTrainTemplate(int pulseLength, int pulseCount)
        {
            int templateLength = (pulseLength * pulseCount) + 5;
            var template = new double[templateLength];
            int templateHalfLength = templateLength / 2;

            for (int i = 0; i < templateLength; i++)
            {
                template[i] = -1.0;
            }

            template[1] = 0.3;
            template[2] = 1.0;
            template[3] = 0.3;

            template[templateHalfLength - 1] = 0.3;
            template[templateHalfLength] = 1.0;
            template[templateHalfLength + 1] = 0.3;

            template[templateLength - 4] = 0.3;
            template[templateLength - 3] = 1.0;
            template[templateLength - 2] = 0.3;
            //template = DataTools.normalise2UnitLength(template);
            return template;
        }

        /// <summary>
        /// returns the length of a pulse interval in frames given pulses and frame rates in seconds.
        /// </summary>
        /// <param name="pulsesPerSecond">number of pulses per second.</param>
        /// <param name="framesPerSecond">frames per second - i.e. assuming the application is applied to a sequence of spectral frames.</param>
        /// <returns>the template.</returns>
        public static double[] GetPulseTrainTemplate(double pulsesPerSecond, double framesPerSecond)
        {
            int frameCount = (int)Math.Round(framesPerSecond / pulsesPerSecond);
            return GetPulseTrainTemplate(frameCount);
        }

        public static double[] GetPulseTrainScore(double[] signal, double pulsesPerSecond, double framesPerSecond, double thresholdValue)
        {
            int pulseCount = 2;
            int frameCount = (int)Math.Round(framesPerSecond / pulsesPerSecond);
            var templates = new List<double[]>
            {
                GetPulseTrainTemplate(frameCount, pulseCount),
                GetPulseTrainTemplate(frameCount - 1, pulseCount),
                GetPulseTrainTemplate(frameCount + 1, pulseCount),
            };
            int signalLength = signal.Length;

            var scores = new double[signalLength];

            for (int i = 2; i < signalLength - templates[2].Length; i++)
            {
                // skip if value is below threshold
                if (signal[i] < thresholdValue)
                {
                    continue;
                }

                // skip if value is not maximum
                if (signal[i] < signal[i - 1] || signal[i] < signal[i + 1])
                {
                    continue;
                }

                // get Cosine similarity for each of three templates.
                var templateScores = new double[3];

                // get the local nh of signal for template 0 and get score
                var nh = DataTools.Subarray(signal, i, templates[0].Length);
                nh = DataTools.normalise2UnitLength(nh);
                templateScores[0] = DataTools.DotProduct(nh, templates[0]);

                // get the local nh of signal for template 1
                nh = DataTools.Subarray(signal, i, templates[1].Length);
                nh = DataTools.normalise2UnitLength(nh);
                templateScores[1] = DataTools.DotProduct(nh, templates[1]);

                // get the local nh of signal for template 2
                nh = DataTools.Subarray(signal, i, templates[2].Length);
                nh = DataTools.normalise2UnitLength(nh);
                templateScores[2] = DataTools.DotProduct(nh, templates[2]);

                double maxScore = templateScores.Max();
                if (maxScore > 0.0)
                {
                    for (int j = 0; j < templates[0].Length - 1; j++)
                    {
                        if (maxScore > scores[i + j])
                        {
                            scores[i + j] = maxScore;
                        }
                    }
                }
            }

            return scores;
        }
        */
    }
}