using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using OpenCvSharp.WpfExtensions;
using System.Runtime.CompilerServices;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;

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

        //Token d'arrêt de thread
        CancellationTokenSource cts = new CancellationTokenSource();

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
                else
                {
                    images.Clear();
                }
                OnPropertyChanged();
            }
        }
        bool? _toSave = false;

        public int epaisseur
        {
            get => _epaisseur;
            set
            {
                _epaisseur = value;
                OnPropertyChanged();

            }
        }
        int _epaisseur = 1;

        Dictionary<string, Mat> images = new Dictionary<string, Mat>();

        public Dictionary<int, DirectShowLib.DsDevice> devices;

        string _f = @"C:\_JJ\DATA\decantation\multicam\";
        public string f;
        public Scalar rouge = new Scalar(0, 0, 255);

        public Dictionary<int, CaptureArguments> ARGS;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }


        void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            devices = CameraSettings.GetDsDevices();
            foreach (var device in devices)
            {
                MenuItem mi = new MenuItem();
                mi.Header = device.Value.Name;
                mi.Click += (sender, e) => Menu_Add_Click(this, new RoutedEventArgs(null, new Tuple<int, object>(device.Key, device.Value)));
                menu_Add.Items.Add(mi);
            }

            new Thread(ThreadSave).Start();

            ARGS = new Dictionary<int, CaptureArguments>();
        }

        private void Menu_Add_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var data = (Tuple<int, object>)e.Source;


            Capture_UC uc = new Capture_UC();

            LayoutAnchorable layoutAnchorable = new LayoutAnchorable();
            layoutAnchorable.CanClose = true;
            layoutAnchorable.CanHide = false;
            layoutAnchorable.Content = uc;

            //if (Avalon_Views.Children.Count == 0)
            Avalon_Views.Children.Add(layoutAnchorable);
            //else
            //    layoutAnchorable.AddToLayout(DManager, AnchorableShowStrategy.Most);


            var arg = new CaptureArguments(layoutAnchorable,
                                          index: data.Item1,
                                          (DirectShowLib.DsDevice)data.Item2,
                                          ARGS.Count,
                                          uc,
                                          images,
                                          this);
            arg.capture_UC._Link(arg);
            ARGS.Add(ARGS.Count, arg);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            cts.Cancel();
            Thread.Sleep(500);

            foreach (var ARG in ARGS)
                ARG.Value.capture_UC._Stop();
        }

        internal void SwitchCamera(int position_precedente, int position_prevue)
        {
            var temp = ARGS[position_prevue];
            ARGS[position_prevue] = ARGS[position_precedente];
            ARGS[position_precedente] = temp;

            ARGS[position_prevue].position = position_prevue;
            ARGS[position_precedente].position = position_precedente;

            ARGS[position_prevue].capture_UC.SetTitleAndInfo();
            ARGS[position_precedente].capture_UC.SetTitleAndInfo();
        }

        #region a mettre dans une classe séparée

        int error = 0;
        public TimeSpan? _timeBetweenFrameToSave
        {
            get => timeBetweenFrameToSave;
            set
            {
                timeBetweenFrameToSave = value;
                OnPropertyChanged();
            }
        }
        TimeSpan? timeBetweenFrameToSave = TimeSpan.FromSeconds(0.1);

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
        #endregion

        //private void Menu_Delete_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Avalon_Views.SelectedContent == null) return;
        //    Capture_UC cuc = (Capture_UC)Avalon_Views.SelectedContent.Content;
        //    cuc._Delete();
        //    //Avalon_Views.Children.Remove(Avalon_Views.SelectedContent);
        //}
    }
}
