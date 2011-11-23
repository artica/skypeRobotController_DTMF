using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace skypeRobotController
{
    class TcpControlServer
    {
        private TcpListener listener;
        private List<HandleClient> clients = new List<HandleClient>();
        public HandleClient Client
        {
            get 
            {
                if (clients.Count > 0)
                    return clients[0];
                else
                    return null;
            }
        }


        public TcpControlServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, Properties.Settings.Default.port);
                listener.Start();
                // Accept the connection. 
                // BeginAcceptSocket() creates the accepted socket.
                listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.ToString());
                throw ex;
            }
        }
        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            // End the operation and display the received data on 
            // the console.
            TcpClient client = listener.EndAcceptTcpClient(ar);
            // Process the connection here. (Add the client to a
            // server table, read data, etc.)
            var handleClient = new HandleClient(client);
            clients.Add(handleClient);
            // Signal the calling thread to continue.

        }
    }
}
