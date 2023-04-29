using Newtonsoft.Json;
using PushCarLib.Interfaces;

namespace PushCarLib
{
    public class Score : IJsonSerializable
    {
        public float Time { get; private set; }
        public float Distance { get; private set; }
        
        public Score(float time, float distance)
        {
            Time = time;
            Distance = distance;
        }
        
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        
        public void Deserialize(string json)
        {
            var obj = JsonConvert.DeserializeObject<Score>(json);
            
            Time = obj.Time;
            Distance = obj.Distance;
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}