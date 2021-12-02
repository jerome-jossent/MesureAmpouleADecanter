using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenCVSharpJJ.Processing.ImPr_Converter;

namespace OpenCVSharpJJ.Processing
{
    public abstract class ImPr
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract ImPrType imPrType { get; }

        public bool hasToBeBinary;
        public bool hasToBeGray; //1 layer

        public Mat Input;
        public Mat Output;

        public virtual void Process() { }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
