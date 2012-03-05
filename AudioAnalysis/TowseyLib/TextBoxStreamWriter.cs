using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace TowseyLib
{
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        } //TextBoxStreamWriter()

        /// <summary>
        /// Override the Write(char value) method, which is called when character data is written to the stream.
        /// </summary>
        /// <param name="value"></param>
        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString()); // When character data is written, append it to the text box.
        }

        /// <summary>
        /// Override the abstract “Encoding” property. 
        /// </summary>
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }


    } //class TextBoxStreamWriter
}
