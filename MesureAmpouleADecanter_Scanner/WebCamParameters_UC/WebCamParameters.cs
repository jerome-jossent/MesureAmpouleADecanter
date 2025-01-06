using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebCamParameters_UC
{
    public class WebCamParameters
    {
        public Dictionary<CameraControlProperty, WebCamParameter_CameraControl> webCamParameters_CameraControl { get; set; }
        public Dictionary<VideoProcAmpProperty, WebCamParameter_VideoProcAmp> webCamParameters_VideoProcAmp { get; set; }
    }

    public abstract class WebCamParameter_abstract
    {
        public int currentValue { get; set; }        
        public bool auto { get; set; }

        public override string ToString()
        {
            return "val=" + currentValue + " auto=" + auto;
        }
    }

    public class WebCamParameter_VideoProcAmp : WebCamParameter_abstract
    {
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public VideoProcAmpFlags flags_vpa;

        public WebCamParameter_VideoProcAmp(
            int currentValue,
            bool auto)
        {
            this.currentValue = currentValue;
            this.auto = auto;
        }
    }

    public class WebCamParameter_CameraControl : WebCamParameter_abstract
    {
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public CameraControlFlags flags_cc;

        public WebCamParameter_CameraControl(
            int currentValue,
            bool auto)
        {
            this.currentValue = currentValue;
            this.auto = auto;
        }
    }


    public class WebCamParameters_Full
    {
        public Dictionary<CameraControlProperty, WebCamParameter_Full_CameraControl> webCamParameters_CameraControl { get; set; }
        public Dictionary<VideoProcAmpProperty, WebCamParameter_Full_VideoProcAmp> webCamParameters_VideoProcAmp { get; set; }
    }

    public abstract class WebCamParameter_Full_abstract
    {
        public string name { get; set; }
        public int currentValue { get; set; }
        public int minValue { get; set; }
        public int maxValue { get; set; }
        public int stepSize { get; set; }
        public int defaultValue { get; set; }
        public bool auto_enabled { get; set; }
        public bool auto { get; set; }

        public override string ToString()
        {
            return name + " val=" + currentValue + " [" + minValue + ";" + maxValue + ":" + stepSize + "] "
                + "default=" + defaultValue + (auto_enabled ? " auto allowed" : " manual only")
                + (auto_enabled ? (" auto=" + auto) : "")
                ;
        }
    }

    public class WebCamParameter_Full_VideoProcAmp : WebCamParameter_Full_abstract
    {
        public VideoProcAmpFlags flags_vpa;

        public WebCamParameter_Full_VideoProcAmp(string name,
            int currentValue,
            int minValue, int maxValue, int stepSize,
            int defaultValue,
            bool auto,
            VideoProcAmpFlags flags_vpa)
        {
            this.name = name;
            this.currentValue = currentValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.stepSize = stepSize;
            this.defaultValue = defaultValue;
            this.flags_vpa = flags_vpa;
            auto_enabled = flags_vpa == (VideoProcAmpFlags.Manual | VideoProcAmpFlags.Auto);
            this.auto = auto;
        }
    }

    public class WebCamParameter_Full_CameraControl : WebCamParameter_Full_abstract
    {
        public CameraControlFlags flags_cc;

        public WebCamParameter_Full_CameraControl(string name,
            int currentValue,
            int minValue, int maxValue, int stepSize,
            int defaultValue,
            bool auto,
            CameraControlFlags flags_cc)
        {
            this.name = name;
            this.currentValue = currentValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.stepSize = stepSize;
            this.defaultValue = defaultValue;
            this.flags_cc = flags_cc;
            auto_enabled = flags_cc == (CameraControlFlags.Manual | CameraControlFlags.Auto);
            this.auto = auto;
        }
    }
}
