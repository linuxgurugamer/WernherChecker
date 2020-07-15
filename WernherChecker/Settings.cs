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

        public WCSettings()
        {
            Log.Info("WCSettings");
            cfgLoaded = false;
            lockOnHover = true;
            checkCrewAssignment = true;
            jebEnabled = true;
            minimized = false;
            windowX = WernherChecker.panelWidth + 3;
            windowY = 120;
            Log.Info("WCSettings 2");


        }

        public bool Load()
        {
            Log.Info("========= Loading Settings =========");
            if (CfgExists())
            {
                cfg = ConfigNode.Load(WernherChecker.DataPath + "WernherChecker.cfg");
                Log.Info("Config file found at " + WernherChecker.DataPath + "WernherChecker.cfg");
                //------------------------------------------------------------------------------
                try
                {
                    this.checkCrewAssignment = bool.Parse(cfg.GetValue("checkCrewAssignment"));
                    Log.Info("SETTINGS - Check crew assignment before launch: " + this.checkCrewAssignment);
                }
                catch { Log.Warning("SETTINGS - checkCrewAssignment field has an invalid value assigned (" + cfg.GetValue("checkCrewAssignment") + "). Please assign valid boolean value."); }
                //-----------------------------------------------------------------------------
                try
                {
                    this.jebEnabled = bool.Parse(cfg.GetValue("jebEnabled"));
                    Log.Info("SETTINGS - Jeb's advice enabled: " + this.jebEnabled);
                }
                catch { Log.Warning("SETTINGS - jebEnabled field has an invalid value assigned (" + cfg.GetValue("jebEnabled") + "). Please assign valid boolean value."); }
                //--------------------------------------------------------------------------
                string str = "";
                if (cfg.TryGetValue("currentChecklist", ref str))
                {
                    WernherChecker.checklistSystem.LoadChecklists();
                    foreach (Checklist checklist in WernherChecker.checklistSystem.availableChecklists)
                    {
                        if (str == checklist.name)
                        {
                            WernherChecker.checklistSystem.ActiveChecklist = checklist;
                            WernherChecker.checklistSelected = true;
                            WernherChecker.checklistSystem.CheckVessel();
                            break;
                        }
                    }
                }

                //--------------------------------------------------------------------------
                try
                {
                    this.minimized = bool.Parse(cfg.GetValue("minimized"));
                    Log.Info("SETTINGS - Minimized: " + this.minimized.ToString());                
                }
                catch { Log.Warning("SETTINGS - minimized field has an invalid value assigned (" + cfg.GetValue("minimized") + "). Please assign valid boolean value."); }
                //--------------------------------------------------------------------------
                try
                {
                    this.windowX = float.Parse(cfg.GetValue("windowX"));
                    Log.Info("SETTINGS - Window X: " + this.windowX.ToString());
                }
                catch { Log.Warning("SETTINGS - windowX field value is unsupported or null."); }
                //--------------------------------------------------------------------------
                try
                {
                    this.windowY = float.Parse(cfg.GetValue("windowY"));
                    Log.Info("SETTINGS - Window Y: " + this.windowY.ToString());
                }
                catch { Log.Warning("SETTINGS - windowY field value is unsupported or null."); }

                cfgLoaded = true;
                return true;
            }

            else
            {
                Log.Warning("Missing config file!");
                return false;
            }
        }

        public void Save()
        {
            Log.Info("========= Saving Settings =========");
            if (CfgExists() && cfgLoaded)
            {
                //--------------------------------------------------------------------------

                cfg.SetValue("minimized", WernherChecker.Instance.minimized.ToString(), true);
                cfg.SetValue("windowX", WernherChecker.Instance.mainWindow.x.ToString(), true);
                cfg.SetValue("windowY", WernherChecker.Instance.mainWindow.y.ToString(), true);
                if (WernherChecker.checklistSystem != null && WernherChecker.checklistSystem.ActiveChecklist != null && WernherChecker.checklistSystem.ActiveChecklist.name != null)
                    cfg.SetValue("currentChecklist", WernherChecker.checklistSystem.ActiveChecklist.name, true);
                cfg.Save(WernherChecker.DataPath + "WernherChecker.cfg");
            }

            else
                Log.Warning("Missing config file!");
        }

        public bool CfgExists()
        {
            Log.Info("CfgExists: " + WernherChecker.DataPath + "WernherChecker.cfg");
            return
                System.IO.File.Exists(WernherChecker.DataPath + "WernherChecker.cfg");
        }
    }
}
