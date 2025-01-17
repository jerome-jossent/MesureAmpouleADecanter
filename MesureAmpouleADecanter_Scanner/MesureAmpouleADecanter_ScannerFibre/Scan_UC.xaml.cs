using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public partial class Scan_UC : UserControl
    {
        public OpenCvSharp.Mat mat;
        public BitmapSource bitmapSource { get => mat.ToBitmapSource(); }
        public string name { get; set; }

        public Scan_UC(OpenCvSharp.Mat scan_mat, string name)
        {
            InitializeComponent();
            DataContext = this;
            mat = scan_mat;
            //bitmapSource.Freeze();
            this.name = name;
        }
    }
}
