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

        public string _string
        {
            //get
            //{
            //    return l_string;
            //}
            set
            {
                //l_string = value;
                //OnPropertyChanged("_string");
                imPr_ListBoxItem._info = value;
            }
        }
        //string l_string;
        #endregion

        [JsonIgnore]
        public ImPr_ListBoxItem imPr_ListBoxItem;

        public Mat Input;
        public Mat Output;

        public void ImPr_Init()
        {
            imPr_ListBoxItem = new ImPr_ListBoxItem();
            imPr_ListBoxItem.ImPr_Link(this);
            Update_string();
        }

        public abstract void Update_string();
        public abstract void Process();
        
        public abstract System.Windows.Controls.UserControl UC();
    }
}