using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shapez2ModConfig
{
    public interface IConfigWidgetBuilder
    {
        void create(Transform parent, HUDDialogModSettings parentComponent, string id, IConfigEntry configEntry);
    }
    public interface IConfigWidgetBuilder<T> : IConfigWidgetBuilder
    {
        void create(Transform parent, HUDDialogModSettings parentComponent, string id, ConfigEntry<T> configEntry);
    }
}
