using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CameraClient.Network
{
    class ClientConnection
    {
        private string serverIP;
        private int serverPort;
        private TcpClient tcpClient;
        private NetworkStream networkStream;


        const int BUF_SIZE = 1024*100;
        byte[] sendBuffer = new byte[BUF_SIZE];

        public ClientConnection(string sIP, int nPort)
        {
            serverIP = sIP;
            serverPort = nPort;
        }

        public void Connect()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(serverIP), serverPort);
                networkStream = tcpClient.GetStream();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());               
            }
        }

        public void Send(Image img)
        {
            try
            {
                sendBuffer = ObjectToByteArray(img);
                tcpClient.GetStream().Write(sendBuffer, 0, sendBuffer.Length);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString()); ;
            }
        }

        public static byte[] ObjectToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public void CloseConnection()
        {
            if(tcpClient != null)
                tcpClient.Close();
            if (networkStream != null)
                networkStream.Close();
        }
    }
}
