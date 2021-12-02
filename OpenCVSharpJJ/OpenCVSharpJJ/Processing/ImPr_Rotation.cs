using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCVSharpJJ.Processing
{
    public class ImPr_Rotation : ImPr
    {
        public override ImPr_Converter.ImPrType imPrType => ImPr_Converter.ImPrType.Rotation;

        RotateFlags rotationType;

        public ImPr_Rotation(RotateFlags rotationType)
        {
            this.rotationType = rotationType;
            hasToBeBinary = false;
            hasToBeGray = false;
        }

        public ImPr_Rotation()
        {
            hasToBeBinary = false;
            hasToBeGray = false;
        }

        public override void Process()
        {
            Cv2.Rotate(Input, Output, rotationType);
        }
    }
}
