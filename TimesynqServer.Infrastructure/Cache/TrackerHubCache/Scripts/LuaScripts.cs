using System.Reflection;
using System.Text;

namespace TimesynqServer.Infrastructure.Cache.TrackerHubCache.Scripts
{
    internal static class LuaScripts
    {
        private static string LoadEmbeddedScript(string filename)
        {
            const string ScriptPathPrefix = "TimesynqServer.Infrastructure.Cache.TrackerHubCache.Scripts";
            string path = $"{ScriptPathPrefix}.{filename}";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(path)
                ?? throw new Exception($"Resource not found: {path}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private record LuaLibraryModule(string Name, string Code);
        private static class LuaLibraryModules
        {

        }

        private class LuaScriptBuilder
        {
            private readonly List<LuaLibraryModule> _modules = [];
            private string _body = string.Empty;

            public LuaScriptBuilder Use(LuaLibraryModule module)
            {
                _modules.Add(module);
                return this;
            }

            public LuaScriptBuilder Body(string body)
            {
                _body = body;
                return this;
            }

            public string Build()
            {
                var sb = new StringBuilder();

                foreach (LuaLibraryModule module in _modules
                    .GroupBy(m => m.Name).Select(g => g.First()))
                {
                    sb.AppendLine(module.Code.Trim());
                    sb.AppendLine();
                }

                sb.AppendLine(_body.Trim());

                return sb.ToString();
            }
        }

        public static readonly string RoomJoinScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("set_connection_and_create_room_if_empty.lua"))
            .Build();

        public static readonly string RoomLeaveScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("remove_connection_and_cleanup_if_empty.lua"))
            .Build();

        public static readonly string RoomRemoveScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("remove_room.lua"))
            .Build();

        public static readonly string BpmUpdateScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("update_bpm.lua"))
            .Build();

        public static readonly string ChannelCountUpdateScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("update_channel_count.lua"))
            .Build();

        public static readonly string LineCountUpdateScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("update_line_count.lua"))
            .Build();

        public static readonly string LinesPerBeatUpdateScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("update_lines_per_beat.lua"))
            .Build();

        public static readonly string ChannelTypeUpdateScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("update_channel_type.lua"))
            .Build();

        public static readonly string LineUpdateScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("update_line.lua"))
            .Build();
    }
}
