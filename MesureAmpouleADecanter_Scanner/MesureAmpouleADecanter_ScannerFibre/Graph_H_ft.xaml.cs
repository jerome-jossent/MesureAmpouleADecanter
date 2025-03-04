using LiveCharts.Defaults;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
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
//using static MesureAmpouleADecanter_ScannerFibre.Graph;

namespace MesureAmpouleADecanter_ScannerFibre
{
    /// <summary>
    /// Logique d'interaction pour Graph_H_ft.xaml
    /// </summary>
    public partial class Graph_H_ft : UserControl
    {
        public ChartValues<ObservablePoint> _chartValues1 { get; set; }
        public ChartValues<ObservablePoint> _chartValues2 { get; set; }
        public ChartValues<ObservablePoint> _chartValues3 { get; set; }


        bool first = true;
        //XY[] xys;
        double[] xs;
        double[] ys;
        List<double[]> ys_previous;
        DateTime t0;


        public Graph_H_ft()
        {
            InitializeComponent();
            DataContext = this;
            Graph_INIT();
        }


        void Graph_INIT()
        {
            _chartValues1 = new ChartValues<ObservablePoint>();
            _chartValues2 = new ChartValues<ObservablePoint>();
            _chartValues3 = new ChartValues<ObservablePoint>();

            //_chart_AxeY.MinValue = 0;
            t0 = DateTime.Now;
        }



        internal void _Update(DateTime x, float y)
        {
            double dT_sec = (x - t0).TotalSeconds;
            _chartValues1.Add(new ObservablePoint(dT_sec, y));
            //_chartValues2.Add(new ObservablePoint(x, 0));
            //_chartValues3.Add(new ObservablePoint(x, i));                    //update min max Axis
            Dispatcher.BeginInvoke(() =>
            {
                //if (xs.Length > 0)
                //{
                //    _chart_AxeX.MinValue = xs.Min();
                //    _chart_AxeX.MaxValue = xs.Max();
                //}
                //if (ys.Length > 0)
                //    _chart_AxeY.MaxValue = ys.Max();

                //_chart_AxeY2.MinValue = -5;
                //_chart_AxeY2.MaxValue = 5;
            });
        }

        void Reset_Click(object sender, RoutedEventArgs e)
        {
            _chartValues1.Clear();
            _chartValues2.Clear();
            _chartValues3.Clear();
            t0 = DateTime.Now;
        }

    }
}
