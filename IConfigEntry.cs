using Core.Events;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2ModConfig
{
    public interface IConfigEntry
    {
        Type ValueType { get; }
        JToken Serialize();
        void Deserialize(JToken token);
    }
}
