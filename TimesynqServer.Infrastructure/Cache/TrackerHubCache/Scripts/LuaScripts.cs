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

        private record LuaLibraryModule(string Name, string Code, params LuaLibraryModule[] Dependencies);
        private static class LuaLibraryModules
        {
            public static LuaLibraryModule KeyBuilderModule = new(
                "key_builder",
                LoadEmbeddedScript("Lib.key_builder.lua")
            );

            public static LuaLibraryModule RoomModule = new(
                "room",
                LoadEmbeddedScript("Lib.room.lua"),
                KeyBuilderModule
            );

            public static LuaLibraryModule FrameModule = new(
                "frame", 
                LoadEmbeddedScript("Lib.frame.lua"),
                RoomModule
            );

            public static LuaLibraryModule OperationLogModule = new(
                "operation_log", 
                LoadEmbeddedScript("Lib.operation_log.lua"),
                RoomModule
            );

            public static LuaLibraryModule ConnectionModule = new(
                "connection",
                LoadEmbeddedScript("Lib.connection.lua")
            );
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
                var visited = new HashSet<string>();

                foreach (LuaLibraryModule module in _modules)
                {
                    AppendModule(module, sb, visited);
                }

                sb.AppendLine(_body.Trim());

                return sb.ToString();
            }

            private void AppendModule(LuaLibraryModule module, StringBuilder sb, HashSet<string> visited)
            {
                if (!visited.Add(module.Name))
                    return;

                foreach (LuaLibraryModule dependency in module.Dependencies)
                {
                    AppendModule(dependency, sb, visited);
                }

                sb.AppendLine(module.Code.Trim());
                sb.AppendLine();
            }
        }

        public static readonly string RoomJoinScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Body(LoadEmbeddedScript("set_connection_and_create_room_if_empty.lua"))
            .Build();

        public static readonly string RoomLeaveScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Body(LoadEmbeddedScript("remove_connection_and_cleanup_if_empty.lua"))
            .Build();

        public static readonly string RoomRemoveScript = new LuaScriptBuilder()
            .Body(LoadEmbeddedScript("remove_room.lua"))
            .Build();

        public static readonly string BpmUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_bpm.lua"))
            .Build();

        public static readonly string ChannelCountUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_channel_count.lua"))
            .Build();

        public static readonly string SequencerLengthUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_sequencer_length.lua"))
            .Build();

        public static readonly string LineCountUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.FrameModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_line_count.lua"))
            .Build();

        public static readonly string LinesPerBeatUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.FrameModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_lines_per_beat.lua"))
            .Build();

        public static readonly string ChannelTypeUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.FrameModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_channel_type.lua"))
            .Build();

        public static readonly string LineUpdateScript = new LuaScriptBuilder()
            .Use(LuaLibraryModules.ConnectionModule)
            .Use(LuaLibraryModules.KeyBuilderModule)
            .Use(LuaLibraryModules.RoomModule)
            .Use(LuaLibraryModules.FrameModule)
            .Use(LuaLibraryModules.OperationLogModule)
            .Body(LoadEmbeddedScript("update_line.lua"))
            .Build();
    }
}
