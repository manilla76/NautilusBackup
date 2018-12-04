using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniLaunch
{
    public struct ProgramModel
    {
        public string Path;
        public string Parameters;
        public StartType StartType;
        public bool? IsSelected;
    }

    public enum StartType
    {
        none,
        min,
        max
    }
}
