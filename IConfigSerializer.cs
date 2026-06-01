using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2ModConfig
{
    public interface IConfigSerializer
    {
        JToken Serialize(object value);

        object Deserialize(JToken token);
    }
    public interface IConfigSerializer<T> : IConfigSerializer
    {
        JToken Serialize(T value);

        T DeserializeTyped(JToken token);
    }

}
