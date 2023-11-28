using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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

using OpenCvSharp;
using static System.Net.Mime.MediaTypeNames;

namespace Tools_ImageProcessing
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string titre
        {
            get => _titre;
            set
            {
                _titre = value;
                OnPropertyChanged();
            }
        }
        string _titre;
        public string TITRE = "OpenCV Processing";

        OpenCvSharp.Rect roi;
        List<FileInfo> sources;
        FileInfo currentSource;
        Mat currentFrame;

        BackgroundSubtractorMOG2 mog2;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Process_ROI_Click(object sender, RoutedEventArgs e) { Process_ROI(); }
        void Process_MotionDetection_Click(object sender, RoutedEventArgs e) { Process_MotionDetection(); }
        void Make_Video_Click(object sender, RoutedEventArgs e) { Process_MakeVideo(); }

        #region COMMON
        bool GetSources(string pathSources)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(pathSources);
            if (directoryInfo.Exists)
            {
                sources = directoryInfo.GetFiles().ToList();
                return true;
            }
            MessageBox.Show(pathSources + "\nn'existe pas.");
            return false;
        }

        bool SetCurrent(int index)
        {
            if (sources.Count == 0) return false;
            currentSource = sources[index];
            currentFrame = new Mat(currentSource.FullName);
            return true;
        }
        #endregion

        #region ROI
        void Process_ROI()
        {
            if (!GetSources(tbx_source.Text)) return;
            if (!SetCurrent(0)) return;
            if (!ROI_Set(currentFrame)) return;
            ROI(sources, tbx_destination.Text, roi);
            titre = TITRE;
            MessageBox.Show("Terminé");
        }

        bool ROI_Set(Mat frame)
        {
            if (frame.Empty())
                return false;

            string window_name = "Valid ROI with 'Enter' or 'Space', cancel with 'c'";

            OpenCvSharp.Rect newroi;
            newroi = Cv2.SelectROI(window_name, frame, true);
            Cv2.DestroyWindow(window_name);
            if (newroi.Width == 0 || newroi.Height == 0) return false;

            roi = newroi;
            return true;
        }

        void ROI(List<FileInfo> sources, string pathDestination, OpenCvSharp.Rect roi)
        {
            Directory.CreateDirectory(pathDestination);

            for (int i = 0; i < sources.Count; i++)
            {
                titre = TITRE + $" {i + 1} / {sources.Count + 1}";
                SetCurrent(i);
                Mat cropped = ROI(currentFrame, roi);
                string path = pathDestination + "\\" + currentSource.Name;
                cropped.SaveImage(path);
            }
        }

        Mat ROI(Mat frame, OpenCvSharp.Rect roi)
        {
            return new Mat(frame, roi);
        }
        #endregion

        #region MOTION DETECTION
        void Process_MotionDetection()
        {
            mog2 = BackgroundSubtractorMOG2.Create();
            if (!GetSources(tbx_MotionDetection_source.Text)) return;
            //if (!SetCurrent(0)) return;
            //MotionDetection(sources, tbx_MotionDetection_destination.Text, double.Parse(tbx_learningRate.Text.Replace(".", ",")));
            MotionDetection(sources, tbx_MotionDetection_destination.Text, tbx_MotionDetection_data.Text);
            titre = TITRE;
            MessageBox.Show("Terminé");
            mog2.Dispose();
        }

        void MotionDetection(List<FileInfo> sources, string pathDestination, string pathDATADestination)
        {
            Directory.CreateDirectory(pathDestination);
            FileInfo datafile = new FileInfo(pathDATADestination);
            string nom = datafile.Name.Replace(datafile.Extension, "");


            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < sources.Count; i++)
            {
                titre = TITRE + $" {i + 1} / {sources.Count + 1}";
                SetCurrent(i);
                Mat motionDetection = MotionDetection(currentFrame);
                double y = Find1HorizontalLine(motionDetection);

                string t = currentSource.Name;
                t = t.Replace(currentSource.Extension, "");
                t = t.Replace(nom, "");
                t = t.Replace(".", ",");
                t = t.Replace("_", ":");

                if (y == -1)
                    stringBuilder.AppendLine(t + ";" + "#N/A");
                else
                    stringBuilder.AppendLine(t + ";" + (1 - y));

                DrawHorizontalLine(y, motionDetection);
                string path = pathDestination + "\\" + currentSource.Name;
                motionDetection.SaveImage(path);
            }
            File.WriteAllText(pathDATADestination, stringBuilder.ToString());
        }


        Mat MotionDetection(Mat mInput)
        {
            Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            Mat fgMaskMOG2 = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            Mat temp = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);

            mog2.Apply(mInput, fgMaskMOG2);
            Cv2.CvtColor(fgMaskMOG2, temp, ColorConversionCodes.GRAY2BGRA);

            using Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.Erode(temp, temp, element);
            temp.CopyTo(mOutput);

            return mOutput;
        }

        /// <summary>
        /// en pourcentage de la hauteur de l'image et en partant du haut (O% : première ligne)
        /// </summary>
        /// <param name="imageWithLines"></param>
        /// <returns></returns>
        double Find1HorizontalLine(Mat imageWithLines)
        {
            Mat imageWithLines_gray = new Mat();
            Cv2.CvtColor(imageWithLines, imageWithLines_gray, ColorConversionCodes.BGR2GRAY);
            Mat imageWithLines_out = imageWithLines.Clone();

            LineSegmentPoint[] lines = Cv2.HoughLinesP(imageWithLines_gray, 1, Cv2.PI / 180, threshold: 250);
            //foreach (var line in lines)            
            //    Cv2.Line(imageWithLines_out, line.P1, line.P2, Scalar.Red, thickness: 2);

            //filtre médian
            List<double> ys = new List<double>();
            foreach (var line in lines)
                ys.Add((line.P1.Y + line.P2.Y) / 2.0);

            if (ys.Count > 0)
            {
                ys.Sort();
                double y = ys[ys.Count / 2] / (double)imageWithLines.Height;
                return y;
                OpenCvSharp.Point P1 = new OpenCvSharp.Point(0, y);
                OpenCvSharp.Point P2 = new OpenCvSharp.Point(imageWithLines_out.Width - 1, y);
                Cv2.Line(imageWithLines_out, P1, P2, Scalar.Red, thickness: 2);
                //return imageWithLines_out;
            }
            else
                return -1;

        }

        void DrawHorizontalLine(double y, Mat frame)
        {
            int h = (int)(y * frame.Height);
            OpenCvSharp.Point P1 = new OpenCvSharp.Point(0, h);
            OpenCvSharp.Point P2 = new OpenCvSharp.Point(frame.Width - 1, h);
            Cv2.Line(frame, P1, P2, Scalar.Red, thickness: 1);
        }
        #endregion

        #region VIDEO
        void Process_MakeVideo()
        {
            if (!GetSources(tbx_video_source.Text)) return;
            if (!SetCurrent(0)) return;
            MakeVideo(tbx_video_destination.Text, double.Parse(tbx_video_fps.Text));
            titre = TITRE;
            MessageBox.Show("Terminé");
        }

        void MakeVideo(string outputVideoPath, double fps)
        {
            int width = currentFrame.Width;
            int height = currentFrame.Height;

            VideoWriter writer = new VideoWriter(outputVideoPath, FourCC.XVID, fps, new OpenCvSharp.Size(width, height));
            for (int i = 0; i < sources.Count; i++)
            {
                FileInfo imagePath = sources[i];
                titre = TITRE + $" {i + 1} / {sources.Count + 1}";
                Mat frame = Cv2.ImRead(imagePath.FullName);

                if (!frame.Empty())
                    writer.Write(frame);
            }
            writer.Release();
        }
        #endregion              
    }
}
