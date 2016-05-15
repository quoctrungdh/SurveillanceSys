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
                            
                            string imgFileName = /*DateTime.Now.ToShortDateString().ToString()*/"abc.jpg";
                            Image saveImg = (Image)recvImg.Clone();
                            saveImg.Save("D:/" + imgFileName, ImageFormat.Jpeg);
                            AddLbStatus(socket.RemoteEndPoint.ToString() + ".jpeg saved");
                            
                            
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
    }
}
