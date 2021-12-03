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
        public enum ImPrType { vide, Rotation, Resize, ROI, SelectLayer, Flip, Canny, RGB_to_Gray, InRange, Threshold, HoughLines, HoughCircles }
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
                case ImPrType.Rotation:
                    return new ImPr_Rotation();
                case ImPrType.Resize:
                    return new ImPr_Resize();

                case ImPrType.ROI:
                    throw new NotImplementedException();
                case ImPrType.SelectLayer:
                    throw new NotImplementedException();
                case ImPrType.Flip:
                    throw new NotImplementedException();
                case ImPrType.Canny:
                    throw new NotImplementedException();
                case ImPrType.RGB_to_Gray:
                    throw new NotImplementedException();
                case ImPrType.InRange:
                    throw new NotImplementedException();
                case ImPrType.Threshold:
                    throw new NotImplementedException();
                case ImPrType.HoughLines:
                    throw new NotImplementedException();
                case ImPrType.HoughCircles:
                    throw new NotImplementedException();

                case ImPrType.vide:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
