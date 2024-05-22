using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace BannerlordExpanded.SettlementInteractions.Inns.Patches
{
    [HarmonyPatchCategory("InnModule")]
    [HarmonyPatch(typeof(VillageEncounter), "CreateAndOpenMissionController")]
    internal static class VillageEncounterPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(IMission __result, Location nextLocation, Location previousLocation = null, CharacterObject talkToChar = null, string playerSpecialSpawnTag = null)
        {
            IMission result = null;
            if (nextLocation.StringId == "village_inn")
            {
                result = CampaignMission.OpenIndoorMission(nextLocation.GetSceneName(0), 0, nextLocation, talkToChar);
                __result = result;
                return false;
            }
            return true;
        }
    }
}
