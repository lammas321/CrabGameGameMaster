# CrabGameGameMaster
A BepInEx mod for Crab Game that adds some fun commands to use.

Depends on [ChatCommands](https://github.com/lammas321/CrabGameChatCommands)

## What commands does this mod add?
- !modes and !maps -> Shows a list of all existing game modes and maps, can be indexed with a page number
- !modeinfo -> Shows the description of the provided game mode, or the current game mode if nothing is provided
- !play -> Queues the given game mode and (optional) map, forcing the next game mode and (again optional) map to be played next
- !start -> Forces the games to start from the lobby, skips the need for enough players to ready up and wait to start
- !skip -> Skips the current game mode being played and starts the next one with all currently living players
- !lobby -> Sends everyone to the lobby, or reloads the lobby if you're already there
- !practice -> Sends everyone to practice mode on the given map
- !togglemode -> Toggles the given mode on or off, allowing you to change what can be played without having to close and open the lobby
- !restart -> Restarts the current game mode from the beginning and revives anyone that died that round, useful if a hacker just flung everybody and you want to bring everyone back
- !time -> Allows you to set the time remaining for the current phase of the current game mode
- !kill -> Allows you to... make yourself (or others if you have the command.kill.others permission) ragdoll
- !explode -> Allows you to... make yourself (or others if you have the command.explode.others permission) spontaneously combust
- !respawn -> Respawns the given players, if the player spawned as a spectator in the current round and you respawn them, they will be frozen in place until the next round where they will spawn normally
  - Respawning spectators that died in a previous round or that joined as a spectator mid game is not recommended, as it can cause desync issues with how Crab Game is networked

More detail on how to use these commands (such as the arguments they take in) can be found in game by using the !help command
