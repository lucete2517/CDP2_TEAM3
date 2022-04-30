using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenCvSharp;

namespace TestApp
{
    public partial class Form1 : Form
    {

        OpenCvSharp.VideoCapture cap = new VideoCapture(@"c:\Temp\Video.mp4");
        Mat frame = new Mat();



        public Form1()
        {
            InitializeComponent();
        }

        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            picSnapShot.Image = picPreview.Image; 
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (btnPlay.Text == "Play")//Stop
            {
                btnPlay.Text = "Stop";
            }
            else
            {
                btnPlay.Text = "Play";
            }

            timer1.Enabled = !timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           cap.Read(frame);

           picPreview.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
        }
    }
}
