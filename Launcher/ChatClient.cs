using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Launcher
{
  public   class ChatClient
    {
        private NetworkStream _stream;
        private TcpClient _client;
        private MainWindow _mainWindow;
        private string _name;
        public void SetName(string name)
        {
            _name = name;
        }
        public ChatClient(string address, int port, MainWindow mainWindow)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(address, port);
                _stream = _client.GetStream();
                _stream.ReadTimeout = 10000;
                _stream.WriteTimeout = 10000;
            }
            catch(Exception e)
            {
                _mainWindow.Dispatcher.Invoke(
                       () =>
                       {
                           _mainWindow.ErrorConnection(e.Message);
                       }
                       );
                return;
            }
            _mainWindow = mainWindow;
          Thread thread = new Thread(new ThreadStart(GetMessage));
            thread.IsBackground = true;
            thread.Start();
        }
        public void SendMessage(string text)
        {
            try
            {
                byte[] sendBytes = Encoding.Unicode.GetBytes(_name + " " + text);
                _stream.Write(sendBytes, 0, sendBytes.Length);
            }
            catch
            {
                _mainWindow.Dispatcher.Invoke(
                      () =>
                      {
                          _mainWindow.ErrorConnection("W");
                      }
                      );
            }
           
        }
        private void GetMessage()
        {
            try
            {

            
            while (true)
            {
                List<byte> _messageBytes = new List<byte>();
                
                if (!_stream.DataAvailable)
                    continue;
                while (_stream.DataAvailable)
                {
                    _messageBytes.Add((byte)_stream.ReadByte());
                }
                string _message = Encoding.Unicode.GetString(_messageBytes.ToArray()).Split('~')[0];
                if (!_message.Replace(" ","").Split(':')[1].StartsWith("mod-"))
                {
                    byte[] color = new byte[3]{
                byte.Parse(Encoding.Unicode.GetString(_messageBytes.ToArray()).Split('~')[1]),
                byte.Parse(Encoding.Unicode.GetString(_messageBytes.ToArray()).Split('~')[2]),
                byte.Parse(Encoding.Unicode.GetString(_messageBytes.ToArray()).Split('~')[3])
                };
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.GetMessage(_message, color);
                    });
                }
                else
                {
                   
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        Button button = new Button();
                        button.Content = _message.Split(':')[1].Split('-')[1];
                        
                        _mainWindow.MessageStackPanelView.Children.Add(button);
                        _mainWindow.MessageTextBox.Text = "";
                        button.Click+= _mainWindow.RepostButton_Click;
                    });
                }
                     
            }
            }
            catch
            {
                _mainWindow.Dispatcher.Invoke(
                      () =>
                      {
                          _mainWindow.ErrorConnection("w");
                      }
                      );
            }
        }
    }
}
