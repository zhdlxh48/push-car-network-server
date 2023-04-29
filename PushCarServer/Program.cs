using System;
using PushCar.Services;
using PushCar.Services.Server;

namespace PushCar
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var server = new TcpServer(9948);
            
            Console.WriteLine("** Server Started **");
            
            server.Run();

            Console.WriteLine("[press any key to shutdown server]");
            Console.ReadLine();
            
            Console.WriteLine("Trying to shutdown server...");

            server.Stop();
            
            Console.WriteLine("** Server Shutdown **");
        }
    }
}