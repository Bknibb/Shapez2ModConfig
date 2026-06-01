using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shapez2ModConfig
{
    public abstract class ConfigWidgetBuilder<T> : IConfigWidgetBuilder<T>
    {
        public abstract void create(Transform parent, HUDDialogModSettings parentComponent, string id, ConfigEntry<T> configEntry);

        public void create(Transform parent, HUDDialogModSettings parentComponent, string id, IConfigEntry configEntry)
        {
            create(parent, parentComponent, id, (ConfigEntry<T>)configEntry);
        }
    }
}
