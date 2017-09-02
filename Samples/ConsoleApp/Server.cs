using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Fleck.Samples.ConsoleApp
{

    public class MessageModel
    {

        public bool IsConnect
        {
            get;
            set;
        } = false;

        public string id
        {
            get;
            set;
        }

        public string data
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
            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.Start(socket =>
                {
                    socket.OnOpen = () =>
                        {
                            Console.WriteLine(socket.ToString());
                            allSockets.Add(socket);
                            socket.Send(new MessageModel() {
                                id = socket.ConnectionInfo.Id.ToString(),
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
                            var requesData=JsonConvert.DeserializeObject<MessageModel>(message);
                            string Id = socket.ConnectionInfo.Id.ToString();
                            var currentSocket = allSockets.Where(m => m.ConnectionInfo.Id.ToString() == Id).SingleOrDefault();
                            if (currentSocket != null)
                            {
                                currentSocket.Send(new MessageModel()
                                {
                                    id = Id,
                                    data = "success"
                                }.toJson());
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
