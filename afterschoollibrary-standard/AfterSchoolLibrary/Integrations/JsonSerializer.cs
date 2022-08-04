using Newtonsoft.Json;

namespace AfterSchoolLibrary.Integrations
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string jsonString);
        string Serialize<T>(T jsonObject);
    }

    public class JsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public string Serialize<T>(T jsonObject)
        {
            return JsonConvert.SerializeObject(jsonObject);
        }
    }
}