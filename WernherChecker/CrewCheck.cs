﻿using PreFlightTests;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WernherChecker
{
    class CrewCheck : IPreFlightTest
    {
        static PreFlightCheck checks;

        public static void OnButtonInput()
        {
            checks = new PreFlightCheck(Complete, Abort);
            checks.AddTest(new CrewCheck());
            checks.RunTests();
        }
       
        public bool Test()
        {
            if (EditorLogic.fetch.editorScreen == EditorScreen.Crew)
                return true;

                foreach (Part part in WernherChecker.VesselParts)
                {
                    if (part.CrewCapacity > 0)
                    {
                        EditorLogic.fetch.Lock(true, true, true, "WernherChecker_crewCheck");
                        return false;
                    }
                }
                return true;
        }

        public string GetWarningTitle()
        {
            return "Warning: Crew Assignment";
        }

        public string GetWarningDescription()
        {
            return "Have you checked the crew assignment?";
        }

        public string GetProceedOption()
        {
            return "Yes, I have. Go for LAUNCH!";
        }

        public string GetAbortOption()
        {
            return "No, I haven't! Thanks for the reminder!";
        }

        public static void Complete()
        {
            Log.Info("[WernherChecker]: Crew is OK, launching vessel.");
            EditorLogic.fetch.Unlock("WernherChecker_crewCheck");
            EditorLogic.fetch.launchVessel();
        }

        public static void Abort()
        {
            Log.Info("[WernherChecker]: Showing crew panel.");
            EditorLogic.fetch.Unlock("WernherChecker_crewCheck");
            EditorLogic.fetch.SelectPanelCrew();
        }
    }        
}
