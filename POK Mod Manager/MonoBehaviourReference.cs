using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace POKModManager
{
    public class MonoBehaviourReference : MonoBehaviour
    {
        public void Start()
        {
            POKManager.instance._Start();
        }

        public void Update()
        {
            POKManager.instance._Update();
        }

        public void FixedUpdate()
        {
            POKManager.instance._FixedUpdate();
        }

        public void OnGUI()
        {
            POKManager.instance._OnGUI();
        }
    }
}
