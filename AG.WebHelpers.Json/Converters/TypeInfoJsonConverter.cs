using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AG.WebHelpers.Json.Converters
{
    public class TypeInfoJsonConverter : JsonConverter
    {
        public Dictionary<Type, string> AlternateTypeNames;
        public string TypeFieldName = "Type";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObject = JObject.FromObject(value, serializer);

            jObject.AddFirst(new JProperty(TypeFieldName, GetTypeName(value.GetType())));
            jObject.WriteTo(writer);
        }

        private string GetTypeName(Type type)
        {
            string typeName;
            if(AlternateTypeNames == null || !AlternateTypeNames.TryGetValue(type, out typeName))
            {
                return type.Name;
            }
            return typeName;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
