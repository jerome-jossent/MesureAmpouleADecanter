using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using System.Windows.Shapes;
using Communication_Série;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace OpenCVSharpJJ
{
    public partial class MesureAmpouleADecanter : System.Windows.Window, INotifyPropertyChanged
    {
        #region BINDINGS IHM
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string _title
        {
            get { return title + _fps; }
            set
            {
                title = value;
                OnPropertyChanged("_title");
            }
        }
        string title = "Mesure ampoule à décanter";

        public string _fps
        {
            get { return fps; }
            set
            {
                fps = value;
                OnPropertyChanged("_title");
            }
        }
        string fps;

        public int Threshold1
        {
            get { return threshold1; }
            set
            {
                threshold1 = value;
                OnPropertyChanged("Threshold1");
            }
        }
        int threshold1 = 127;
        public int Threshold2
        {
            get { return threshold2; }
            set
            {
                threshold2 = value;
                OnPropertyChanged("Threshold2");
            }
        }
        int threshold2 = 127;

        public System.Drawing.Bitmap _imageSource
        {
            get
            {
                if (imageSource == null)
                    return null;
                return imageSource;
            }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged("_imageSource");
                }
            }
        }
        System.Drawing.Bitmap imageSource;

        public System.Drawing.Bitmap _imageCalque
        {
            get
            {
                if (imageCalque == null)
                    return null;
                return imageCalque;
            }
            set
            {
                if (imageCalque != value)
                {
                    imageCalque = value;
                    OnPropertyChanged("_imageCalque");
                }
            }
        }
        System.Drawing.Bitmap imageCalque;
        public System.Drawing.Bitmap _image1
        {
            get
            {
                if (image1 == null)
                    return null;
                return image1;
            }
            set
            {
                if (image1 != value)
                {
                    image1 = value;
                    OnPropertyChanged("_image1");
                }
            }
        }
        System.Drawing.Bitmap image1;

        public System.Drawing.Bitmap _image2
        {
            get
            {
                if (image2 == null)
                    return null;
                return image2;
            }
            set
            {
                if (image2 != value)
                {
                    image2 = value;
                    OnPropertyChanged("_image2");
                }
            }
        }
        System.Drawing.Bitmap image2;

        public System.Drawing.Bitmap _image3
        {
            get
            {
                if (image3 == null)
                    return null;
                return image3;
            }
            set
            {
                if (image3 != value)
                {
                    image3 = value;
                    OnPropertyChanged("_image3");
                }
            }
        }
        System.Drawing.Bitmap image3;

        public ObservableCollection<string> ArduinoMessages
        {
            get
            {
                return arduinoMessages;
            }
            set
            {
                arduinoMessages = value;
                OnPropertyChanged("ArduinoMessages");
            }
        }
        ObservableCollection<string> arduinoMessages = new ObservableCollection<string>();
        #endregion

        #region ENUMERATIONS
        public enum ImageType
        {
            original,
            rotated,
            roi1, roi2,
            gray1, gray2,
            bw1, bw2,
            canny,
            bgr1, bgr2,
            debug1, debug2, debug3, debug4, debug5
        }

        public enum ProcessingType
        {
            process1, process2, process3, process4,
        }
        #endregion

        #region CLASSES
        public class NamedMat
        {
            public Mat mat = new Mat();
            public ImageType imageType;
            public NamedMat(ImageType imageType)
            {
                this.imageType = imageType;
            }
        }

        public class NamedMats
        {
            public Dictionary<ImageType, NamedMat> MatNamesToMats = new Dictionary<ImageType, NamedMat>();
            internal NamedMat Get(ImageType imageType)
            {
                if (!MatNamesToMats.ContainsKey(imageType))
                    MatNamesToMats.Add(imageType, new NamedMat(imageType));
                return MatNamesToMats[imageType];
            }
        }
        #endregion

        #region PARAMETERS
        ProcessingType processingType;

        long T0;
        NamedMats NMs;
        NamedMat frame = new NamedMat(ImageType.original);
        NamedMat rotated = new NamedMat(ImageType.rotated);
        NamedMat frameGray = new NamedMat(ImageType.gray1);
        NamedMat cannymat = new NamedMat(ImageType.canny);
        NamedMat BGR = new NamedMat(ImageType.bgr1);
        NamedMat ROI1 = new NamedMat(ImageType.roi1);
        NamedMat ROI2 = new NamedMat(ImageType.roi2);
        NamedMat debug1 = new NamedMat(ImageType.debug1);
        NamedMat debug2 = new NamedMat(ImageType.debug2);
        NamedMat debug3 = new NamedMat(ImageType.debug3);
        NamedMat debug4 = new NamedMat(ImageType.debug4);
        NamedMat bw1 = new NamedMat(ImageType.bw1);
        NamedMat bw2 = new NamedMat(ImageType.bw2);
        Mat[] bgr;

        Thread thread;
        int indexDevice;
        VideoInInfo.Format format;
        Dictionary<string, VideoInInfo.Format> formats;
        VideoCapture capture;
        bool isRunning = false;
        bool first = true;
        System.Diagnostics.Stopwatch chrono = new System.Diagnostics.Stopwatch();

        VideoWriter videoWriter;
        OpenCvSharp.Rect roi;

        Communication_Série.Communication_Série cs;
        string buffer;
        char[] split_car = new char[] { '\n' };
        float camera_pos;
        float camera_pos_max;

        bool camera_pos_low_switch, camera_pos_high_switch;
        #endregion

        #region WINDOW MANAGEMENT
        public MesureAmpouleADecanter()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Camera_Init();
            Arduino_Init();
            ImageProcessing_Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isRunning = false;
            CaptureCameraStop();
            cs?.PortCom_OFF();
        }

        private void gds_mouseenter(object sender, MouseEventArgs e)
        {
            dottedline.Visibility = Visibility.Visible;
        }

        private void gds_mouseleave(object sender, MouseEventArgs e)
        {
            dottedline.Visibility = Visibility.Hidden;
        }
        #endregion

        #region CAPTURE DEVICE
        private void Camera_Init()
        {
            ListDevices();
            cbx_device.SelectedIndex = 1;
            cbx_deviceFormat.SelectedIndex = 0;
            Capture_Start();
        }

        private void Button_ListDevices_Click(object sender, MouseButtonEventArgs e)
        {
            ListDevices();
        }

        private void ListDevices()
        {
            var devices = VideoInInfo.EnumerateVideoDevices_JJ();
            if (cbx_device != null)
                cbx_device.ItemsSource = devices.Select(d => d.Name).ToList();
        }

        private void Button_CaptureDevice_Click(object sender, MouseButtonEventArgs e)
        {
            Capture_Start();
        }

        void Capture_Start()
        {
            chrono.Start();
            isRunning = !isRunning;

            if (isRunning)
            {
                indexDevice = cbx_device.SelectedIndex;
                CaptureCamera(indexDevice);
                Button_CaptureDevicePlay.Visibility = Visibility.Collapsed;
                Button_CaptureDeviceStop.Visibility = Visibility.Visible;
            }
            else
            {
                CaptureCameraStop();
                Button_CaptureDevicePlay.Visibility = Visibility.Visible;
                Button_CaptureDeviceStop.Visibility = Visibility.Collapsed;
            }
        }

        private void Combobox_CaptureDevice_Change(object sender, SelectionChangedEventArgs e)
        {
            indexDevice = cbx_device.SelectedIndex;
            formats = VideoInInfo.EnumerateSupportedFormats_JJ(indexDevice);
            cbx_deviceFormat.ItemsSource = formats.OrderBy(f => f.Value.format).ThenByDescending(f => f.Value.w).Select(f => f.Key);
        }

        private void Combobox_CaptureDeviceFormat_Change(object sender, SelectionChangedEventArgs e)
        {
            format = formats[cbx_deviceFormat.SelectedValue as string];
        }

        private void Button_CaptureDeviceRECORD_Click(object sender, MouseButtonEventArgs e)
        {
            if (videoWriter == null)
            {
                //ok
                //videoWriter = new VideoWriter("D:\\video.avi", FourCC.FromString("XVID"), 30, new OpenCvSharp.Size(1080, 1920));
                //ok
                videoWriter = new VideoWriter("D:\\Projets\\video.mp4", FourCC.MP4V, 30, new OpenCvSharp.Size(frame.mat.Width, frame.mat.Height));

                //pas ok
                //videoWriter = new VideoWriter("D:\\Projets\\video.mp4", FourCC.H264, 30, new OpenCvSharp.Size(frame.Width, frame.Height));

                Button_CaptureDeviceRECORD.Visibility = Visibility.Collapsed;
                Button_CaptureDeviceRECORDSTOP.Visibility = Visibility.Visible;
            }
            else
            {
                videoWriter.Release();
                videoWriter = null;
                Button_CaptureDeviceRECORD.Visibility = Visibility.Visible;
                Button_CaptureDeviceRECORDSTOP.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region CAMERA MANAGEMENT
        void CaptureCamera(int index)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                Thread.Sleep(100);
            }
            indexDevice = index;
            thread = new Thread(new ThreadStart(CaptureCameraCallback));
            thread.Start();
        }

        void CaptureCameraStop()
        {
            thread?.Abort();
            first = true;
            thread = null;
        }

        OpenCvSharp.Window w;
        bool display_in_OpenCVSharpWindow = true;

        void CaptureCameraCallback()
        {
            int actualindexDevice = indexDevice;
            frame.mat = new Mat();
            capture = new VideoCapture(indexDevice);
            capture.Open(indexDevice, VideoCaptureAPIs.DSHOW);

            if (capture.IsOpened())
            {
                while (isRunning)
                {
                    //Si changement de camera
                    if (indexDevice != actualindexDevice)
                    {
                        capture.Open(indexDevice, VideoCaptureAPIs.DSHOW);
                        actualindexDevice = indexDevice;
                    }

                    //si changement de format vidéo
                    if (format != null)
                    {
                        capture.Set(VideoCaptureProperties.FrameWidth, format.w);
                        capture.Set(VideoCaptureProperties.FrameHeight, format.h);
                        capture.Set(VideoCaptureProperties.Fps, format.fr);
                        capture.Set(VideoCaptureProperties.FourCC, OpenCvSharp.FourCC.FromString(format.format));
                        format = null;
                    }

                    if (first)
                    {
                        Display_Init();
                        MatNamesToMats_Reset();
                        first = false;
                    }

                    //captation de l'image
                    capture.Read(frame.mat);

                    //Viewer debug
                    if (display_in_OpenCVSharpWindow)
                    {
                        if (frame.mat.Empty())
                            Cv2.DestroyWindow(w.Name);
                        else
                        {
                            if (w == null)
                                w = new OpenCvSharp.Window();

                            w.ShowImage(frame.mat);
                            Cv2.WaitKey(1);
                        }
                    }

                    Cv2.Rotate(frame.mat, rotated.mat, RotateFlags.Rotate90Counterclockwise);

                    if (!rotated.mat.Empty())
                        switch (processingType)
                        {
                            case ProcessingType.process1:
                                FrameProcessing1(rotated.mat);
                                break;
                            case ProcessingType.process2:
                                FrameProcessing2(rotated.mat);
                                break;
                            case ProcessingType.process3:
                                FrameProcessing3(rotated.mat);
                                break;
                            case ProcessingType.process4:
                                FrameProcessing4(rotated.mat);
                                break;
                        }
                    UpdateDisplayImages();

                    videoWriter?.Write(rotated.mat);

                    DisplayFPS();
                }
            }
        }
        #endregion

        #region IMAGE PROCESSINGS
        private void ImageProcessing_Init()
        {
            cbx_processingType.ItemsSource = Enum.GetValues(typeof(ProcessingType));
            cbx_processingType.SelectedIndex = 0;

            MatNamesToMats_Reset();

            i1._UpdateCombobox(NMs);
            i2._UpdateCombobox(NMs);
            i3._UpdateCombobox(NMs);
            i4._UpdateCombobox(NMs);

            int i = 1;
            i1.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i2.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i3.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i4.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
        }

        void MatNamesToMats_Reset()
        {
            NMs = new NamedMats();
            NMs.MatNamesToMats.Add(ImageType.original, frame);
            NMs.MatNamesToMats.Add(ImageType.rotated, rotated);
            NMs.MatNamesToMats.Add(ImageType.bw1, bw1);
            NMs.MatNamesToMats.Add(ImageType.debug1, debug1);
            NMs.MatNamesToMats.Add(ImageType.gray1, frameGray);
            NMs.MatNamesToMats.Add(ImageType.canny, cannymat);
            NMs.MatNamesToMats.Add(ImageType.bgr1, BGR);
            NMs.MatNamesToMats.Add(ImageType.roi1, ROI1);
            NMs.MatNamesToMats.Add(ImageType.roi2, ROI2);
            NMs.MatNamesToMats.Add(ImageType.debug2, debug2);
            NMs.MatNamesToMats.Add(ImageType.debug3, debug3);
            NMs.MatNamesToMats.Add(ImageType.debug4, debug4);
            NMs.MatNamesToMats.Add(ImageType.bw2, bw2);
        }

        private void Combobox_processingType_Change(object sender, SelectionChangedEventArgs e)
        {
            Enum.TryParse<ProcessingType>(cbx_processingType.SelectedValue.ToString(), out processingType);
            first = true;
        }

        private void Button_CaptureDeviceROI_Click(object sender, RoutedEventArgs e)
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', cancel with 'c'";
            OpenCvSharp.Rect newroi = Cv2.SelectROI(window_name, rotated.mat, true);
            if (newroi.Width > 0)
                roi = newroi;
            _title = roi.ToString();
            Cv2.DestroyWindow(window_name);
        }

        void FrameProcessing1(Mat rotated)
        {
            if (roi.Width > 0)
                ROI1.mat = new Mat(rotated, roi);
            else
                ROI1.mat = rotated;

            Cv2.CvtColor(ROI1.mat, frameGray.mat, ColorConversionCodes.RGB2GRAY);

            Cv2.Canny(frameGray.mat, cannymat.mat, 50, 200);

            bgr[0] = cannymat.mat;
            bgr[1] = new Mat(bgr[0].Size(), MatType.CV_8UC1);
            bgr[2] = bgr[1];

            Cv2.Merge(bgr, BGR.mat);
        }

        void FrameProcessing2(Mat rotated)
        {
            if (roi.Width > 0)
                ROI1.mat = new Mat(rotated, roi);
            else
                ROI1.mat = rotated;

            Cv2.CvtColor(ROI1.mat, frameGray.mat, ColorConversionCodes.RGB2GRAY);

            NamedMat BW1 = NMs.Get(ImageType.bw1);

            //BW1.mat = frameGray.mat.Threshold(Threshold1, 255, ThresholdTypes.Binary);
            Cv2.InRange(frameGray.mat, new Scalar(Threshold1), new Scalar(Threshold2), BW1.mat);

        }

        void FrameProcessing3(Mat rotated)
        {


        }

        void FrameProcessing4(Mat rotated)
        {


        }
        #endregion

        #region DISPLAY IMAGE(S)
        void Display_Init()
        {
            bgr = new Mat[] { new Mat(frame.mat.Size(), MatType.CV_8UC1),
                              new Mat(frame.mat.Size(), MatType.CV_8UC1),
                              new Mat(frame.mat.Size(), MatType.CV_8UC1)};
        }

        private void UpdateDisplayImages()
        {
            i1._Update(NMs.MatNamesToMats);
            i2._Update(NMs.MatNamesToMats);
            i3._Update(NMs.MatNamesToMats);
            i4._Update(NMs.MatNamesToMats);
        }

        void DisplayFPS()
        {
            long T = chrono.ElapsedMilliseconds;
            float f = 1000f / (T - T0);
            T0 = T;
            _fps = " [" + f.ToString("N1") + " fps]";
        }
        #endregion

        #region ARDUINO
        private void Arduino_Init()
        {
            COMBaudsRefresh();
            string nom = "USB Serial Port (COM4)";
            if (cbx_COM.Items.Contains(nom))
                cbx_COM.Text = nom;
            cbx_bauds.Text = "9600";
        }

        private void Button_COMRefresh_Click(object sender, MouseButtonEventArgs e)
        {
            COMBaudsRefresh();
        }

        void COMBaudsRefresh()
        {
            Communication_Série.Communication_Série.PortCom_Fill(cbx_COM);
            Communication_Série.Communication_Série.Bauds_Fill(cbx_bauds);
        }

        private void Button_Connexion_Click(object sender, MouseButtonEventArgs e)
        {
            if (cs == null)
            {
                cs = new Communication_Série.Communication_Série(cbx_COM.Text, cbx_bauds.Text, datareceived);
                if (cs.PortCom_ON())
                {
                    Button_DeviceConnect.Visibility = Visibility.Visible;
                    Button_DeviceDisconnect.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                cs.PortCom_OFF();
                cs = null;
                Button_DeviceConnect.Visibility = Visibility.Collapsed;
                Button_DeviceDisconnect.Visibility = Visibility.Visible;
            }
        }

        private void datareceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            sp.Encoding = Encoding.UTF8;
            string indata = sp.ReadExisting();
            indata = indata.Replace("\r", "");
            buffer += indata;

            string[] lignes = buffer.Split(split_car);

            for (int i = 0; i < lignes.Length - 1; i++)
            {
                ArduinoInterpretMessage(lignes[i]);
                AddTextInLBX(lignes[i]);
            }
            buffer = lignes[lignes.Length - 1];
        }

        private void ArduinoInterpretMessage(string txt)
        {
            string val_txt;
            if (txt.Contains("Position = "))
            {
                //Serial.print("Position = ");
                //Serial.print(d);
                //Serial.println("mm");
                val_txt = txt.Replace("Position = ", "");
                val_txt = val_txt.Replace("mm", "");
                val_txt = val_txt.Replace(".", ",");
                camera_pos = float.Parse(val_txt);
                SLD_camera_position(camera_pos);
            }
            if (txt.Contains("D max = "))
            {
                //Serial.print("D max = ");
                //Serial.print(d_abs_mm);
                //Serial.println("mm");
                val_txt = txt.Replace("D max = ", "");
                val_txt = val_txt.Replace("mm", "");
                val_txt = val_txt.Replace(".", ",");
                camera_pos_max = float.Parse(val_txt);
                SLD_camera_position_max(camera_pos_max);
            }

            if (txt.Contains("Y max atteint"))
            {
                camera_pos_high_switch = true;
            }

            if (txt.Contains("Y min atteint"))
            {
                camera_pos_low_switch = true;
            }


        }

        void SLD_camera_position(float value)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    if (value > sld_camera_position_mm.Maximum)
                        sld_camera_position_mm.Maximum = value;
                    sld_camera_position_mm.Value = value;
                }));
        }
        void SLD_camera_position_max(float value)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    sld_camera_position_mm.Maximum = value;
                }));
        }

        void AddTextInLBX(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ////last is up
                    //ArduinoMessages.Insert(0, message);
                    //while (ArduinoMessages.Count > 100)
                    //    ArduinoMessages.RemoveAt(ArduinoMessages.Count - 1);

                    //last is down
                    ArduinoMessages.Add(message);
                    while (ArduinoMessages.Count > 100)
                        ArduinoMessages.RemoveAt(0);
                    lbx_arduino_received.ScrollIntoView(lbx_arduino_received.Items[lbx_arduino_received.Items.Count - 1]);
                }));

            OnPropertyChanged("ArduinoMessages");
        }

        private void tbx_txt_to_arduino_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendToArduino();
        }
        private void SendToArduino(object sender, MouseButtonEventArgs e)
        {
            SendToArduino();
        }
        private void SendToArduino()
        {
            string txt = tbx_txt_to_arduino.Text;
            SendToArduino(txt);
            tbx_txt_to_arduino.Text = "";
        }
        private void SendToArduino(string txt)
        {
            if (cs != null)
            {
                if (!txt.Contains('\n'))
                    txt += '\n';
                cs?.Envoyer(txt);
            }
            else
            {
                AddTextInLBX("!! ARDUINO DISCONNECTED !!");
            }
        }

        private void Button_Clear_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ArduinoMessages.Clear();
                }));
            OnPropertyChanged("ArduinoMessages");
        }

        private void Button_Etalonnage_Click(object sender, RoutedEventArgs e)
        {
            SendToArduino("e");
        }

        private void Button_UP_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("h");
        }

        private void Button_DOWN_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("b");
        }
        private void Button_GetPosition_Click(object sender, RoutedEventArgs e)
        {
            SendToArduino("p");
        }

        private void Button_ARRETURGENCE_Click(object sender, RoutedEventArgs e)
        {
            SendToArduino("a");
        }
        #endregion

        private void Button_CaptureDeviceSCAN_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(Scan);
            t.Start();
        }

        private void Scan()
        {
            camera_pos_high_switch = false;
            camera_pos_low_switch = false;

            ////monte de 1mm
            //SendToArduino("h1");
            camera_pos = 0;
            //va à l'extrémité haute
            SendToArduino("H");
            while (!camera_pos_high_switch)
            {
                Thread.Sleep(100);
            }

            ////va à l'extrémité basse
            //SendToArduino("B");
            int H = 4;
            OpenCvSharp.Size s = rotated.mat.Size();
            OpenCvSharp.Point p = new OpenCvSharp.Point(0, s.Height / 2 - H / 2);
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(p, new OpenCvSharp.Size(s.Width, H));

            NamedMat d = NMs.Get(ImageType.debug1);
            d.mat = new Mat();
            while (!camera_pos_low_switch)
            {
                //descend de 1mm
                float camera_pos_prec = camera_pos;
                float delta_mm = 1;
                while (camera_pos_prec - camera_pos < delta_mm)// && !camera_pos_low_switch)
                {
                    SendToArduino("b1");
                    Thread.Sleep(200);
                }


                //améliorer la neteté
                //TODO : prendre plusieurs fois la ROI et en faire la médiane
                List<Mat> mediane = new List<Mat>();









                NamedMat roi2 = NMs.Get(ImageType.roi2);
                roi2.mat = new Mat(rotated.mat, rect);

                if (d.mat.Size().Width == 0)
                    Cv2.VConcat(new List<Mat> { roi2.mat }, d.mat);
                else
                    Cv2.VConcat(new List<Mat> { d.mat, roi2.mat }, d.mat);

            }
        }

        #region Clic droit sur image (INACTIF)  
        //Décommenter dans XAML les 2 images

        //private void img_mousedown(object sender, MouseButtonEventArgs e)
        //{
        //    if (ctxm_hideothers.IsChecked)
        //    {
        //        if (e.ChangedButton == MouseButton.Left)
        //            this.DragMove();
        //    }
        //}

        //private void ctxm_alwaysontop_Click(object sender, RoutedEventArgs e)
        //{
        //    Topmost = ctxm_alwaysontop.IsChecked;
        //}

        //private void ctxm_hideothers_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ctxm_hideothers.IsChecked)
        //    {
        //        grd_visu.Width = new GridLength(0);
        //        WindowStyle = WindowStyle.None;
        //    }
        //    else
        //    {
        //        grd_visu.Width = new GridLength(1, GridUnitType.Auto);
        //        WindowStyle = WindowStyle.SingleBorderWindow;
        //    }
        //}

        //private void ctxm_quit_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        //private void ctxm_calque_Add_Click(object sender, RoutedEventArgs e)
        //{
        //    string file = @"D:\Images\_JJ\_\cadran.png";
        //    FileInfo fi = new FileInfo(file);

        //    StackPanel sp = new StackPanel();
        //    sp.Orientation = System.Windows.Controls.Orientation.Horizontal;

        //    System.Windows.Controls.Image im = new System.Windows.Controls.Image();
        //    im.Source = new BitmapImage(new Uri(file));
        //    im.Width = 20;
        //    im.Height = 20;
        //    sp.Children.Add(im);

        //    Label lbl = new Label();
        //    lbl.Content = fi.Name;
        //    sp.Children.Add(lbl);

        //    MenuItem mi = new MenuItem();
        //    mi.Header = sp;
        //    ctxm_calque.Items.Add(mi);


        //    _imageCalque = new Bitmap(file);
        //}
        #endregion
    }
}