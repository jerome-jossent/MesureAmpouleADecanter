using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using OpenCvSharp.WpfExtensions;
using System.Runtime.CompilerServices;

namespace MultiCam
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        //IDEES :
        //
        // un UC par capture
        // afficher par UC les réglages caméras
        // définir la position du capteur(haut, milieu, bas) : permutation des caméras
        // ROI
        // Binarisation (rgb => gris => bw threshold)
        // 2D => 1D (somme)
        // detection du front, 2 sens ("sens par le haut", "sens par le bas")
        // enregistrement bouton Record/Stop + création d'un dossier

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //Token d'arrêt des threads
        CancellationTokenSource cts = new CancellationTokenSource();
        VideoCapture cap1;
        VideoCapture cap2;
        VideoCapture cap3;

        ////images IHM
        //public ImageSource FrameImage1
        //{
        //    get { return frameImage1; }
        //    set
        //    {
        //        frameImage1 = value;
        //        OnPropertyChanged();
        //    }
        //}
        //ImageSource frameImage1;

        //public ImageSource FrameImage2
        //{
        //    get => frameImage2;
        //    set
        //    {
        //        frameImage2 = value;
        //        OnPropertyChanged();
        //    }
        //}
        //ImageSource frameImage2;

        //public ImageSource FrameImage3
        //{
        //    get => frameImage3;
        //    set
        //    {
        //        frameImage3 = value;
        //        OnPropertyChanged();
        //    }
        //}
        //ImageSource frameImage3;

        public bool? toSave
        {
            get => _toSave;
            set
            {
                _toSave = value;
                if (value == true)
                {
                    f = _f + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + "\\";
                    System.IO.Directory.CreateDirectory(f);
                }
                OnPropertyChanged();
            }
        }
        bool? _toSave = false;


        Dictionary<string, OpenCvSharp.Mat> images = new Dictionary<string, Mat>();
        //Dictionary<int, DateTime> nextrecord = new Dictionary<int, DateTime>();

        public int epaisseur
        {
            get => _epaisseur;
            set
            {
                _epaisseur = value;
                OnPropertyChanged();

            }
        }
        int _epaisseur = 5;


        string _f = @"C:\_JJ\DATA\decantation\multicam\";
        public string f;
        public Scalar rouge = new Scalar(0, 0, 255);

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }


        void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            cap1 = new VideoCapture();
            cap2 = new VideoCapture();
            cap3 = new VideoCapture();

            new Thread(ThreadSave).Start();

            List<CaptureArguments> args = new List<CaptureArguments>();
            args.Add(new CaptureArguments(videoCapture: cap1, index: 0, CaptureArguments.Position.Bas, CAPTURE1, cts, images, this));
            args.Add(new CaptureArguments(videoCapture: cap2, index: 2, CaptureArguments.Position.Haut, CAPTURE2, cts, images, this));
            args.Add(new CaptureArguments(videoCapture: cap3, index: 3, CaptureArguments.Position.Milieu, CAPTURE3, cts, images, this));

            foreach (CaptureArguments arg in args)
            {
                arg.capture_UC._Link(arg);
                //new Thread(() => Thread_cap(arg)).Start();
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            cts.Cancel();
            Thread.Sleep(500);
            try { cap1?.Dispose(); } catch (Exception) { }
            try { cap2?.Dispose(); } catch (Exception) { }
            try { cap3?.Dispose(); } catch (Exception) { }
        }


        //void Thread_cap(CaptureArguments arg)
        //{
        //    var cap = arg.videoCapture;
        //    int index = arg.index;

        //    cap.Open(index, VideoCaptureAPIs.DSHOW);
        //    if (!cap.IsOpened())
        //    {
        //        Close();
        //        return;
        //    }

        //    cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));
        //    cap.Set(VideoCaptureProperties.FrameWidth, 1920);
        //    cap.Set(VideoCaptureProperties.FrameHeight, 1080);
        //    cap.Set(VideoCaptureProperties.Fps, 30);
        //    cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));

        //    while (!cts.IsCancellationRequested)
        //    {
        //        using (var frameMat = cap.RetrieveMat())
        //        {
        //            //GC.Collect();
        //            Mat m = frameMat.Clone();

        //            DateTime t = DateTime.Now;
        //            if (arg.t < t && m != null && !m.Empty())
        //            {
        //                images.Add(f + arg.position.ToString() + DateTime.Now.ToString("hh_mm_ss.fff") + ".jpg", m);
        //                arg.t = t + TimeSpan.FromSeconds(1);
        //            }

        //            //ligne au centre
        //            Cv2.Line(frameMat, 0, frameMat.Rows / 2, frameMat.Cols, frameMat.Rows / 2, rouge, epaisseur);

        //            Dispatcher.Invoke(() =>
        //            {
        //                if (arg.position == CaptureArguments.Position.Haut) { arg.Update(frameMat.ToWriteableBitmap()); }
        //                if (arg.position == CaptureArguments.Position.Milieu) { arg.Update(frameMat.ToWriteableBitmap()); }
        //                if (arg.position == CaptureArguments.Position.Bas) { arg.Update(frameMat.ToWriteableBitmap()); }
        //            });
        //        }
        //    }
        //}

        int error = 0;

        void ThreadSave()
        {

            while (!cts.IsCancellationRequested)
            {
                if (toSave == true)
                {
                    while (images.Count > 0)
                    {
                        try
                        {
                            var image = images.First();
                            image.Value.SaveImage(image.Key);
                            images.Remove(image.Key);
                        }
                        catch (Exception ex)
                        {
                            error++;
                            Dispatcher.Invoke(() => { Title = "[" + error + "] : " + ex.Message; });
                        }
                    }
                }
                else
                {
                    images.Clear();
                }

                Thread.Sleep(10);
            }

        }

    }
}
