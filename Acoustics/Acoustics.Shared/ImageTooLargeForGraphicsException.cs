// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageTooLargeForGraphicsException.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// The image too large for graphics exception. Used with GraphicsSegmented to indicate Error resulting from limitations of Drawing.Graphics.
    /// </summary>
    [Serializable]
    public class ImageTooLargeForGraphicsException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTooLargeForGraphicsException"/> class. 
        /// Initializer for spectrogram generation.
        /// </summary>
        /// <param name="widthAtException">
        /// The width at exception.
        /// </param>
        /// <param name="heightAtException">
        /// The height at exception.
        /// </param>
        /// <param name="audioLengthAtException">audio Length At Exception.</param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="graphicsException">
        /// The graphics exception.
        /// </param>
        public ImageTooLargeForGraphicsException(
            int? widthAtException, int? heightAtException, int? audioLengthAtException, string message, ArgumentException graphicsException)
            : base(message, graphicsException)
        {
            this.WidthAtException = widthAtException;
            this.HeightAtException = heightAtException;
            this.AudioLengthAtException = audioLengthAtException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTooLargeForGraphicsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="graphicsException">
        /// The graphics exception.
        /// </param>
        public ImageTooLargeForGraphicsException(string message, ArgumentException graphicsException)
            : base(message, graphicsException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTooLargeForGraphicsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public ImageTooLargeForGraphicsException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTooLargeForGraphicsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public ImageTooLargeForGraphicsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTooLargeForGraphicsException"/> class.
        /// </summary>
        public ImageTooLargeForGraphicsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTooLargeForGraphicsException"/> class.
        /// </summary>
        /// <param name="info">
        /// Serialization Info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected ImageTooLargeForGraphicsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets WidthAtException.
        /// </summary>
        public int? WidthAtException { get; private set; }

        /// <summary>
        /// Gets HeightAtException.
        /// </summary>
        public int? HeightAtException { get; private set; }

        /// <summary>
        /// Gets AudioLengthAtException.
        /// </summary>
        public int? AudioLengthAtException { get; private set; }

        /// <summary>
        /// String representation of ImageTooLargeForGraphicsException.
        /// </summary>
        /// <returns>
        /// String representation.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder("ImageTooLargeForGraphicsException: ");
            if (WidthAtException.HasValue) sb.Append(" Image Width: " + WidthAtException.Value);
            if (HeightAtException.HasValue) sb.Append(" Image Height: " + HeightAtException.Value);
            if (AudioLengthAtException.HasValue) sb.Append(" Audio Length: " + AudioLengthAtException.Value + "ms");
            if (!string.IsNullOrEmpty(this.Message)) sb.Append(" " + this.Message + " ");
            return sb + Environment.NewLine + base.ToString();
        }

        /// <summary>
        /// Get Object Data (override from ISerializable).
        /// </summary>
        /// <param name="info">
        /// Serialization Info.
        /// </param>
        /// <param name="context">
        /// Streaming Context.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("WidthAtException", WidthAtException);
            info.AddValue("HeightAtException", HeightAtException);
            info.AddValue("AudioLengthAtException", AudioLengthAtException);
        }
    }
}