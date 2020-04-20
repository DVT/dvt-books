using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace DVTBooks.API.Serialization
{
    public class JsonLowerCaseUnderscoreContractResolver : DefaultContractResolver
    {
        private Regex _regex = new Regex("(?!(^[A-Z][a-z]))([A-Z][a-z])");

        protected override string ResolvePropertyName(string propertyName)
        {
            return _regex.Replace(propertyName, "_$2").ToLower();
        }
    }
}
