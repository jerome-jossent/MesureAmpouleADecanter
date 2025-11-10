using OpenCvSharp;
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
    /// <summary>
    /// Logique d'interaction pour ROI_UC.xaml
    /// </summary>
    public partial class ROI_UC : UserControl, INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public int _circlesCount { get => circlesCount; set { circlesCount = value; OnPropertyChanged(); } }
        int circlesCount;

        public ImageSource _image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }
        ImageSource image;

        public ImageSource _sensormap
        {
            get => sensormap;
            set
            {
                sensormap = value;
                OnPropertyChanged();
            }
        }
        ImageSource sensormap;


        public OpenCvSharp.Rect _roi;
        internal string _name => _roi.Width + "x" + _roi.Height + " (" + _roi.TopLeft + ")";

        public ROI_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ROI_UC(OpenCvSharp.Rect roi)
        {
            InitializeComponent();
            DataContext = this;

            _roi = roi;
        }

        internal void Show(Mat frame)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var temp = frame.ToWriteableBitmap();
                    _image = temp;
                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            }));
        }

        internal void Show_sensormap(Mat frame)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _sensormap = frame.ToWriteableBitmap();
                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            }));
        }

    }
}
