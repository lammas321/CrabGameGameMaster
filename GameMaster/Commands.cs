using ChatCommands;
using SteamworksNative;
using System.Collections.Generic;
using System.Linq;
using static ChatCommands.CommandArgumentParser;
using static GameMaster.GameMaster;

namespace GameMaster
{
    public class ModesCommand : BaseCommand
    {
        public ModesCommand()
        {
            id = "modes";
            description = "Lists all of the game modes that can be played.";
            args = new([
                new(
                    [typeof(int)],
                    "page",
                    false
                )
            ]);
        }

        public BaseCommandResponse ShowModesPage(BaseExecutionMethod executionMethod, int page = 1)
        {
            if (page < 1)
                return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);

            int currentPage = 1;
            int currentLine = 0;
            List<string> lines = [string.Empty];

            GameModeData[] gameModes = GameModeManager.Instance.allGameModes;
            for (int i = 0; i < gameModes.Length; i++)
            {
                GameModeData gameMode = gameModes[i];
                string str = gameMode.modeName;
                if (i != gameModes.Length - 1)
                    str += ", ";

                if (lines[currentLine].Length + str.Length - 1 > executionMethod.MaxResponseLength)
                {
                    currentLine++;
                    if (currentLine == 3)
                    {
                        if (currentPage + 1 > page)
                            break;

                        currentPage++;
                        currentLine = 0;
                        lines = [];
                    }

                    lines.Add(string.Empty);
                }

                lines[currentLine] += str;
            }

            if (page != currentPage)
                return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);

            return new StyledCommandResponse($"Modes Page #{page}", [.. lines], CommandResponseType.Private);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return ShowModesPage(executionMethod, 1);

            ParsedResult<int> pageResult = Api.CommandArgumentParser.Parse<int>(args);
            if (pageResult.successful)
                return ShowModesPage(executionMethod, pageResult.result);

