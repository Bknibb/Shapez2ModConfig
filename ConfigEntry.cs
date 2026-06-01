using Core.Events;
using Core.Localization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2ModConfig
{
    public class ConfigEntry<T> : IConfigEntry
    {
        public T Value {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                onChangedEvent.Invoke(value);
            }
        }
        private T value;
        public T DefaultValue { get; }
        public Type ValueType => typeof(T);
        public readonly ModConfig ModConfig;
        private readonly MultiRegisterEvent<T> onChangedEvent = new MultiRegisterEvent<T>();
        public IEvent<T> OnChanged => onChangedEvent;
        public ConfigEntry(T defaultValue, ModConfig modConfig)
        {
            value = defaultValue;
            DefaultValue = defaultValue;
            ModConfig = modConfig;
        }

        public JToken Serialize()
        {
            return ((IConfigSerializer<T>)
                ModConfig.GetSerializer(typeof(T)))
                .Serialize(Value);
        }

        public void Deserialize(JToken token)
        {
            try
            {
                Value = ((IConfigSerializer<T>)
                ModConfig.GetSerializer(typeof(T)))
                .DeserializeTyped(token);
            } catch (Exception)
            {
                Value = DefaultValue;
            }
        }
    }
    public sealed class SliderConfigEntry<T> : ConfigEntry<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public T Step { get; set; }
        public SliderConfigEntry(T defaultValue, ModConfig modConfig, T min, T max, T step) : base(defaultValue, modConfig)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
    public sealed class InputConfigEntry<T> : ConfigEntry<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public bool AllowDecimal { get; set; }
        public InputConfigEntry(T defaultValue, ModConfig modConfig, T min, T max, bool allowDecimal) : base(defaultValue, modConfig)
        {
            Min = min;
            Max = max;
            AllowDecimal = allowDecimal;
        }
    }
    public sealed class TextBoxConfigEntry : ConfigEntry<string>
    {
        public int MaxLength { get; set; }
        public TextBoxConfigEntry(string defaultValue, ModConfig modConfig, int maxLength) : base(defaultValue, modConfig)
        {
            MaxLength = maxLength;
        }
    }
    public sealed class DropdownConfigEntry<T> : ConfigEntry<T>
    {
        public Dictionary<T, IText> Values { get; set; }
        public DropdownConfigEntry(T defaultValue, ModConfig modConfig, Dictionary<T, IText> values) : base(defaultValue, modConfig)
        {
            Values = values;
        }
    }
}
