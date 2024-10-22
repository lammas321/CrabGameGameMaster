using HarmonyLib;
using static GameMaster.GameMaster;

namespace GameMaster
{
    internal static class Patches
    {
        // Next map/game mode override
        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.LoadMap), [typeof(int), typeof(int)])]
        [HarmonyPrefix]
        [HarmonyPriority(int.MaxValue)]
        internal static void PreServerSendLoadMap(ref int param_0, ref int param_1)
        {
            if (Instance.nextGameModeId == -1 || param_1 == GameModeManager.Instance.defaultMode.id)
                return;
            
            param_1 = Instance.nextGameModeId;
            Instance.nextGameModeId = -1;
            if (Instance.nextMapId != -1)
            {
                param_0 = Instance.nextMapId;
                Instance.nextMapId = -1;
                return;
            }

            param_0 = GameModeManager.Instance.allGameModes[param_1].Method_Public_Map_Int32_0(GameManager.Instance.GetPlayersAlive()).id;
        }

        // Respawn player if they died in the lobby
        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.PlayerDied))]
        [HarmonyPostfix]
        internal static void PostServerSendPlayerDied(ulong param_0)
        {
            if (LobbyManager.Instance.gameMode == GameModeManager.Instance.defaultMode)
                Utility.QueueRespawn(param_0, 3f);
        }
    }
}