using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class Scan_UC : UserControl, INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;



        public OpenCvSharp.Mat _mat;
        public DateTime _t;
        public TimeSpan _deltatT;

        public BitmapSource _bitmapSource
        {
            get
            {
                if (_mat == null || _mat.Empty() || _mat.Width == 0 || _mat.Height == 0)
                    return null;
                return _mat.ToBitmapSource();
            }
        }
        public string _name { get; set; }

        public Scan_UC(OpenCvSharp.Mat scan_mat, DateTime t, TimeSpan deltatT)
        {
            InitializeComponent();
            DataContext = this;
            _mat = scan_mat;
            //bitmapSource.Freeze();
            _t= t;
            _deltatT = deltatT;
            _name = deltatT.TotalSeconds.ToString("f1");

        }

        public void _Update(OpenCvSharp.Mat mat)
        {
            _mat = mat;
            OnPropertyChanged(nameof(_bitmapSource));
        }
    }
}
