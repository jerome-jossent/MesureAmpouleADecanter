using OpenCvSharp.WpfExtensions;
using OpenCvSharp;
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
using System.Threading;

namespace MultiCam
{
    public partial class Capture_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ImageSource FrameImage
        {
            get { return frameImage; }
            set
            {
                frameImage = value;
                OnPropertyChanged();
            }
        }
        ImageSource frameImage;

        public string info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }
        string _info;

        CaptureArguments args;

        public Capture_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void _Link(CaptureArguments args)
        {
            this.args = args;
            new Thread(() => Thread_cap(args)).Start();
            info = args.index.ToString();
        }

        void Thread_cap(CaptureArguments arg)
        {
            var cap = arg.videoCapture;
            int index = arg.index;

            cap.Open(index, VideoCaptureAPIs.DSHOW);
            if (!cap.IsOpened())
            {
                return;
            }

            cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));
            cap.Set(VideoCaptureProperties.FrameWidth, 1920);
            cap.Set(VideoCaptureProperties.FrameHeight, 1080);
            cap.Set(VideoCaptureProperties.Fps, 30);
            cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));

            while (!arg.cts.IsCancellationRequested)
            {
                using (var frameMat = cap.RetrieveMat())
                {
                    //GC.Collect();
                    Mat m = frameMat.Clone();

                    DateTime t = DateTime.Now;
                    if (arg.t < t && m != null && !m.Empty())
                    {
                        arg.images_to_save.Add(arg.mainWindow.f + arg.position.ToString() + DateTime.Now.ToString("hh_mm_ss.fff") + ".jpg", m);
                        arg.t = t + TimeSpan.FromSeconds(1);
                    }

                    //ligne au centre
                    Cv2.Line(frameMat, 0, frameMat.Rows / 2, frameMat.Cols, frameMat.Rows / 2, arg.mainWindow.rouge, arg.mainWindow.epaisseur);

                    Dispatcher.Invoke(() =>
                    {
                        if (arg.position == CaptureArguments.Position.Haut) { arg.Update(frameMat.ToWriteableBitmap()); }
                        if (arg.position == CaptureArguments.Position.Milieu) { arg.Update(frameMat.ToWriteableBitmap()); }
                        if (arg.position == CaptureArguments.Position.Bas) { arg.Update(frameMat.ToWriteableBitmap()); }
                    });
                }
            }
        }


        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            CameraSettings.CAMERA_SETTINGS(args.ds_device);

        }
    }
}
