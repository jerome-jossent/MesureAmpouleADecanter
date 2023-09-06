using ScottPlot;
using ScottPlot.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace OpenCVSharpJJ
{
    public partial class Plot_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string decantation_speed
        {
            get => a_minimum == null ? "-" : ((double)a_minimum).ToString("F2");
        }



        bool autoScale = true;
        ScottPlot.Plottable.DataLogger logger1;

        double? a_minimum
        {
            get => _a_minimum;
            set
            {
                if (_a_minimum == value) return;

                _a_minimum = value;
                OnPropertyChanged("decantation_speed");
            }
        }
        double? _a_minimum = 1;

        double b_ou_a_est_minimum;

        DateTime t0;
        float val_mm = 200;
        float dt = 0.01f;
        float speed_mm_par_sec = -10;

        DispatcherTimer timer;



        public Plot_UC()
        {
            InitializeComponent();
            DataContext = this;

            t0 = DateTime.Now;

            WpfPlot1.Plot.Style(new ScottPlot.Styles.Gray1());

            logger1 = WpfPlot1.Plot.AddDataLogger();
            logger1.MarkerSize = 5;
            logger1.Color = System.Drawing.Color.Lime;

            //tangeanteMax
            b_ou_a_est_minimum = val_mm;

            var func1 = new Func<double, double?>((x) => a_minimum * x + b_ou_a_est_minimum);
            WpfPlot1.Plot.AddFunction(func1,
                color: System.Drawing.Color.Orange,
                lineWidth: 2,
                lineStyle: LineStyle.Dash);


            WpfPlot1.MouseDown += WpfPlot1_MouseDown;
            WpfPlot1.MouseDoubleClick += WpfPlot1_MouseDoubleClick;

            Simulator_Start();
        }

        #region interaction avec graphique 
        DateTime lastdoubleclick;
        TimeSpan doubleclcik_time_ms = TimeSpan.FromSeconds(0.3);

        void WpfPlot1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            lastdoubleclick = DateTime.Now;
            AutoScale(true);
            WpfPlot1.Plot.Benchmark(false);
        }

        void WpfPlot1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DateTime time = DateTime.Now;
            if (time - lastdoubleclick < doubleclcik_time_ms)
                WpfPlot1.Plot.Benchmark(false);
            else
                AutoScale(false);

            lastdoubleclick = time;
        }

        void AutoScale(bool val)
        {
            autoScale = val;
            logger1.ManageAxisLimits = val;
        }
        #endregion

        #region simulateur
        void Simulator_Start()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(dt);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            SimulateNewData();
        }

        void SimulateNewData()
        {
            DateTime t = DateTime.Now;
            double x = (t - t0).TotalSeconds;

            val_mm += (float)Math.Sin(Math.PI / 180 * x * speed_mm_par_sec);

            _Add(x, val_mm);
        }
        #endregion

        #region ADD DATA
        public void _Add(MesureAmpouleADecanter.PointJJ point)
        {
            _Add(point.t, point.z_mm);
        }

        public void _Add(double t, double val)
        {
            CalculPenteMaximum(t, val);
            logger1.Add(t, val);
            Update();
        }

        public void _Clear()
        {
            logger1.Clear();
            a_minimum = null;
            Update();
        }
        #endregion

        void Update()
        {
            if (a_minimum == null) return;

            if (autoScale)
                WpfPlot1.Plot.AxisAuto();

            WpfPlot1.Refresh();
        }

        #region calcul pente
        List<double> last_t = new List<double>();
        List<double> last_val = new List<double>();
        int nbr_bal = 5;

        void CalculPenteMaximum(double t, double val)
        {
            //fenêtre glissante
            last_t.Add(t);
            while (last_t.Count > nbr_bal) { last_t.RemoveAt(0); }

            last_val.Add(val);
            while (last_val.Count > nbr_bal) { last_val.RemoveAt(0); }

            //régression linéaire
            AB ab = LinearRegression(last_t, last_val);

            if (!(ab.a is double.NaN))
            {
                //stockage de la pente la plus forte
                if (a_minimum == null)
                    a_minimum = ab.a;

                if (ab.a < a_minimum)
                {
                    a_minimum = ab.a;
                    b_ou_a_est_minimum = ab.b;
                }
            }
        }

        public AB LinearRegression(List<double> Xs, List<double> Ys)
        {
            List<PointD> points = new List<PointD>();
            for (int i = 0; i < Xs.Count; i++)
                points.Add(new PointD(Xs[i], Ys[i]));

            return LinearRegression(points);
        }

        public AB LinearRegression(List<PointD> points)
        {
            double a = vv.Variance(points, p => p.X, p => p.Y) / vv.Variance(points, p => p.X, p => p.X);
            double b = points.Average(p => p.Y) - a * points.Average(p => p.X);
            return new AB(a, b);
        }

        public class PointD
        {
            public double X { get; set; }
            public double Y { get; set; }
            public PointD(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public class AB
        {
            public double a { get; set; }
            public double b { get; set; }
            public AB(double a, double b)
            {
                this.a = a;
                this.b = b;
            }
        }
        #endregion
    }

    public static class vv
    {
        public static double Variance<T>(this IEnumerable<T> list, Func<T, double> selectA, Func<T, double> selectB)
        {
            return list.Average(p => selectA(p) * selectB(p)) - (list.Average(p => selectA(p)) * list.Average(p => selectB(p)));
        }
    }
}