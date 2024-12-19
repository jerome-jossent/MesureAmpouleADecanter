using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class SensorMap
    {
        public struct ROI
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public ROI roi_left_up_right_bottom { get; set; }
        public List<Sensor> sensors { get; set; } = new List<Sensor>();

        public SensorMap()
        {
        }

        public static SensorMap FromJSON(string jsonString)
        {
            SensorMap sensorMap = JsonSerializer.Deserialize<SensorMap>(jsonString);
            return sensorMap;
        }

        public string ToJSON()
        {
            string jsonString = JsonSerializer.Serialize(this);
            return jsonString;
        }

        internal void DefineROI(int roi_left, int roi_top, int roi_right, int roi_bottom)
        {
            roi_left_up_right_bottom = new ROI()
            {
                left = roi_left,
                top = roi_top,
                right = roi_right,
                bottom = roi_bottom
            };
        }

        internal void AddSensor(Sensor s)
        {
            sensors.Add(s);
        }
    }
}
