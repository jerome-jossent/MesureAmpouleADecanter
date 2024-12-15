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

        Mat frame;
        Mat cercles_mat = new Mat();
        List<Cercle> cercles;
        bool circle_Recompute;

        public string _file
        {
            get => file;
            set
            {
                file = value;
                OnPropertyChanged();
                Settings.Default.file = file;
                Settings.Default.Save();
                if (MessageBox.Show("Réinitialiser les réglages de crop/roi ?", "Reset Crop ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    ResetROI();
                ProcessFile(_file);
            }
        }
        string file = Settings.Default.file;

        VideoCapture capVideo;
        int sleepTime;
        Thread videoThread;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public bool _play
        {
            get => play;
            set
            {
                play = value;
                OnPropertyChanged();
            }
        }
        bool play = false;

        public int _videoTotalFrames
        {
            get
            {
                if (capVideo != null)
                    return capVideo.FrameCount;
                return 1;
            }
        }

        public int _videoPositionFrame
        {
            get
            {
                if (capVideo != null)
                    return capVideo.PosFrames;
                return 1;
            }
            set
            {
                if (capVideo != null)
                    newWantedPosFrames = value;
            }
        }
        int? newWantedPosFrames;

        #region ROI Parameters
        bool roi_change;
        public double _roi_left
        {
            get => roi_left;
            set
            {
                roi_left = value;
                OnPropertyChanged();
                Settings.Default.roi_left = roi_left;
                Settings.Default.Save();
                roi_change = true;
            }
        }
        double roi_left = Settings.Default.roi_left;

        public double _roi_right
        {
            get => roi_right;
            set
            {
                roi_right = value;
                OnPropertyChanged();
                Settings.Default.roi_right = roi_right;
                Settings.Default.Save();
                roi_change = true;
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
                roi_change = true;
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
                roi_change = true;
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
                roi_change = true;
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
                roi_change = true;
            }
        }
        double roi_height_maximum;
        #endregion

        #region HoughCircle Parameters
        bool houghCircle_change;
        public double _houghcircle_dp
        {
            get => houghcircle_dp;
            set
            {
                houghcircle_dp = value;
                OnPropertyChanged();
                Settings.Default.houghcircle_dp = houghcircle_dp;
                Settings.Default.Save();
                houghCircle_change = true;
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
                houghCircle_change = true;
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
                houghCircle_change = true;
            }
        }
        double houghcircle_param2 = Settings.Default.houghcircle_param2;


        public int _houghcircle_radius_min
        {
            get => houghcircle_radius_min;
            set
            {
                houghcircle_radius_min = value;
                Settings.Default.houghcircle_radius_min = houghcircle_radius_min;
                Settings.Default.Save();
                OnPropertyChanged();
                houghCircle_change = true;
            }
        }
        int houghcircle_radius_min = Settings.Default.houghcircle_radius_min;

        public int _houghcircle_radius_max
        {
            get => houghcircle_radius_max;
            set
            {
                houghcircle_radius_max = value;
                Settings.Default.houghcircle_radius_max = houghcircle_radius_max;
                Settings.Default.Save();
                OnPropertyChanged();
                houghCircle_change = true;
            }
        }
        int houghcircle_radius_max = Settings.Default.houghcircle_radius_max;
        #endregion

        public int _rayon
        {
            get => rayon;
            set
            {
                rayon = value;
                Settings.Default.rayon = rayon;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        int rayon = Settings.Default.rayon;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayCrop(false);
            DisplayHoughCircle(false);
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

        void ResetROI()
        {
            _roi_left = 0;
            _roi_top = 0;
            _roi_right = 0;
            _roi_bottom = 0;
        }

        void ProcessFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return;

            VideoStop();

            capVideo = new VideoCapture(filePath);
            sleepTime = (int)Math.Round(1000 / capVideo.Fps);
            OnPropertyChanged("_videoTotalFrames");

            CancellationToken token = cancellationTokenSource.Token;
            Task.Factory.StartNew(() => PlayVideo(token), token);
        }

        void VideoStop()
        {
            capVideo?.Dispose();
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

            if (_roi_right == 0)
                _roi_right = _roi_width_maximum;
            if (_roi_top == 0)
                _roi_top = _roi_height_maximum;

            bool loop = true;
            while (loop)
            {
                //init
                capVideo.PosFrames = 0;
                CirclesReset();

                while (!capVideo.IsDisposed &&
                    !token.IsCancellationRequested)
                {
                    frame = new Mat();

                    //si goto frame number
                    if (newWantedPosFrames != null)
                    {
                        capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
                        newWantedPosFrames = null;
                    }

                    //read next frame
                    capVideo.Read(frame);
                    OnPropertyChanged("_videoPositionFrame");

                    //si arrivé à la fin
                    if (frame.Empty())
                        break;

                    Thread.Sleep(sleepTime);

                    if (roi_change || houghCircle_change)
                        CirclesReset();

                    ProcessFrame(frame);

                    //si pause
                    while (!_play)
                    {
                        Thread.Sleep(10);

                        if (newWantedPosFrames != null)
                        {
                            capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
                            newWantedPosFrames = null;
                            capVideo.Read(frame);
                            ProcessFrame(frame);
                        }

                        if (roi_change || houghCircle_change || circle_Recompute)
                        {
                            circle_Recompute = false;
                            CirclesReset();
                            ProcessFrame(frame);
                        }
                    }
                }
            }
        }

        Scalar CirclesReset_color = Scalar.Black;

        void CirclesReset()
        {
            int left = (int)_roi_left;// 20;
            int right = (int)(_roi_width_maximum - _roi_right);//150;
            int top = (int)(_roi_height_maximum - _roi_top);//1720;
            int bottom = (int)(_roi_bottom);//100;

            int w = capVideo.FrameWidth - left - right;
            int h = capVideo.FrameHeight - top - bottom;

            //Cercles = new Mat(new OpenCvSharp.Size(w, h), MatType.CV_8UC3, CirclesReset_color);
            cercles = new List<Cercle>();
        }

        void ProcessFrame(Mat frame)
        {
            Mat frame_copy = frame.Clone();
            roi_change = false;
            houghCircle_change = false;
            try
            {
                Mat R = ROI(frame_copy);
                Mat G = ToGray(R);

                CircleSegment[] circleSegments = Cv2.HoughCircles(G,
                    method: HoughModes.Gradient,
                    dp: _houghcircle_dp, //1.9 4k vertical
                    minDist: 1,    //1
                    param1: _houghcircle_param1,   //100
                    param2: _houghcircle_param2,   //37
                    minRadius: _houghcircle_radius_min,  //0
                    maxRadius: _houghcircle_radius_max); //20 

                CapteursAdd(circleSegments, R.Width, R.Height);

                CapteursUpdate(G.Width, G.Height);

                //dessine les cercles sur la frame
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

            }
        }

        void CapteursAdd(CircleSegment[] newCircleSegments, int width, int height)
        {
            if (cercles_mat.Empty())
                cercles_mat = new Mat(height, width, MatType.CV_8UC3);

            //pour chaque nouveau cercle
            for (int i = 0; i < newCircleSegments.Length; i++)
            {
                try
                {
                    Point2f center = newCircleSegments[i].Center;
                    int x = (int)center.X;
                    int y = (int)center.Y;

                    //est-ce que le point de mesure existe déjà ?
                    int j = 0;
                    bool found = false;
                    for (j = 0; j < cercles.Count; j++)
                    {
                        double d = Point2f.Distance(center, cercles[j].circleSegment.Center);
                        if (d < _rayon)
                        {
                            found = true;
                            break;
                        }
                    }
                    Cercle? c;
                    if (found)
                    {
                        c = cercles[j];

                        //moyenne position centre
                        float X_moy = (center.X + c.nbr_fois_centre_repere * c.circleSegment.Center.X) / (c.nbr_fois_centre_repere + 1);
                        float Y_moy = (center.Y + c.nbr_fois_centre_repere * c.circleSegment.Center.Y) / (c.nbr_fois_centre_repere + 1);
                        c.circleSegment.Center = new Point2f(X_moy, Y_moy);
                        c.nbr_fois_centre_repere++;
                    }
                    else
                    {
                        //new circle
                        c = new Cercle(newCircleSegments[i], cercles.Count);
                        cercles.Add(c);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        //void IsDejaPresent(Point2f center)
        //{
        //    int x = (int)center.X;
        //    int y = (int)center.Y;
        //    //read pixel
        //    //Vec3b pixelValue = Cercles.At<Vec3b>(y, x);

        //    ////différent de couleur par défaut du fond
        //    //bool isDejaPresent = !(pixelValue.Item0 == CirclesReset_color.Val0 &&
        //    //             pixelValue.Item1 == CirclesReset_color.Val1 &&
        //    //             pixelValue.Item2 == CirclesReset_color.Val2);

        //    //if (isDejaPresent)
        //    //{
        //    //quel point de mesure ?
        //    int i = 0;
        //    bool found = false;
        //    for (i = 0; i < cercles.Count; i++)
        //    {
        //        if (Point2f.Distance(center, cercles[i].circleSegment.Center) < _rayon)
        //        {
        //            found = true;
        //            break;
        //        }
        //    }
        //    if (!found)
        //    {

        //    }

        //    Cercle c = cercles[i];

        //    //moyenne position centre
        //    float X_moy = (center.X + c.nbr_fois_centre_repere * c.circleSegment.Center.X) / (c.nbr_fois_centre_repere + 1);
        //    float Y_moy = (center.Y + c.nbr_fois_centre_repere * c.circleSegment.Center.Y) / (c.nbr_fois_centre_repere + 1);
        //    c.circleSegment.Center = new Point2f(X_moy, Y_moy);
        //    c.nbr_fois_centre_repere++;
        //    //}
        //    //return isDejaPresent;
        //}

        void CapteursUpdate(int w, int h)
        {
            cercles_mat = new Mat(new OpenCvSharp.Size(w, h), MatType.CV_8UC3, CirclesReset_color);

            //Dessine tous les cercles
            foreach (Cercle c in cercles)
            {
                //dessin du disque
                Cv2.Circle(cercles_mat, (OpenCvSharp.Point)c.circleSegment.Center, _rayon, Cercle.couleur, -1);

                //écrit le numéro
                OpenCvSharp.Point xy = new OpenCvSharp.Point(c.circleSegment.Center.X, c.circleSegment.Center.Y);
                xy.Y += 5;
                string text = c.numero.ToString();
                if (text.Length == 1)
                    xy.X += -5;
                else if (text.Length == 2)
                    xy.X += -10;
                else
                    xy.X += -15;
                double zoom = 0.5;//0.5 quand r=15 ; 3 quand r=50
                Cv2.PutText(cercles_mat, text, xy,
                    HersheyFonts.HersheySimplex,
                    zoom, Cercle.couleurTexte, thickness: 1);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _image1 = Convert(cercles_mat.ToBitmap());
                }
                catch (Exception ex)
                {

                }
            }));
        }

        BitmapImage Convert(Bitmap src)
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

        void DisplayCrop(bool status)
        {
            if (status)
            {
                grid_column_crop.Width = new GridLength(1, GridUnitType.Auto);
                grid_row_crop.Height = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                grid_column_crop.Width = new GridLength(0);
                grid_row_crop.Height = new GridLength(0);
            }
        }

        void DisplayHoughCircle(bool status)
        {
            if (status)
                grid_row_houghcircle.Height = new GridLength(1, GridUnitType.Auto);
            else
                grid_row_houghcircle.Height = new GridLength(0);
        }

        #region Click (IHM)
        void Folder_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Video";
            dialog.DefaultExt = ".mp4";
            dialog.Filter = "Videos (.mp4)|*.mp4";
            if (File.Exists(_file))
                dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(_file);

            if (dialog.ShowDialog() == true)
                _file = dialog.FileName;
        }

        void btn_pause_Click(object sender, MouseButtonEventArgs e)
        {
            _play = true;
            PlayPauseButtonUpdate();
        }

        void btn_play_Click(object sender, MouseButtonEventArgs e)
        {
            _play = false;
            PlayPauseButtonUpdate();
        }

        void Crop_Click(object sender, MouseButtonEventArgs e)
        {
            if (grid_column_crop.Width == new GridLength(1, GridUnitType.Auto))
                DisplayCrop(false);
            else
                DisplayCrop(true);
        }

        void HoughCircle_Click(object sender, MouseButtonEventArgs e)
        {
            if (grid_row_houghcircle.Height == new GridLength(1, GridUnitType.Auto))
                DisplayHoughCircle(false);
            else
                DisplayHoughCircle(true);
        }

        void CirclesReset_Click(object sender, MouseButtonEventArgs e)
        {
            circle_Recompute = true;
            if (_play)
                CirclesReset();
        }

        void Image_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _play = !_play;
                PlayPauseButtonUpdate();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                CirclesReset();
                ProcessFrame(frame);
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {

            }
        }

        void PlayPauseButtonUpdate()
        {
            btn_pause.Visibility = (_play) ? Visibility.Hidden : Visibility.Visible;
        }
        #endregion
    }
}