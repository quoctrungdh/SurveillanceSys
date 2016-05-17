using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing.Imaging;

using System.Data.SqlClient;
using System.Globalization;


namespace Server
{
    public partial class Form1 : Form
    {
        string ipAddress;
        int port;

        string sNoti;

        const int BUF_SIZE = 1024 * 100;
        byte[] sendData = new byte[BUF_SIZE];
        byte[] recvData = new byte[BUF_SIZE];

        Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket interactSocket;

        Thread th_InitServer, th_InteractClient, th_CreateConnection;

        List<Socket> clientsList = new List<Socket>();

        public Form1()
        {
            InitializeComponent();
            //CheckForIllegalCrossThreadCalls = false;
            port = 2014;
            try
            {
                th_InitServer = new Thread(CreateConnection);
                th_InitServer.IsBackground = true;
                th_InitServer.Start();

                //string serverName = Dns.GetHostName();
                //sNoti = "Server " + serverName + " started " + GetIPByName(serverName);
                //lbNoti.Items.Add(sNoti);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void InitServer()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            ipAddress = Convert.ToString(localIPs[localIPs.Length - 2]);
            IPEndPoint iEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            listenSocket.Bind(iEP);
            listenSocket.Listen(10);

            AddLbStatus("Server started " + listenSocket.LocalEndPoint.ToString());
            
            while(true)
            {
                interactSocket = listenSocket.Accept();
                clientsList.Add(interactSocket);
                ThreadPool.QueueUserWorkItem(Interact, interactSocket);
            }
        }
        private void CreateConnection()
        {
            th_CreateConnection = new Thread(InitServer);
            th_CreateConnection.IsBackground = true;
            th_CreateConnection.Start();
        }

        private void Interact(object obj)
        {
            string tmp = string.Empty;
            var socket = (Socket)obj;
            var handle = socket.Handle;

            //sendData = Encoding.Unicode.GetBytes("Successfully connected at " + ipAddress);
            //socket.Send(sendData);

            AddLbStatus(socket.RemoteEndPoint.ToString() + " connected");

            th_InteractClient = new Thread((ThreadStart)(() =>
            {
                while (true)
                {
                   // byte[] rcvData = new byte[1024];
                    try
                    {
                        socket.Receive(recvData);
                        if (recvData.Length > 10000)
                        {
                            Image recvImg = (Image)ByteArrayToObject(recvData);
                            pictureBox1.Image = recvImg;
                            AddLbStatus("Received image from " + socket.RemoteEndPoint.ToString());

                            //string datetime = DateTime.Now.ToString();

                            //datetime = datetime.Replace(@" ", "");
                            //datetime = datetime.Replace(@":", "");
                            //datetime = datetime.Replace(@"/", "");
                            string datetime = BuildTimeString(DateTime.Now);
                            string imgFileName = Application.StartupPath + "\\" + datetime + ".jpg";
                            string ipCam = socket.RemoteEndPoint.ToString();

                            Image saveImg = (Image)recvImg.Clone();
                            saveImg.Save(imgFileName, ImageFormat.Jpeg);

                            InsertDB(datetime, ipCam, imgFileName);

                            AddLbStatus(socket.RemoteEndPoint.ToString() + ".jpeg saved at " + datetime);    
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString()); ;
                    }
                    
                }
            }));
            th_InteractClient.IsBackground = true;
            th_InteractClient.Start();
        }

        string BuildTimeString(DateTime recvTime)
        {
            string date, time, seperator = "-";
            date = recvTime.Year + seperator + recvTime.Month.ToString() + seperator + recvTime.Day;
            time = recvTime.Hour.ToString() + seperator + recvTime.Minute.ToString() + seperator + recvTime.Second.ToString();
            return date + "_" + time;
        }

        void InsertDB(string datetime, string ipCam, string link)
        {
            //SqlConnection connection = new SqlConnection("Data Source=(localdb)\\ProjectsV12;Initial Catalog=SurveillanceSys_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False");
            SqlConnection connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\PROJECTS\\SurveillanceSys_Git\\SurveillanceSys\\SurveillanceSystemServer\\SurveillanceSystemWebApp\\App_Data\\SurveillanceSysDB.mdf;Integrated Security=True");

            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            using (SqlCommand cmd = new SqlCommand("INSERT INTO [Table] VALUES(@time, @ip, @link)", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@time", datetime));
                cmd.Parameters.Add(new SqlParameter("@ip", ipCam));
                cmd.Parameters.Add(new SqlParameter("@link", link));
                cmd.ExecuteNonQuery();
            }

            connection.Close();
        }

        private string GetIPByName(string name)
        {
            try
            {
                IPAddress[] ipAdds = Dns.GetHostAddresses(name);
                return Convert.ToString(ipAdds[ipAdds.Length - 2]);
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static Image ByteArrayToObject(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        delegate void SetTextCallback(string text);
        public void AddLbStatus(String text)
        {
            if (this.lbNoti.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AddLbStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lbNoti.Items.Add(text);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            
        }

        bool SendNoti(string phone, string message)
        {
            string url = "http://api.esms.vn/MainService.svc/xml/SendMultipleMessage_V2/";
            // declare ascii encoding
            UTF8Encoding encoding = new UTF8Encoding();

            string strResult = string.Empty;


            string customers = "";

            string[] lstPhone = phone.Split(',');

            for (int i = 0; i < lstPhone.Count(); i++)
            {
                customers = customers + @"<CUSTOMER>"
                                + "<PHONE>" + lstPhone[i] + "</PHONE>"
                                + "</CUSTOMER>";
            }

            //string SampleXml = @"<RQST>"

            string APIKey = "F52DDAE968CBCF351AFEA429F38036";
            string SecretKey = "0949AF1207C64D28B197A5D1A8AC41";

            string SampleXml = @"<RQST>"
                               + "<APIKEY>" + APIKey + "</APIKEY>"
                               + "<SECRETKEY>" + SecretKey + "</SECRETKEY>"
                               + "<ISFLASH>0</ISFLASH>"
                               + "<SMSTYPE>7</SMSTYPE>"
                               + "<CONTENT>" + message + "</CONTENT>"
                               + "<CONTACTS>" + customers + "</CONTACTS>"


           + "</RQST>";
            string postData = SampleXml.Trim().ToString();
            // convert xmlstring to byte using ascii encoding
            byte[] data = encoding.GetBytes(postData);
            // declare httpwebrequet wrt url defined above
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            // set method as post
            webrequest.Method = "POST";
            webrequest.Timeout = 500000;
            // set content type
            webrequest.ContentType = "application/x-www-form-urlencoded";
            // set content length
            webrequest.ContentLength = data.Length;
            // get stream data out of webrequest object
            Stream newStream = webrequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // declare & read response from service
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

            // set utf8 encoding
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            // read response stream from response object
            StreamReader loResponseStream =
                new StreamReader(webresponse.GetResponseStream(), enc);
            // read string from stream data
            strResult = loResponseStream.ReadToEnd();
            // close the stream object
            loResponseStream.Close();
            // close the response object
            webresponse.Close();
            // below steps remove unwanted data from response string
            strResult = strResult.Replace("</string>", "");
            return true;
        }
    }
}
