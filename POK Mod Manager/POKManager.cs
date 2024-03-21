using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine.Events;

namespace POKModManager
{
    /// <summary>
    /// Mod class to get properties from
    /// </summary>
    public struct Mod
    {
        public ModClass ModClass;
        public string Name;
        public string Version;
        public string Description;
        public string[] Properties;
        public Mod(ModClass mod, string name, string version, string description, string[] properties)
        {
            ModClass = mod;
            Name = name;
            Version = version;
            Description = description;
            Properties = properties;
        }
    }
    /// <summary>
    /// Used for getting range of sliders
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class POKRange : Attribute
    {
        public int min;

        public int max;

        public POKRange(int Max)
        {
            min = 0;
            max = Max;
        }

        public POKRange(int Min, int Max)
        {
            min = Min;
            max = Max;
        }
    }

    /// <summary>
    /// Do not save modified values
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DoNotSave : Attribute
    {
    }

    /// <summary>
    /// Basic mod manager for enabling and disabling mods.
    /// </summary>
    [BepInPlugin("Data.POKManager", "POK Manager", "1.0.0")]
    public class POKManager : BaseUnityPlugin
    {
        public static POKManager instance;

        public static ManualLogSource ls;
        public static List<Mod> mods;
        static Font peaksOfYoreFont;
        static GameObject modMenu;
        static GameObject configMenu;
        static GameObject menu;
        static GameObject options;

        /// <summary>
        /// Register a mod to the manager
        /// </summary>
        public static void RegisterMod(ModClass Mod, string ModName, string Version, string Description, params string[] properties)
        {
            if (FindObjectOfType<POKManager>() != null)
            {
                if (properties.Length != 0)
                {
                    foreach (string property in properties)
                    {
                        PropertyInfo info = Mod.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);

                        if (info == null)
                        {
                            throw new NullReferenceException("Mod property doesn't exist!");
                        }

                        if (properties.ToList().FindAll(x => x == property).Count >= 2)
                        {
                            throw new ArgumentException("Mod property cannot be registered more than twice!");
                        }

                        POKRange attribute = info.GetCustomAttribute(typeof(POKRange)) as POKRange;

                        Type propertyType = info.PropertyType;

                        switch (propertyType.Name)
                        {
                            case nameof(Int32):
                                if (attribute == null) throw new ArgumentException("Ints are required the attribute POKRange");
                                if (!PlayerPrefs.HasKey($"{Mod.GetType().Name}_{property}"))
                                {
                                    PlayerPrefs.SetInt($"{Mod.GetType().Name}_{property}", (int)info.GetValue(Mod));
                                }
                                else 
                                {
                                    info.SetValue(Mod, PlayerPrefs.GetInt($"{Mod.GetType().Name}_{property}"));
                                }
                                break;
                            case nameof(Single):
                                if (attribute == null) throw new ArgumentException("Floats are required the attribute POKRange");
                                if (!PlayerPrefs.HasKey($"{Mod.GetType().Name}_{property}"))
                                {
                                    PlayerPrefs.SetFloat($"{Mod.GetType().Name}_{property}", (float)info.GetValue(Mod));
                                }
                                else
                                {
                                    info.SetValue(Mod, PlayerPrefs.GetFloat($"{Mod.GetType().Name}_{property}"));
                                }
                                break;
                            case nameof(Boolean):
                                if (!PlayerPrefs.HasKey($"{Mod.GetType().Name}_{property}"))
                                {
                                    PlayerPrefs.SetInt($"{Mod.GetType().Name}_{property}", ((bool)info.GetValue(Mod) == true ? 1 : 0));
                                }
                                else
                                {
                                    info.SetValue(Mod, (PlayerPrefs.GetInt($"{Mod.GetType().Name}_{property}") == 1 ? true : false));
                                }
                                break;
                            default:
                                throw new ArgumentException($"Cannot have type {propertyType.Name}");
                        }
                    }
                }

                Mod mod = new Mod(Mod, ModName, Version, Description, properties);

                if (mods == null)
                {
                    mods = new List<Mod>();
                }

                if (ls == null)
                {
                    ls = BepInEx.Logging.Logger.CreateLogSource("Data.POKManager");
                }

                if (mod.ModClass == null)
                {
                    throw new NullReferenceException("mod is null!");
                }

                if (mods.Any(x => x.ModClass.GetType() == mod.ModClass.GetType()))
                {
                    throw new ArgumentOutOfRangeException("mod already exists!");
                }

                ls.LogInfo($"Registering mod {mod.Name}");

                if (!PlayerPrefs.HasKey(mod.Name))
                {
                    PlayerPrefs.SetInt(mod.Name, System.Convert.ToInt32(true));
                }

                mod.ModClass.Enabled = System.Convert.ToBoolean(PlayerPrefs.GetInt(mod.Name));

                if (mod.ModClass.Enabled)
                {
                    mod.ModClass.OnEnabled();
                }
                else
                {
                    mod.ModClass.OnDisabled();
                }

                mods.Add(mod);

                GetRowAndColumn(mods.Count - 1, out int row, out int column);
                GenerateButton(mods.Count - 1, column, row);
            }
            else
            {
                Debug.LogError("Do not register the mod on awake!");
            }
        }

