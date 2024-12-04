using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesureAmpouleADecanter_Scanner
{
    public class Resolution_dpi
    {
        public int dpi;
        public int width;
        public int height;
        public int width_min;
        public int height_min;

        public Resolution_dpi(int dpi, int width, int height)
        {
            this.dpi = dpi;
            this.width = width;
            this.height = height;
            width_min = 8 * dpi / 100;
            height_min = 8 * dpi / 100;
            //minimum 16
            if (width_min < 16) width_min = 16;
            if (height_min < 16) height_min = 16;
        }
    }
}
