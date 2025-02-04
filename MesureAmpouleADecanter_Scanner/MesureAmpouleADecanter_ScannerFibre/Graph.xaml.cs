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

using ScottPlot;
using static OpenTK.Graphics.OpenGL.GL;
using System.Collections.ObjectModel;

namespace MesureAmpouleADecanter_ScannerFibre
{

    //https://scottplot.net/cookbook/5.0/Signal/SignalDateTime/

    public partial class Graph : UserControl
    {
        //ScottPlot.Plottables.DataLogger chart;
        ScottPlot.Plottables.SignalXY signalPlot;

        public Graph()
        {
            InitializeComponent();
            DataContext = this;
            Graph_INIT();
        }

        void Graph_INIT()
        {
            //chart = WpfPlot1.Plot.Add.DataLogger();
            WpfPlot1.Plot.Grid.LineColor = ScottPlot.Colors.Blue.WithAlpha(.2);
            WpfPlot1.Plot.Grid.LinePattern = LinePattern.Dotted;
            WpfPlot1.Plot.YLabel("Intensity (%)");
            WpfPlot1.Plot.Title("Height (mm)");

        }

        public static XY[] SortedXY(ObservableCollection<Sensor> sensors)
        {
            XY[] Data = new XY[sensors.Count];
            for (int i = 0; i < sensors.Count; i++)
                Data[i] = new XY(sensors[i]);
            List<XY> sortedPoints = Data.OrderBy(p => p.x).ToList();

            return sortedPoints.ToArray();
        }


        public class XY
        {
            public double x, y;
            public XY(Sensor sensor)
            {
                x = sensor.hauteur_mm;
                y = sensor.intensity*100;
            }
        }
        bool first = true;

        XY[] xys;

        double[] xs;
        double[] ys;


        internal void _Update(ObservableCollection<Sensor> sensors)
        {
            try
            {
                xys = SortedXY(sensors);

                if (first)
                {
                    first = false;

                    xs = xys.Select(p => p.x).ToArray();
                    ys = xys.Select(p => p.y).ToArray();
                    signalPlot = WpfPlot1.Plot.Add.SignalXY(xs, ys);
                }
                else
                {
                    double[] ys_new = new double[sensors.Count];
                    for (int i = 0; i < xys.Length; i++)
                        ys_new[i] = xys[i].y;
                    ys = ys_new;
                }

                WpfPlot1.Refresh();
            }
            catch (Exception)
            {

            }
        }
    }
}