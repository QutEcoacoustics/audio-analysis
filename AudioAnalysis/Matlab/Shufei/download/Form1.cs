using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using DBSCAN;
using System.Data.OleDb;
using System.Drawing.Drawing2D;

namespace DBSCAN
{
	/// <summary>
	/// Form1 的摘要说明。
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TrackBar trackBar_EPS;
		private System.Windows.Forms.TrackBar trackBar1;


		public DBSCAN ds;

		public Form1()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();
		
			

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
		}

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.trackBar_EPS = new System.Windows.Forms.TrackBar();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_EPS)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(643, 240);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(90, 25);
			this.button1.TabIndex = 0;
			this.button1.Text = "Run";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(624, 136);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(120, 21);
			this.textBox2.TabIndex = 3;
			this.textBox2.Text = "3";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(576, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(29, 18);
			this.label1.TabIndex = 3;
			this.label1.Text = "eps";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(566, 144);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 17);
			this.label2.TabIndex = 4;
			this.label2.Text = "MinPts";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(624, 34);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(120, 21);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "15";
			// 
			// trackBar_EPS
			// 
			this.trackBar_EPS.LargeChange = 10;
			this.trackBar_EPS.Location = new System.Drawing.Point(616, 72);
			this.trackBar_EPS.Maximum = 50;
			this.trackBar_EPS.Name = "trackBar_EPS";
			this.trackBar_EPS.Size = new System.Drawing.Size(120, 45);
			this.trackBar_EPS.TabIndex = 2;
			this.trackBar_EPS.TickFrequency = 2;
			this.trackBar_EPS.Value = 15;
			this.trackBar_EPS.Scroll += new System.EventHandler(this.trackBar_EPS_Scroll);
			// 
			// trackBar1
			// 
			this.trackBar1.Location = new System.Drawing.Point(624, 184);
			this.trackBar1.Maximum = 7;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new System.Drawing.Size(104, 45);
			this.trackBar1.TabIndex = 4;
			this.trackBar1.Value = 3;
			this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(759, 580);
			this.Controls.Add(this.trackBar1);
			this.Controls.Add(this.trackBar_EPS);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "DBSCAN Algorithms Demo";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackBar_EPS)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			OleDbConnection cn = new OleDbConnection();
			cn.ConnectionString = "Provider=Microsoft.JET.OLEDB.4.0; data source=" + 
				Environment.CurrentDirectory + @"\..\..\sxdb.mdb";
			cn.Open();
			OleDbCommand cmd = new OleDbCommand("Select * From Table1",cn);
			OleDbDataReader dr = cmd.ExecuteReader();

			ds = new DBSCAN();
			while(dr.Read())
			{
				ds.AddDataPoint(new DataPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"])));
			}
			ds.PrepareDBSCAN_Table();
			dr.Close();
			cn.Close();
			button1_Click(this, null);		
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics dc= e.Graphics;
			Pen pen=new Pen(Color.Black,3);

			foreach(DataPoint dp in ds.DataPoints)
			{
				pen.Color=this.GetColor(dp.class_id);
				dc.DrawEllipse(pen,(float)dp.d1+200,(float)dp.d2+200,2,2);
			}
		}

		public System.Drawing.Color GetColor(int index)
		{
			Color[] xColor={Color.Black, Color.Red , Color.Pink, Color.Green, Color.Gold, Color.Purple, Color.Blue, Color.Orange,Color.Plum };
			return xColor[index % 9];
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			int a=Convert.ToInt32(this.textBox1.Text);
			int b=Convert.ToInt32(this.textBox2.Text);
			ds.BuildCorePoint(a,b);
			ds.DBSCAN_Cluster();
			this.Invalidate();
		}

		private void trackBar_EPS_Scroll(object sender, System.EventArgs e)
		{
			this.textBox1.Text = trackBar_EPS.Value.ToString();
			button1_Click(this, null);
		}

		private void trackBar1_Scroll(object sender, System.EventArgs e)
		{
			this.textBox2.Text = trackBar1.Value.ToString();
			button1_Click(this, null);		
		}
	}
}
