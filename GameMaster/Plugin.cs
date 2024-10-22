using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Globalization;

namespace GameMaster
{
    [BepInPlugin($"lammas123.{MyPluginInfo.PLUGIN_NAME}", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("lammas123.ChatCommands")]
    public class GameMaster : BasePlugin
    {
        internal static GameMaster Instance;

        internal int nextGameModeId = -1;
        internal int nextMapId = -1;
        
        public override void Load()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            Instance = this;

            ChatCommands.Api.RegisterCommand(new ModesCommand());
            ChatCommands.Api.RegisterCommand(new MapsCommand());
            ChatCommands.Api.RegisterCommand(new ModeInfoCommand());
            ChatCommands.Api.RegisterCommand(new PlayCommand());
            ChatCommands.Api.RegisterCommand(new StartCommand());
            ChatCommands.Api.RegisterCommand(new SkipCommand());
            ChatCommands.Api.RegisterCommand(new LobbyCommand());
            ChatCommands.Api.RegisterCommand(new PracticeCommand());
            ChatCommands.Api.RegisterCommand(new ToggleModeCommand());
            ChatCommands.Api.RegisterCommand(new RestartCommand());
            ChatCommands.Api.RegisterCommand(new TimeCommand());

            ChatCommands.Api.RegisterCommand(new KillCommand());
            ChatCommands.Api.RegisterCommand(new ExplodeCommand());
            ChatCommands.Api.RegisterCommand(new RespawnCommand());
            
            Harmony.CreateAndPatchAll(typeof(Patches));
            Log.LogInfo($"Loaded [{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION}]");
        }
    }
}