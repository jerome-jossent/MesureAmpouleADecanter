﻿using OpenCvSharp;
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
        public int x { get => (int)cercle.x; }
        public int y { get => (int)cercle.y; }

        public float intensity { get; set; }
        public float intensity_min { get; set; } = -1;
        public float intensity_max { get; set; } = -1;
        public float intensity_threshold { get; set; }

        public bool ON { get => intensity > intensity_threshold; }
        public bool ON_previous;

        public Scalar color_scalar { get; set; }
        public Vec3b pixelValue { get; set; }
        public static int SensorsNextIndex { get; private set; }

        public Cercle cercle;
        internal Sensor_UC uc;

        public Sensor(Cercle c)
        {
            cercle = c;
            c.sensor = this;
            numero = c.numero;
        }

        public void SetColor(Vec3b pixelValue)
        {
            this.pixelValue = pixelValue;
            color_scalar = new Scalar(pixelValue.Item0, pixelValue.Item1, pixelValue.Item2);
            intensity = (float)(((float)pixelValue.Item0 + pixelValue.Item1 + pixelValue.Item2) / (255 * 3));

            if (intensity_min == -1) intensity_min = intensity;
            if (intensity_max == -1) intensity_max = intensity;

            if (intensity_min > intensity) intensity_min = intensity;
            if (intensity_max < intensity) intensity_max = intensity;

            intensity_threshold = (intensity_max + intensity_min) / 2;

            if (numero == null)
            {
                if (ON_previous != ON)
                {
                    numero = Sensor.SensorsNextIndex;
                    SensorsNextIndex++;
                    uc._SetIndexName((int)numero);
                    cercle.SetNumero((int)numero);
                }
            }

            ON_previous = ON;

            uc?._Update(pixelValue);
        }

        public static void ResetSensorsOrder()
        {
            SensorsNextIndex = 0;
        }
    }
}
