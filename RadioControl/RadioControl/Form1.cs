using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace RadioControl
{
    public partial class Form1 : Form
    {
        const int WIFI = 1;
        const int PHONE = 2;
        const int BLUETOOTH = 4;
        
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern bool PrepareRadios();
        
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern bool CleanupRadios();

        [DllImport("AudioPhotoLibrary.dll")]
        public static extern int RadioStates();
        
        [DllImport("AudioPhotoLibrary.dll")]
        public static extern bool EnableRadio(int devices, bool turnOn);

        public Form1()
        {
            InitializeComponent();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            //InitializeAudioRecording();
            PrepareRadios();            
            RadioStates();
            EnableRadio(WIFI+PHONE, true);
            CleanupRadios();
        }
    }
}