using Core.Logging;
using Game.Core.Modding;
using HarmonyLib;
using Shapez2UILib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Core.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace Shapez2ModConfig
{
    public class Shapez2ModConfig : IMod
    {
        public static Core.Logging.ILogger logger;
        private readonly Harmony harmony;
        public static readonly Sprite SettingsSprite = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "Settings");
        public static PrefabReference<HUDDialogModSettings> DialogModSettings { get; private set; }
        public static readonly string ModConfigPath = Path.Join(GameEnvironment.DataPath, "mod configs");
        public Shapez2ModConfig(Core.Logging.ILogger logger)
        {
            Shapez2ModConfig.logger = logger;
            harmony = new Harmony("bknibb.Shapez2ModConfig");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmony.PatchAll(typeof(Shapez2ModConfig));
            Directory.CreateDirectory(ModConfigPath);
            UIHook.HookUIConstructor<HUDModMenuEntry>(ModMenuEntryConstructor);
            ModConfig.RegisterGlobalWidgetBuilder(new StringWidgetBuilder());
            ModConfig.RegisterGlobalWidgetBuilder(new BoolWidgetBuilder());
            UIDialogFactory.CreateDialog<HUDDialogModSettings>(CreateDialog, dialog => DialogModSettings = dialog, "Mod Settings Dialog", new Core.Localization.TranslationId("menu.modsettings.title"));
        }
        public void Dispose()
        {
            harmony.UnpatchSelf();
        }
        private void ModMenuEntryConstructor(HUDModMenuEntry hudModMenuEntry)
        {
            var manifest = hudModMenuEntry.GetDependencyResolver().Resolve<ModManifest>();
            if (!(hudModMenuEntry.GetDependencyResolver().Resolve<IModdingFrameworkEnvironment>().Context.ExecutableMods.Cast<ExecutableMod?>().FirstOrDefault(mod => mod.Value.Metadata == manifest) is ExecutableMod mod)) return;
            if (!ModConfig.TryGetByModClass(mod.EntryPoint.GetType(), out ModConfig modConfig)) return;
            var dialogStack = hudModMenuEntry.GetDependencyResolver().Resolve<IHUDDialogStack>();
            var iconButton = UIFactory.AddIconButton(hudModMenuEntry.transform.GetChild(1), hudModMenuEntry, true, SettingsSprite);
            iconButton.name = "ModSettingsButton";
            var layoutElement = iconButton.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 48;
            layoutElement.minWidth = 48;
            iconButton.OnClick.AddListener(() =>
            {
                var dialog = dialogStack.Show<HUDDialogModSettings>(DialogModSettings);
                dialog.SetConfig(modConfig);
            });
        }
        private static void CreateDialog(HUDDialogModSettings hudDialogModSettings, Transform contentsTransform)
        {
            var scrollContainer = UIFactory.AddScrollContainer(contentsTransform, hudDialogModSettings);
            var scrollContainerRectTransform = scrollContainer.GetComponent<RectTransform>();
            scrollContainerRectTransform.anchorMin = Vector2.zero;
            scrollContainerRectTransform.anchorMax = Vector2.one;
            scrollContainerRectTransform.offsetMax = new Vector2(-20f, 0f);
            scrollContainerRectTransform.offsetMin = new Vector2(20f, 0f);
            var container = scrollContainer.GetComponent<ScrollRect>().content;
            hudDialogModSettings.container = container;
        }
    }
}
