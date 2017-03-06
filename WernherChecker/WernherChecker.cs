﻿/*
 * License: The MIT License (MIT)
 * Version: v0.4
 * 
 * Minimizing button powered by awesome Toolbar Plugin - http://forum.kerbalspaceprogram.com/threads/60863 by blizzy78!
 */
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using UnityEngine.Events;

namespace WernherChecker
{

    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class WernherChecker : MonoBehaviour
    {
        //Window variables
        public static float panelWidth = 100; //  = EditorPanels.Instance.partsEditor.panelTransform.rect.xMax;
        public Rect mainWindow; // = new Rect(panelWidth + 3, 120, 0, 0);
        Rect settingsWindow = new Rect();
        bool showAdvanced = false;
        bool showSettings = false;
        public bool minimized = false;
        bool minimizedManually = false;

        //Checklist managment
        public bool checklistSelected = false;
        public bool selectionInProgress = false;
        public bool checkSelected = false;
        bool selectedShowed = false;
        public PartSelection partSelection;

        //Tooltips
        public string globalTooltip = "";
        public float hoverTime = 0f;
        public string lastTooltip;

        //Other
        string Version = "";
        bool HideWC_UI = false;
        internal static String _AssemblyName { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }
        int baseWindowID = UnityEngine.Random.Range(1000, 2000000) + _AssemblyName.GetHashCode();
        UnityAction launchDelegate; // = new UnityAction(CrewCheck.OnButtonInput);
        UnityAction defaultLaunchDelegate; // = new UnityAction(EditorLogic.fetch.launchVessel);
        bool KCTInstalled = false;
        public toolbarType activeToolbar;
        bool settings_BlizzyToolbar = false;
        bool settings_CheckCrew = true;
        bool settings_LockWindow = true;
        public static string DataPath = KSPUtil.ApplicationRootPath + "GameData/WernherChecker/PluginData/";
        public static Texture2D settingsTexture = GameDatabase.Instance.GetTexture("WernherChecker/Images/settings", false);
        public static Texture2D tooltipBGTexture = GameDatabase.Instance.GetTexture("WernherChecker/Images/tooltip_BG", false);
        IButton wcbutton;
        ApplicationLauncherButton appButton;
        public Vector2 mousePos = Input.mousePosition;
        public static List<Part> VesselParts
        {
            get
            {
                if (HighLogic.LoadedScene != GameScenes.EDITOR)
                    return null;
                return EditorLogic.fetch.ship.Parts;
            }
        }

        //Instances
        public WCSettings Settings = new WCSettings();
        public ChecklistSystem checklistSystem = new ChecklistSystem();
        public static WernherChecker Instance;

        public enum toolbarType
        {
            STOCK,
            BLIZZY
        }

        // GUI Styles
        public static GUIStyle windowStyle = new GUIStyle(HighLogic.Skin.window);
        public static GUIStyle boxStyle = new GUIStyle(HighLogic.Skin.box);
        public static GUIStyle buttonStyle = new GUIStyle(HighLogic.Skin.button);
        public static GUIStyle toggleStyle = new GUIStyle(HighLogic.Skin.toggle);
        public static GUIStyle labelStyle = new GUIStyle(HighLogic.Skin.label);
        public static GUIStyle tooltipStyle = new GUIStyle(HighLogic.Skin.textArea)
        {
            padding = new RectOffset(4, 4, 4, 4),
            border = new RectOffset(2, 2, 2, 2),
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            normal = { background = tooltipBGTexture },
            richText = true,
        };


        public static List<String> installedMods = new List<String>();
        static void buildModList()
        {
            Log.Info("buildModList");
            //https://github.com/Xaiier/Kreeper/blob/master/Kreeper/Kreeper.cs#L92-L94 <- Thanks Xaiier!
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                string name = a.name;
                Log.Info(string.Format("Loading assembly: {0}", name));
                installedMods.Add(name);
            }
        }
        public static bool hasMod(string modIdent)
        {
            if (installedMods.Count == 0)
                buildModList();
            return installedMods.Contains(modIdent);
        }


        public void Awake()
        {
            Log.Info("WernherChecker.Awake");
            ReloadSettings();
            GameEvents.OnGameSettingsApplied.Add(ReloadSettings);
        }




