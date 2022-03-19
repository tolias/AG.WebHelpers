using AG.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AG.WebHelpers.TCP
{
    public class TcpMessagesSender
    {
        private readonly int port;
        private TcpListener _tcpClientsListener;
        private readonly List<TcpClient> tcpClients = new List<TcpClient>();
        public LoggerBase Logger;

        public TcpMessagesSender(int port)
        {
            this.port = port;
        }

        public IAsyncResult Start()
        {
            _tcpClientsListener = new TcpListener(IPAddress.Any, port);
            _tcpClientsListener.Start();
            return WaitForNextTcpClient();
        }

        private IAsyncResult WaitForNextTcpClient()
        {
            return _tcpClientsListener.BeginAcceptTcpClient(BeginAcceptTcpClientHandler, null);
        }

        private void BeginAcceptTcpClientHandler(IAsyncResult asyncResult)
        {
            try
            {
                TcpClient tcpClient = _tcpClientsListener.EndAcceptTcpClient(asyncResult);
                tcpClients.Add(tcpClient);
                Logger?.Log(LogLevel.Info, "Accepted tcpClient: " + tcpClient.Client.RemoteEndPoint);
            }
            catch (WebException ex)
            {
                Logger?.Log(LogLevel.Error, ex, "Error accepting tcpClient");
            }
            WaitForNextTcpClient();
        }

        ///<summary> Sends a length-prepended(Pascal) string over the network</summary>
        public async Task SendMessage(string message)
        {
            if (tcpClients.Count == 0)
                return;

            // we won't use a binary writer, because the endianness is unhelpful

            // turn the string message into a byte[] (encode)
            byte[] messageBytes = Encoding.UTF8.GetBytes(message); // a UTF-8 encoder would be 'better', as this is the standard for network communications

            // determine length of message
            int length = messageBytes.Length;

            // convert the length into bytes using BitConverter (encode)
            byte[] lengthBytes = BitConverter.GetBytes(length);

            // flip the bytes if we are a little-endian system: reverse the bytes in lengthBytes to do so
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }

            var tasks = tcpClients.Select(client => Task.Run(() =>
            {
                NetworkStream networkStream = client.GetStream();
                // send length
                networkStream.Write(lengthBytes, 0, lengthBytes.Length);

                // send message
                networkStream.Write(messageBytes, 0, length);
            }));

            await Task.WhenAll(tasks);

            //TcpClient tcpClient = new TcpClient(new IPEndPoint(IPAddress.Any, Port));

        }
    }
}
