using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Size = OpenCvSharp.Size;

namespace imageblack
{
    public partial class Form1 : Form
    {
        Image image;
        Bitmap gBitmap;
        string image_path = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void search_Click(object sender, EventArgs e)//파일선택
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    image_path = ofd.FileName;
                    pictureBox1.Image = Image.FromFile(image_path);

                    image = pictureBox1.Image;
                    gBitmap = new Bitmap(image);
                }
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.Message);
            }
        }

        private void changeall_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd1 = new FolderBrowserDialog();//jpg, txt 폴더 위치
            FolderBrowserDialog fbd2 = new FolderBrowserDialog();//저장될 폴더 위치

            if(fbd1.ShowDialog() == DialogResult.OK)
            {
                if (fbd2.ShowDialog() == DialogResult.OK)
                {
                    string[] imgfiles = Directory.GetFiles(fbd1.SelectedPath, "*.jpg");//jpg파일
                    string[] txtfiles = Directory.GetFiles(fbd1.SelectedPath, "*.txt");//txt파일

                    int n = 0;//n번째 파일

                    while (n < imgfiles.GetLength(0))
                    {
                        image = Image.FromFile(imgfiles[n]);
                        gBitmap = new Bitmap(image);



                        //흑백
                        /*int brightness;
                        Color color, gray;

                        for (int x = 0; x < image.Width; x++)
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                color = gBitmap.GetPixel(x, y);
                                brightness = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                                gray = Color.FromArgb(brightness, brightness, brightness);
                                gBitmap.SetPixel(x, y, gray);
                            }
                        }*/



                        //가우시안
                        /*Mat src = Cv2.ImRead(imgfiles[n]);
                        Mat dst = new Mat();

                        //Cv2.BilateralFilter(src, dst, 5, 250, 10);
                        Cv2.GaussianBlur(src, dst, new Size(5, 5), 3, 3); //가우시안
                        //Cv2.MedianBlur(src, dst, 3);

                        gBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);//dst[] 0.bilateral  1.gaussian  2.median*/



                        //threshold
                        Mat tmp = new Mat(imgfiles[n], ImreadModes.AnyColor);//유색
                        Mat src = new Mat(imgfiles[n], ImreadModes.Grayscale);//흑백

                        Mat dst = new Mat();

                        Cv2.Threshold(src, dst, 140, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);//127: 가중치 조절가능

                        gBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                        //가우시안 + threshold
                        /*Mat src = Cv2.ImRead(imgfiles[n]);
                        Mat dst = new Mat();

                        //Cv2.BilateralFilter(src, dst, 5, 250, 10);
                        Cv2.GaussianBlur(src, dst, new Size(5, 5), 3, 3); //가우시안
                        //Cv2.MedianBlur(src, dst, 3);

                        Mat dst2 = new Mat();

                        Cv2.Threshold(dst, dst2, 140, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);//127: 가중치 조절가능

                        gBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst2);*/

                        string[] pathsplit = imgfiles[n].Split('\\');
                        string name = pathsplit[pathsplit.GetLength(0) - 1];//jpgname

                        string[] txtpathsplit = txtfiles[n++].Split('\\');
                        string txtname = txtpathsplit[txtpathsplit.GetLength(0) - 1];//txtname

                        image = gBitmap;
                        image.Save(fbd2.SelectedPath + "\\gausthres" + name);//저장폴더경로 + 파일 이름
                        File.Copy(fbd1.SelectedPath + "\\" + txtname, fbd2.SelectedPath + "\\gausthres" + txtname);//텍스트파일 복붙
                    }
                }
            }
        }

        private void CB_Click(object sender, EventArgs e)//흑백
        {
            int brightness;
            Color color, gray;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    color = gBitmap.GetPixel(x, y);
                    brightness = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                    gray = Color.FromArgb(brightness, brightness, brightness);
                    gBitmap.SetPixel(x, y, gray);
                    pictureBox2.Image = gBitmap;
                }
            }
            /*Color co;
            int Average; //RGB값의 평균

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    co = gBitmap.GetPixel(x, y);
                    Average = (co.R + co.G + co.B) / 3;

                    // R.G.B 를 모두 같은 값으로 지정하면 회색임.
                    co = Color.FromArgb(Average, Average, Average);

                    gBitmap.SetPixel(x, y, co);     // 해당 좌표 픽셀의 컬러값을 변경
                }
            }*/
        }

        private void gaussian_Click(object sender, EventArgs e)//가우시안
        {
            Mat src = Cv2.ImRead(image_path); 
            Mat[] dst = new Mat[3] { new Mat(), new Mat(), new Mat() }; 

            Cv2.BilateralFilter(src, dst[0], 5, 250, 10); 
            Cv2.GaussianBlur(src, dst[1], new Size(5, 5), 3, 3); //가우시안
            Cv2.MedianBlur(src, dst[2], 3);

            gBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst[1]);//dst[] 1.bilateral  2.gaussian  3.median
            pictureBox2.Image = gBitmap;

            /*Cv2.ImShow("src", src); 
            Cv2.ImShow("Bilateral", dst[0]); 
            Cv2.ImShow("Gaussian", dst[1]); 
            Cv2.ImShow("Median", dst[2]); 

            Cv2.WaitKey(0); 
            Cv2.DestroyAllWindows();*/

        }

        private void threshold_Click(object sender, EventArgs e)
        {
            Mat tmp = new Mat(image_path, ImreadModes.AnyColor);//유색
            Mat src = new Mat(image_path, ImreadModes.Grayscale);//흑백

            Mat dst = new Mat();

            Cv2.Threshold(tmp, dst, 127, 255, ThresholdTypes.Binary);//127: 가중치 조절가능

            gBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            pictureBox2.Image = gBitmap;
        }
    }
}
