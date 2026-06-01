using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2ModConfig
{
    public sealed class EnumAsStringSerializer<T> : ConfigSerializer<T> where T : struct, Enum
    {
        public override JToken Serialize(T value)
        {
            return new JValue(value.ToString());
        }

        public override T DeserializeTyped(JToken token)
        {
            var str = token.Value<string>();

            if (Enum.TryParse<T>(str, ignoreCase: true, out var result))
                return result;

            throw new JsonException($"Invalid enum value '{str}' for {typeof(T).Name}");
        }
    }
}
