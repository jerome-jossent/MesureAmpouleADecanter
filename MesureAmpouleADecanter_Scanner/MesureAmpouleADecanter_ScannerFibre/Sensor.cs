using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class Sensor : INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public int? numero { get; set; }

        public int x { get; set; }
        public int y { get; set; }

        public float hauteur_mm { get; set; }

        [JsonIgnore]
        public float intensity { get; set; }
        public float intensity_min { get; set; } = -1;
        public float intensity_max { get; set; } = -1;
        [JsonIgnore]
        public float intensity_threshold { get; set; }
        public Vec3b pixelValue_intensity_min { get; set; }
        public Vec3b pixelValue_intensity_max { get; set; }

        [JsonIgnore]
        public bool ON { get => intensity > intensity_threshold; }
        [JsonIgnore]
        public bool ON_previous;

        [JsonIgnore]
        public Scalar color_scalar { get; set; }
        [JsonIgnore]
        public Vec3b pixelValue { get; set; }
        [JsonIgnore]
        public Vec3b pixelValueNormalized { get; set; }
        public static int SensorsNextIndex { get; private set; }

        [JsonIgnore]
        public Cercle cercle;

        [JsonIgnore]
        internal Sensor_UC uc;

        public Sensor() { }

        public Sensor(Cercle c)
        {
            cercle = c;
            c.sensor = this;
            numero = c.numero;
            Save();
        }




        public float[] normalisation_a = new float[] { 0, 0, 0 }, normalisation_b = new float[] { 0, 0, 0 };



        void ComputeNormalizationFactors()
        {
            try
            {
                normalisation_a[0] = (pixelValue_intensity_max.Item0 - pixelValue_intensity_min.Item0) / 255f;
                normalisation_a[1] = (pixelValue_intensity_max.Item1 - pixelValue_intensity_min.Item1) / 255f;
                normalisation_a[2] = (pixelValue_intensity_max.Item2 - pixelValue_intensity_min.Item2) / 255f;

                normalisation_b[0] = -normalisation_a[0] * pixelValue_intensity_min.Item0;
                normalisation_b[1] = -normalisation_a[1] * pixelValue_intensity_min.Item1;
                normalisation_b[2] = -normalisation_a[2] * pixelValue_intensity_min.Item2;
            }
            catch (Exception ex)
            {

            }
        }

        public void SetMeasure(Vec3b pixelValue)
        {
            this.pixelValue = pixelValue;
            color_scalar = new Scalar(pixelValue.Item0, pixelValue.Item1, pixelValue.Item2);
            intensity = (float)(((float)pixelValue.Item0 + pixelValue.Item1 + pixelValue.Item2) / (255 * 3));

            if (intensity_min > intensity || intensity_min == -1)
            {
                intensity_min = intensity;
                pixelValue_intensity_min = pixelValue;
                ComputeNormalizationFactors();
            }
            if (intensity_max < intensity || intensity_max == -1)
            {
                intensity_max = intensity;
                pixelValue_intensity_max = pixelValue;
                ComputeNormalizationFactors();
            }

            intensity_threshold = (intensity_max + intensity_min) / 2;

            if (numero == null)
            {
                if (ON_previous != ON)
                {
                    numero = Sensor.SensorsNextIndex;
                    SensorsNextIndex++;
                    uc._SetIndexName((int)numero);
                    cercle?.SetNumero((int)numero);
                }
            }

            ON_previous = ON;

            pixelValueNormalized = ComputeNormalizationPixel(pixelValue);

            uc?._Update(pixelValue);
            uc?._UpdateNormalized(pixelValueNormalized);
        }

        Vec3b ComputeNormalizationPixel(Vec3b pixelValue)
        {
            Vec3b N = new Vec3b();
            N.Item0 = (byte)(normalisation_a[0] * pixelValue.Item0 + normalisation_b[0]);
            N.Item1 = (byte)(normalisation_a[1] * pixelValue.Item1 + normalisation_b[1]);
            N.Item2 = (byte)(normalisation_a[2] * pixelValue.Item2 + normalisation_b[2]);
            return N;
        }

        public static void ResetSensorsOrder()
        {
            SensorsNextIndex = 0;
        }

        internal void Save()
        {
            if (cercle != null)
            {
                x = cercle.x;
                y = cercle.y;
            }
        }

        internal void Load()
        {

        }

        internal void Reset()
        {
            uc._index = "?";
            numero = null;
        }
    }
}
