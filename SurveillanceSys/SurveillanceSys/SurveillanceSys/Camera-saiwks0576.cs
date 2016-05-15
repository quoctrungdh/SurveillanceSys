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

namespace SurveillanceSys
{
    public partial class Camera : Form
    {
        Mat cam, firstFrame, gray, frameDelta, thresh;
        List<Mat> contours;

        public Camera()
        {
            InitializeComponent();
            
            Capture cap = new Capture();

            contours = new List<Mat>();

            while (true)
            {
                cam = cap.QueryFrame();

                if (cam.IsEmpty)
                    break;

                thresh = cam;
                try
                {
                    CvInvoke.CvtColor(cam, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                    CvInvoke.GaussianBlur(gray, gray, new Size(640, 480), 20);

                    CvInvoke.AbsDiff(firstFrame, gray, frameDelta);
                    
                    CvInvoke.Threshold(frameDelta, thresh, 25, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                    //CvInvoke.Dilate(thresh, thresh, null, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Default,new MCvScalar(0));

                    //CvInvoke.FindContours(thresh.Clone(),contours,)

                }
                catch (Exception)
                {   
                }

                if (firstFrame == null)
                {
                    firstFrame = gray;
                }

                CvInvoke.Imshow("thresh", thresh);

                CvInvoke.Imshow("Cam", cam);
                CvInvoke.WaitKey(33);
            }
        }


    }
}
