using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yolov3_ex
{
    public partial class Form1 : Form
    {
        Net net;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form_FormClosing);
            yolov3_ready();
        }

        private void yolov3_ready()
        {
            string cfg_path = Application.StartupPath + @"\running\yolov3_testing.cfg";
            string model_path = Application.StartupPath + @"\running\yolov3_training_last.weights";
            net = CvDnn.ReadNetFromDarknet(cfg_path, model_path);
            net.SetPreferableBackend((Backend)3);
            //무엇을 기반으로 0 사용x 1 halide 2 inference range 3 onpencv

            net.SetPreferableTarget((Target)2);
            //0 cpu 1 opencl 2 opencl_fp16 3 myraid 4 fpga
        }

        private void yolov3_start()
        {
            Mat blob = CvDnn.BlobFromImage(new Mat(filename), 1.0/255, new OpenCvSharp.Size(416,416), new Scalar(), true, false);
            //mat => 입력 영상
            //1.0/255 => 픽셀 값에 곱할 값. 딥러닝 학습진행시 0-255 인지 0-1인지 지정
            //0-1로 정규화하여 학습을 진행했으면 1/255를 입력해줘야됨

            net.SetInput(blob);
            //위에서 입력받은 정보들을 계산

            var outName = net.GetUnconnectedOutLayersNames();//출력 레이어 이름 얻기
            var outs = outName.Select(_ => new Mat()).ToArray();//출력 레이어를 위한 패드 생성

            net.Forward(outs, outName);
            GetResult(outs, true, pictureBox1, null, richTextBox1);
        
        }

        private static void GetResult(Mat[] outs, bool nms, PictureBox pictureBox1, Image image, RichTextBox richTextBox1, bool morethreshold = true)
        {
            float threshold = 0.1f;//정확성 0~ 1 경계상자확률?
            float nmsThreshold = 0.1f;//상자 곂치는 거 필터작용

            try
            {
                if (outs != null)
                {

                    List<int> classed = new List<int>();//해당 객체가 무엇인지
                    List<float> confidences = new List<float>();//전체 정확성
                    List<float> probilities = new List<float>();//정확성
                    List<Rect2d> boxes = new List<Rect2d>();//박스 그리기 
                    //IEnumerable 은 사용자 정의에 한한 것을 반복하게 하는데 쓰인다. 즉 외부 Mat 같은것의 배열을 반복하는데 좋음.
                    int w = 0;
                    int h = 0;

                    if (image != null)
                    {
                        pictureBox1.Image = image;
                        w = image.Width;//이미지의 가로 길이 담기
                        h = image.Height;//이미지의 세로 길이 담기
                    }
                    else
                    {
                        w = pictureBox1.Image.Width;//이미지의 가로 길이 담기
                        h = pictureBox1.Image.Height;//이미지의 세로 길이 담기
                    }

                    const int prefix = 5; //134번 줄에 설명있음
                                          //const 2트째지만 고정 값이다. 변경 불가
                                          // int classnumber = -1;

                    richTextBox1.Text = "";
                    foreach (Mat mat in outs)
                    {

                        // classnumber++;
                        // richTextBox1.AppendText(classnumber.ToString());
                        //foreach는 for 문으로  output의 배열 길이 만큼 돌리면서 해당 카운트의 Mat을 Mat count 넣음.
                        //예를 들어 첫번째 돌때는 Mat mat = output[0], 두번째는  Mat mat = output[1] 이런식으로 돌아간다.
                        for (int i = 0; i < mat.Rows; i++)
                        {
                            float confidence = mat.At<float>(i, 4);//해당 객체의 정확성 담기
                            if (confidence > threshold)
                            { //confidence의 값이 threshold(0.5)보다 높으면
                                Cv2.MinMaxLoc(mat.Row(i).ColRange(prefix, mat.Cols), out _, out OpenCvSharp.Point max);
                                int classes = max.X;//발견한 클래스의 넘버 값.
                                float probability = mat.At<float>(i, classes + prefix);//5번째 이후부터 검사한 클래스의 확률이기 때문에 5를 더한다.  
                                if (probability > threshold)
                                {
                                    //mat 에 배열에 따라 들어가있는 값이 다름. 
                                    //0은 이미지의 중앙 x축 위치 정보값
                                    //1 은 이미지의 중앙 y축 위치 정보값
                                    //2 는 객체 상자의 가로 길이(w)
                                    //3 은 객체 상자의 세로 길이(h)
                                    //4 는 객체의 정확성 0~ 1사이로 뜬다.
                                    //5 이상 부터는 여러 클래스 박스 확률이 담겼다,

                                    float centerX = mat.At<float>(i, 0) * w;//경계상자중심x좌표
                                    float centerY = mat.At<float>(i, 1) * h;//중심y좌표
                                    float width = mat.At<float>(i, 2) * w;//경계상자너비
                                    float height = mat.At<float>(i, 3) * h;//경계상자높이
                                    //richTextBox1.AppendText(i.ToString() + " "+ classes.ToString() + Label[classes] +" " +mat.At<float>(i, classes+1 + prefix).ToString() + "%끝\n");
                                    //그리기 작업문
                                    //image 와 도구상자의 중앙 위치, 가로 세로 길이 보내기
                                    classed.Add(classes);
                                    confidences.Add(confidence);
                                    probilities.Add(probability);
                                    boxes.Add(new Rect2d(centerX, centerY, width, height));

                                }
                            }


                        }
                    }
                    if (boxes.Count == 0)
                    {

                        if (richTextBox1.Text != "관측되지 않음\n") richTextBox1.Text = "관측되지 않음\n";
                    }
                    else if (nms)
                    {
                        CvDnn.NMSBoxes(boxes, confidences, threshold, nmsThreshold, out int[] indices);
                        foreach (int i in indices)
                        {
                            Rect2d box = boxes[i];
                            richTextBox1.AppendText($"{Label[classed[i]]} {probilities[i] * 100:0.0}%\n");
                            richTextBox1.AppendText("x,y = " + ((int)box.X).ToString() + "/" + ((int)box.Y).ToString() + "\n" + "w,h = " + ((int)box.Width).ToString() + "/" + ((int)box.Height).ToString() + "\n\n");
                            Draw(classed[i], probilities[i], (float)box.X, (float)box.Y, (float)box.Width, (float)box.Height, pictureBox1);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < boxes.Count; i++)
                        {
                            Rect2d box = boxes[i];
                            richTextBox1.AppendText($"{ Label[classed[i]]} {probilities[i] * 100:0.0}% \n"); ;
                            richTextBox1.AppendText(((int)box.X).ToString() + "/" + ((int)box.Y).ToString() + "\n" + ((int)box.Width).ToString() + "/" + ((int)box.Height).ToString() + "\n\n");
                            Draw(classed[i], probilities[i], (float)box.X, (float)box.Y, (float)box.Width, (float)box.Height, pictureBox1);
                        }
                    }
                }

            }
            catch
            {
                richTextBox1.AppendText("GetResult 문에서 실패");
            }
        }

        private static void Draw(int classes, float probability, float centerX, float centerY, float width, float height, PictureBox pictureBox1)
        {
            string text = $"{Label[classes]} ({probability * 100:0.0}%)";
            //https://bananamandoo.tistory.com/30 여기 사이트 참고 위에건 소수점 한자리 숫자만 나타내겠다는 뜻
            float x = centerX - width / 2;//중앙점에서 가로의 반을 빼줘야 그리는 점의 시작 포인트가 됨
            float y = centerY - height / 2;//중앙점에서 세로의 반을 빼줘야 그리는 점의 시작 포인트가 됨

            using (Graphics thumbnailGraphic = Graphics.FromImage(pictureBox1.Image))//그리는 도화지를 pictureBox1.Image로 설정
            {
                thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Define Text Options
                Font drawFont = new Font("Arial", 12, FontStyle.Bold);//폰트는 Arial 에 크기는 12, 굵기는 굵게
                SizeF size = thumbnailGraphic.MeasureString(text, drawFont);
                SolidBrush fontBrush = new SolidBrush(Color.Black);
                System.Drawing.Point atPoint = new System.Drawing.Point((int)x, (int)y - (int)size.Height - 1); ;
                // Define BoundingBox options
                Pen pen = new Pen(color[classes], 3.2f);//color[classes] 151 번째 줄을 보면 해당 객체마다 보여주는 색을 정함 
                SolidBrush colorBrush = new SolidBrush(color[classes]);//color[classes] 151 번째 줄을 보면 해당 객체마다 보여주는 색을 정함 

                thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
                //사각형 그리기
                thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);
                //해당 객체 글자 그리기

                // Draw bounding box on image
                thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
            }
        }

        private static string[] Label = new string[]
        {
            "apple"
        };

        private static Color[] color = new Color[]
        {
            Color.Green,
            Color.Orange
        };

        string filename = "";
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
                pictureBox1.Image = new Bitmap(filename);
                yolov3_start();
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
