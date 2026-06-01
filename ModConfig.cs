using Core.Events;
using Core.Localization;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shapez2ModConfig
{
    public class ModConfig
    {
        private readonly Dictionary<string, IConfigEntry> entries = new Dictionary<string, IConfigEntry>();
        private readonly Dictionary<Type, IConfigSerializer> _serializers = new Dictionary<Type, IConfigSerializer>();
        private static readonly Dictionary<Type, IConfigSerializer> _globalSerializers = new Dictionary<Type, IConfigSerializer>();
        private readonly Dictionary<Type, IConfigWidgetBuilder> _widgetBuilders = new Dictionary<Type, IConfigWidgetBuilder>();
        private static readonly Dictionary<Type, IConfigWidgetBuilder> _globalWidgetBuilders = new Dictionary<Type, IConfigWidgetBuilder>();
        private static readonly Dictionary<string, ModConfig> modConfigById = new Dictionary<string, ModConfig>();
        private static readonly Dictionary<Type, ModConfig> modConfigByModClass = new Dictionary<Type, ModConfig>();
        public readonly MultiRegisterEvent OnChanged = new MultiRegisterEvent();
        public string Id { get; private set; }
        public Type ModClass { get; private set; }
        public string Path { get; private set; }
        public ModConfig(string id, Type modClass)
        {
            Id = id;
            ModClass = modClass;
            Path = System.IO.Path.Join(Shapez2ModConfig.ModConfigPath, id + ".json");
            modConfigById[id] = this;
            modConfigByModClass[modClass] = this;
        }
        public static ModConfig GetById(string id)
        {
            return modConfigById[id];
        }
        public static bool TryGetById(string id, out ModConfig modConfig)
        {
            return modConfigById.TryGetValue(id, out modConfig);
        }
        public static ModConfig GetByModClass(Type type)
        {
            return modConfigByModClass[type];
        }
        public static bool TryGetByModClass(Type type, out ModConfig modConfig)
        {
            return modConfigByModClass.TryGetValue(type, out modConfig);
        }
        public void RegisterSerializer<T>(IConfigSerializer<T> serializer)
        {
            _serializers[typeof(T)] = serializer;
        }
        public static void RegisterGlobalSerializer<T>(IConfigSerializer<T> serializer)
        {
            _globalSerializers[typeof(T)] = serializer;
        }
        public void RegisterWidgetBuilder<T>(IConfigWidgetBuilder<T> widgetBuilder)
        {
            _widgetBuilders[typeof(T)] = widgetBuilder;
        }
        public static void RegisterGlobalWidgetBuilder<T>(IConfigWidgetBuilder<T> widgetBuilder)
        {
            _globalWidgetBuilders[typeof(T)] = widgetBuilder;
        }
        public IConfigSerializer GetSerializer(Type type)
        {
            if (_serializers.TryGetValue(type, out var serializer)) return serializer;
            if (_globalSerializers.TryGetValue(type, out var globalSerializer)) return globalSerializer;
            if (type.IsEnum)
            {
                serializer = CreateEnumSerializer(type);
                _serializers[type] = serializer;
                return serializer;
            }
            var generatedType = typeof(JsonNetSerializer<>).MakeGenericType(type);
            serializer = (IConfigSerializer)Activator.CreateInstance(generatedType)!;
            _serializers[type] = serializer;
            return serializer;
        }
        private static IConfigSerializer CreateEnumSerializer(Type enumType)
        {
            var genericType =
                typeof(EnumAsStringSerializer<>)
                    .MakeGenericType(enumType);

            return (IConfigSerializer)
                Activator.CreateInstance(genericType)!;
        }
        public IConfigWidgetBuilder? GetWidgetBuilder(Type type)
        {
            if (_widgetBuilders.TryGetValue(type, out var widgetBuilder)) return widgetBuilder;
            if (_globalWidgetBuilders.TryGetValue(type, out widgetBuilder)) return widgetBuilder;
            if (type.IsEnum)
            {
                widgetBuilder = CreateEnumWidgetBuilder(type);
                _widgetBuilders[type] = widgetBuilder;
                return widgetBuilder;
            }
            if (type.IsNumber())
            {
                widgetBuilder = CreateNumberWidgetBuilder(type);
                _widgetBuilders[type] = widgetBuilder;
                return widgetBuilder;
            }
            return null;
        }
        private static IConfigWidgetBuilder CreateEnumWidgetBuilder(Type enumType)
        {
            var genericType =
                typeof(EnumWidgetBuilder<>)
                    .MakeGenericType(enumType);

            return (IConfigWidgetBuilder)
                Activator.CreateInstance(genericType)!;
        }
        private static IConfigWidgetBuilder CreateNumberWidgetBuilder(Type enumType)
        {
            var genericType =
                typeof(NumberWidgetBuilder<>)
                    .MakeGenericType(enumType);

            return (IConfigWidgetBuilder)
                Activator.CreateInstance(genericType)!;
        }
        public void Load()
        {
            if (!File.Exists(Path)) return;
            var root = JObject.Parse(File.ReadAllText(Path));
            foreach (var entry in entries)
            {
                if (root.TryGetValue(entry.Key, out var token)) entry.Value.Deserialize(token);
            }
            OnChanged.Invoke();
        }
        public void Save()
        {
            var root = new JObject();
            foreach (var entry in entries)
            {
                root[entry.Key] = entry.Value.Serialize();
            }
            File.WriteAllText(Path, root.ToString(Newtonsoft.Json.Formatting.Indented));
        }
        public ConfigEntry<T> RegisterEntry<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (entries.ContainsKey(key)) throw new Exception($"An entry with the key {key} has already been registered to this mod config");
            var entry = new ConfigEntry<T>(defaultValue, this);
            entries[key] = entry;
            return entry;
        }
        public SliderConfigEntry<T> RegisterSliderEntry<T>(string key, T defaultValue, T min, T max, T step)
        {
            if (!typeof(T).IsNumber()) throw new ArgumentException("generic argument T is not a numeric type");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (entries.ContainsKey(key)) throw new Exception($"An entry with the key {key} has already been registered to this mod config");
            var entry = new SliderConfigEntry<T>(defaultValue, this, min, max, step);
            entries[key] = entry;
            return entry;
        }
        public InputConfigEntry<T> RegisterInputEntry<T>(string key, T defaultValue, T min, T max, bool allowDecimal)
        {
            if (!typeof(T).IsNumber()) throw new ArgumentException("generic argument T is not a numeric type");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (entries.ContainsKey(key)) throw new Exception($"An entry with the key {key} has already been registered to this mod config");
            var entry = new InputConfigEntry<T>(defaultValue, this, min, max, allowDecimal);
            entries[key] = entry;
            return entry;
        }
        public TextBoxConfigEntry RegisterTextBoxEntry(string key, string defaultValue, int maxLength)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (entries.ContainsKey(key)) throw new Exception($"An entry with the key {key} has already been registered to this mod config");
            var entry = new TextBoxConfigEntry(defaultValue, this, maxLength);
            entries[key] = entry;
            return entry;
        }
        public DropdownConfigEntry<T> RegisterDropdownEntry<T>(string key, T defaultValue, Dictionary<T, IText> values)
        {
            if (!typeof(T).IsEnum && typeof(T) != typeof(string)) throw new ArgumentException("generic argument T is not a enum or string");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (entries.ContainsKey(key)) throw new Exception($"An entry with the key {key} has already been registered to this mod config");
            var entry = new DropdownConfigEntry<T>(defaultValue, this, values);
            entries[key] = entry;
            return entry;
        }
        public ConfigEntry<T> GetEntry<T>(string key)
        {
            return (ConfigEntry<T>)entries[key];
        }
        public IReadOnlyDictionary<string, IConfigEntry> GetEntries()
        {
            return entries;
        }
    }
}
