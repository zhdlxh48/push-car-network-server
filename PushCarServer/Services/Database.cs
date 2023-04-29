using System;
using System.Linq;
using System.Net;
using MySql.Data.MySqlClient;
using PushCarLib;

namespace PushCar.Services
{
    public class Database
    {
        private MySqlConnection _conn;

        public Database(string server, int port, string db, string uid, string pwd)
        {
            var connStr = GetConnectionString(server, port, db, uid, pwd);
            _conn = new MySqlConnection(connStr);
        }

        private string GetConnectionString(string server, int port, string db, string uid, string pwd)
        {
            return $"Server={server};Port={port};Database={db};Uid={uid};Pwd={pwd};SslMode=none;";
        }
        
        public void AddScore(Score score)
        {
            try
            {
                _conn.Open();
                
                var sqlStr = $"INSERT INTO scores (distance, time) VALUES ({score.Distance}, {score.Time});";
                var cmd = new MySqlCommand(sqlStr, _conn);
                cmd.ExecuteNonQuery();
                
                _conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        public Score[] GetScores(int limit)
        {
            var scores = new Score[limit];
            var count = 0;
            try
            {
                _conn.Open();
                
                var sqlStr = $"SELECT * FROM scores ORDER BY distance ASC LIMIT {limit};";
                var cmd = new MySqlCommand(sqlStr, _conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var distance = reader.GetFloat(0);
                    var time = reader.GetFloat(1);

                    scores[count++] = new Score(time, distance);
                    
                    Console.WriteLine($"distance: {distance}, time: {time}");
                }
                
                reader.Close();
                _conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            var retArr = new Score[count];
            Array.Copy(scores, retArr, count);
            return retArr;
        }

        public object GetRandomScore()
        {
            Score score = null;
            try
            {
                _conn.Open();
                
                var sqlStr = "SELECT * FROM scores ORDER BY RAND() LIMIT 1;";
                var cmd = new MySqlCommand(sqlStr, _conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var distance = reader.GetFloat(0);
                    var time = reader.GetFloat(1);

                    score = new Score(time, distance);
                    
                    Console.WriteLine($"distance: {distance}, time: {time}");
                }
                
                reader.Close();
                _conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return score;
        }
        
        private IPAddress GetIpAddress(string addrStr)
        {
            try
            {
                var entry = Dns.GetHostEntry(addrStr);
                return entry.AddressList[0];
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Format is not valid");
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("Address string argument is null");
            }

            return IPAddress.Loopback;
        }
    }
}