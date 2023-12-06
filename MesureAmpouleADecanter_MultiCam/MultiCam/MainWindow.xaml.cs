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
using System.Text.Json;
using Newtonsoft.Json;
using DirectShowLib;

namespace MultiCam
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        //IDEES :
        //
        // afficher par UC les réglages caméras
        // définir la position du capteur en mm

        // Binarisation (rgb => gris => bw threshold)
        // 2D => 1D (somme)
        // detection du front, 2 sens ("sens par le haut", "sens par le bas")

        // enregistrement bouton Record/Stop + création d'un dossier

        public Dictionary<int, DirectShowLib.DsDevice> devices;

        public string f;
        public Scalar rouge = new Scalar(0, 0, 255);
        public Dictionary<int, CaptureArguments> capturesParameters; //clef = index device

        //SAVE
        Dictionary<string, Mat> images = new Dictionary<string, Mat>();
        CancellationTokenSource cts = new CancellationTokenSource();


        #region PARAMETRES BINDES AVEC IHM
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool? toSave
        {
            get => _toSave;
            set
            {
                _toSave = value;
                if (value == true)
                {
                    f = folder + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + "\\";
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

        public string folder
        {
            get => _folder;
            set
            {
                _folder = value;
                Properties.Settings.Default.folder = _folder;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        string _folder = Properties.Settings.Default.folder;

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
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) { INITS(); }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            cts.Cancel();
            Thread.Sleep(500);

            foreach (var ARG in capturesParameters)
                ARG.Value.capture_UC._Stop();
        }

        void INITS()
        {
            capturesParameters = new Dictionary<int, CaptureArguments>();

            RefreshDeviceList();
            new Thread(ThreadSave).Start();
        }

        void RefreshDeviceList_Click(object sender, RoutedEventArgs e) { RefreshDeviceList(); }

        void RefreshDeviceList()
        {
            menu_Add.Items.Clear();

            devices = CameraSettings.GetDsDevices();

            foreach (var device in devices)
            {
                MenuItem mi = new MenuItem();
                mi.Header = $"{device.Value.Name} ({device.Key})";
                menu_Add.Items.Add(mi);

                if (capturesParameters.ContainsKey(device.Key))
                    mi.IsEnabled = false;
                else
                    mi.Click += (mi, e) => Menu_AddCamera_Click(mi, new RoutedEventArgs(null, new Tuple<int, object>(device.Key, device.Value)));

                //var v = CameraSettings.GetAllAvailableResolution(device.Value);
            }
        }

        void Menu_AddCamera_Click(object sender, System.Windows.RoutedEventArgs e) { AddCamera((MenuItem)sender, (Tuple<int, object>)e.Source); }

        void AddCamera(MenuItem mi, Tuple<int, object> data)
        {
            Capture_UC uc = new Capture_UC();

            LayoutAnchorable layoutAnchorable = new LayoutAnchorable();
            layoutAnchorable.CanClose = true;
            layoutAnchorable.CanHide = false;
            layoutAnchorable.Content = uc;

            Avalon_Views.Children.Add(layoutAnchorable);

            var arg = new CaptureArguments(data.Item1,
                                          (DsDevice)data.Item2,
                                          GetFirstAvailablePosition());

            arg._LinkWithIHM(layoutAnchorable, uc, images, this, mi);

            arg.capture_UC._Link(arg);

            //CLEAN DICO BEFORE
            foreach (int clef in capturesParameters.Keys)
            {
                if (capturesParameters[clef].videoCapture.IsDisposed)
                    capturesParameters.Remove(clef);
            }

            capturesParameters.Add(arg.ac_data.deviceIndex, arg);
        }

        int GetFirstAvailablePosition()
        {
            List<int> positions = new List<int>();
            foreach (CaptureArguments ca in capturesParameters.Values)
                positions.Add(ca.ac_data.position);
            positions.Sort();

            int position = 0;
            while (positions.Contains(position))
                position++;
            return position;
        }


        internal void SwitchPositions(int position_precedente, int position_prevue)
        {
            CaptureArguments cp_source = null;
            CaptureArguments cp_target = null;
            foreach (CaptureArguments cp in capturesParameters.Values)
            {
                if (cp.ac_data.position == position_precedente)
                    cp_source = cp;

                if (cp.ac_data.position == position_prevue)
                    cp_target = cp;
            }

            //y a t'il une autre caméra avec cette position ?
            if (cp_target != null)
            {
                cp_target.ac_data.position = position_precedente;
                cp_target.capture_UC.SetTitle();
            }

            cp_source.ac_data.position = position_prevue;
            cp_source.capture_UC.SetTitle();
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

        void Save_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<int, CaptureArguments_data> cads = new Dictionary<int, CaptureArguments_data>();
            foreach (var cap in capturesParameters)
                cads.Add(cap.Key, cap.Value.ac_data);

            string json = JsonConvert.SerializeObject(cads, new JsonSerializerSettings { Formatting = Formatting.Indented });
            Properties.Settings.Default.save1 = json;
            Properties.Settings.Default.Save();
        }

        void Load_Click(object sender, RoutedEventArgs e)
        {
            string json = Properties.Settings.Default.save1;
            Dictionary<int, CaptureArguments_data> cads = JsonConvert.DeserializeObject<Dictionary<int, CaptureArguments_data>>(json);

            capturesParameters.Clear();
            foreach (var cad in cads)
                capturesParameters.Add(cad.Key, NewCamera(cad.Value));
        }


        CaptureArguments NewCamera(CaptureArguments_data cad)
        {
            Capture_UC uc = new Capture_UC();

            LayoutAnchorable layoutAnchorable = new LayoutAnchorable();
            layoutAnchorable.CanClose = true;
            layoutAnchorable.CanHide = false;
            layoutAnchorable.Content = uc;

            Avalon_Views.Children.Add(layoutAnchorable);

            var arg = new CaptureArguments(cad.deviceIndex,
                                          devices[cad.deviceIndex],
                                          GetFirstAvailablePosition(),
                                          cad);

            MenuItem mi = null;
            foreach (MenuItem menuItem in menu_Add.Items)
            {
                if (menuItem.Header == $"{devices[cad.deviceIndex].Name} ({cad.deviceIndex})")
                    mi = menuItem;
            }

            arg._LinkWithIHM(layoutAnchorable, uc, images, this, mi);

            arg.capture_UC._Link(arg);

            return arg;
        }

        private void SetSaveFolder_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
