using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenCVSharpJJ.Processing
{
    public class ImPr_Rotation : ImPr
    {
        public override ImPr_Converter.ImPrType imPrType => ImPr_Converter.ImPrType.Rotation;

        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public RotateFlags rotationType { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public ImPr_Rotation_UC ImPr_Rotation_UC;

        public ImPr_Rotation()
        {
            hasToBeBinary = false;
            hasToBeGray = false;
        }

        public ImPr_Rotation(RotateFlags rotationType) : this()
        {
            this.rotationType = rotationType;
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
                Cv2.Rotate(Input, Output, rotationType);

                //AFFICHE CHRONO
                long T1 = Cv2.GetTickCount();
                long T = (T1 - T0) * 1000;
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
            _string = rotationType.ToString();
        }

        public override UserControl UC()
        {
            if (ImPr_Rotation_UC == null)
            {
                ImPr_Rotation_UC = new ImPr_Rotation_UC();
                ImPr_Rotation_UC.Link(this);
            }
            return ImPr_Rotation_UC;
        }

        public override void Update_Debug(string txt, System.Windows.Media.SolidColorBrush color)
        {
            if (ImPr_Rotation_UC == null)
                return;

            ImPr_Rotation_UC._ImPr_Debug._debuginfo = txt;
            ImPr_Rotation_UC._ImPr_Debug._debugcolor = color;
        }
    }
}
