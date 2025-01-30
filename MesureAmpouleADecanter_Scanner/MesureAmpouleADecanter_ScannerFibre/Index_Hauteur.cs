using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class Index_Hauteur
    {
        public int index_en_partantdubas;
        public float hauteur_en_partantdubas;

        public Index_Hauteur(int index_en_partantdubas, float hauteur_en_partantdubas)
        {
            this.index_en_partantdubas = index_en_partantdubas;
            this.hauteur_en_partantdubas = hauteur_en_partantdubas;
        }        
    }
}