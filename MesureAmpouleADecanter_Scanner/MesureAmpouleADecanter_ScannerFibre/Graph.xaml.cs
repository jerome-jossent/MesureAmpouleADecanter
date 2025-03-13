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
using Xceed.Wpf.AvalonDock.Controls;
using LiveCharts.Wpf;

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

        public ChartValues<ObservablePoint> _chartValues1 { get; set; }
        public ChartValues<ObservablePoint> _chartValues2 { get; set; }
        public ChartValues<ObservablePoint> _chartValuesDerivate { get; set; }

        int ys_previous_index;

        public int ys_previous_window = 10;

        bool first = true;
        XY[] xys;
        double[] xs;
        double[] ys;
        List<double[]> ys_previous;
        DateTime last = DateTime.MinValue;


        public Graph()
        {
            InitializeComponent();
            DataContext = this;
            Graph_INIT();
        }

        void Graph_INIT()
        {
            _chartValues1 = new ChartValues<ObservablePoint>();
            _chartValues2 = new ChartValues<ObservablePoint>();
            _chartValuesDerivate = new ChartValues<ObservablePoint>();

            _LineSeries1.Title = "I lum (%)";
            _LineSeries2.Title = "dI/dt";
            _LineSeries3.Title = "H à max(dI/dt)";

            _chart_AxeY.MinValue = 0;
        }

        public static XY[] SortedXY(Sensor[] sensors)
        {
            XY[] Data = new XY[sensors.Length];
            for (int i = 0; i < sensors.Length; i++)
                Data[i] = new XY(sensors[i]);
            List<XY> sortedPoints = Data.OrderBy(p => p.x).ToList();

            return sortedPoints.ToArray();
        }


        internal float? _Update(Sensor[] sensors)
        {
            try
            {
                xys = SortedXY(sensors);

                if (first || xs.Length != sensors.Length)
                {
                    first = false;
                    _chartValues1.Clear();
                    _chartValues2.Clear();
                    _chartValuesDerivate.Clear();

                    xs = xys.Select(p => p.x).ToArray();
                    ys = xys.Select(p => p.y).ToArray();
                    ys_previous = new List<double[]>();

                    for (int i = 0; i < ys_previous_window; i++)
                        ys_previous.Add(new double[ys.Length]);

                    for (int i = 0; i < xs.Length; i++)
                    {
                        double x = xs[i];
                        double y = ys[i];
                        _chartValues1.Add(new ObservablePoint(x, y));
                        _chartValues2.Add(new ObservablePoint(x, 0));
                        _chartValuesDerivate.Add(new ObservablePoint(x, i));
                    }
                    return null;
                }
                else
                {
                    DateTime t0 = DateTime.Now;
                    var dt = t0 - last;
                    Dispatcher.BeginInvoke(() =>
                    {
                        _txt.Text = dt.TotalMilliseconds.ToString("000") + " ms";
                    });
                    last = t0;

                    ys = xys.Select(p => p.y).ToArray();

                    int H_max_pos = 0;
                    double H_max;
                    double DeltaI_Max = 0;

                    for (int i = 0; i < xys.Length; i++)
                    {
                        _chartValues1[i].Y = xys[i].y;

                        double y_previous = 0;
                        //moyenne
                        for (int j = 0; j < ys_previous.Count; j++)
                        {
                            y_previous += ys_previous[j][i];
                        }
                        y_previous /= ys_previous.Count;

                        //_chartValues2[i].Y = y_previous;

                        double delta_tmoins1 = xys[i].y - y_previous;
                        //if (delta_tmoins1 != 0)
                        _chartValuesDerivate[i].Y = delta_tmoins1;

                        if (delta_tmoins1 > DeltaI_Max)
                        {
                            DeltaI_Max = delta_tmoins1;
                            H_max_pos = i;
                            H_max = _chartValues1[H_max_pos].X;
                        }
                        _chartValues2[i].Y = 0;
                    }

                    _chartValues2[H_max_pos].Y = H_max_pos;

                    //update data list pour moyenne glissante
                    ys_previous.Add(ys);
                    if (ys_previous.Count > ys_previous_window)
                        ys_previous.RemoveAt(0);

                    //update min max Axis
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (xs.Length > 0)
                        {
                            _chart_AxeX.MinValue = xs.Min();
                            _chart_AxeX.MaxValue = xs.Max();
                        }
                        if (ys.Length > 0)
                            _chart_AxeY.MaxValue = ys.Max();

                        _chart_AxeY2.MinValue = -5;
                        _chart_AxeY2.MaxValue = 5;
                    });

                    ys_previous_index++;
                    if (ys_previous_index > ys_previous_window - 1)
                        ys_previous_index = 0;

                    if (DeltaI_Max > 1) //A CHANGER !!!
                        return (float)_chartValues2[H_max_pos].Y;
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                ex = ex;
            }
            return null;
        }

        void Reset_Click(object sender, RoutedEventArgs e)
        {
            first = true;
        }
    }
}