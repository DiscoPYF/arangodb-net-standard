using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ArangoDBNetStandard.Serialization
{
    /// <summary>
    /// Implements a <see cref="IApiClientSerialization"/> that uses Json.NET.
    /// </summary>
    public class JsonNetApiClientSerialization : IApiClientSerialization
    {
        private readonly static IDictionary<System.Type, string[]> _specialPropertyNamesByType =
            new Dictionary<System.Type, string[]>()
            {
                {
                    typeof(CursorApi.Models.PostCursorBody),
                    new string[]{ nameof(CursorApi.Models.PostCursorBody.BindVars) }
                },
                {
                    typeof(TransactionApi.Models.PostTransactionBody),
                    new string[]{ nameof(TransactionApi.Models.PostTransactionBody.Params) }
                }
            };

        /// <summary>
        /// Deserializes the JSON structure contained by the specified stream
        /// into an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="stream">The stream containing the JSON structure to deserialize.</param>
        /// <returns></returns>
        public virtual T DeserializeFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default(T);
            }

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();

                T result = js.Deserialize<T>(jtr);

                return result;
            }
        }

        /// <summary>
        /// Serializes the specified object to a JSON string encoded as UTF-8 bytes,
        /// following the provided rules for camel case property name and null value handling.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="item">The object to serialize.</param>
        /// <param name="useCamelCasePropertyNames">Whether property names should be camel cased (camelCase).</param>
        /// <param name="ignoreNullValues">Whether null values should be ignored.</param>
        /// <returns></returns>
        public virtual byte[] Serialize<T>(
            T item,
            bool useCamelCasePropertyNames,
            bool ignoreNullValues)
        {
            string json;

            if (_specialPropertyNamesByType.ContainsKey(typeof(T)))
            {
                var jsonSerializer = new JsonSerializer()
                {
                    NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var specialTypeJObject = JObject.FromObject(item, jsonSerializer);

                foreach (string specialPropertyName in _specialPropertyNamesByType[typeof(T)])
                {
                    var propertyInfo = typeof(T).GetProperty(specialPropertyName);

                    var specialPropertyValueJObject = JObject.FromObject(propertyInfo.GetValue(item));

                    string propertyNameToReplace = char.ToLowerInvariant(specialPropertyName[0]) +
                        specialPropertyName.Substring(1);

                    specialTypeJObject[propertyNameToReplace] = specialPropertyValueJObject;
                }

                json = specialTypeJObject.ToString(Formatting.None);
            }
            else
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include
                };

                if (useCamelCasePropertyNames)
                {
                    jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }

                json = JsonConvert.SerializeObject(item, jsonSettings);
            }

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
