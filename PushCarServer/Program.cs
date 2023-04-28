using PushCar.Services;

namespace PushCar
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var server = new TcpServer();
            var db = new DBConnector();
        }
    }
}