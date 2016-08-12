using System.IO;
using Newtonsoft.Json;

namespace FatPiggy
{
    public class Config
    {
        public int MaxPiggies = 5;
        public string BypassMaxPermission = "fatpiggy.bypass";

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }

        public static Config Read(string path)
        {
            return !File.Exists(path)
                ? new Config()
                : JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}