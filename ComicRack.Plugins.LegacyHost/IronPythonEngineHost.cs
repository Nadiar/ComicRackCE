using System;
using System.Collections.Concurrent;
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using cYo.Projects.ComicRack.Plugins.LegacyHost.Bridge;

namespace cYo.Projects.ComicRack.Plugins.LegacyHost
{
    public class IronPythonEngineHost
    {
        private readonly ScriptEngine _engine;
        private readonly ConcurrentDictionary<string, ScriptScope> _scopes = new ConcurrentDictionary<string, ScriptScope>();
        private readonly LyoutProxy _comicRackProxy;

        public IronPythonEngineHost()
        {
            _engine = Python.CreateEngine();
            _comicRackProxy = new LyoutProxy("ComicRack");
            
            // Inject basic proxy into top level
            var scope = _engine.GetSysModule();
            // In IronPython 2.7.4, sys module and others might need different setup
        }

        public object ExecuteMethod(string scriptPath, string methodName, object[] args)
        {
            var scope = _scopes.GetOrAdd(scriptPath, path => 
            {
                var s = _engine.CreateScope();
                s.SetVariable("ComicRack", _comicRackProxy);
                _engine.ExecuteFile(path, s);
                return s;
            });

            if (string.IsNullOrEmpty(methodName)) return null;

            var method = scope.GetVariable(methodName);
            return _engine.Operations.Invoke(method, args);
        }
    }
}
