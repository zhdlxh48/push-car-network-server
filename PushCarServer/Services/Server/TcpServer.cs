using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using PushCar.Utils;
using PushCarLib;

namespace PushCar.Services.Server
{
    public class TcpServer
    {
        private const int BufferSize = 512;
        
        private readonly int _port;
        private readonly int _listenMax;

        private Socket _listenSocket;
        private readonly Thread _serverThread;
        
        private static Database _db;

        public TcpServer(int port, int backlog = 1000)
        {
            _port = port;
            _listenMax = backlog;

            _serverThread = new Thread(ListenClient);
            
            // _db = new Database("127.0.0.1", "ckgame", "test", "RealTjshd*499");
            _db = new Database("svc.sel4.cloudtype.app", 31504, "ckgame", "test", "201813086");
        }

        public void Run()
        {
            _listenSocket = SocketFactory.Create(ProtocolType.Tcp);
            _listenSocket.BindAndListen(_port, _listenMax);
            
            _serverThread.Start(_listenSocket);
        }

        public void Stop()
        {
            _listenSocket.Close();
            _listenSocket.Dispose();
            
            _serverThread.Abort();
        }

        private static void ListenClient(object sock)
        {
            var listenSocket = sock as Socket;
            while (true)
            {
                try
                {
                    var clientSocket = listenSocket.Accept();

                    var clientEP = clientSocket.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine("{0}:{1} - 클라이언트 연결 허용", clientEP.Address, clientEP.Port);

                    var procThread = new Thread(ProcessClient);
                    procThread.Start(clientSocket);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"[Network Socket Error] Code: {ex.ErrorCode}, {ex.Message}");
                    break;
                }
                catch (ThreadStateException ex)
                {
                    Console.WriteLine($"[Threading Error] {ex.Message}");
                    break;
                }
            }

            // listenSocket.Close();
            // listenSocket.Dispose();
        }

        private static void ProcessClient(object sock)
        {
            var buffer = new byte[BufferSize];
            
            var clientSocket = sock as Socket;
            var clientEp = clientSocket.RemoteEndPoint as IPEndPoint;

            while (true)
            {
                try
                {
                    var recvLen = clientSocket.Receive(buffer, BufferSize, SocketFlags.None);
                    if (recvLen == 0) break;

                    var json = Encoding.UTF8.GetString(buffer, 0, recvLen);
                    var obj = JObject.Parse(json);
                    
                    var flag = obj.GetValue("flag")?.ToString();
                    Console.WriteLine("{0}:{1} - {2}", clientEp.Address, clientEp.Port, flag);
                    
                    var response = new JObject
                    {
                        ["flag"] = flag.Replace("game", "server"),
                        ["response"] = null, 
                        ["result"] = "failed"
                    };
                    
                    if (flag == "game/result")
                    {
                        var score = new Score(0f, 0f);
                        score.Deserialize(obj.GetValue("score")?.ToString());
                        Console.WriteLine($"[Time] {score.Time} / [Distance] {score.Distance}");
                        
                        _db.AddScore(score);
                        
                        response["result"] = "success";
                    }

                    if (flag == "game/req-scores")
                    {
                        var count = obj.GetValue("count")?.ToObject<int>() ?? 0;
                        Console.WriteLine($"[Count] {count}");
                        
                        var scores = _db.GetScores(count);
                        Console.WriteLine($"[Scores] {scores.Length}");
                        
                        response["response"] = scores.Length > 0 ? JArray.FromObject(scores) : null;
                        response["result"] = "success";
                    }

                    if (flag == "game/req-random")
                    {
                        var score = _db.GetRandomScore();
                        Console.WriteLine($"[Score] {score}");
                        
                        response["response"] = score != null ? JObject.FromObject(score) : null;
                        response["result"] = "success";
                    }

                    var resJson = Encoding.UTF8.GetBytes(response.ToString());
                    int sendLen = resJson.Length > BufferSize ? BufferSize : resJson.Length;
                    clientSocket.Send(resJson, sendLen, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"[Network Socket Error] Code: {ex.ErrorCode}, {ex.Message}");
                    break;
                }
                catch (ThreadStateException ex)
                {
                    Console.WriteLine($"[Threading Error] {ex.Message}");
                    break;
                }
            }

            Console.WriteLine("{0}:{1} - 클라이언트 종료", clientEp.Address, clientEp.Port);

            clientSocket.Close();
            clientSocket.Dispose();
        }
    }
}