using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class Index_Hauteur_Manager
    {
        public List<Index_Hauteur> coordonnees = new List<Index_Hauteur>() {
                new Index_Hauteur(0,   0.05f),
                new Index_Hauteur(10,  2.30f ),
                new Index_Hauteur(20,  4.75f ),
                new Index_Hauteur(30,  7.25f ),
                new Index_Hauteur(40,  9.75f ),
                new Index_Hauteur(50,  12.20f),
                new Index_Hauteur(60,  14.65f),
                new Index_Hauteur(70,  17.15f),
                new Index_Hauteur(80,  19.65f),
                new Index_Hauteur(90,  22.15f),
                new Index_Hauteur(100, 24.65f),
                new Index_Hauteur(110, 27.12f),
                new Index_Hauteur(120, 29.60f),
                new Index_Hauteur(130, 32.50f),
                new Index_Hauteur(140, 34.55f),
                new Index_Hauteur(150, 37.20f),
                new Index_Hauteur(160, 39.50f)};

        public float _GetHauteur(int index)
        {
            try
            {
                for (int i = 0; i < coordonnees.Count;)
                {
                    Index_Hauteur ih = coordonnees[i];
                    if (ih.index_en_partantdubas == index)
                        return ih.hauteur_en_partantdubas;
                    if (ih.index_en_partantdubas < index)
                    {
                        //se trouve entre les 2 bornes
                        float index_min = coordonnees[i].index_en_partantdubas;
                        float index_max = coordonnees[i + 1].index_en_partantdubas;

                        float h_min = coordonnees[i].hauteur_en_partantdubas;
                        float h_max = coordonnees[i + 1].hauteur_en_partantdubas;

                        //y=a.x+b
                        float val = YEgalAXPlusB((float)index, index_min, index_max, h_min, h_max);
                        return val;
                    }
                }
                throw new Exception("GetHauteur : out of bounds index = " + index);
            }
            catch (Exception ex)
            {
                ex = ex;
                return 66;
            }
        }

        //y = a*x+b
        float YEgalAXPlusB(float x, float x0, float x1, float y0, float y1)
        {
            float a = (y1 - y0) / (x1 - x0);
            float b = y0 - a * x0;
            return a * x + b;
        }
    }
}