        void ReloadSettings()
        {
            Log.Info("ReloadSettings 1");
            Settings.checkCrewAssignment = HighLogic.CurrentGame.Parameters.CustomParams<WernersSettings>().checkCrewAssignment;
            Log.Info("ReloadSettings 2");
            Settings.lockOnHover = HighLogic.CurrentGame.Parameters.CustomParams<WernersSettings>().lockOnHover;
            Log.Info("ReloadSettings 3");
            settings_BlizzyToolbar = HighLogic.CurrentGame.Parameters.CustomParams<WernersSettings>().settings_BlizzyToolbar;
            Log.Info("ReloadSettings 4");
        }


        public void Start()
        {
            Log.Warning("WernherChecker is loading..., scene: " + HighLogic.LoadedScene.ToString());
            Version = this.GetType().Assembly.GetName().Version.ToString();
            Log.Warning(string.Format("WernherChecker Version is {0}", Version));
            Instance = this;
            
            if (Settings.Load())
            {
                minimized = Settings.minimized;
                mainWindow.x = Settings.windowX;
                mainWindow.y = Settings.windowY;
            }
            
            checklistSystem.LoadChecklists();
            
            //if (AssemblyLoader.loadedAssemblies.Any(a => a.dllName == "KerbalConstructionTime"))
            if (hasMod("KerbalConstructionTime"))
                KCTInstalled = true;
            else
                KCTInstalled = false;
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {

                panelWidth = EditorPanels.Instance.partsEditor.panelTransform.rect.xMax;
                mainWindow = new Rect(panelWidth + 3, 120, 0, 0);
                GameEvents.onEditorScreenChange.Add(onEditorPanelChange);
                GameEvents.onEditorShipModified.Add(checklistSystem.CheckVessel);
                GameEvents.onEditorRestart.Add(checklistSystem.CheckVessel);
                GameEvents.onEditorShowPartList.Add(checklistSystem.CheckVessel);

                launchDelegate = new UnityAction(CrewCheck.OnButtonInput);
                defaultLaunchDelegate = new UnityAction(EditorLogic.fetch.launchVessel);

                if (Settings.checkCrewAssignment && !KCTInstalled)
                {
                    EditorLogic.fetch.launchBtn.onClick.RemoveListener(defaultLaunchDelegate);
                    EditorLogic.fetch.launchBtn.onClick.AddListener(launchDelegate);
                }
            }
            else
            {
                mainWindow = new Rect(panelWidth + 3, 120, 0, 0);
            }

            if (Settings.wantedToolbar == toolbarType.BLIZZY && ToolbarManager.ToolbarAvailable)
            {
                AddToolbarButton(toolbarType.BLIZZY, true);
            }
            else
            {
                AddToolbarButton(toolbarType.STOCK, true);
            }
            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);
        }

        void OnDestroy()
        {
            if (activeToolbar == toolbarType.BLIZZY)
                wcbutton.Destroy();
            if (InputLockManager.lockStack.ContainsKey("WernherChecker_partSelection"))
            {
                InputLockManager.RemoveControlLock("WernherChecker_partSelection");
                selectionInProgress = false;
            }

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                GameEvents.onEditorScreenChange.Remove(onEditorPanelChange);
                GameEvents.onEditorShipModified.Remove(checklistSystem.CheckVessel);
                GameEvents.onEditorRestart.Remove(checklistSystem.CheckVessel);
                GameEvents.onEditorShowPartList.Remove(checklistSystem.CheckVessel);
            }
            Settings.Save();
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
            GameEvents.OnGameSettingsApplied.Remove(ReloadSettings);
        }


        private void ShowUI()
        {
            HideWC_UI = false;
        }
        void HideUI()
        {
            HideWC_UI = true;
        }

        #region Toolbar stuff
        void AddToolbarButton(toolbarType type, bool useStockIfProblem)
        {
            if (type == toolbarType.BLIZZY)
            {
                if (ToolbarManager.ToolbarAvailable)
                {
                    {
                        activeToolbar = toolbarType.BLIZZY;
                        wcbutton = ToolbarManager.Instance.add("WernherChecker", "wcbutton"); //creating toolbar button
                        wcbutton.TexturePath = "WernherChecker/Images/icon_24";
                        wcbutton.ToolTip = "WernherChecker";
                        wcbutton.OnClick += (e) =>
                        {
                            if (minimizedManually)
                                MiniOff();
                            else
                                MiniOn();
                        };
                    }
                }
                else if (useStockIfProblem)
                    AddToolbarButton(toolbarType.STOCK, true);
            }

            if (type == toolbarType.STOCK)
            {
                activeToolbar = toolbarType.STOCK;
                if (ApplicationLauncher.Ready)
                    CreateAppButton();
                GameEvents.onGUIApplicationLauncherReady.Add(CreateAppButton);
                GameEvents.onGUIApplicationLauncherUnreadifying.Add(DestroyAppButton);
            }
        }

        void CreateAppButton()
        {
            appButton = ApplicationLauncher.Instance.AddModApplication(
                MiniOn,
                MiniOff,
                null, null, null, null,
                ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                (Texture)GameDatabase.Instance.GetTexture("WernherChecker/Images/icon",
                false));

            if (!minimized)
                appButton.SetTrue(true);
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
                minimized = !HighLogic.CurrentGame.Parameters.CustomParams<WernersSettings>().alwaysOpeninEditor;
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                minimized = !HighLogic.CurrentGame.Parameters.CustomParams<WernersSettings>().alwaysOpenInFlight;
        }

        void DestroyAppButton(GameScenes gameScenes)
        {
            ApplicationLauncher.Instance.RemoveModApplication(appButton);
            GameEvents.onGUIApplicationLauncherReady.Remove(CreateAppButton);
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(DestroyAppButton);
        }
        #endregion

        void onEditorPanelChange(EditorScreen screen)
        {
            if (screen == EditorScreen.Actions || screen == EditorScreen.Crew)
            {
                minimized = true;
            }

            if (screen == EditorScreen.Parts)
            {
                minimized = minimizedManually;
            }
        }

        void MiniOn()
        {
            minimizedManually = true;
            minimized = true;
        }

        void MiniOff()
        {
            minimizedManually = false;
            minimized = false;
        }


        void OnGUI()
        {
            if (HideWC_UI)
                return;
            mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            if (!minimized)
                mainWindow = GUILayout.Window(baseWindowID + 1, mainWindow, OnWindow, string.Format("WernherChecker v{0}", Version), windowStyle);
            if (showSettings && !minimized)
                settingsWindow = GUILayout.Window(baseWindowID + 2, settingsWindow, OnSettingsWindow, "WernherChecker - Settings", windowStyle);
            if (checklistSelected && checklistSystem.ActiveChecklist.items.Exists(i => i.paramsDisplayed) && !minimized)
                checklistSystem.paramsWindow = GUILayout.Window(baseWindowID + 3, checklistSystem.paramsWindow, checklistSystem.DrawParamsWindow, "Edit Parameters", HighLogic.Skin.window);

            mainWindow.x = Mathf.Clamp(mainWindow.x, 0, Screen.width - mainWindow.width);
            mainWindow.y = Mathf.Clamp(mainWindow.y, 0, Screen.height - mainWindow.height);

            if (partSelection != null && selectionInProgress)
                partSelection.Update(mousePos);

            if (Settings.lockOnHover)
            {
                if (HighLogic.LoadedScene == GameScenes.EDITOR)
                {
                    if (!minimized && (mainWindow.Contains(mousePos) || (showSettings && settingsWindow.Contains(mousePos)) || (checklistSystem.ActiveChecklist.items.Exists(i => i.paramsDisplayed) && checklistSystem.paramsWindow.Contains(mousePos))) && !InputLockManager.lockStack.ContainsKey("WernherChecker_windowLock"))
                        EditorLogic.fetch.Lock(true, true, true, "WernherChecker_windowLock");
                    else if (((!mainWindow.Contains(mousePos) && !settingsWindow.Contains(mousePos) && !checklistSystem.paramsWindow.Contains(mousePos)) || minimized) && InputLockManager.lockStack.ContainsKey("WernherChecker_windowLock"))
                        EditorLogic.fetch.Unlock("WernherChecker_windowLock");
                }
            }

            DrawToolTip(globalTooltip);
        }

        public void SetTooltipText()
        {
            if (Event.current.type == EventType.Repaint)
                globalTooltip = GUI.tooltip;
        }

        void DrawToolTip(string tooltipText)
        {
            if (tooltipText != lastTooltip)
                hoverTime = 0;
            if (lastTooltip == tooltipText && Event.current.type == EventType.Repaint && tooltipText != "")
                hoverTime += Time.deltaTime;

            lastTooltip = tooltipText;

            if (tooltipText == "" || hoverTime < 0.5f)
                return;

            //Log.Info(tooltipText);           
            GUIContent tooltip = new GUIContent(tooltipText);
            Rect tooltipPosition = new Rect(mousePos.x + 15, mousePos.y + 15, 0, 0);
            float maxw, minw;
            tooltipStyle.CalcMinMaxWidth(tooltip, out minw, out maxw);
            tooltipPosition.width = Math.Min(Math.Max(200, minw), maxw);
            tooltipPosition.height = tooltipStyle.CalcHeight(tooltip, tooltipPosition.width);
            GUI.Label(/*new Rect(EditorPanels.Instance.partsPanelWidth + 5, Screen.height - 60, 100, 60)*/tooltipPosition, tooltip, tooltipStyle);
            GUI.depth = 0;


        }

        public void SelectChecklist()
        {
            GUILayout.BeginVertical(boxStyle);
            foreach (Checklist checklist in checklistSystem.availableChecklists)
            {
                //parsedItem.editorOnly = (scene == "editor");
                //parsedItem.flightOnly = (scene == "flight");
                if (checklist.editorOnly && HighLogic.LoadedScene == GameScenes.EDITOR ||
                    checklist.flightOnly && HighLogic.LoadedScene == GameScenes.FLIGHT ||
                    (checklist.editorOnly == false && checklist.flightOnly == false))
                {
                    if (GUILayout.Button(new GUIContent(checklist.name, "Items:\n" + string.Join("\n", checklist.items.Select(x => "<color=cyan><b>–</b></color> <i>" + x.name + "</i>").ToArray())), buttonStyle))
                    {
                        checklistSystem.ActiveChecklist = checklist;
                        checklistSelected = true;
                        checklistSystem.CheckVessel();
                    }
                }
            }
            if (checklistSystem.availableChecklists.Count == 0)
            {
                GUILayout.Label("No valid checklists found!");
                if (GUILayout.Button("Try Again", buttonStyle))
                {
                    Settings.Load();
                    checklistSystem.LoadChecklists();
                    mainWindow.height = 0;
                }
            }

            GUILayout.EndVertical();
            GUILayout.Label("You can create your own checklist in the config file.", labelStyle);
        }

        void OnSettingsWindow(int windowID)
        {
            settingsWindow.x = mainWindow.x + mainWindow.width;
            settingsWindow.y = mainWindow.y;
            GUILayout.BeginVertical(GUILayout.Width(220f), GUILayout.ExpandHeight(true));
            GUILayout.BeginVertical(boxStyle);
            settings_LockWindow = GUILayout.Toggle(settings_LockWindow, new GUIContent("Prevent clicking-throught", "Lock the editor while hovering over a window"), toggleStyle);

            if (!KCTInstalled)
                settings_CheckCrew = GUILayout.Toggle(settings_CheckCrew, new GUIContent("Check crew assignment", "Allow pre-launch crew assignment reminder"), toggleStyle);

            if (ToolbarManager.ToolbarAvailable)
            {
                settings_BlizzyToolbar = GUILayout.Toggle(settings_BlizzyToolbar, settings_BlizzyToolbar ? "Use blizzy78's toolbar" : "Use stock toolbar", toggleStyle);
            }
            GUILayout.EndVertical();
            if (GUILayout.Button(new GUIContent("Reload data", "Reload the config file"), buttonStyle))
            {
                Settings.Load();
                if (checklistSystem.LoadChecklists())
                    checklistSelected = false;
                mainWindow.height = 0;
            }

            if (GUILayout.Button("Apply & Close", buttonStyle))
            {
                Settings.lockOnHover = settings_LockWindow;

                if (!settings_CheckCrew && Settings.checkCrewAssignment)
                {
                    Settings.checkCrewAssignment = false;
                    if (HighLogic.LoadedScene == GameScenes.EDITOR)
                    {
                        EditorLogic.fetch.launchBtn.onClick.RemoveListener(launchDelegate);
                        EditorLogic.fetch.launchBtn.onClick.AddListener(defaultLaunchDelegate);
                    }
                }
                else if (settings_CheckCrew && !Settings.checkCrewAssignment)
                {
                    Settings.checkCrewAssignment = true;
                    if (HighLogic.LoadedScene == GameScenes.EDITOR)
                    {
                        EditorLogic.fetch.launchBtn.onClick.RemoveListener(defaultLaunchDelegate);
                        EditorLogic.fetch.launchBtn.onClick.AddListener(launchDelegate);
                    }
                }
                //--------------------------------------------------------------------------
                if (activeToolbar == toolbarType.BLIZZY && !settings_BlizzyToolbar)
                {
                    wcbutton.Destroy();
                    AddToolbarButton(toolbarType.STOCK, true);
                }

                if (activeToolbar == toolbarType.STOCK && settings_BlizzyToolbar)
                {
                    // apparently don't need a correct value in following call
                    DestroyAppButton(GameScenes.EDITOR);
                    AddToolbarButton(toolbarType.BLIZZY, true);
                }

                showSettings = false;
            }

            GUILayout.EndVertical();

            SetTooltipText();
        }

        Vector2 scrollPos;
        const int SCROLL_GREATER = 12;

        void OnWindow(int windowID)
        {

            GUILayout.BeginVertical(GUILayout.Width(225));

            if (Settings.cfgLoaded) //If the cfg file exists
            {
                if (checklistSelected) //If the checklist is selected
                {
                    if (!selectionInProgress) //If the mode, where the checked parts are set, is active
                    {
                        if (checklistSystem.ActiveChecklist.items.Count > SCROLL_GREATER)
                            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(500)); //  , GUILayout.Width((panelWidth + 3)));
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Current checklist:");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(checklistSystem.ActiveChecklist.name, labelStyle);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical(boxStyle);
                        for (int i = 0; i < checklistSystem.ActiveChecklist.items.Count; i++)
                        {
                            ChecklistItem tempItem = checklistSystem.ActiveChecklist.items[i];
                            if (tempItem.editorOnly && HighLogic.LoadedScene != GameScenes.EDITOR ||
                                 tempItem.flightOnly && HighLogic.LoadedScene != GameScenes.FLIGHT)
                                continue;
                            tempItem.DrawItem();
                            checklistSystem.ActiveChecklist.items[i] = tempItem;
                        }
                        if (Settings.jebEnabled == true)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("MOAR BOOSTERS!!!", labelStyle); //small joke :P
                            GUILayout.FlexibleSpace();
                            GUILayout.Toggle(false, "", ChecklistItem.checkboxStyle);
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();
                        if (checklistSystem.ActiveChecklist.items.Count > SCROLL_GREATER)
                            GUILayout.EndScrollView();

                        if (GUILayout.Button("Change checklist", buttonStyle, GUILayout.Height(24f)))
                        {
                            mainWindow.height = 0f;
                            checklistSelected = false;
                        }

                        //-------------------------------------------------------------------------------------------
                        //

                        if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                        {
                            checkSelected = false;
                            checklistSystem.CheckVessel();
                        }

                        if (showAdvanced && HighLogic.LoadedScene == GameScenes.EDITOR) //Advanced options showed
                        {
#if false
                            if (!showSettings)
                            {
                                if (GUILayout.Button("Show settings", buttonStyle, GUILayout.Height(24f)))
                                {
                                    mainWindow.height = 0f;
                                    showSettings = true;
                                    if (activeToolbar == toolbarType.BLIZZY)
                                        settings_BlizzyToolbar = true;
                                    else
                                        settings_BlizzyToolbar = false;
                                    settings_CheckCrew = Settings.checkCrewAssignment;
                                    settings_LockWindow = Settings.lockOnHover;
                                }
                            }
#endif
                            if (GUILayout.Button(new GUIContent("Recheck vessel", "Use this if the automatic checking doesn't work for some reason"), buttonStyle, GUILayout.Height(24f)))
                                checklistSystem.CheckVessel();


                            {
                                GUILayout.Label("Checked area:", labelStyle);
                                if (GUILayout.Toggle(!checkSelected, new GUIContent("Entire ship", "Check the entire ship"), toggleStyle) != !checkSelected)
                                {
                                    checkSelected = false;
                                    checklistSystem.CheckVessel();
                                    mainWindow.height = 0f;
                                }
                                if (GUILayout.Toggle(checkSelected, new GUIContent(partSelection == null || EditorLogic.RootPart == null ? "Selected parts (0)" : "Selected parts (" + partSelection.selectedParts.Intersect(EditorLogic.fetch.ship.parts).ToList().Count + ")", "Check only a selected section of the ship (e.g. lander/booster stage)"), toggleStyle) == !checkSelected)
                                {
                                    checkSelected = true;
                                    checklistSystem.CheckVessel();
                                }
                            }
                            if (checkSelected && EditorLogic.RootPart != null)
                            {
                                if (GUILayout.Button(new GUIContent("Select parts", "Select the checked parts"), buttonStyle, GUILayout.Height(24f)))
                                {
                                    mainWindow.height = 0f;
                                    print("[WernherChecker]: Engaging selection mode");
                                    foreach (Part part in VesselParts)
                                    {
                                        part.SetHighlightDefault();
                                    }
                                    partSelection = new PartSelection();
                                    selectionInProgress = true;
                                    selectedShowed = false;
                                    if (HighLogic.LoadedScene == GameScenes.EDITOR)
                                        InputLockManager.SetControlLock(ControlTypes.EDITOR_PAD_PICK_PLACE | ControlTypes.EDITOR_UI, "WernherChecker_partSelection");
                                }

                                if (!selectedShowed)
                                {
                                    if (GUILayout.Button(new GUIContent("Highlight selected parts", "Highlight the parts selected for checking"), buttonStyle, GUILayout.Height(24f)))
                                    {
                                        if (partSelection != null)
                                        {
                                            foreach (Part part in partSelection.selectedParts)
                                            {
                                                if (WernherChecker.VesselParts.Contains(part))
                                                {
                                                    part.SetHighlightType(Part.HighlightType.AlwaysOn);
                                                    part.SetHighlightColor(new Color(10f, 0.9f, 0f));
                                                }
                                            }
                                        }
                                        selectedShowed = true;
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button("Hide selected parts", buttonStyle, GUILayout.Height(24f)))
                                    {
                                        /*float max, min;
                                        GUI.skin.label.CalcMinMaxWidth(new GUIContent("Thisisthecontentasdsdfsdfsd"), out min, out max);
                                        Log.Info("Min: " + min + ", Max: " + max);*/
                                        foreach (Part part in WernherChecker.VesselParts)
                                        {
                                            part.SetHighlightDefault();
                                        }
                                        selectedShowed = false;
                                    }
                                }
                            }

                        }
                        if (HighLogic.LoadedScene == GameScenes.EDITOR)
                        {
                            if (GUILayout.Button(new GUIContent(showAdvanced ? "︽ Fewer Options ︽" : "︾ More Options ︾", "Show/Hide advanced options"), buttonStyle, GUILayout.Height(24f)))
                            {
                                mainWindow.height = 0f;
                                showAdvanced = !showAdvanced;
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Select parts to check by holding LMB and moving mouse", labelStyle);
                        GUILayout.Label("Current selection: " + partSelection.selectedParts.Count + " part(s)");
                        if (GUILayout.Button(new GUIContent("Done", "Finish part selection"), buttonStyle))
                        {
                            mainWindow.height = 0f;
                            print("[WernherChecker]: " + partSelection.selectedParts.Count + " parts selected");
                            foreach (Part part in WernherChecker.VesselParts)
                            {
                                part.SetHighlightDefault();
                            }
                            selectionInProgress = false;
                            if (HighLogic.LoadedScene == GameScenes.EDITOR)
                                InputLockManager.RemoveControlLock("WernherChecker_partSelection");
                            checklistSystem.CheckVessel();
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Please select checklist", labelStyle);
                    SelectChecklist();
                }
            }

            else
            {
                GUILayout.Label("Cannot find config file!", labelStyle);
            }

            GUILayout.EndVertical();
            GUI.DragWindow(); //making it dragable
            SetTooltipText();
        }
    }
}