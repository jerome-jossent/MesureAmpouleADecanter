using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using static OpenCVSharpJJ.Processing.ImPr_Converter;

namespace OpenCVSharpJJ.Processing
{
    public abstract class ImPr: INotifyPropertyChanged
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract ImPrType imPrType { get; }

        public bool hasToBeBinary; //1 layer with 0 or 255 values
        public bool hasToBeGray; //1 layer

        public bool _actived { get; set; } = true;
        #region BINDINGS IHM
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [JsonIgnore]
        public string _name
        {
            get
            {
                return imPrType.ToString();
            }
        }

        [JsonIgnore]
        public string _string
        {
            set
            {
                imPr_ListBoxItem._info = value;
            }
            get {
                return imPr_ListBoxItem._info;
            }
        }
        #endregion

        [JsonIgnore]
        public ImPr_ListBoxItem imPr_ListBoxItem;
        [JsonIgnore]
        public System.Windows.Media.SolidColorBrush transparent = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
        [JsonIgnore]
        public System.Windows.Media.SolidColorBrush red = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
         [JsonIgnore]
        public System.Windows.Media.SolidColorBrush black = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);

        public Mat Input;
        public Mat Output;

        public void ImPr_Init()
        {
            imPr_ListBoxItem = new ImPr_ListBoxItem();
            imPr_ListBoxItem.ImPr_Link(this);
            Update_string();
        }

        public abstract void Update_string();
        public abstract void Update_Debug(string txt, System.Windows.Media.SolidColorBrush color);
        public abstract void Process();
        
        public abstract System.Windows.Controls.UserControl UC();
    }
}