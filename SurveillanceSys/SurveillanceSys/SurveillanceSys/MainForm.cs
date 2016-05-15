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

namespace SurveillanceSys
{
    public partial class MainForm : Form
    {
        Mat cam, firstFrame, gray, frameDelta, thresh;
        List<Mat> contours;
        Capture cap;

        const int MAX_NUM_OBJECTS = 50;

        public MainForm()
        {
            InitializeComponent();

            OpenVideo("video4_1.avi"/*"example_02.mp4""demo.avi"*/);

            Process();
            
        }

        void OpenVideo(string file)
        {
            cap = new Capture(file);
        }

        void Process()
        {
            thresh = gray = new Mat();

            while (true)
            {
                cam = cap.QueryFrame();
                
                //if cam = null it's mean end of the video
                if (cam == null)
                    break;

                try
                {
                    //convert frame from BGR to Gray
                    CvInvoke.CvtColor(cam, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                    //blur it
                    CvInvoke.GaussianBlur(gray, gray, new Size(21, 21), 0);

                    //show gray frame
                    CvInvoke.Imshow("gray", gray);

                    //firstFrame is first frame of video, if it is null, 
                    if (firstFrame == null)
                    {
                        firstFrame = gray.Clone();
                        frameDelta = firstFrame.Clone();
                        continue;
                    }
                    CvInvoke.AbsDiff(gray, firstFrame, frameDelta);
                    CvInvoke.Imshow("frameDelta", frameDelta);

                    CvInvoke.Threshold(frameDelta, thresh, 25, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                    Morphops(thresh);

                    CvInvoke.Imshow("thresh", thresh);

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
                }
            }
        }
    }
}
