using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIA;

namespace MesureAmpouleADecanter_Scanner
{
    public class Scanner
    {
        public int i;
        public DeviceInfo scanner;
        public string nom;
        public string description;
        public string port;

        public string name
        {
            get => ToString();
        }

        public Scanner(int i, DeviceInfo scanner)
        {
            this.i = i;
            this.scanner = scanner;
            nom = scanner.Properties["Name"].get_Value();
            description = scanner.Properties["Description"].get_Value();
            port = scanner.Properties["Port"].get_Value();
        }

        public override string ToString()
        {
            return nom + "\t" + description + "\t" + port;
        }
    }
}