            return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);
        }
    }

    public class MapsCommand : BaseCommand
    {
        public MapsCommand()
        {
            id = "maps";
            description = "Lists all of the maps that can be played.";
            args = new([
                new(
                    [typeof(int)],
                    "page",
                    false
                )
            ]);
        }

        public BaseCommandResponse ShowMapsPage(BaseExecutionMethod executionMethod, int page = 1)
        {
            if (page < 1)
                return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);
            
            int currentPage = 1;
            int currentLine = 0;
            List<string> lines = [string.Empty];

            Map[] maps = MapManager.Instance.maps;
            for (int i = 0; i < maps.Length; i++)
            {
                Map map = maps[i];
                string str = map.mapName;
                if (i != maps.Length - 1)
                    str += ", ";

                if (lines[currentLine].Length + str.Length - 1 > executionMethod.MaxResponseLength)
                {
                    currentLine++;
                    if (currentLine == 3)
                    {
                        if (currentPage + 1 > page)
                            break;

                        currentPage++;
                        currentLine = 0;
                        lines = [];
                    }

                    lines.Add(string.Empty);
                }

                lines[currentLine] += str;
            }

            if (page != currentPage)
                return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);

            return new StyledCommandResponse($"Maps Page #{page}", [.. lines], CommandResponseType.Private);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return ShowMapsPage(executionMethod, 1);

            ParsedResult<int> pageResult = Api.CommandArgumentParser.Parse<int>(args);
            if (pageResult.successful)
                return ShowMapsPage(executionMethod, pageResult.result);

            return new BasicCommandResponse(["You didn't specify a valid page number."], CommandResponseType.Private);
        }
    }

    public class ModeInfoCommand : BaseCommand
    {
        public ModeInfoCommand()
        {
            id = "modeinfo";
            description = "Shows the description for the given mode.";
            args = new([
                new(
                    [typeof(GameModeData)],
                    "mode"
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new StyledCommandResponse("Mode Info", [LobbyManager.Instance.gameMode.modeName, .. Utility.FormatGameModeDescription(LobbyManager.Instance.gameMode.modeDescription)]);

            ParsedResult<GameModeData> gameModeResult = Api.CommandArgumentParser.Parse<GameModeData>(args);
            if (gameModeResult.successful)
                return new StyledCommandResponse("Mode Info", [gameModeResult.result.modeName, .. Utility.FormatGameModeDescription(gameModeResult.result.modeDescription)]);

            return new BasicCommandResponse(["You didn't specify a valid mode."], CommandResponseType.Private);
        }
    }

    public class PlayCommand : BaseCommand
    {
        public PlayCommand()
        {
            id = "play";
            description = "Sets the next mode and map to be played.";
            args = new([
                new(
                    [typeof(GameModeData), typeof(DefaultCommandArgumentParsers.Reset)],
                    "mode/reset",
                    true
                ),
                new(
                    [typeof(Map)],
                    "map"
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A mode or 'reset' is required for the first argument."], CommandResponseType.Private);

            ParsedResult<DefaultCommandArgumentParsers.Reset> resetResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.Reset>(args);
            if (resetResult.successful)
            {
                Instance.nextGameModeId = -1;
                Instance.nextMapId = -1;
                return new StyledCommandResponse("Play", ["Reset the queued mode and map."], CommandResponseType.Private);
            }

            ParsedResult<GameModeData> gameModeResult = Api.CommandArgumentParser.Parse<GameModeData>(args);
            if (!gameModeResult.successful)
                return new BasicCommandResponse(["You didn't specify an existing mode."], CommandResponseType.Private);

            if (gameModeResult.result == GameModeManager.Instance.defaultMode || gameModeResult.result == GameModeManager.Instance.practiceMode)
                return new BasicCommandResponse(["You cannot set the next mode to Waiting Room or Practice."], CommandResponseType.Private);
            Instance.nextGameModeId = gameModeResult.result.id;

            ParsedResult<Map> mapResult = Api.CommandArgumentParser.Parse<Map>(gameModeResult.newArgs);
            if (!mapResult.successful)
                return new StyledCommandResponse("Play", [$"Set the next mode to {gameModeResult.result.modeName}."], CommandResponseType.Private);

            Instance.nextMapId = mapResult.result.id;
            return new StyledCommandResponse("Play", [$"Set the next mode to '{gameModeResult.result.modeName}'.", $"Set the next map to '{mapResult.result.mapName}'."], CommandResponseType.Private, false);
        }
    }

    public class StartCommand : BaseCommand
    {
        public StartCommand()
        {
            id = "start";
            description = "Starts the games if you're in the lobby or practice.";
            args = new([]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (LobbyManager.Instance.gameMode.id != GameModeManager.Instance.defaultMode.id && LobbyManager.Instance.gameMode.id != GameModeManager.Instance.practiceMode.id)
                return new BasicCommandResponse(["Unable to start while not in the lobby or practice."], CommandResponseType.Private);

            GameLoop.Instance.StartGames();
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class SkipCommand : BaseCommand
    {
        public SkipCommand()
        {
            id = "skip";
            description = "Skips the current mode.";
            args = new([]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (LobbyManager.Instance.gameMode.id == GameModeManager.Instance.defaultMode.id || LobbyManager.Instance.gameMode.id == GameModeManager.Instance.practiceMode.id)
                return new BasicCommandResponse(["Unable to skip while not playing a mode."], CommandResponseType.Private);

            if (!LobbyManager.Instance.gameMode.skipAsString)
            {
                GameManager.Instance.gameMode.modeState = GameModeState.Ended;
                GameManager.Instance.gameMode.EndRound();
                ServerSend.SendGameModeTimer(1f, (int)GameManager.Instance.gameMode.modeState);
            }

            GameLoop.Instance.NextGame();
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class LobbyCommand : BaseCommand
    {
        public LobbyCommand()
        {
            id = "lobby";
            description = "Sends everyone to the lobby.";
            args = new([]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            Instance.nextGameModeId = -1;
            Instance.nextMapId = -1;

            if (!LobbyManager.Instance.gameMode.skipAsString)
            {
                GameManager.Instance.gameMode.modeState = GameModeState.Ended;
                GameManager.Instance.gameMode.EndRound();
                ServerSend.SendGameModeTimer(1f, (int)GameManager.Instance.gameMode.modeState);
            }

            GameLoop.Instance.RestartLobby();
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class PracticeCommand : BaseCommand
    {
        public PracticeCommand()
        {
            id = "practice";
            description = "Goes into practice mode with the given map.";
            args = new([
                new(
                    [typeof(Map)],
                    "map",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A map is required for the first argument."], CommandResponseType.Private);

            ParsedResult<Map> mapResult = Api.CommandArgumentParser.Parse<Map>(args);
            if (!mapResult.successful)
                return new BasicCommandResponse(["You didn't specify an existing map."], CommandResponseType.Private);

            Instance.nextGameModeId = -1;
            Instance.nextMapId = -1;

            if (!LobbyManager.Instance.gameMode.skipAsString)
            {
                GameManager.Instance.gameMode.modeState = GameModeState.Ended;
                GameManager.Instance.gameMode.EndRound();
                ServerSend.SendGameModeTimer(1f, (int)GameManager.Instance.gameMode.modeState);
            }

            ServerSend.LoadMap(mapResult.result.id, GameModeManager.Instance.practiceMode.id);
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class ToggleModeCommand : BaseCommand
    {
        public ToggleModeCommand()
        {
            id = "togglemode";
            description = "Toggle a mode on or off.";
            args = new([
                new(
                    [typeof(GameModeData)],
                    "mode",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A mode is required for the first argument."], CommandResponseType.Private);

            ParsedResult<GameModeData> gameModeResult = Api.CommandArgumentParser.Parse<GameModeData>(args);
            if (!gameModeResult.successful)
                return new BasicCommandResponse(["You didn't specify an existing mode."], CommandResponseType.Private);

            if (gameModeResult.result == GameModeManager.Instance.defaultMode || gameModeResult.result == GameModeManager.Instance.practiceMode)
                return new BasicCommandResponse(["You cannot toggle Waiting Room or Practice on."], CommandResponseType.Private);

            if (GameModeManager.Instance.allPlayableGameModes.Contains(gameModeResult.result))
            {
                GameModeManager.Instance.allPlayableGameModes.Remove(gameModeResult.result);
                SteamMatchmaking.SetLobbyData(SteamManager.Instance.currentLobby, "Modes", GameModeManager.Instance.GetAvailableModesString());
                return new StyledCommandResponse("Disabled Mode", [gameModeResult.result.modeName], CommandResponseType.Private);
            }
            GameModeManager.Instance.allPlayableGameModes.Add(gameModeResult.result);
            SteamMatchmaking.SetLobbyData(SteamManager.Instance.currentLobby, "Modes", GameModeManager.Instance.GetAvailableModesString());
            return new StyledCommandResponse("Enabled Mode", [gameModeResult.result.modeName], CommandResponseType.Private);
        }
    }

    public class RestartCommand : BaseCommand
    {
        public RestartCommand()
        {
            id = "restart";
            description = "Restart the current round.";
            args = new([]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            foreach (ulong playerClientId in GameManager.Instance.activePlayers.Keys)
                LobbyManager.Instance.GetClient(playerClientId).field_Public_Boolean_0 = true; // Participating (will spawn players that died in the current round when the round restarts)

            if (!LobbyManager.Instance.gameMode.skipAsString)
            {
                GameManager.Instance.gameMode.modeState = GameModeState.Ended;
                GameManager.Instance.gameMode.EndRound();
                ServerSend.SendGameModeTimer(1f, (int)GameManager.Instance.gameMode.modeState);
            }

            ServerSend.StartGame();
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class TimeCommand : BaseCommand
    {
        public TimeCommand()
        {
            id = "time";
            description = "Set the mode timer in seconds.";
            args = new([
                new(
                    [typeof(float)],
                    "seconds",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A time in seconds is required for the first argument."], CommandResponseType.Private);

            if (GameManager.Instance == null || LobbyManager.Instance.gameMode.id == GameModeManager.Instance.defaultMode.id || LobbyManager.Instance.gameMode.id == GameModeManager.Instance.practiceMode.id)
                return new BasicCommandResponse(["You cannot set the mode timer right now."], CommandResponseType.Private);

            ParsedResult<float> timeResult = Api.CommandArgumentParser.Parse<float>(args);
            if (!timeResult.successful || timeResult.result < 0f)
                return new BasicCommandResponse(["You didn't specify a positive number."], CommandResponseType.Private);

            ServerSend.SendGameModeTimer(timeResult.result == 0f ? float.Epsilon : timeResult.result, (int)GameManager.Instance.gameMode.modeState);
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }


    public class KillCommand : BaseCommand
    {
        public KillCommand()
        {
            id = "kill";
            description = "Kills the given player(s).";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId[]), typeof(DefaultCommandArgumentParsers.OnlineClientId)],
                    "player(s)",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (GameManager.Instance == null || (!LobbyManager.Instance.gameMode.skipAsString && GameManager.Instance.gameMode.modeState == GameModeState.Freeze))
                return new BasicCommandResponse(["You cannot kill players right now."], CommandResponseType.Private);

            if (args.Length == 0)
            {
                if (executionMethod is not ChatExecutionMethod)
                    return new BasicCommandResponse(["A player selector or player is required for the first argument."], CommandResponseType.Private);

                if (!GameManager.Instance.activePlayers.ContainsKey((ulong)executorDetails) || GameManager.Instance.activePlayers[(ulong)executorDetails].dead)
                    return new BasicCommandResponse(["You are already dead."], CommandResponseType.Private);

                GameServer.PlayerDied((ulong)executorDetails, (ulong)executorDetails, UnityEngine.Vector3.zero);
                return new BasicCommandResponse([], CommandResponseType.Private);
            }

            if (!ignorePermissions && !executionMethod.HasPermission(executorDetails, "command.kill.others"))
                return new BasicCommandResponse(["You don't have sufficient permission to kill other players."], CommandResponseType.Private);

            IEnumerable<ulong> clientIds;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId[]> playersResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId[]>(args);
            if (playersResult.successful)
                clientIds = playersResult.result.Select(clientId => (ulong)clientId);
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> playerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
                if (playerResult.successful)
                    clientIds = [playerResult.result];
                else
                    return new BasicCommandResponse(["You did not select any players."], CommandResponseType.Private);
            }

            foreach (ulong clientId in clientIds)
                if (GameManager.Instance.activePlayers.ContainsKey(clientId) && !GameManager.Instance.activePlayers[clientId].dead)
                    GameServer.PlayerDied(clientId, clientId, UnityEngine.Vector3.zero);
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class ExplodeCommand : BaseCommand
    {
        public ExplodeCommand()
        {
            id = "explode";
            description = "Explodes the given player(s).";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId[]), typeof(DefaultCommandArgumentParsers.OnlineClientId)],
                    "player(s)",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (GameManager.Instance == null || (!LobbyManager.Instance.gameMode.skipAsString && GameManager.Instance.gameMode.modeState == GameModeState.Freeze))
                return new BasicCommandResponse(["You cannot explode players right now."], CommandResponseType.Private);

            if (args.Length == 0)
            {
                if (executionMethod is not ChatExecutionMethod)
                    return new BasicCommandResponse(["A player selector or player is required for the first argument."], CommandResponseType.Private);

                if (!GameManager.Instance.activePlayers.ContainsKey((ulong)executorDetails) || GameManager.Instance.activePlayers[(ulong)executorDetails].dead)
                    return new BasicCommandResponse(["You are already dead."], CommandResponseType.Private);

                GameServer.PlayerDied((ulong)executorDetails, 1, UnityEngine.Vector3.zero);
                return new BasicCommandResponse([], CommandResponseType.Private);
            }

            if (!ignorePermissions && !executionMethod.HasPermission(executorDetails, "command.explode.others"))
                return new BasicCommandResponse(["You don't have sufficient permission to explode other players."], CommandResponseType.Private);

            IEnumerable<ulong> clientIds;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId[]> playersResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId[]>(args);
            if (playersResult.successful)
                clientIds = playersResult.result.Select(clientId => (ulong)clientId);
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> playerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
                if (playerResult.successful)
                    clientIds = [playerResult.result];
                else
                    return new BasicCommandResponse(["You did not select any players."], CommandResponseType.Private);
            }

            foreach (ulong clientId in clientIds)
                if (GameManager.Instance.activePlayers.ContainsKey(clientId) && !GameManager.Instance.activePlayers[clientId].dead)
                    GameServer.PlayerDied(clientId, 1, UnityEngine.Vector3.zero);
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }

    public class RespawnCommand : BaseCommand
    {
        public RespawnCommand()
        {
            id = "respawn";
            description = "Respawns the given player(s).";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId[]), typeof(DefaultCommandArgumentParsers.OnlineClientId)],
                    "player(s)",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (GameManager.Instance == null)
                return new BasicCommandResponse(["You cannot respawn players right now."], CommandResponseType.Private);

            if (args.Length == 0)
            {
                if (executionMethod is not ChatExecutionMethod)
                    return new BasicCommandResponse(["A player selector or player is required for the first argument."], CommandResponseType.Private);

                if (GameManager.Instance.activePlayers.ContainsKey((ulong)executorDetails) && !GameManager.Instance.activePlayers[(ulong)executorDetails].dead)
                    return new BasicCommandResponse(["You are already alive."], CommandResponseType.Private);

                Utility.RespawnPlayer((ulong)executorDetails);
                return new BasicCommandResponse([], CommandResponseType.Private);
            }

            IEnumerable<ulong> clientIds;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId[]> playersResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId[]>(args);
            if (playersResult.successful)
                clientIds = playersResult.result.Select(clientId => (ulong)clientId);
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> playerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
                if (playerResult.successful)
                    clientIds = [playerResult.result];
                else
                    return new BasicCommandResponse(["You did not select any players."], CommandResponseType.Private);
            }

            foreach (ulong clientId in clientIds)
                if (!GameManager.Instance.activePlayers.ContainsKey(clientId) || GameManager.Instance.activePlayers[clientId].dead)
                    Utility.RespawnPlayer(clientId);
            return new BasicCommandResponse([], CommandResponseType.Private);
        }
    }
}