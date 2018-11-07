using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AG.WebHelpers
{
    public class TcpMessagesListener : IDisposable
    {
        public int Port;
        public int ReceiveBufferSize;
        private TcpListener _listener;
        //private object _syncObj = new object();

        public event EventHandler<MessageEventArgs> MessageReceived;

        public TcpMessagesListener(int port, int receiveBufferSize = 0)
        {
            Port = port;
            ReceiveBufferSize = receiveBufferSize;
        }

        public bool Started => _listener != null;

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            StartAccept();
        }

        private void StartAccept()
        {
            //lock (_syncObj)
            {
                _listener?.BeginAcceptTcpClient(HandleAsyncConnection, _listener);
            }
        }

        private void HandleAsyncConnection(IAsyncResult asyncRes)
        {
            if (_listener == null)
                return;

            StartAccept();

            using (TcpClient tcpClient = _listener.EndAcceptTcpClient(asyncRes))
            {
                var messageReceivedHandler = MessageReceived;
                if(messageReceivedHandler != null)
                {
                    if (ReceiveBufferSize > 0)
                    {
                        tcpClient.ReceiveBufferSize = ReceiveBufferSize;
                    }

                    Console.WriteLine("Client connected completed: " + tcpClient.Client.RemoteEndPoint);

                    using (var netStream = tcpClient.GetStream())
                    {
                        byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                        netStream.Read(bytes, 0, tcpClient.ReceiveBufferSize);
                        string returndata = Encoding.UTF8.GetString(bytes);
                        returndata = returndata.TrimEnd('\0');
                        messageReceivedHandler(this, new MessageEventArgs(returndata));
                        netStream.Close();
                    }
                }
                tcpClient.Close();
            }
        }

        public void Stop()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public readonly string Message;

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
