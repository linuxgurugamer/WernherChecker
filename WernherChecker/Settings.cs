using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WernherChecker
{
    public class WCSettings
    {
        public ConfigNode cfg;
        public bool cfgLoaded;
        public bool lockOnHover;
        public bool checkCrewAssignment;
        public bool jebEnabled;
        public bool minimized;
        public float windowX;
        public float windowY;
        public WernherChecker.toolbarType wantedToolbar;

        public WCSettings()
        {
            cfgLoaded = false;
            lockOnHover = true;
            checkCrewAssignment = true;
            jebEnabled = true;
            minimized = false;
            windowX = WernherChecker.panelWidth + 3;
            windowY = 120;
            wantedToolbar = WernherChecker.toolbarType.STOCK;
        }

        public bool Load()
        {
            Log.Info("[WernherChecker]: ========= Loading Settings =========");
            if (CfgExists())
            {
                cfg = ConfigNode.Load(WernherChecker.DataPath + "WernherChecker.cfg");
                Log.Info("[WernherChecker]: Config file found at " + WernherChecker.DataPath + "WernherChecker.cfg");
                //------------------------------------------------------------------------------
                try
                {
                    this.lockOnHover = bool.Parse(cfg.GetValue("lockOnHover"));
                    Log.Info("[WernherChecker]: SETTINGS - Lock editor while hovering over the main window: " + this.lockOnHover);
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - lockOnHover field has an invalid value assigned (" + cfg.GetValue("lockOnHover") + "). Please assign valid boolean value."); }
                //----------------------------------------------------------------------------
                try
                {
                    this.checkCrewAssignment = bool.Parse(cfg.GetValue("checkCrewAssignment"));
                    Log.Info("[WernherChecker]: SETTINGS - Check crew assignment before launch: " + this.checkCrewAssignment);
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - checkCrewAssignment field has an invalid value assigned (" + cfg.GetValue("checkCrewAssignment") + "). Please assign valid boolean value."); }
                //-----------------------------------------------------------------------------
                try
                {
                    this.jebEnabled = bool.Parse(cfg.GetValue("jebEnabled"));
                    Log.Info("[WernherChecker]: SETTINGS - Jeb's advice enabled: " + this.jebEnabled);
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - jebEnabled field has an invalid value assigned (" + cfg.GetValue("jebEnabled") + "). Please assign valid boolean value."); }
                //--------------------------------------------------------------------------
                try
                {
                    this.wantedToolbar = (WernherChecker.toolbarType)Enum.Parse(typeof(WernherChecker.toolbarType), cfg.GetValue("toolbarType"));
                    Log.Info("[WernherChecker]: SETTINGS - Active toolbar: " + this.wantedToolbar.ToString());
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - toolbarType field has an invalid value assigned (" + cfg.GetValue("toolbarType") + "). Please assign valid value (BLIZZY / STOCK)."); }
                //--------------------------------------------------------------------------
                try
                {
                    this.minimized = bool.Parse(cfg.GetValue("minimized"));
                    Log.Info("[WernherChecker]: SETTINGS - Minimized: " + this.minimized.ToString());
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - minimized field has an invalid value assigned (" + cfg.GetValue("minimized") + "). Please assign valid boolean value."); }
                //--------------------------------------------------------------------------
                try
                {
                    this.windowX = float.Parse(cfg.GetValue("windowX"));
                    Log.Info("[WernherChecker]: SETTINGS - Window X: " + this.windowX.ToString());
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - windowX field value is unsupported or null."); }
                //--------------------------------------------------------------------------
                try
                {
                    this.windowY = float.Parse(cfg.GetValue("windowY"));
                    Log.Info("[WernherChecker]: SETTINGS - Window Y: " + this.windowY.ToString());
                }
                catch { Log.Warning("[WernherChecker]: SETTINGS - windowY field value is unsupported or null."); }

                cfgLoaded = true;
                return true;
            }

            else
            {
                Log.Warning("[WernherChecker]: Missing config file!");
                return false;
            }
        }

        public void Save()
        {
            Log.Info("[WernherChecker]: ========= Saving Settings =========");
            if (CfgExists() && cfgLoaded)
            {
                if (cfg.HasValue("lockOnHover"))
                    cfg.SetValue("lockOnHover", this.lockOnHover.ToString());
                else
                    cfg.AddValue("lockOnHover", this.lockOnHover.ToString());
                //--------------------------------------------------------------------------
                if (cfg.HasValue("checkCrewAssignment"))
                    cfg.SetValue("checkCrewAssignment", this.checkCrewAssignment.ToString());
                else
                    cfg.AddValue("checkCrewAssignment", this.checkCrewAssignment.ToString());
                //--------------------------------------------------------------------------
                if (cfg.HasValue("toolbarType"))
                    cfg.SetValue("toolbarType", WernherChecker.Instance.activeToolbar.ToString());
                else
                    cfg.AddValue("toolbarType", WernherChecker.Instance.activeToolbar.ToString());
                //--------------------------------------------------------------------------
                if (cfg.HasValue("minimized"))
                    cfg.SetValue("minimized", WernherChecker.Instance.minimized.ToString());
                else
                    cfg.AddValue("minimized", WernherChecker.Instance.minimized.ToString());
                //--------------------------------------------------------------------------
                if (cfg.HasValue("windowX"))
                    cfg.SetValue("windowX", WernherChecker.Instance.mainWindow.x.ToString());
                else
                    cfg.AddValue("windowX", WernherChecker.Instance.mainWindow.x.ToString());
                //--------------------------------------------------------------------------
                if (cfg.HasValue("windowY"))
                    cfg.SetValue("windowY", WernherChecker.Instance.mainWindow.y.ToString());
                else
                    cfg.AddValue("windowY", WernherChecker.Instance.mainWindow.y.ToString());
                //--------------------------------------------------------------------------
                cfg.Save(WernherChecker.DataPath + "WernherChecker.cfg");
            }

            else
                Log.Warning("[WernherChecker]: Missing config file!");
        }

        public bool CfgExists()
        {
            return
                System.IO.File.Exists(WernherChecker.DataPath + "WernherChecker.cfg");
        }
    }
}
