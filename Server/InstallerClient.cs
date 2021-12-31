using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Server
{
  public  class InstallerClient:Client
    {
        private byte[] receivedBytes = new byte[1];
        public InstallerClient(TcpClient tcpClient) : base(tcpClient)
        {
            Stream.BeginRead(receivedBytes,0, receivedBytes.Length,new AsyncCallback(EndRead),null);
        }
        private void EndWrite(IAsyncResult result)
        {
            Stream.Close();
            MyClient.Close();

        }
        private void EndRead(IAsyncResult result)
        {
            try
            {
                int readBytes = Stream.EndRead(result);
                int state = receivedBytes[0];
                switch (state)
                {
                    case 0:
                        Stream.Write(new byte[] { (byte)MainServer.GetVersion() }, 0, 1);
                        Stream.BeginRead(receivedBytes, 0, receivedBytes.Length, new AsyncCallback(EndRead), null);
                        break;
                    case 1:
                        byte[] launcherBytes = File.ReadAllBytes(MainServer.PATH + "launcher.zip");
                      
                        Stream.Write(launcherBytes, 0, launcherBytes.Length);
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("{0}\n{1}", e, e.Message);
                Console.WriteLine("InstallerClient {0} отключился",Ip);
            }
        }
    }
}
