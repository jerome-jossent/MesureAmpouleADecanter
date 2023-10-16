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
        [JsonIgnore] float _resize_factor = 1;

        public float rotation_angle
        {
            get { return _rotation_angle; }
            set
            {
                if (_rotation_angle == value)
                    return;

                while (value < 0)
                    value += 360;

                while (value > 360)
                    value -= 360;

                if (value == 360)
                    value = 0;

                _rotation_angle = value;

                OnPropertyChanged();
            }
        }
        [JsonIgnore] float _rotation_angle = 0;

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

        public double bande_morte_haut_mm
        {
            get { return _bande_morte_haut_mm; }
            set
            {
                if (_bande_morte_haut_mm == value)
                    return;
                _bande_morte_haut_mm = value;
                OnPropertyChanged();
                OnPropertyChanged("bande_morte_haut_pix");
            }
        }
        [JsonIgnore] double _bande_morte_haut_mm = 1;
        
        public double bande_morte_bas_mm
        {
            get { return _bande_morte_bas_mm; }
            set
            {
                if (_bande_morte_bas_mm == value)
                    return;
                _bande_morte_bas_mm = value;
                OnPropertyChanged();
                OnPropertyChanged("bande_morte_bas_pix");
            }
        }
        [JsonIgnore] double _bande_morte_bas_mm = 1;

        public double bar_mm_by_turn
        {
            get { return _bar_mm_by_turn; }
            set
            {
                if (_bar_mm_by_turn == value)
                    return;
                _bar_mm_by_turn = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] double _bar_mm_by_turn = 1;

        public int coder_imp_by_turn
        {
            get { return _coder_imp_by_turn; }
            set
            {
                if (_coder_imp_by_turn == value)
                    return;
                _coder_imp_by_turn = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] int _coder_imp_by_turn = 1;

        public int motor_steps_by_turn
        {
            get { return _motor_steps_by_turn; }
            set
            {
                if (_motor_steps_by_turn == value)
                    return;
                _motor_steps_by_turn = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] int _motor_steps_by_turn = 1;

        public int motor_step_duration
        {
            get { return _motor_step_duration; }
            set
            {
                if (_motor_step_duration == value)
                    return;
                _motor_step_duration = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] int _motor_step_duration = 1;

        public int motor_step_pause
        {
            get { return _motor_step_pause; }
            set
            {
                if (_motor_step_pause == value)
                    return;
                _motor_step_pause = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] int _motor_step_pause = 1;

        [JsonIgnore]
        public int bande_morte_haut_pix
        {
            get
            {
                return (int)(bande_morte_haut_mm * ratio_pix_par_mm);
            }
        }

        [JsonIgnore]
        public int bande_morte_bas_pix
        {
            get
            {
                return (int)(bande_morte_bas_mm * ratio_pix_par_mm);
            }
        }

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

        public bool? displayGrids
        {
            get { return _displayGrids; }
            set
            {
                if (_displayGrids == value)
                    return;
                _displayGrids = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _displayGrids = false;

        public bool? displayROI
        {
            get { return _displayROI; }
            set
            {
                if (_displayROI == value)
                    return;
                _displayROI = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _displayROI = false;

        public bool? deadBand
        {
            get { return _deadBand; }
            set
            {
                if (_deadBand == value)
                    return;
                _deadBand = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _deadBand = false;

        public bool? displayCenter
        {
            get { return _displayCenter; }
            set
            {
                if (_displayCenter == value)
                    return;
                _displayCenter = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] bool? _displayCenter = true;

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