using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
namespace Server
{
  
 public   class ChatClient : Client
    {
        public byte[] MyColor { get; private set; }
        private byte[] _resultBytes = new byte[1000];
        public ChatClient(TcpClient tcpClient):base(tcpClient)
        {
         
            MyColor = MainServer.GetColor();
            ReceiveMessages();
        }
        private void EndRead(IAsyncResult result)
        {
            try
            {
                int bytesRead = Stream.EndRead(result);
                while (Stream.DataAvailable)
                {
                    Stream.ReadByte();
                }
                _resultBytes = _resultBytes.ToList().Take(bytesRead).ToArray();
                string message = Encoding.Unicode.GetString(_resultBytes).Replace('~', ' ');
             
                if(message.Replace(" ","").Split(':')[1].StartsWith("mod-"))
                {
                    Console.WriteLine("{1} - {0} ", message, Ip);
                    _resultBytes = Encoding.Unicode.GetBytes(message);

                }
                else
                {
                    string colorAndMessage = message + "~" + MyColor[0] + "~" + MyColor[1] + "~" + MyColor[2];
                    Console.WriteLine("{1} - {0} ", colorAndMessage, Ip);
                    _resultBytes = Encoding.Unicode.GetBytes(colorAndMessage);
                }
                MainServer.SendBytesData(_resultBytes);
                ReceiveMessages();
            }

            catch (Exception e)
            {
                Console.WriteLine("{0}\n{1}", e, e.Message);
                MainServer.KillClient(this, TypeClient.BaseClient);
            }
        }
        public void ReceiveMessages()
        {
            try
            {
                _resultBytes = new byte[1000];
                Stream.BeginRead(_resultBytes, 0, _resultBytes.Length,new AsyncCallback(EndRead),null);      

            }
            catch
            {
            }
        }
    }
}
