using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace skypeRobotController
{
    //Class to handle each client request separatly

    class HandleClient
    {
        private TcpClient clientSocket;
        //private string dataFromClient;
        private string dataFromClient;
        public string DataFromClient
        {
            get { return dataFromClient; }
            set { dataFromClient = value; }
        }

        //public string DataFromClient { get; set; }
        public HandleClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            Thread ctThread = new Thread(DoChat);
            ctThread.Start();
        }
        private void DoChat()
        {
            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];

            while ((clientSocket.Connected))
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    DataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
        }
    }
}
