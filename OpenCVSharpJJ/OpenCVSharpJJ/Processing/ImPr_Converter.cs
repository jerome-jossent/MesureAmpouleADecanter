using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenCVSharpJJ.Processing.ImPr;

namespace OpenCVSharpJJ.Processing
{
    public class ImPr_Converter : CustomCreationConverter<ImPr>
    {
        public enum ImPrType { vide, Rotation }
        ImPrType _imPrType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.ReadFrom(reader);
            _imPrType = jobj["Type"].ToObject<ImPrType>();
            return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
        }

        public override ImPr Create(Type objectType)
        {
            switch (_imPrType)
            {
                case ImPrType.vide:
                    throw new NotImplementedException();
                case ImPrType.Rotation:
                    return new ImPr_Rotation();
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
