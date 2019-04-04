namespace DeadLine2019.Infrastructure
{
    using System;
    using System.IO;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class JsonExtensions
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss",
        };

        public static string ToJson<T>(this T obj)
        {
            return ToJson(obj, DefaultJsonSerializerSettings);
        }

        public static string ToJson<T>(this T obj, JsonSerializerSettings jsonSerializerSettings)
        {
            return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
        }

        public static T FromJson<T>(this string text)
        {
            return FromJson<T>(text, DefaultJsonSerializerSettings);
        }

        public static T FromJson<T>(this string text, T anonymousDefinition)
        {
            return FromJson<T>(text, DefaultJsonSerializerSettings);
        }

        public static T FromJson<T>(this string text, JsonSerializerSettings jsonSerializerSettings)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return JsonConvert.DeserializeObject<T>(text, jsonSerializerSettings);
        }

        public static T FromJson<T>(this string text, T anonymousDefinition, JsonSerializerSettings jsonSerializerSettings)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return JsonConvert.DeserializeAnonymousType(text, anonymousDefinition, jsonSerializerSettings);
        }

        public static object FromJson(this string text, Type type)
        {
            return FromJson(text, type, DefaultJsonSerializerSettings);
        }

        public static object FromJson(this string text, Type type, JsonSerializerSettings jsonSerializerSettings)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return JsonConvert.DeserializeObject(text, type, jsonSerializerSettings);
        }

        public static object FromJson(this Stream stream, Type type)
        {
            return FromJson(stream, type, DefaultJsonSerializerSettings);
        }

        public static object FromJson(this Stream stream, Type type, JsonSerializerSettings jsonSerializerSettings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var serializer = JsonSerializer.Create(jsonSerializerSettings);

            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize(jsonTextReader, type);
                }
            }
        }

        public static string ToJson(this object value, bool isIndent = true)
        {
            var formatting = isIndent ? Formatting.Indented : Formatting.None;

            return JsonConvert.SerializeObject(
                value,
                formatting,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }
    }
}
