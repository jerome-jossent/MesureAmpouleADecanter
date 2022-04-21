using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace Standard_UC_JJO
{
    public partial class Slider_INT_JJO : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Definition d'un dependency property pour effectuer le binding sur la vue
        /// https://stackoverflow.com/questions/1636807/what-exactly-does-wpf-data-bindings-relativesource-findancestor-do
        /// </summary>
        public static readonly DependencyProperty intvalue =
            DependencyProperty.Register(
                "_value",
                typeof(int),
                typeof(Slider_INT_JJO),
                new FrameworkPropertyMetadata(null));
        public int _ring_number
        {
            get { return (int)GetValue(intvalue); }
            set { SetValue(intvalue, value); }
        }

        public Slider _sld { get { return sld; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        //public event EventHandler<EventArgs> _Slider_ValueChanged;
        public event EventHandler<EventArgs> _ValueChanged;

        public int _index;
        public string _name;

        public string _label_title
        {
            get { return label_title; }
            set
            {
                if (value == label_title) return;
                label_title = value;
                OnPropertyChanged("_label_title");
            }
        }
        string label_title;

        public string _label_value
        {
            get { return label_value; }
            set
            {
                if (value == label_value) return;
                label_value = value;
                OnPropertyChanged("_label_value");
            }
        }
        string label_value;

        public int _value
        {
            get { return value_p; }
            set
            {
                if (value == value_p) return;
                value_p = value;
                _label_value = _value.ToString();
                OnPropertyChanged("_value");
                //_ValueChanged?.Invoke(this, null);
            }
        }
        int value_p;

        public int _value_min
        {
            get { return value_min; }
            set
            {
                if (value == value_min) return;
                value_min = value;
                OnPropertyChanged("_value_min");
            }
        }
        int value_min;

        public int _value_max
        {
            get { return value_max; }
            set
            {
                if (value == value_max) return;
                value_max = value;
                OnPropertyChanged("_value_max");
            }
        }
        int value_max;

        public Slider_INT_JJO()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //_Slider_ValueChanged?.Invoke(sender, e);
            _ValueChanged?.Invoke(sender, e);
        }
    }
}
