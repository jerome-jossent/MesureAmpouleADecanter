using DirectShowLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace WebCamParameters_UC
{
    public class WebCamConfig
    {
        public string device_name { get; set; }
        public Dictionary<string, WebCamParameter> cameraControlProperties { get; set; } = new Dictionary<string, WebCamParameter>();
        public Dictionary<string, WebCamParameter> videoProcAmpProperties { get; set; } = new Dictionary<string, WebCamParameter>();

        public WebCamConfig() { }//JSON

        public WebCamConfig(string name,
            Dictionary<CameraControlProperty, WebCamParameter_CameraControl> cameraControlParameters,
            Dictionary<VideoProcAmpProperty, WebCamParameter_VideoProcAmp> videoProcAmpParameters)
        {
            this.device_name = name;
            foreach (var item in cameraControlParameters)
                cameraControlProperties.Add(item.Key.ToString(), new WebCamParameter(item.Value));

            foreach (var item in videoProcAmpParameters)
                videoProcAmpProperties.Add(item.Key.ToString(), new WebCamParameter(item.Value));
        }

        internal static WebCamConfig? FromFile(string filePath)
        {
            string jsonString = System.IO.File.ReadAllText(filePath);
            return FromJSON(jsonString);
        }

        internal static WebCamConfig? FromJSON(string jsonString)
        {
            WebCamConfig? wcc = JsonConvert.DeserializeObject<WebCamConfig>(jsonString);
            return wcc;
        }
    }
}