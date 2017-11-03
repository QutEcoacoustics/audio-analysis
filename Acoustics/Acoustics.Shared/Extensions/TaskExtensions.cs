// <copyright file="TaskExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace System.Threading.Tasks
{
    using Diagnostics;

    public static class TaskExtensions
    {
        public static Task<T> TimeoutAfter<T>(this Task<T> task, double delaySeconds)
        {
            return TimeoutAfter(task, delaySeconds.Seconds());
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan delay)
        {
            if (Debugger.IsAttached)
            {
                delay = delay.Multiply(10);
            }

            await TaskEx.WhenAny(task, TaskEx.Delay(delay));

            if (!task.IsCompleted)
            {
                throw new TimeoutException("Timeout processing task.");
            }

            return await task;
        }
    }
}