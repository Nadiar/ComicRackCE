using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cYo.Projects.ComicRack.Plugins.LegacyHost.Bridge
{
    /// <summary>
    /// A dynamic proxy that forwards IronPython calls to the main CE process via IPC.
    /// </summary>
    public class LyoutProxy : DynamicObject
    {
        private readonly string _path;
        private static ApiIpcClient _apiClient;

        public static void SetApiClient(ApiIpcClient client) => _apiClient = client;

        public LyoutProxy(string path)
        {
            _path = path;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string fullPath = string.IsNullOrEmpty(_path) ? binder.Name : $"{_path}.{binder.Name}";
            
            if (_apiClient == null)
            {
                result = new LyoutProxy(fullPath);
                return true;
            }

            var request = new IpcRequest { Command = "GetMember", MethodName = fullPath };
            var response = _apiClient.SendRequestAsync(request).GetAwaiter().GetResult();

            if (response.Success && response.Result != null)
            {
                // If the result is a simple type, return it.
                // If it's a marker for an object, return a new proxy.
                if (response.Result is string s && s.StartsWith("__PROXY__:"))
                {
                    result = new LyoutProxy(fullPath);
                }
                else
                {
                    result = response.Result;
                }
            }
            else
            {
                result = new LyoutProxy(fullPath);
            }
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            string fullPath = string.IsNullOrEmpty(_path) ? binder.Name : $"{_path}.{binder.Name}";
            
            if (_apiClient == null)
            {
                result = null;
                return true;
            }

            var request = new IpcRequest { Command = "InvokeMember", MethodName = fullPath, Arguments = args };
            var response = _apiClient.SendRequestAsync(request).GetAwaiter().GetResult();

            result = response.Success ? response.Result : null;
            return true;
        }

        public override string ToString()
        {
            return $"ComicRackProxy({_path})";
        }
    }
}
