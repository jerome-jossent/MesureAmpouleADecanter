using Newtonsoft.Json;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCVSharpJJ
{
    public class Configuration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string deplacement_portCOM
        {
            get { return _deplacement_portCOM; }
            set
            {
                if (_deplacement_portCOM == value)
                    return;
                _deplacement_portCOM = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] string _deplacement_portCOM;

        public string deplacement_bauds
        {
            get { return _deplacement_bauds; }
            set
            {
                if (_deplacement_bauds == value)
                    return;
                _deplacement_bauds = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] string _deplacement_bauds;

        public int camera_index
        {
            get { return _camera_index; }
            set
            {
                if (_camera_index == value)
                    return;
                _camera_index = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] int _camera_index;

        public VideoInInfo.Format camera_encodage_resolution
        {
            get { return _camera_encodage_resolution; }
            set
            {
                if (_camera_encodage_resolution == value)
                    return;
                _camera_encodage_resolution = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] VideoInInfo.Format _camera_encodage_resolution;

        public float resize_factor
        {
            get { return _resize_factor; }
            set
            {
                if (_resize_factor == value)
                    return;
                _resize_factor = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] float _resize_factor;

        public float rotation_angle
        {
            get { return _rotation_angle; }
            set
            {
                if (_rotation_angle == value)
                    return;
                _rotation_angle = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] float _rotation_angle;

        public Rect roi
        {
            get { return _roi; }
            set
            {
                if (_roi == value)
                    return;
                _roi = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] Rect _roi = new Rect(0, 0, 100, 100);

        public double ratio_pix_par_mm
        {
            get { return _ratio_pix_par_mm; }
            set
            {
                if (_ratio_pix_par_mm == value)
                    return;
                _ratio_pix_par_mm = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] double _ratio_pix_par_mm = 1;

        public double seuil_perte_intensite
        {
            get { return _seuil_perte_intensite; }
            set
            {
                if (_seuil_perte_intensite == value)
                    return;
                _seuil_perte_intensite = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] double _seuil_perte_intensite = 0.3;

        public bool? inverser
        {
            get { return _inverser; }
            set
            {
                if (_inverser == value)
                    return;
                _inverser = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _inverser;

        public double bande_morte_mm
        {
            get { return _bande_morte_mm; }
            set
            {
                if (_bande_morte_mm == value)
                    return;
                _bande_morte_mm = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] double _bande_morte_mm = 1;

        public bool? saveFrame
        {
            get { return _saveFrame; }
            set
            {
                if (_saveFrame == value)
                    return;
                _saveFrame = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _saveFrame;

        public string savedImageFolder
        {
            get { return _savedImageFolder; }
            set
            {
                if (_savedImageFolder == value)
                    return;
                _savedImageFolder = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] string _savedImageFolder;

        public bool? addDirectory
        {
            get { return _addDirectory; }
            set
            {
                if (_addDirectory == value)
                    return;
                _addDirectory = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _addDirectory;


        [JsonIgnore] static string filename = "Configuration.json";

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            string stringtosave = JsonConvert.SerializeObject(this, Formatting.Indented);
            return stringtosave;
        }

        public static Configuration FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<Configuration>(jsonString);
        }

        public void Save()
        {
            System.IO.File.WriteAllText(GetPath(), Serialize());
        }

        public static Configuration Load()
        {
            Configuration c = FromJson(System.IO.File.ReadAllText(GetPath()));
            return c;
        }

        static string GetPath()
        {
            string d = AppDomain.CurrentDomain.BaseDirectory + filename;
            return d;
        }
    }
}
