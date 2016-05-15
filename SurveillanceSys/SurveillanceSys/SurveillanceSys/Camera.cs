using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using CameraClient.Network;
using System.Threading;
using System.Net;

namespace SurveillanceSys
{
    public partial class Camera : Form
    {
        const int MAX_NUM_OBJECTS = 50;

        public static string IP;
        public string cameraStatus;
        public string ipAddress;

        ClientConnection clientConnection;        
        Thread th_connectServer, th_sendServer;

        Mat cam;
        Mat firstFrame;
        Mat gray;
        Mat frameDelta;
        Mat thresh;
        Capture cap;

        bool isConnected;
        bool isActived;
        bool stop;

        DateTime time_detect;

        public Camera()
        {
            InitializeComponent();

            Init();

        }

        void Init()
        {
            clientConnection = null;
            th_connectServer = null;
            ipAddress = "192.168.1.4";
            cameraStatus = "";

            thresh = gray = new Mat();

            isConnected = false;
            isActived = false;
            stop = false;
        
            OpenFile("video4_1.avi"/*"example_02.mp4""video1.mp4""demo.avi"*/);
        }

        void OpenFile(string file)
        {
            cap = new Capture(file);
        }

        void Process()
        {
            while (!stop)
            {
                // read frame by frame from video or camera
                cam = cap.QueryFrame();

                // end of video
                if (cam == null)
                    break;

                try
                {
                    // convert color from BGR to Gray image
                    CvInvoke.CvtColor(cam, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                    // blur it
                    CvInvoke.GaussianBlur(gray, gray, new Size(21, 21), 0);
                    
                    CvInvoke.Imshow("gray", gray);

                    // if first frame is null meaning it's first frame of video
                    if (firstFrame == null)
                    {
                        //clone from gray frame
                        firstFrame = gray.Clone();
                        frameDelta = firstFrame.Clone();
                        CvInvoke.Imshow("firstFrame", firstFrame);
                        continue;
                    }

                    // every frame change, compair with first frame, if there is difference between them, save it to frame delta
                    CvInvoke.AbsDiff(gray, firstFrame, frameDelta);
                    CvInvoke.Imshow("frameDelta", frameDelta);

                    // threshold frame delta
                    CvInvoke.Threshold(frameDelta, thresh, 25, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                    
                    
                    Morphops(thresh);

                    CvInvoke.Imshow("thresh", thresh);

                    // find contours from thresh image
                    findContours(thresh, cam);
                }
                catch (Exception)
                {
                }
                

                CvInvoke.Imshow("Cam", cam);
                CvInvoke.WaitKey(33);
            }
        }
        void Morphops(Mat threshold)
        {
            //create structuring element that will be used to "dilate" and "erode" image
            //the element chosen here is a 3px by 3px rectangle
            Mat erodeElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(3, 3), new Point(0, 0));

            //dilate  with larger element so make sure object is nicely visible
            Mat dilateElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(8, 8), new Point(0, 0));

            //erode
            CvInvoke.Erode(threshold, threshold, erodeElement, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));
            CvInvoke.Erode(threshold, threshold, erodeElement, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));

            //dilate
            CvInvoke.Dilate(threshold, threshold, dilateElement, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));
            CvInvoke.Dilate(threshold, threshold, dilateElement, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));
        }
        
        void findContours(Mat threshold, Mat cam)
        {
            Mat temp;
            temp = threshold.Clone();// threshold.copyTo(temp);

            //these two vectors needed for output of findContours
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();

            //find contours of the filtered image using openCV findContours function
            CvInvoke.FindContours(temp, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Ccomp,Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size ; i++ )
            {
                Mat c = new Mat();

                //if the contour is too small, ignore it
                if (CvInvoke.ContourArea(contours[i]) < 1000)
                {
                    continue;
                }

                //compute the bounding box for the contour, draw it on the frame
                Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                if (rect.Width > 40 && rect.Height > 40)
                {
                    CvInvoke.Rectangle(cam, rect, new MCvScalar(0, 0, 255), 2);
                    if (time_detect == null)
                    {
                        time_detect = DateTime.Now;
                    }
                    else if(Detecting())
                    {
                        continue;
                    }
                    Bitmap bmp = new Bitmap(cam.Bitmap);
                    Image sendImg = (Image)bmp.Clone();

                    th_sendServer = new Thread(() => clientConnection.Send(sendImg));
                    th_sendServer.Start();
                   

                    
                    string datetime = time_detect.ToShortTimeString().ToString()
                                    + " " + time_detect.ToShortDateString().ToString();
                    cameraStatus = "Phat hien chuyen dong vao luc: " + datetime;
                    lbLog.Items.Add(cameraStatus);

                    
                }
            }
        }

        bool Detecting()
        {
            DateTime now = DateTime.Now;
            if (time_detect.Date <= now.Date
                && time_detect.Hour <= now.Hour
                && time_detect.Minute <= now.Minute
                && (now.Second - time_detect.Second) < 5)
            {
                return true;
            }
            time_detect = DateTime.Now;

            return false;
        }

        public bool Connect()
        {
            try
            {
                clientConnection = new ClientConnection(ipAddress, 2014);
                th_connectServer = new Thread(new ThreadStart(clientConnection.Connect));
                th_connectServer.Start();
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                ipAddress = txtServerIP.Text;
                if (Connect())
                {
                    isConnected = true;
                    sttConnect.Text = "Connected";
                    sttConnect.ForeColor = Color.Green;
                    btnStart_Reset.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Cannot connect to server!", "Connect error", MessageBoxButtons.OK);
                }
            }

        }

        private void btnStart_Reset_Click(object sender, EventArgs e)
        {
            if (!isActived)
            {
                isActived = true;
                sttActive.ForeColor = Color.Green;
                sttActive.Text = "Active";
                btnStart_Reset.Text = "Stop";
                stop = false;
                Process();
            }
            else
            {
                btnStart_Reset.Text = "Start";
                isActived = false;
                sttActive.ForeColor = Color.Red;
                sttActive.Text = "Disactive";
                stop = true;
                Process();
            }
        }

        private void Camera_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop = true;
            Application.Exit();
        }

        private void Camera_Load(object sender, EventArgs e)
        {
            txtServerIP.Text = "127.0.0.1";
        }
    }
}
