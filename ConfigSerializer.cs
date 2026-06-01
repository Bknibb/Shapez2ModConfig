using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2ModConfig
{
    public abstract class ConfigSerializer<T>
    : IConfigSerializer<T>
    {
        public abstract JToken Serialize(T value);

        public abstract T DeserializeTyped(JToken token);

        JToken IConfigSerializer.Serialize(object value)
            => Serialize((T)value);

        object IConfigSerializer.Deserialize(JToken token)
            => DeserializeTyped(token);
    }
}
