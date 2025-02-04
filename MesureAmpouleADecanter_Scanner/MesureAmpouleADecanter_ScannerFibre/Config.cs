using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WebCamParameters_UC.WebCamFormat;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class Config
    {
        public static Config _instance { get => instance; }
        static Config instance;

        public string webcam_name { get; set; }
        public Format webcam_format { get; set; }
        public WebCamParameters_UC.WebCamParameters webcam_parameters { get; set; }
        public List<Rect> rois { get; set; } = new List<Rect>();
        public List<Sensor> sensors { get; set; } = new List<Sensor>();

        [JsonIgnore] // pour le moment
        public Index_Hauteur_Manager index_Hauteur_Manager { get; set; } = new Index_Hauteur_Manager();

        public Config() { instance = this; }

        public static Config FromJSON(string jsonString)
        {
            //Config config = System.Text.Json.JsonSerializer.Deserialize<Config>(jsonString);
            Config config = JsonConvert.DeserializeObject<Config>(jsonString);

            //recompute H
            foreach (Sensor sensor in config.sensors)
            {
                if (sensor.numero != null)
                    sensor.hauteur_mm = Config._instance.index_Hauteur_Manager._GetHauteur((int)sensor.numero);
            }

            return config;
        }

        public string ToJSON()
        {
            //string jsonString = System.Text.Json.JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true});
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
            return jsonString;
        }
    }
}
