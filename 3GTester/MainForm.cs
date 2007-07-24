using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Qut3GTester
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }              

        private void cmdSelectFolder_Click(object sender, EventArgs e)
        {
            ofd1.ShowDialog();
            if (!String.IsNullOrEmpty(ofd1.FileName))
                txtFileName.Text = ofd1.FileName;
        }

        double currentSpeed = 0.00;

        private void button1_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(txtFileName.Text);
            double fileSize = fi.Length;
            DateTime start,finished;
            start = DateTime.Now; 
            listBox1.Items.Clear();            
            listBox1.Items.Add(String.Format("File size : {0:0.00} Kb", fileSize /1024));
            listBox1.Items.Add(String.Format("File size : {0:0.00} Mb", fileSize / 1048576));
            listBox1.Items.Add(String.Format("Operation Started : {0}",DateTime.Now.ToShortTimeString()));
            QutSensor.Service service = new QutSensor.Service();
            FileStream fs = new FileStream(txtFileName.Text, FileMode.Open);
            byte[] oFileByte = new byte[fs.Length];
            fs.Read(oFileByte, 0, (int)fs.Length);

            Application.DoEvents();
            String str = String.Empty;
            try
            {
                service.UploadFile(oFileByte, ref str);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.ToString());
            }
            
            finished = DateTime.Now;
            TimeSpan ts = finished.Subtract(start);
            listBox1.Items.Add(String.Format("Operation Finished : {0}",DateTime.Now.ToShortTimeString()));
            listBox1.Items.Add(String.Format("Time elapsed : {0}",ts.ToString()));
            if (ts.TotalSeconds <= 0)
                ts = new TimeSpan(0, 0, 1);

            
            currentSpeed = fileSize / 1024 / ts.TotalSeconds;            
            listBox1.Items.Add(String.Format("Speed = {0:0.00} kBps", currentSpeed));

            using (StreamWriter writer = new StreamWriter("SpeedTest.txt", true))
            {
                writer.Write(DateTime.Now.ToString("g"));
                writer.Write(": ");
                writer.WriteLine("Speed recorded {0:0.00} kbps @ {1}", currentSpeed, txtLocation.Text);
            }

            listBox1.Items.Add("Results saved.");
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            
        }
    }
}