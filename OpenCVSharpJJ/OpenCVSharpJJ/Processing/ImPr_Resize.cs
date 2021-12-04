using System;
using System.Collections.Generic;
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
        public Size size { get; set; }

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

            try
            {
                Cv2.Resize(Input, Output, size, interpolation: interpolationType);
            }
            catch (Exception ex)
            {

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
    }
}
