using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2ModConfig
{
    public sealed class JsonNetSerializer<T> : ConfigSerializer<T>
    {
        public override JToken Serialize(T value)
        {
            return JToken.FromObject(value);
        }
        public override T DeserializeTyped(JToken token)
        {
            return token.ToObject<T>();
        }
    }
}
