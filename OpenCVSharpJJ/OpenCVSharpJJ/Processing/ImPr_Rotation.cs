using OpenCvSharp;
using System;
using System.Collections.Generic;
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
            if (Output == null) Output = new Mat();
            Cv2.Rotate(Input, Output, rotationType);
        }

        public override ListBoxItem ListBoxItem()
        {
            System.Windows.Controls.ListBoxItem lbi = new ListBoxItem();

            Label lbl_titre = new Label() { Content = imPrType.ToString(), FontWeight = System.Windows.FontWeights.Bold };
            Label lbl_1 = new Label() { Content = rotationType.ToString() };

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(lbl_titre);
            sp.Children.Add(lbl_1);

            lbi.Content = sp;

            return lbi;
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
    }
}
