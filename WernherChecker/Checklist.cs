using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WernherChecker
{
    public class Checklist
    {
        public string name = "";
        public bool editorOnly = false;
        public bool flightOnly = false;
        public List<ChecklistItem> items = new List<ChecklistItem>();

        public WernherChecker MainInstance
        {
            get { return WernherChecker.Instance; }
        }

    }
}
