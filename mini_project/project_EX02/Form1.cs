
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

namespace Yolov3_EX01
{
    public partial class Form1 : Form
    {
        Net net;
        public Form1()
        {
            InitializeComponent();
            // MessageBox.Show(Application.StartupPath);
            yolov3_Ready();
            // 강제종료 (수정 필요)
            // => this.FormClosing += new System.Windows.Forms.FormClosedEventHandler(Form_FormClosing);
        }
        // yolov3 연결
        private void yolov3_Ready()
        {
            string cfg_Path = Application.StartupPath + @"\running\yolov3_testing.cfg";
            string model_Path = Application.StartupPath + @"\running\yolov3_training_last.weights";
            net = CvDnn.ReadNetFromDarknet(cfg_Path, model_Path);

            // 밑의 설정으로 무엇을 기반으로 둘지 정하는 설정값
            // 0, 1, 2 => default, halide, 지식베이스 검색
            // 3 => OPENCV
            net.SetPreferableBackend((Backend)3);

            // DNN_TARGET 어떤 걸로 할지 (cpu, gpu, ....)
            // 0,1 => CPU, OPENCL
            // 2 => OPENCL_FP16
            // 3, 4 => MYRIAD, FPGA(설계 가능 논리소자)
            net.SetPreferableTarget((Target)2);
        }

        // 사진을 받은후 확인.
        private void yolov3_Start()
        {
            // mat => 입력영상
            // 1.0 / 255 => 입력 영상 픽셀 값에 곱할값 (1.0) / 딥러닝 학습을 진행할 때 입력 영상을 0-255 픽셀값을 사용했는지.
            // (0 ~ 1.0)으로 정규화 시켜줌.
            Mat blob = CvDnn.BlobFromImage(new Mat(filename), 1.0/255, new OpenCvSharp.Size(416, 416), new Scalar(), true, false); // yolo size 크기가 416

            // 변환 값을 net에 넣어줌 (입력 정보를 계산).
            net.SetInput(blob);

            // 출력 레이어 이름 얻음.
            var outName = net.GetUnconnectedOutLayersNames();
            // 출력 레이어를 위한 패드 생성.
            var outs = outName.Select(_ => new Mat()).ToArray();

            net.Forward(outs, outName);
            GetResult(outs, true, pictureBox1, pictureBox1.Image, richTextBox1);
        }

        
        // 출력값을 getResult로 받아옴
        // 
        // nms => 가까운 box를 지워줌. 지우기 싫으면 false
        // picturebox1 => box영역 체크의 도화지.
        // image => 이미지 위에 box를 그 위에 덮어 그림.
        // RichTextBox => 정보(x, y좌표, box의 크기, accuracy_percentage)
        private static void GetResult(Mat[] outs, bool nms, PictureBox pictureBox1, Image image, RichTextBox richTextBox1, bool morethreshold = true)
        {
            float threshold = 0.1f; // 정확성. (0 ~ 1) (사과일 확률이 0.5 이상이면 검사함)
            float nmsThreshold = 0.1f; // 상자 곂치는거 필터 작용.
            try
            {
                // outs가 오류로 null이면 작동 X
                if(outs != null)
                {
                    List<int> classed = new List<int>();            // 해당객체가 무엇인지
                    List<float> confidences = new List<float>();    // 전체 정확성 (이미지 전체의)
                    List<float> probilities = new List<float>();    // box의 정확성
                    List<Rect2d> boxes = new List<Rect2d>();        // box

                    int w = 0, h = 0; // image(도화지)의 크기.

                    //w = pictureBox1.Image.Width;
                    //h = pictureBox1.Image.Height;

                    // 원래의 코드(image를 불러오는데 실패할것을 잡아줌.)
                    
                    if(image != null)
                    {
                        pictureBox1.Image = image;
                        w = image.Width;
                        h = image.Height;
                    }
                    else
                    {
                        w = pictureBox1.Image.Width;
                        h = pictureBox1.Image.Height;
                    }
                    

                    // mat의 앞의 5개는 클래스 확률이 아님.
                    const int prefix = 5;

                    // richbox 초기화.
                    richTextBox1.Text = "";

                    // 여러 객체를 하나씩 비교해감 (각 객체의 percent에 따라 threshold 초과인것만 저장함 )
                    foreach(Mat mat in outs)
                    {
                        for(int i = 0; i < mat.Rows; i++)
                        {
                            // 해당 객체에 대한 정확도
                            float confidence = mat.At<float>(i, 4);

                            if(confidence > threshold)
                            {
                                // 이해불가.
                                Cv2.MinMaxLoc(mat.Row(i).ColRange(prefix, mat.Cols), out _, out OpenCvSharp.Point max);
                                int classes = max.X; // class수 (txt파일의 0)
                                /// 앞의 5개를 제외하고 해당하는 class의 정확도 
                                float probability = mat.At<float>(i, classes + prefix); // 5번째 이후부터 검사한 클래스의 확률이므로.

                                if(probability > threshold)
                                {
                                    // mat의 값 받아옴.
                                    float centerX = mat.At<float>(i, 0) * w;
                                    float centerY = mat.At<float>(i, 1) * h;
                                    float width = mat.At<float>(i, 2) * w;
                                    float height = mat.At<float>(i, 3) * h;

                                    classed.Add(classes);
                                    confidences.Add(confidence);
                                    probilities.Add(probability);
                                    boxes.Add(new Rect2d(centerX, centerY, width, height));
                                }
                            }
                        }
                    }

                    // 모든 객체가 0.5 이하일때
                    if(boxes.Count == 0)
                    {
                        if (richTextBox1.Text != "관측되지 않음\n")
                            richTextBox1.Text = "관측되지 않음\n";
                    }
                    // 중복 box 삭제
                    else if(nms)
                    {
                        CvDnn.NMSBoxes(boxes, confidences, threshold, nmsThreshold, out int[] indices);
                        
                        foreach(int i in indices)
                        {
                            Rect2d box = boxes[i];
                            // class, probility 추가.
                            richTextBox1.AppendText($"{Label[classed[i]]} {probilities[i] * 100:0.0}%\n");
                            richTextBox1.AppendText("x,y = " + ((int)box.X).ToString() + "/" + ((int)box.Y).ToString() + "\n" + "w,h = " + ((int)box.Width).ToString() + "/" + ((int)box.Height).ToString() + "\n\n");

                            Draw(classed[i], probilities[i], (float)box.X, (float)box.Y, (float)box.Width, (float)box.Height, pictureBox1);
                        }
                    }
                    // 중복 box가 없을때
                    else
                    {
                        for(int i = 0; i < boxes.Count; i++)
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
                richTextBox1.AppendText("GetResult Fail!");
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
        

        

        private static string[] Label = new string[] {
            "apple",
            "orange"
        };

        private static Color[] color = new Color[]
        {
            Color.Red,
            Color.Orange
        };

        // button click => 사진 불러옴
        String filename = "";
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
                pictureBox1.Image = new Bitmap(filename);
                yolov3_Start();
            }
        }

        // 끌때 Event 함수
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill(); // 즉각 모든 프로그램 종료.
        }
    }
}