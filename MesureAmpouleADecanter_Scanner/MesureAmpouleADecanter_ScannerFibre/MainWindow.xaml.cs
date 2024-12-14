using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MesureAmpouleADecanter_ScannerFibre.Properties;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MesureAmpouleADecanter_ScannerFibre
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

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

        public ImageSource _image1
        {
            get => image1;
            set
            {
                image1 = value;
                OnPropertyChanged();
            }
        }
        ImageSource image1;

        #region ROI
        public double _roi_left
        {
            get => roi_left;
            set
            {
                roi_left = value;
                OnPropertyChanged();
                Settings.Default.roi_left = roi_left;
                Settings.Default.Save();
            }
        }
        double roi_left =Settings.Default.roi_left;

        public double _roi_right
        {
            get => roi_right;
            set
            {
                roi_right = value;
                OnPropertyChanged();
                Settings.Default.roi_right = roi_right;
                Settings.Default.Save();
            }
        }
        double roi_right = Settings.Default.roi_right;

        public double _roi_width_maximum
        {
            get => roi_width_maximum;
            set
            {
                roi_width_maximum = value;
                OnPropertyChanged();
            }
        }
        double roi_width_maximum;


        public double _roi_top
        {
            get => roi_top;
            set
            {
                roi_top = value;
                OnPropertyChanged();
                Settings.Default.roi_top = roi_top;
                Settings.Default.Save();
            }
        }
        double roi_top = Settings.Default.roi_top;

        public double _roi_bottom
        {
            get => roi_bottom;
            set
            {
                roi_bottom = value;
                OnPropertyChanged();
                Settings.Default.roi_bottom = roi_bottom;
                Settings.Default.Save();
            }
        }
        double roi_bottom = Settings.Default.roi_bottom;

        public double _roi_height_maximum
        {
            get => roi_height_maximum;
            set
            {
                roi_height_maximum = value;
                OnPropertyChanged();
            }
        }
        double roi_height_maximum;
        #endregion

        public double _houghcircle_dp
        {
            get => houghcircle_dp;
            set
            {
                houghcircle_dp = value;
                OnPropertyChanged();
                Settings.Default.houghcircle_dp = houghcircle_dp;
                Settings.Default.Save();
            }
        }
        double houghcircle_dp = Settings.Default.houghcircle_dp;

        public double _houghcircle_param1
        {
            get => houghcircle_param1;
            set
            {
                houghcircle_param1 = value;
                Settings.Default.houghcircle_param1 = houghcircle_param1;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        double houghcircle_param1 = Settings.Default.houghcircle_param1;

        public double _houghcircle_param2
        {
            get => houghcircle_param2;
            set
            {
                houghcircle_param2 = value;
                Settings.Default.houghcircle_param2 = houghcircle_param2;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        double houghcircle_param2 = Settings.Default.houghcircle_param2;


        //string file = @"D:\DATA\decantation\videos\20241212_224353.mp4";
        string file = @"D:\DATA\decantation\videos\2024-12-13 22-22-30.mp4";

        VideoCapture capVideo;
        int sleepTime;
        Thread videoThread;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessFile(file);
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            StopAll();
        }

        void StopAll()
        {
            cancellationTokenSource.Cancel();
        }

        void ProcessFile(string filePath)
        {
            capVideo = new VideoCapture(filePath);
            sleepTime = (int)Math.Round(1000 / capVideo.Fps);

            CancellationToken token = cancellationTokenSource.Token;
            Task.Factory.StartNew(() => PlayVideo(token), token);
        }

        Mat ToGray(Mat src)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        Mat ROI(Mat src)
        {
            int left = (int)_roi_left;// 20;
            int right = (int)(_roi_width_maximum - _roi_right);//150;
            int top = (int)(_roi_height_maximum - _roi_top);//1720;
            int bottom = (int)(_roi_bottom);//100;

            Dispatcher.BeginInvoke(() =>
            {
                //Title = left + " ↔ " + right + "\t" + top + " ↨ " + bottom;
                Title = _houghcircle_dp.ToString("0.00") + "\t" + _houghcircle_param1.ToString("0.00") + "\t" + _houghcircle_param2.ToString("0.00");
            });

            return ROI(src, left, top, right, bottom);
        }

        Mat ROI(Mat src, int left, int top, int right, int bottom)
        {
            int w = src.Width - left - right;
            int h = src.Height - top - bottom;
            OpenCvSharp.Rect roi_rect = new OpenCvSharp.Rect(left, top, w, h);

            Mat roi_frame = new Mat(src, roi_rect);
            return roi_frame;
        }

        Mat Resize(Mat src, float zoom)
        {
            Mat res = new Mat();
            int nh = (int)(src.Height * zoom);
            int nw = (int)(src.Width * zoom);
            Cv2.Resize(src, res, new OpenCvSharp.Size(nh, nw), interpolation: InterpolationFlags.Cubic);
            return res;
        }


        void PlayVideo(object? obj)
        {
            CancellationToken token = (CancellationToken)obj;
            int framestart = 1190;
            capVideo.PosFrames = framestart;

            _roi_width_maximum = capVideo.FrameWidth;
            _roi_height_maximum = capVideo.FrameHeight;

            _roi_right = _roi_width_maximum;
            _roi_top = _roi_height_maximum;

            bool loop = true;
            while (loop)
            {
                capVideo.PosFrames = 0;
                Cercles = new Mat();
                while (!capVideo.IsDisposed && !token.IsCancellationRequested)
                {
                    Mat frame = new Mat();
                    capVideo.Read(frame);
                    if (frame.Empty())
                        break;
                    //Thread.Sleep(sleepTime);
                    Thread.Sleep(1);
                    try
                    {
                        Mat R = ROI(frame);
                        Mat G = ToGray(R);

                        CircleSegment[] circleSegments = Cv2.HoughCircles(G,
                            method: HoughModes.Gradient,
                            dp: _houghcircle_dp, //1.9 4k vertical
                            minDist: 1,    //1
                            param1: _houghcircle_param1,   //100
                            param2: _houghcircle_param2,   //37
                            minRadius: 0,  //0
                            maxRadius: 20); //20 

                        CirclesAdd(circleSegments, R.Width, R.Height);

                        for (int i = 0; i < circleSegments.Length; i++)
                            Cv2.Circle(R, (OpenCvSharp.Point)circleSegments[i].Center, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), 1);

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                _image = Convert(R.ToBitmap());
                            }
                            catch (Exception ex)
                            {

                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }
        }

        Mat Cercles = new Mat();
        void CirclesAdd(CircleSegment[] circleSegments, int width, int height)
        {
            if (Cercles.Empty())
                Cercles = new Mat(height, width, MatType.CV_8UC3);

            for (int i = 0; i < circleSegments.Length; i++)
                Cv2.Circle(Cercles, (OpenCvSharp.Point)circleSegments[i].Center, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), -1);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _image1 = Convert(Cercles.ToBitmap());
                }
                catch (Exception ex)
                {

                }
            }));
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

    }
}