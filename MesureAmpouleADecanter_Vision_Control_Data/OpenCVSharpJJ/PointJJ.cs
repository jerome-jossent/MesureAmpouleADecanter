using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCVSharpJJ
{
    public class PointJJ
    {
        public DateTime T { get; set; }
        public double t { get; set; }
        public string T_string
        {
            get
            {
                string chaine = T.ToString("yyyy/MM/dd HH:mm:ss.fff");
                return chaine;
            }
        }
        public float z_mm { get; set; }

        public int erreur_pixel { get; set; }

        public PointJJ(DateTime T, float z_mm, int erreur_pixel)
        {
            this.T = T;
            this.z_mm = z_mm;
            this.erreur_pixel = erreur_pixel;
            this.t = (T - MesureAmpouleADecanter.t0).TotalSeconds;
        }
        public PointJJ(float z_mm, int erreur_pixel)
        {
            T = DateTime.Now;
            this.z_mm = z_mm;
            this.erreur_pixel = erreur_pixel;
        }

        public override string ToString()
        {
            string ligne = "date,t(s),z(mm),erreur(pix)\n\r";

            return 
                T_string + "," +                
                t.ToString().Replace(",", ".") + "," +
                z_mm.ToString().Replace(",", ".") + "," +
                erreur_pixel;
        }
    }
}
