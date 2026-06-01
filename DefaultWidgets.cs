using Core.Localization;
using Game.Core.Localization;
using Game.Core.Session;
using Shapez2UILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace Shapez2ModConfig
{
    public sealed class NumberWidgetBuilder<T> : ConfigWidgetBuilder<T>
    {
        public override void create(Transform parent, HUDDialogModSettings parentComponent, string id, ConfigEntry<T> configEntry)
        {
            if (configEntry is SliderConfigEntry<T> sliderConfigEntry)
            {
                var slider = UIFactory.AddSlider(parent, parentComponent, true);
                slider.Formatter = new HUDSliderControl.FormatterDelegate(value => new GenericFormattedNumberText(new GenericFloatFormatter(value, "0.##")));
                var min = (float)Convert.ChangeType(sliderConfigEntry.Min, typeof(float));
                var max = (float)Convert.ChangeType(sliderConfigEntry.Max, typeof(float));
                var step = (float)Convert.ChangeType(sliderConfigEntry.Step, typeof(float));
                slider.MinValue = min;
                slider.MaxValue = max;
                slider.SliderSteps = Convert.ToInt32((max - min) / step);
                slider.Value = (float)Convert.ChangeType(configEntry.Value, typeof(float));
                slider.OnChanged.Register(() => configEntry.Value = (T)Convert.ChangeType(slider.Value, typeof(T)));
            } else
            {
                var input = UIFactory.AddInputField(parent, parentComponent, true);
                TMP_InputField tmp_InputField = input.GetComponent<TMP_InputField>();
                bool allowDecimal = configEntry is InputConfigEntry<T> inputConfigEntry ? inputConfigEntry.AllowDecimal : (typeof(T) == typeof(float) || typeof(T) == typeof(decimal) || typeof(T) == typeof(double));
                tmp_InputField.characterValidation = allowDecimal ? TMP_InputField.CharacterValidation.Decimal : TMP_InputField.CharacterValidation.Integer;
                tmp_InputField.contentType = allowDecimal ? TMP_InputField.ContentType.DecimalNumber : TMP_InputField.ContentType.IntegerNumber;
                tmp_InputField.keyboardType = allowDecimal ? TouchScreenKeyboardType.DecimalPad : TouchScreenKeyboardType.NumberPad;
                input.Value = configEntry.Value.ToString();
                input.OnChange.AddListener(v =>
                {
                    float value = float.TryParse(v, out float f) ? f : (float)Convert.ChangeType(configEntry.Value, typeof(float));
                    if (configEntry is InputConfigEntry<T> inputConfig)
                    {
                        value = MathF.Min(MathF.Max(value, (float)Convert.ChangeType(inputConfig.Min, typeof(float))), (float)Convert.ChangeType(inputConfig.Max, typeof(float)));
                    }
                    configEntry.Value = (T)Convert.ChangeType(value, typeof(T));
                    input.Value = string.IsNullOrEmpty(v) ? "" : (v.EndsWith(".") ? value.ToString() + "." : value.ToString());
                });
            }
        }
    }
    public sealed class StringWidgetBuilder : ConfigWidgetBuilder<string>
    {
        public override void create(Transform parent, HUDDialogModSettings parentComponent, string id, ConfigEntry<string> configEntry)
        {
            if (configEntry is DropdownConfigEntry<string> dropdownConfigEntry)
            {
                var dropdown = UIFactory.AddDropdown(parent, parentComponent, true);
                dropdown.Options = dropdownConfigEntry.Values.Values;
                dropdown.Value = dropdownConfigEntry.Values.Keys.ToList().IndexOf(configEntry.Value);
                dropdown.OnValueChanged.AddListener(index => configEntry.Value = dropdownConfigEntry.Values.Keys.ToList()[index]);
            } else
            {
                var input = UIFactory.AddInputField(parent, parentComponent, true);
                if (configEntry is TextBoxConfigEntry textBoxConfigEntry)
                {
                    TMP_InputField tmp_InputField = input.GetComponent<TMP_InputField>();
                    tmp_InputField.characterLimit = textBoxConfigEntry.MaxLength;
                }
                input.Value = configEntry.Value;
                input.OnChange.AddListener(value => configEntry.Value = value);
            }
        }
    }
    public sealed class BoolWidgetBuilder : ConfigWidgetBuilder<bool>
    {
        public override void create(Transform parent, HUDDialogModSettings parentComponent, string id, ConfigEntry<bool> configEntry)
        {
            var toggle = UIFactory.AddToggle(parent, parentComponent, true);
            toggle.Value = configEntry.Value;
            toggle.OnChanged.Register(() => configEntry.Value = toggle.Value);
        }
    }
    public sealed class EnumWidgetBuilder<T> : ConfigWidgetBuilder<T> where T : struct, Enum
    {
        public override void create(Transform parent, HUDDialogModSettings parentComponent, string id, ConfigEntry<T> configEntry)
        {
            var enumSelector = UIFactory.AddEnumSelector(parent, parentComponent, true);
            if (configEntry is DropdownConfigEntry<T> dropdownConfigEntry)
            {
                enumSelector.Values = dropdownConfigEntry.Values.Values;
                enumSelector.CurrentValueIndex = dropdownConfigEntry.Values.Keys.ToList().IndexOf(configEntry.Value);
                enumSelector.OnValueChangeRequested.Register(index => configEntry.Value = dropdownConfigEntry.Values.Keys.ToList()[index]);
            } else
            {
                var names = Enum.GetNames(typeof(T));
                enumSelector.Values = names.Select(name => new RawText(name));
                enumSelector.CurrentValueIndex = names.IndexOf(Enum.GetName(typeof(T), configEntry.Value));
                enumSelector.OnValueChangeRequested.Register(index => configEntry.Value = (T)Enum.Parse(typeof(T), names[index]));
            }
        }
    }
}
