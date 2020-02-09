// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveBehavior.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SaveBehavior type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;

    /// <summary>
    /// Determines if data should be saved per segment
    /// </summary>
    public enum SaveBehavior
    {
        /// <summary>
        /// Never save the associated resource. This is a synonym for <c>Never</c> and is provided for compatibility.
        /// </summary>
        False = 0,

        /// <summary>
        /// Never save the associated resource.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Always save the associated resource. This is a synonym for <c>Always</c> and is  provided for compatibility.
        /// </summary>
        True = 1,

        /// <summary>
        /// Always save the associated resource.
        /// </summary>
        Always = 1,

        /// <summary>
        /// Only save the associated resource when events have been found in the segment
        /// </summary>
        WhenEventsDetected = 2
    }

    public static class SaveBehaviorExtensions
    {
        public static bool ShouldSave(this SaveBehavior saveBehavior, int eventCount = 0)
        {
            bool result;
            switch (saveBehavior)
            {
                case SaveBehavior.Never:
                    result = false;
                    break;
                case SaveBehavior.Always:
                    result = true;
                    break;
                case SaveBehavior.WhenEventsDetected:
                    result = eventCount > 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveBehavior), saveBehavior, null);
            }

            return result;
        }
    }
}
