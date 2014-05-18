namespace AudioBrowser
{
    using System;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;

    public class TextBoxStreamWriter : TextWriter
    {
        readonly TextBox output;

        public TextBoxStreamWriter(TextBox output)
        {
            this.output = output;
        }

        /// <summary>
        /// Override the Write(char value) method, which is called when character data is written to the stream.
        /// </summary>
        /// <param name="value"></param>
        public override void Write(char value)
        {
            base.Write(value);

            // When character data is written, append it to the text box.
            Action append = () => this.output.AppendText(value.ToString());
            if (this.output.InvokeRequired)
            {
                this.output.BeginInvoke(append);
            }
            else
            {
                append();
            }

        }

        /// <summary>
        /// Override the abstract “Encoding” property. 
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
