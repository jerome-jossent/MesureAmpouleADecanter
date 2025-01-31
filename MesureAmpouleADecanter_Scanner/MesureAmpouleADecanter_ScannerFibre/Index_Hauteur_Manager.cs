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
                new Index_Hauteur(0,   00.5f),
                new Index_Hauteur(10,  23.0f ),
                new Index_Hauteur(20,  47.5f ),
                new Index_Hauteur(30,  72.5f ),
                new Index_Hauteur(40,  97.5f ),
                new Index_Hauteur(50,  122.0f),
                new Index_Hauteur(60,  146.5f),
                new Index_Hauteur(70,  171.5f),
                new Index_Hauteur(80,  196.5f),
                new Index_Hauteur(90,  221.5f),
                new Index_Hauteur(100, 246.5f),
                new Index_Hauteur(110, 271.2f),
                new Index_Hauteur(120, 296.0f),
                new Index_Hauteur(130, 325.0f),
                new Index_Hauteur(140, 345.5f),
                new Index_Hauteur(150, 372.0f),
                new Index_Hauteur(160, 395.0f)};

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
