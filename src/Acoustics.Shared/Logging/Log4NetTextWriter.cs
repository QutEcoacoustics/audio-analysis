// <copyright file="Log4NetTextWriter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.Logging
{
    using System;
    using System.IO;
    using System.Text;
    using log4net;

    public class Log4NetTextWriter : TextWriter
    {
        private readonly TextWriter baseStream;
        private readonly StringBuilder stringBuilder;
        private readonly Action<string> logCall;

        public Log4NetTextWriter(TextWriter baseStream, ILog log = null, Mode mode = Mode.Out)
        {
            this.baseStream = baseStream;
            log = log ?? NoConsole.Log;
            if (mode == Mode.Error)
            {
                this.logCall = log.Error;
            }
            else
            {
                this.logCall = log.Info;
            }

            this.stringBuilder = new StringBuilder();
        }

        public enum Mode
        {
            Out,
            Error,
        }

        // Not used by us, so don't set it
        public override Encoding Encoding => throw new NotImplementedException();

        public override void Write(char value)
        {
            switch (value)
            {
                case '\r':
                    break;
                case '\n':
                    this.logCall(this.stringBuilder.ToString());
                    this.stringBuilder.Clear();
                    break;
                default:
                    this.stringBuilder.Append(value);
                    break;
            }

            this.baseStream.Write(value);
        }

        public override void Write(string value)
        {
            switch (value)
            {
                case string _ when value.EndsWith(Environment.NewLine):
                    this.logCall(this.stringBuilder.ToString());
                    this.stringBuilder.Clear();
                    return;
                default:
                    this.stringBuilder.Append(value);
                    break;
            }

            this.baseStream.Write(value);
        }
    }
}
