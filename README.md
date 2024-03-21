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
You will also need to add a reference to the Poky Mod Manager dll.
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

the mod class has a field called "RunUpdateOnMenu", this will tell the mod manager if to run update in the menu, and another field called "Enabled", this is just says if the mod is enabled or not.

the mod manager can override many built in functions into mod class,

```
void OnEnabled() <- called when the game is started/when the mod is enabled
        
void OnDisabled() <- called when the game is started/when the mod is disabled
        
void SceneChange(int sceneIndex) <- called on scene change, is ran even if the mod is disabled
        
void GUIUpdate() <- called on OnGUI
        
void Start() <- called on start
        
void Update(float deltaTime) <- called on update
        
void FixedUpdate(float deltaTime) <- called on fixed update
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

mod description (string), 

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

This is how you would register the mod properties,

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

and thats it.