        public void EnableMod(int place)
        {
            string name = mods[place].GetType().Name;

            bool previousVal = System.Convert.ToBoolean(PlayerPrefs.GetInt(name));

            PlayerPrefs.SetInt(name, System.Convert.ToInt32(true));

            mods[place].ModClass.Enabled = true;

            if (mods[place].ModClass.Enabled != previousVal)
            {
                if (mods[place].ModClass.Enabled)
                {
                    mods[place].ModClass.OnEnabled();
                }
                else
                {
                    mods[place].ModClass.OnDisabled();
                }
            }
        }

        public void DisableMod(int place)
        {
            string name = mods[place].GetType().Name;

            bool previousVal = System.Convert.ToBoolean(PlayerPrefs.GetInt(name));

            PlayerPrefs.SetInt(name, System.Convert.ToInt32(false));

            mods[place].ModClass.Enabled = false;

            if (mods[place].ModClass.Enabled != previousVal)
            {
                if (mods[place].ModClass.Enabled)
                {
                    mods[place].ModClass.OnEnabled();
                }
                else
                {
                    mods[place].ModClass.OnDisabled();
                }
            }
        }

        void Awake()
        {
            instance = this;

            if (mods == null)
            {
                mods = new List<Mod>();
            }

            if (ls == null)
            {
                ls = BepInEx.Logging.Logger.CreateLogSource("Data.POKManager");
            }

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        public void _Start()
        {
            foreach (Mod mod in mods)
            {
                if (mod.ModClass.Enabled == false) continue;

                mod.ModClass.Start();
            }
        }

        public void _Update()
        {
            foreach (Mod mod in mods)
            {
                if (mod.ModClass.Enabled == false) continue;

                if (!mod.ModClass.RunUpdateOnMenu && SceneManager.GetActiveScene().buildIndex == 0) continue;

                mod.ModClass.Update(Time.deltaTime);
            }
        }

        public void _FixedUpdate()
        {
            foreach (Mod mod in mods)
            {
                if (mod.ModClass.Enabled == false) continue;

                if (!mod.ModClass.RunUpdateOnMenu && SceneManager.GetActiveScene().buildIndex == 0) continue;

                mod.ModClass.Update(Time.fixedDeltaTime);
            }
        }
        public void _OnGUI()
        {
            foreach (Mod mod in mods)
            {
                if (mod.ModClass.Enabled == false) continue;

                if (!mod.ModClass.RunUpdateOnMenu && SceneManager.GetActiveScene().buildIndex == 0) continue;

                mod.ModClass.GUIUpdate();
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            GameObject reference = new GameObject("MonoBehaviourReference");
            reference.AddComponent<MonoBehaviourReference>();

            if (arg0.buildIndex == 0)
            {
                //Generate Mod Menu
                GameObject button = GameObject.Find("Options");

                GameObject modButton = Instantiate(button, button.transform.parent);

                modButton.name = "Mods";

                peaksOfYoreFont = modButton.GetComponentInChildren<Text>().font;

                modButton.GetComponentInChildren<Text>().text = "Mods";

                //Generate Menu
                menu = null;
                modMenu = null;

                menu = FindObjectOfType<InGameMenu>().menu;

                options = FindObjectOfType<InGameMenu>().optionsPg;

                modMenu = Instantiate(options, options.transform.parent);

                modMenu.name = "Mods_Menu";

                modMenu.SetActive(true);

                Destroy(modMenu.transform.Find("Delete Progress").gameObject);

                Destroy(modMenu.transform.Find("Defaults").gameObject);

                UnityEngine.UI.Button back = modMenu.transform.Find("Back").GetComponent<UnityEngine.UI.Button>();

                Transform modHolder = modMenu.transform.Find("holder");

                Destroy(modHolder.Find("KeybindingsOption").gameObject);

                Destroy(modHolder.Find("GraphicsOption").gameObject);

                Destroy(modHolder.Find("mainanchor").gameObject);

                back.onClick.AddListener(() => {
                    options.SetActive(false);
                    modMenu.SetActive(false);
                    menu.SetActive(true);
                });

                UnityEngine.UI.Button goToModMenu = modButton.GetComponent<UnityEngine.UI.Button>();

                goToModMenu.onClick.AddListener(() => {
                    options.SetActive(false);
                    modMenu.SetActive(true);
                    menu.SetActive(false);
                });

                configMenu = Instantiate(GameObject.Find("Mods_Menu"), options.transform.parent);

                modMenu.SetActive(false);

                // for some reason i need this
                options.SetActive(true);

                configMenu.name = "Config_Menu";

                configMenu.SetActive(false);

                Destroy(configMenu.transform.Find("Delete Progress").gameObject);

                Destroy(configMenu.transform.Find("Defaults").gameObject);

                UnityEngine.UI.Button backConfig = configMenu.transform.Find("Back").GetComponent<UnityEngine.UI.Button>();

                Transform configHolder = configMenu.transform.Find("holder");

                Destroy(configHolder.Find("KeybindingsOption").gameObject);

                Destroy(configHolder.Find("GraphicsOption").gameObject);

                Destroy(configHolder.Find("mainanchor").gameObject);

                backConfig.onClick.AddListener(() => {
                    options.SetActive(false);
                    configMenu.SetActive(false);
                    menu.SetActive(false);
                    modMenu.SetActive(true);

                    List<Transform> children = new List<Transform>();

                    for (int i = 0; i < configHolder.childCount; i++)
                    {
                        children.Add(configHolder.GetChild(i));
                    }

                    foreach (Transform child in children)
                    {
                        Destroy(child.gameObject);
                    }
                });

                Destroy(GameObject.Find("ControlMapping_OpenButton"));

                options.SetActive(false);

                for (int i = 0; i < mods.Count; i++)
                {
                    GetRowAndColumn(i, out int row, out int column);
                    GenerateButton(i, column, row);
                }
            }

            foreach (Mod mod in mods)
            {
                if (!mod.ModClass.Enabled) continue;

                mod.ModClass.SceneChange(arg0.buildIndex);
            }
        }

        static void GenerateSlider(string Name, bool Int, int min, int max, int i, float defaultValue, UnityAction<float> action = null)
        {
            Transform configHolder = configMenu.transform.Find("holder");
            Transform Target = new GameObject($"Slider {Name}").transform;
            Target.transform.parent = configHolder.transform;
            GetRowAndColumn(i, out int row, out int column);
            Target.transform.localPosition = new Vector3(-675 + 325 * row, 300 - 134 * column, 0);
            Target.transform.localScale = new Vector3(0.7f, 0.7f, 1);

            GameObject textObject = new GameObject("Slider Text");
            textObject.transform.SetParent(Target);
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.text = defaultValue.ToString();
            textComponent.color = Color.white;
            textComponent.fontSize = 20;
            textComponent.raycastTarget = false;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.font = peaksOfYoreFont;
            textComponent.alignment = TextAnchor.MiddleCenter;

            GameObject nameObject = Instantiate(textObject, textObject.transform.parent);
            nameObject.GetComponent<Text>().fontSize = 35;
            nameObject.GetComponent<Text>().text = Name;
            nameObject.GetComponent<Text>().name = "Name";
            nameObject.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(Target, false);

            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = Color.gray;

            RectTransform backgroundRectTransform = background.transform as RectTransform;
            backgroundRectTransform.sizeDelta = new Vector2(400f, 20f);
            backgroundRectTransform.anchoredPosition = new Vector2(0f, 0f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(background.transform, false);

            Image fillImage = fillArea.AddComponent<Image>();
            fillImage.color = new Color(0.15f, 0.15f, 0.15f);

            RectTransform fillRectTransform = (fillArea.transform as RectTransform);
            fillRectTransform.anchorMin = new Vector2(0f, 0f);
            fillRectTransform.anchorMax = new Vector2(1f, 1f);
            fillRectTransform.sizeDelta = Vector2.zero;

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(background.transform, false);

            Image handleImage = handleArea.AddComponent<Image>();
            handleImage.color = Color.white;

            RectTransform handleRectTransform = (handleArea.transform as RectTransform);
            handleRectTransform.sizeDelta = new Vector2(20f, 20f);
            handleRectTransform.anchoredPosition = new Vector2(0f, 0f);

            Slider slider = Target.gameObject.AddComponent<Slider>();

            slider.minValue = min;

            slider.maxValue = max;

            slider.value = defaultValue;

            slider.wholeNumbers = Int;

            slider.handleRect = handleRectTransform;

            slider.fillRect = fillRectTransform;

            (textComponent.transform as RectTransform).localPosition = new Vector3(245, 10, 0);

            (nameObject.transform as RectTransform).localPosition = new Vector3(-130, 55, 0);

            slider.onValueChanged.AddListener((x) => textComponent.text = (Math.Round(x, 2)).ToString());

            if (action != null)
            {
                slider.onValueChanged.AddListener(action);
            }
        }

        static void GenerateToggle(string Name, int i, bool defaultValue, UnityAction<bool> action = null)
        {
            Transform configHolder = configMenu.transform.Find("holder");
            Transform Target = new GameObject($"Toggle {Name}").transform;
            Target.transform.parent = configHolder.transform;
            GetRowAndColumn(i, out int row, out int column);
            Target.transform.localPosition = new Vector3(-675 + 325 * row, 300 - 134 * column, 0);
            Target.transform.localScale = new Vector3(0.7f, 0.7f, 1);

            GameObject background = new GameObject("Background");
            background.transform.SetParent(Target, false);

            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = Color.gray;

            RectTransform backgroundRectTransform = background.transform as RectTransform;
            backgroundRectTransform.sizeDelta = new Vector2(50, 50);
            backgroundRectTransform.anchoredPosition = new Vector2(0f, 0f);

            GameObject handleArea = new GameObject("Handle Area");
            handleArea.transform.SetParent(background.transform, false);

            Image handleImage = handleArea.AddComponent<Image>();
            handleImage.color = new Color(1, 1, 1, 0.7f);

            RectTransform handleRectTransform = handleArea.GetComponent<RectTransform>();
            handleRectTransform.sizeDelta = new Vector2(50, 50);
            handleRectTransform.anchoredPosition = new Vector2(0, 0);

            Toggle toggle = Target.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic = handleImage;
            toggle.isOn = defaultValue;

            GameObject textObject = new GameObject("Toggle Text");
            textObject.transform.SetParent(background.transform, false);
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.text = defaultValue ? "On" : "Off";
            textComponent.color = Color.white;
            textComponent.fontSize = 30;
            textComponent.font = peaksOfYoreFont;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.raycastTarget = false;
            textComponent.alignment = TextAnchor.MiddleCenter;
            (textComponent.transform as RectTransform).localPosition += new Vector3(56, 0);

            GameObject nameObject = Instantiate(textObject, textObject.transform.parent);
            nameObject.transform.parent = textObject.transform.parent;

            nameObject.GetComponent<Text>().fontSize = 40;
            nameObject.GetComponent<Text>().text = Name;
            nameObject.GetComponent<Text>().name = "Name";
            nameObject.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            (nameObject.transform as RectTransform).localPosition = new Vector3(24.9882f, 68.1265f, 0);

            (background.transform as RectTransform).localPosition = new Vector3(-176.6874f, 0, 0);

            toggle.onValueChanged.AddListener((x) => textComponent.text = x ? "On" : "Off");

            if (action != null)
            {
                toggle.onValueChanged.AddListener(action);
            }
        }

        static void SetupConfig(int i)
        {
            Mod Mod = mods[i];

            for (int x = 0; x < Mod.Properties.Length; x++)
            {
                PropertyInfo info = Mod.ModClass.GetType().GetProperty(Mod.Properties[x], BindingFlags.Public | BindingFlags.Instance);

                string propertyName = info.Name;

                POKRange Rangeattribute = info.GetCustomAttribute(typeof(POKRange)) as POKRange;
                DoNotSave DoNotSaveAttribute = info.GetCustomAttribute(typeof(DoNotSave)) as DoNotSave;

                Type propertyType = info.PropertyType;

                switch (propertyType.Name)
                {
                    case nameof(Int32):
                        GenerateSlider(propertyName, true, Rangeattribute.min, Rangeattribute.max, x, (int)info.GetValue(Mod.ModClass), (v) => { 
                            if (DoNotSaveAttribute == null) 
                            {
                                PlayerPrefs.SetInt($"{Mod.ModClass.GetType().Name}_{propertyName}", (int)info.GetValue(Mod.ModClass));
                            } 
                            info.SetValue(Mod.ModClass, (int)v); 
                        });
                        break;
                    case nameof(Single):
                        GenerateSlider(propertyName, false, Rangeattribute.min, Rangeattribute.max, x, (float)info.GetValue(Mod.ModClass), (v) => {
                            if (DoNotSaveAttribute == null)
                            {
                                PlayerPrefs.SetFloat($"{Mod.ModClass.GetType().Name}_{propertyName}", (float)info.GetValue(Mod.ModClass));
                            }
                            info.SetValue(Mod.ModClass, v);
                        });
                        break;
                    case nameof(Boolean):
                        GenerateToggle(propertyName, x, (bool)info.GetValue(Mod.ModClass), (v) => {
                            if (DoNotSaveAttribute == null)
                            {
                                PlayerPrefs.SetInt($"{Mod.ModClass.GetType().Name}_{propertyName}", (v == true ? 1 : 0));
                            }
                            info.SetValue(Mod.ModClass, v); 
                        });
                        break;

                }
            }
        }

        static void GetRowAndColumn(int i, out int row, out int column)
        {
            column = i % 5;
            row = i != 0 ? i / 5 : 0;
        }

        static void GenerateButton(int i, int column, int row)
        {
            GameObject obj = new GameObject($"ModButton {mods[i].Name}");
            obj.transform.parent = modMenu.transform;
            obj.transform.localScale = new Vector3(3, 1.5f, 1);
            obj.transform.localPosition = new Vector3(-675 + 325 * column, 410 - 234 * row, 0);
            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.07f, 0.07f, 0.07f);
            RectTransform rectTransform = obj.transform as RectTransform;
            rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height + 50);

            GameObject EnableButton = Instantiate(obj, obj.transform);
            EnableButton.transform.localPosition = new Vector3(-24, -54, 0);
            EnableButton.transform.localScale = new Vector3(0.45f, 0.3f, 1);
            RectTransform enableRectTransform = EnableButton.transform as RectTransform;
            enableRectTransform.sizeDelta = new Vector2(enableRectTransform.rect.width, enableRectTransform.rect.height - 40);
            EnableButton.name = "Enable button";
            EnableButton.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.3f);
            EnableButton.AddComponent<UnityEngine.UI.Button>();

            GameObject DisableButton = Instantiate(EnableButton, obj.transform);
            DisableButton.name = "Disable button";
            DisableButton.transform.localPosition = new Vector3(24, -54, 0);
            DisableButton.transform.localScale = new Vector3(0.45f, 0.3f, 1);

            GameObject ConfigButton = null;

            if (mods[i].Properties.Length != 0)
            {
                ConfigButton = Instantiate(EnableButton, obj.transform);
                ConfigButton.name = "Config";
                ConfigButton.transform.localPosition = new Vector3(24f, 32f, 0);
                ConfigButton.transform.localScale = new Vector3(0.45f, 0.3f, 1);

                ConfigButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    configMenu.SetActive(true);
                    modMenu.SetActive(false);
                    SetupConfig(i);
                    GameObject.Find("Click").GetComponent<AudioSource>().Play();
                });
            }

            GameObject nameobj = new GameObject("Name");
            nameobj.name = "Name";
            nameobj.transform.parent = obj.transform;
            nameobj.transform.localPosition = new Vector3(-24, 17, 0);
            nameobj.transform.localScale = new Vector3(0.45f, 1, 1);
            Text nameText = nameobj.AddComponent<Text>();

            nameText.font = peaksOfYoreFont;
            nameText.fontSize = 20;
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameText.text = mods[i].Name;
            nameText.raycastTarget = false;
            nameText.maskable = false;

            GameObject Status = Instantiate(nameobj, obj.transform);
            Status.name = "Status";
            Status.transform.localPosition = new Vector3(-24, -2, 0);
            Status.GetComponent<Text>().text = "Unknown";

            GameObject Version = Instantiate(nameobj, obj.transform);
            Version.name = "Version";
            Version.transform.localPosition = new Vector3(-24, -20, 0);
            Version.GetComponent<Text>().text = "V" + mods[i].Version;

            GameObject Description = Instantiate(nameobj, obj.transform);
            Description.name = "Description";
            Description.transform.localPosition = new Vector3(-0.2f, -15f, 0);
            string description = mods[i].Description;
            description.Replace('\n', (char)0);
            if (description.Length > 105)
            {
                description = description.Substring(0, 105);
            }
            Description.GetComponent<Text>().text = description;

            RectTransform descriptionTransform = Description.GetComponent<Text>().transform as RectTransform;
            Description.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
            Description.GetComponent<Text>().lineSpacing = 2.3f;
            Description.GetComponent<Text>().fontSize = 18;
            descriptionTransform.sizeDelta = new Vector2(210, 50);

            GameObject EnableText = Instantiate(nameobj, EnableButton.transform);
            EnableText.GetComponent<Text>().fontSize = 30;
            EnableText.GetComponent<Text>().text = "Enable";
            EnableText.transform.localPosition = new Vector3(10, -97, 0);
            EnableText.transform.localScale = new Vector3(0.8f, 2.5f, 1);

            if (mods[i].Properties.Length != 0)
            {
                GameObject ConfigText = Instantiate(nameobj, ConfigButton.transform);
                ConfigText.GetComponent<Text>().fontSize = 30;
                ConfigText.GetComponent<Text>().text = "Config";
                ConfigText.transform.localPosition = new Vector3(10, -97, 0);
                ConfigText.transform.localScale = new Vector3(0.8f, 2.5f, 1);
            }

            GameObject DisableText = Instantiate(EnableText, DisableButton.transform);
            DisableText.GetComponent<Text>().text = "Disable";
            DisableText.transform.localPosition = new Vector3(10, -97, 0);
            DisableText.transform.localScale = new Vector3(0.8f, 2.5f, 1);

            ModButton button = obj.AddComponent<ModButton>();

            button.Name = nameText;
            button.mod = i;
            button.Status = Status.GetComponent<Text>();
            button.Name = Status.GetComponent<Text>();
            button._Enable = EnableButton.GetComponent<UnityEngine.UI.Button>();
            button._Disable = DisableButton.GetComponent<UnityEngine.UI.Button>();
        }

        public static void printInfo(object obj, string name)
        {
            ls.LogInfo($"| [{name}] : " + obj);
        }

        public static void printWarning(object obj, string name)
        {
            ls.LogWarning($"| [{name}] : " + obj);
        }

        public static void printError(object obj, string name)
        {
            ls.LogError($"| [{name}] : " + obj);
        }
    }
}
