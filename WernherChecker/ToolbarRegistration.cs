
using UnityEngine;
using ToolbarControl_NS;

namespace WernherChecker
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(WernherChecker.MODID, WernherChecker.MODNAME);
        }
    }
}