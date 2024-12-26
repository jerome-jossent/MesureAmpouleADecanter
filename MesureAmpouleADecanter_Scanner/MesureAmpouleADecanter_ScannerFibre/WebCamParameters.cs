using DirectShowLib;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public abstract class WebCamParameters
    {
        public string name;
        public int currentValue, minValue, maxValue, stepSize, defaultValue;
        public bool auto_enabled;
        public override string ToString()
        {
            return name + " val=" + currentValue + " [" + minValue + ";" + maxValue + ":" + stepSize + "] default=" + defaultValue + (auto_enabled? " auto allowed": " manual only");
        }
    }

    public class WebCamParameters_VideoProcAmp : WebCamParameters
    {
        public VideoProcAmpFlags flags_vpa;

        public WebCamParameters_VideoProcAmp(string name,
            int currentValue,
            int minValue, int maxValue, int stepSize,
            int defaultValue,
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
        }
    }

    public class WebCamParameters_CameraControl : WebCamParameters
    {
        public CameraControlFlags flags_cc;

        public WebCamParameters_CameraControl(string name,
            int currentValue,
            int minValue, int maxValue, int stepSize,
            int defaultValue,
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
        }
    }
}
