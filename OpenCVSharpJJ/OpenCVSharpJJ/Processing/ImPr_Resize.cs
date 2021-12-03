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
            Cv2.Resize(Input, Output, size, interpolation: interpolationType);
        }

        public override ListBoxItem ListBoxItem()
        {
            System.Windows.Controls.ListBoxItem lbi = new ListBoxItem();

            Label lbl_titre = new Label() { Content = imPrType.ToString(), FontWeight = System.Windows.FontWeights.Bold };
            Label lbl_1 = new Label() { Content = size.Width + "x" + size.Height };
            Label lbl_2 = new Label() { Content = interpolationType.ToString() };

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(lbl_titre);
            sp.Children.Add(lbl_1);
            sp.Children.Add(lbl_2);

            lbi.Content = sp;

            return lbi;
        }
    }
}
