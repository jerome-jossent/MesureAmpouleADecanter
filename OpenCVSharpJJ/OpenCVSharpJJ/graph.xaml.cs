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

using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace OpenCVSharpJJ
{
    /// <summary>
    /// Logique d'interaction pour graph.xaml
    /// </summary>
    public partial class graph : UserControl
    {
        Dictionary<string, Serie> series;
        public ChartValues<ObservablePoint> ValuesA { get; set; } = new ChartValues<ObservablePoint>();

        Serie serie;

        public graph()
        {
            InitializeComponent();

            series = new Dictionary<string, Serie>();
            serie = new Serie("BDG", 0, ValuesA, _S_R);
            series.Add(serie.code, serie);

            series.Add("0", serie);

            for (int i = 0; i < 2000; i++)
            {
                serie.valeurs.Add(new ObservablePoint(i, i));
            }
            DataContext = this;

        }


        class JJPoint
        {
            public string serie;
            public double x, y;
        }

        void AddPoints(string serie, double x, double y)
        {
            //lock (graph_data_lock)
            //{
            //    graph_data_buffer.Add(new JJPoint() { serie = serie, x = x, y = y });
            //}
            ////AddPoints(series[serie].valeurs, x, y);
        }
        void AddPoints(ChartValues<ObservablePoint> serie, double x, double y)
        {
            serie.Add(new ObservablePoint(x, y));
        }

        class Serie
        {
            public string code;
            public double y_val;
            public ChartValues<ObservablePoint> valeurs;
            public ScatterSeries courbe;
            public Serie(string code, double y_val, ChartValues<ObservablePoint> valeurs, ScatterSeries courbe)
            {
                this.code = code;
                this.y_val = y_val;
                this.valeurs = valeurs;
                this.courbe = courbe;
                this.courbe.Title = code;
            }
        }

        public void add()
        {



            //https://lvcharts.net/App/examples/v1/wpf/Performance%20Tips



            //while (graph_data_buffer.Count > 0)
            //{
            //    JJPoint jjPoint = graph_data_buffer.Take();
            //    if (!series_buffer.ContainsKey(jjPoint.serie))
            //        series_buffer.Add(jjPoint.serie, new List<ObservablePoint>());
            //    series_buffer[jjPoint.serie].Add(new ObservablePoint(jjPoint.x, jjPoint.y));
            //}

            //foreach (var item in series_buffer)
            //{
            //    series[item.Key].valeurs.AddRange(item.Value);
            //    item.Value.Clear();
            //}
        }

        internal void UpdateAllYs(int[] valeurs)
        {
            for (int i = 0; i < valeurs.Length; i++)
            {
                int item = valeurs[i];
                serie.valeurs[i].Y = item;
            }
        }
    }
}
