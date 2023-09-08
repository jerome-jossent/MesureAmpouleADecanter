using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using OpenCvSharp;

namespace OpenCVSharpJJ.Processing
{
    public class ImPr_Resize : ImPr
    {
        public override ImPr_Converter.ImPrType imPrType => ImPr_Converter.ImPrType.Resize;
        public OpenCvSharp.Size size { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public InterpolationFlags interpolationType { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public ImPr_Resize_UC ImPr_Resize_UC;

        public ImPr_Resize()
        {
            hasToBeBinary = false;
            hasToBeGray = false;
        }

        public ImPr_Resize(OpenCvSharp.Size size, InterpolationFlags interpolationType = InterpolationFlags.Linear) : this()
        {
            this.size = size;
            this.interpolationType = interpolationType;
        }

        public override void Process()
        {
            if (!_actived)
            {
                Output = Input;
                return;
            }
            if (Output == null)
                Output = new Mat();

            //CHRONO
            long T0 = Cv2.GetTickCount();
            try
            {
                Cv2.Resize(Input, Output, size, interpolation: interpolationType);

                //AFFICHE CHRONO
                long T1 = Cv2.GetTickCount();
                long T = (T1 - T0)*1000;
                Update_Debug((T / Cv2.GetTickFrequency()).ToString("F1") + "ms", black);
            }
            catch (Exception ex)
            {
                //AFFICHE STACKTRACE
                Update_Debug(ex.Message, red);
            }
        }

        public override void Update_string()
        {
            _string = size.Width + "x" + size.Height + 
                " [" + interpolationType.ToString() + "]";
        }

        public override UserControl UC()
        {
            if (ImPr_Resize_UC == null)
            {
                ImPr_Resize_UC = new ImPr_Resize_UC();
                ImPr_Resize_UC.Link(this);
            }
            return ImPr_Resize_UC;
        }

        public override void Update_Debug(string txt, System.Windows.Media.SolidColorBrush color)
        {
            if (ImPr_Resize_UC == null) 
                return;
            ImPr_Resize_UC._ImPr_Debug._debuginfo = txt;
            ImPr_Resize_UC._ImPr_Debug._debugcolor = color;
        }
    }
}
