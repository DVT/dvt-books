using DVTBooks.API.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DVTBooks.API.Extensions
{
    public static class JsonSerializerSettingsExtensions
    {
        public static JsonSerializerSettings Configure(this JsonSerializerSettings settings)
        {
            settings.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
            settings.Converters.Add(new StringEnumConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            settings.DateParseHandling = DateParseHandling.DateTimeOffset;

            return settings;
        }
    }
}
