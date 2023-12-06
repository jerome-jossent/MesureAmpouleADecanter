using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCam
{
    public class CaptureArguments_data
    {
        //exposure
        //focus
        //hauteur

        public int deviceIndex { get; set; }
        public int position { get; set; }
        public OpenCvSharp.Rect roi { get; set; }
    }
}
