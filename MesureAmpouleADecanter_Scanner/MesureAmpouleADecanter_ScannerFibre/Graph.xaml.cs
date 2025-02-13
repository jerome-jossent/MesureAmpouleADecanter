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

using System.Collections.ObjectModel;
using LiveCharts.Defaults;
using LiveCharts;
using System.Windows.Threading;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public partial class Graph : UserControl
    {
        public class XY
        {
            public double x, y;
            public XY(Sensor sensor)
            {
                x = sensor.hauteur_mm;
                y = sensor.intensity * 100;
            }
        }

        public ChartValues<ObservablePoint> _chartValues { get; set; }
        public ChartValues<ObservablePoint> _chartValuesDerivate { get; set; }

        bool first = true;
        XY[] xys;
        double[] xs;
        double[] ys;
        double[] ys_previous;

        public Graph()
        {
            InitializeComponent();
            DataContext = this;
            Graph_INIT();
        }

        void Graph_INIT()
        {
            _chartValues = new ChartValues<ObservablePoint>();
            _chartValuesDerivate = new ChartValues<ObservablePoint>();
            _chart_AxeX.MinValue = 0;
            _chart_AxeY.MinValue = 0;
        }

        public static XY[] SortedXY(ObservableCollection<Sensor> sensors)
        {
            XY[] Data = new XY[sensors.Count];
            for (int i = 0; i < sensors.Count; i++)
                Data[i] = new XY(sensors[i]);
            List<XY> sortedPoints = Data.OrderBy(p => p.x).ToList();

            return sortedPoints.ToArray();
        }

        internal void _Update(ObservableCollection<Sensor> sensors)
        {
            try
            {
                xys = SortedXY(sensors);

                if (first || xs.Length != sensors.Count)
                {
                    first = false;
                    _chartValues.Clear();
                    xs = xys.Select(p => p.x).ToArray();
                    ys = xys.Select(p => p.y).ToArray();
                    ys_previous = ys.ToArray();

                    for (int i = 0; i < xs.Length; i++)
                    {
                        double x = xs[i];
                        double y = ys[i];
                        _chartValues.Add(new ObservablePoint(x, y));
                        _chartValuesDerivate.Add(new ObservablePoint(x, 0));
                    }

                    Dispatcher.BeginInvoke(() =>
                    {
                        _chart_AxeX.MaxValue = xs.Max();
                        _chart_AxeY.MaxValue = ys.Max();
                    });
                }
                else
                {
                    for (int i = 0; i < xys.Length; i++)
                    {
                        _chartValues[i].Y = xys[i].y;

                        double derive = xys[i].y - ys_previous[i];
                        _chartValuesDerivate[i].Y = derive * 100; //en attendant d'utiliser un axe secondaire
                        ys_previous[i] = xys[i].y;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}