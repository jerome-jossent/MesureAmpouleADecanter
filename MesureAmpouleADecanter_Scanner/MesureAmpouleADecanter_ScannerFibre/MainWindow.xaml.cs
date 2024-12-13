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

        string file = @"D:\DATA\decantation\videos\20241212_224353.mp4";

        VideoCapture capVideo;
        int sleepTime;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessFile(file);
        }

        void ProcessFile(string filePath)
        {
            capVideo = new VideoCapture(filePath);
            sleepTime = (int)Math.Round(1000 / capVideo.Fps);
            new Thread(PlayVideo).Start();
        }

        Mat ToGray(Mat src)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        Mat ROI(Mat src)
        {
            int left = 20;
            int top = 1720;
            int right = 150;
            int bottom = 100;           
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

        void PlayVideo()
        {
            int framestart = 1190;
            capVideo.PosFrames = framestart;

            while (!capVideo.IsDisposed)
            {
                Mat frame = new Mat();
                capVideo.Read(frame);
                if (frame.Empty())
                    break;
                //Thread.Sleep(sleepTime);
                Thread.Sleep(1);

                Mat R = ROI(frame);
                Mat G = ToGray(R);

                CircleSegment[] circleSegments = Cv2.HoughCircles(G, HoughModes.Gradient, 1.9, 1);

                CirclesAdd(circleSegments, R.Width, R.Height);

                for (int i = 0; i < circleSegments.Length; i++)
                {
                    Cv2.Circle(R, (OpenCvSharp.Point)circleSegments[i].Center, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), -1);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _image = Convert(R.ToBitmap());
                }));
            }
        }

        Mat Cercles = new Mat();
        void CirclesAdd(CircleSegment[] circleSegments, int width, int height)
        {
            if (Cercles.Empty())            
                Cercles = new Mat(height, width, MatType.CV_8UC3);

            for (int i = 0; i < circleSegments.Length; i++)
            {
                Cv2.Circle(Cercles, (OpenCvSharp.Point)circleSegments[i].Center, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), -1);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _image1 = Convert(Cercles.ToBitmap());
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