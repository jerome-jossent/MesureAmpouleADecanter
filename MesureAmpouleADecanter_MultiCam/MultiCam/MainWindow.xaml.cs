using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace MultiCam
{
    public partial class MainWindow : System.Windows.Window
    {
        readonly VideoCapture cap1;
        readonly BackgroundWorker bkgWorker1;

        readonly VideoCapture cap2;
        readonly BackgroundWorker bkgWorker2;

        readonly VideoCapture cap3;
        readonly BackgroundWorker bkgWorker3;

        readonly VideoCapture cap4;
        readonly BackgroundWorker bkgWorker4;

        public MainWindow()
        {
            InitializeComponent();

            cap1 = new VideoCapture();
            bkgWorker1 = new BackgroundWorker { WorkerSupportsCancellation = true };
            bkgWorker1.DoWork += Worker_DoWork1;

            cap2 = new VideoCapture();
            bkgWorker2 = new BackgroundWorker { WorkerSupportsCancellation = true };
            bkgWorker2.DoWork += Worker_DoWork2;

            cap3 = new VideoCapture();
            bkgWorker3 = new BackgroundWorker { WorkerSupportsCancellation = true };
            bkgWorker3.DoWork += Worker_DoWork3;

            cap4 = new VideoCapture();
            bkgWorker4 = new BackgroundWorker { WorkerSupportsCancellation = true };
            bkgWorker4.DoWork += Worker_DoWork4;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            bkgWorker1.RunWorkerAsync(0);
            bkgWorker2.RunWorkerAsync(1);
            bkgWorker3.RunWorkerAsync(2);
            bkgWorker4.RunWorkerAsync(3);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            bkgWorker1.CancelAsync();
            cap1.Dispose();

            bkgWorker2.CancelAsync();
            cap2.Dispose();

            bkgWorker3.CancelAsync();
            cap3.Dispose();

            bkgWorker4.CancelAsync();
            cap4.Dispose();
        }

        void Worker_DoWork1(object sender, DoWorkEventArgs e)
        {
            int index = (int)(e as object);
            cap1.Open(index, VideoCaptureAPIs.ANY);
            if (!cap1.IsOpened())
            {
                Close();
                return;
            }
            var worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                using (var frameMat = cap1.RetrieveMat())
                {

                    // Must create and use WriteableBitmap in the same thread(UI Thread).
                    Dispatcher.Invoke(() =>
                    {
                        FrameImage1.Source = frameMat.ToWriteableBitmap();
                    });
                }

                Thread.Sleep(30);
            }
        }

        void Worker_DoWork2(object sender, DoWorkEventArgs e)
        {
            cap2.Open(1, VideoCaptureAPIs.ANY);
            if (!cap2.IsOpened())
            {
                Close();
                return;
            }
            var worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                using (var frameMat = cap2.RetrieveMat())
                    Dispatcher.Invoke(() => FrameImage2.Source = frameMat.ToWriteableBitmap());
                Thread.Sleep(30);
            }
        }

        void Worker_DoWork3(object sender, DoWorkEventArgs e)
        {
            cap3.Open(2, VideoCaptureAPIs.ANY);
            if (!cap3.IsOpened())
            {
                Close();
                return;
            }
            var worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                using (var frameMat = cap3.RetrieveMat())
                    Dispatcher.Invoke(() => FrameImage3.Source = frameMat.ToWriteableBitmap());
                Thread.Sleep(30);
            }
        }

        void Worker_DoWork4(object sender, DoWorkEventArgs e)
        {
            cap4.Open(3, VideoCaptureAPIs.ANY);
            if (!cap4.IsOpened())
            {
                Close();
                return;
            }
            var worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                using (var frameMat = cap4.RetrieveMat())
                    Dispatcher.Invoke(() => FrameImage4.Source = frameMat.ToWriteableBitmap());
                Thread.Sleep(30);
            }
        }
    }
}
