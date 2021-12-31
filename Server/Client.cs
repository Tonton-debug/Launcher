using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Server
{
  public abstract  class Client
    {
        public NetworkStream Stream { get; private set; }
        public TcpClient MyClient { get; private set; }
        public readonly string Ip;
        public Client(TcpClient tcpClient)
        {
            MyClient = tcpClient;
            Stream = MyClient.GetStream();
            Ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            Console.WriteLine("{0} подключился",Ip);
        }
        
       protected byte[] GetDataOld()
        {
            List<byte> buffer = new List<byte>();
            do
            {
                buffer.Add((byte)Stream.ReadByte());
              
            } while (Stream.DataAvailable);
            Console.WriteLine(buffer.Count);
            return buffer.ToArray();
        }
       
        protected string StringToBytesString(byte[] finalNameBytes)
        {
            string fileName = "";
            foreach (var item in finalNameBytes)
            {
                fileName += item;
            }
            return fileName;
        }
        protected void SendMassiveData(byte[] data)
        {
            int position=0;
            using (var stream =new MemoryStream(data))
            {
               
                while (position < stream.Length-1)
                {
              
                    byte[] buffer = new byte[4096];
                    if (position + buffer.Length >= stream.Length)
                    {
                       buffer = new byte[stream.Length- position];
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    position += buffer.Length;

                    SendDataAsync(buffer);
                    Console.WriteLine("Отправлено {0}/{1}", position, stream.Length - 1);
                    Stream.WriteByte(1);
                    
                }
                Stream.WriteByte(0);
            }       
        }
        protected void SendDataAsync(byte[] data)
        {

            Stream.WriteAsync(data, 0, data.Length);
            Console.WriteLine("End write");
        }
        protected void SendDataNotFast(byte[] data, bool waitAnswer)
        {
            foreach (var item in data)
            {
                Stream.WriteByte(item);
            }
            if (waitAnswer)
                Stream.ReadByte();
        }
        protected void SendData(byte[] data,bool waitAnswer)
        {
           
            Stream.Write(data, 0, data.Length);
            Console.WriteLine("End write");
            if (waitAnswer)
                Stream.ReadByte();
        }
       
        
    }
}
