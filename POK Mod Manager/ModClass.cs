using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POKModManager
{
    /// <summary>
    /// mod class for POK manager
    /// </summary>
    public class ModClass
    {
        /// <summary>
        /// is the mod enabled?
        /// </summary>
        public bool Enabled = true;
        /// <summary>
        /// Run stuff in the menu?
        /// </summary>
        public bool RunUpdateOnMenu = false;
        /// <summary>
        /// runs when the mod is enabled
        /// </summary>
        public virtual void OnEnabled() { }
        /// <summary>
        /// runs when the mod is disabled
        /// </summary>
        public virtual void OnDisabled() { }
        /// <summary>
        /// runs whenever a scene is started
        /// </summary>
        public virtual void SceneChange(int sceneIndex) { }
        /// <summary>
        /// update for GUI
        /// </summary>
        public virtual void GUIUpdate() { }
        /// <summary>
        /// start is called in the start
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        /// update is ran every frame
        /// </summary>
        public virtual void Update(float deltaTime) { }
        /// <summary>
        /// fixedupdate is ran for physics
        /// </summary>
        public virtual void FixedUpdate(float deltaTime) { }
        /// <summary>
        /// prints a message, wrapper for the manager printer
        /// </summary>
        public void print(object obj)
        {
            POKManager.printInfo(obj, GetType().Name);
        }
        /// <summary>
        /// prints a warning message, wrapper for the manager printer
        /// </summary>
        public void printWarning(object obj)
        {
            POKManager.printInfo(obj, GetType().Name);
        }
        /// <summary>
        /// prints a error message, wrapper for the manager printer
        /// </summary>
        public void printError(object obj)
        {
            POKManager.printInfo(obj, GetType().Name);
        }
    }
}
