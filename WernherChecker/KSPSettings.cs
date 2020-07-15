
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;



namespace WernherChecker
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class WernersSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "General Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Wernher's Checker"; } }
        public override string DisplaySection { get { return "Wernher's Checker"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomParameterUI("Always open when entering editor")]
        public bool alwaysOpeninEditor = true;

        [GameParameters.CustomParameterUI("Always open when entering flight")]
        public bool alwaysOpenInFlight = false;

        [GameParameters.CustomParameterUI("Check Crew Assignment")]
        public bool checkCrewAssignment = true;

        [GameParameters.CustomParameterUI("Remember last checklist")]
        public bool rememberLastChecklist = true;

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            //if (member.Name == "enabled")
            //    return true;

            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return true;
            //            return true; //otherwise return true
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }

    }
}
