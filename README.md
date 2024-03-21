[![Download](https://img.shields.io/badge/-Download-blue.svg)](https://github.com/KinexDev/Poky-Mod-Manager/releases)
[![Discord](https://img.shields.io/badge/-Discord-yellow.svg)](https://discord.gg/NM7kQXZH)

![](https://github.com/KinexDev/Poky-Mod-Manager/blob/main/PokyModManager.png)

# Poky Mod Manager
Poky Mod Manager is a simple mod manager for peaks of yore made in bepinex, it allows enabling, disabling and configurating mods easily.

Poky is still in development so expect bugs.


![](https://github.com/KinexDev/Poky-Mod-Manager/blob/main/POKManager.gif)

# Examples

![Custom FOV](https://github.com/KinexDev/CustomFOV)

![Freecam](https://github.com/KinexDev/Freecam)

# Documentation

Template for poky mods is available ![here](https://github.com/KinexDev/Poky-Mod-Template).

If you need help setting up bepinex look at this [![resource](resource)](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/1_setup.html).

To create a mod, you will need to create a new mod class.

```cs
using POKModManager;

namespace Mod
{
    public class TemplateMod : ModClass
    {
    }
}
```

and then you will need to register the new mod with using bepinex.

```cs
using BepInEx;
using POKModManager;

namespace Mod
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            POKManager.RegisterMod(new TemplateMod(), MOD_NAME, MOD_VERSION, MOD_DESCRIPTION);
        }
    }
}
```

POKManager.RegisterMod is a function that adds the mod into the mod manager.
It has 4 parameters,
The mod (ModClass), 
mod name (string), 
mod version (string), 
mod description (string) 
and then mod properties (param string[])

only three data types are currently supported with mod properties - Ints, floats and booleans.

```cs
using POKModManager;

namespace Mod
{
    public class TemplateMod : ModClass
    {
            [POKRange(0, 10)] public int TestInt { get; set; }
            [POKRange(0, 10)] public float TestFloat { get; set; }
            public bool TestBool { get; set; }
    }
}
```

Ints and floats are required to have the attribute "POKRange", it has 2 parameters, min and max.

There is also another attribute called "DoNotSave", this will not save the modified values.

This is how you would register with the mod properties,

```cs
using BepInEx;
using POKModManager;

namespace Mod
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            POKManager.RegisterMod(new TemplateMod(), MOD_NAME, MOD_VERSION, MOD_DESCRIPTION, "TestInt", "TestFloat", "TestBool");
        }
    }
}
```
