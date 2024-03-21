using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace POKModManager
{
    public class ModButton : MonoBehaviour
    {
        public int mod;
        public UnityEngine.UI.Text Name;
        public UnityEngine.UI.Button _Enable;
        public UnityEngine.UI.Button _Disable;
        public UnityEngine.UI.Text Status;

        public void Start()
        {
            Status.text = POKManager.mods[mod].ModClass.Enabled ? "Enabled" : "Disabled";

            _Enable.onClick.AddListener(() => {
                POKManager.instance.EnableMod(mod);
                GameObject.Find("Click").GetComponent<AudioSource>().Play();
                Status.text = POKManager.mods[mod].ModClass.Enabled ? "Enabled" : "Disabled";
            });

            _Disable.onClick.AddListener(() => {
                POKManager.instance.DisableMod(mod);
                GameObject.Find("Click").GetComponent<AudioSource>().Play();
                Status.text = POKManager.mods[mod].ModClass.Enabled ? "Enabled" : "Disabled";
            });
        }
    }
}
