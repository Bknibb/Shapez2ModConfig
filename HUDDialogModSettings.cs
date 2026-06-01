using Core.Dependency;
using Core.Localization;
using Shapez2UILib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Shapez2ModConfig
{
    public class HUDDialogModSettings : HUDDialog
    {
        [Construct]
        private void Construct()
        {
            OnClosed.Register(() =>
            {
                Config.Save();
                Config.OnChanged.Invoke();
            });
        }
        public void SetConfig(ModConfig config)
        {
            if (Config != null) return;
            Config = config;
            foreach (var entry in config.GetEntries())
            {
                var widgetBuilder = config.GetWidgetBuilder(entry.Value.ValueType);
                if (widgetBuilder == null) continue;
                GameObject entryObject = new GameObject(entry.Key);
                entryObject.layer = LayerMask.NameToLayer("UI");
                entryObject.transform.SetParent(container);
                entryObject.transform.localScale = Vector3.one;
                var layoutElement = entryObject.AddComponent<LayoutElement>();
                layoutElement.minHeight = 100;
                layoutElement.preferredHeight = 100;
                var text = UIFactory.AddLocalizedTextPrimary(entryObject.transform, this, true);
                RectTransform textRectTransform = text.gameObject.GetComponent<RectTransform>();
                textRectTransform.anchorMin = Vector2.zero;
                textRectTransform.anchorMax = new Vector2(0.5f, 1);
                textRectTransform.offsetMin = Vector2.zero;
                textRectTransform.offsetMax = Vector2.zero;
                text.Alignment = TMPro.TextAlignmentOptions.Left;
                text.Text = new RawText(entry.Key);
                GameObject entryContainer = new GameObject("Container");
                entryContainer.layer = LayerMask.NameToLayer("UI");
                entryContainer.transform.SetParent(entryObject.transform);
                entryContainer.transform.localScale = Vector3.one;
                RectTransform containerRectTransform = entryContainer.AddComponent<RectTransform>();
                containerRectTransform.anchorMin = new Vector2(0.5f, 0);
                containerRectTransform.anchorMax = Vector2.one;
                containerRectTransform.offsetMin = Vector2.zero;
                containerRectTransform.offsetMax = Vector2.zero;
                widgetBuilder.create(entryContainer.transform, this, entry.Key, entry.Value);
            }
        }
        public ModConfig Config;
        public Transform container;
    }
}
