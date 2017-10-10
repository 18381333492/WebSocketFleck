using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Fleck.Samples.ConsoleApp
{

    public class SocketModel
    {

        /// <summary>
        /// 是否链接
        /// </summary>
        public bool IsConnect
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Socket链接的唯一标识
        /// </summary>
        public string sPriKey
        {
            get;
            set;
        }

        /// <summary>
        /// 需要发送消息的socket的标识
        /// </summary>
        public string sSendPriKey
        {
            get;
            set;
        }

        /// <summary>
        /// 发送/接受的数据
        /// </summary>
        public object message
        {
            get;
            set;
        }



        public string toJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }


    class Server
    {
        static void Main()
        {
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://127.0.0.1:7777");
            server.Start(socket =>
                {
                    socket.OnOpen = () =>
                        {
                            Console.WriteLine(socket.ToString());
                            allSockets.Add(socket);
                            socket.Send(new SocketModel()
                            {
                                sPriKey = socket.ConnectionInfo.Id.ToString(),
                                IsConnect = true
                            }.toJson());
                        };
                    socket.OnClose = () =>
                        {
                            Console.WriteLine("Close!");
                            allSockets.Remove(socket);
                        };
                    socket.OnMessage = message =>
                        {
                            var revices = JsonConvert.DeserializeObject<SocketModel>(message);
                            if (!string.IsNullOrEmpty(revices.sSendPriKey))
                            {
                                var sendSocket = allSockets.Where(m => m.ConnectionInfo.Id.ToString() == revices.sSendPriKey).SingleOrDefault();
                                if (sendSocket != null)
                                {
                                    sendSocket.Send(new SocketModel()
                                    {
                                        sPriKey = sendSocket.ConnectionInfo.Id.ToString(),
                                        message = "success"
                                    }.toJson());
                                }
                            }
                        };
                });
            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }

        }
    }
}
