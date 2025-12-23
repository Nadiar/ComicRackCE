using System;
using cYo.Common.Runtime;

namespace cYo.Projects.ComicRack.Plugins
{
    public class PythonSettings
    {
        public bool PythonDebug
        {
            get;
            set;
        }

        public bool PythonEnableFrames
        {
            get;
            set;
        }

        public bool PythonEnableFullFrames
        {
            get;
            set;
        }

        public bool PythonEnableTracing
        {
            get;
            set;
        }

        public bool PythonAutosaveEnabled
        {
            get;
            set;
        }

        public string PythonAutosavePath
        {
            get;
            set;
        }
    }
}
