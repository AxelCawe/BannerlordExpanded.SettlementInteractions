using HarmonyLib;
using SandBox.CampaignBehaviors;
using SandBox.Objects;
using SandBox.Objects.Usables;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace BannerlordExpanded.SettlementInteractions.HostPartyForSoldiers.Patches
{
    [HarmonyPatchCategory("HostPartyForSoldiersModule")]
    [HarmonyPatch(typeof(SettlementMusiciansCampaignBehavior), "OnMissionStarted")]
    public static class SettlementMusiciansCampaignBehaviorPatch_HostPartyForSoldiers

    {
        [HarmonyPostfix]
        static void Postfix(IMission mission)
        {
            //InformationManager.DisplayMessage(new InformationMessage($"Attempting patch {CampaignMission.Current.Location.StringId}"));
            Mission mission2;
            if ((mission2 = (mission as Mission)) != null && CampaignMission.Current != null && PlayerEncounter.LocationEncounter != null && PlayerEncounter.LocationEncounter.Settlement != null && (CampaignMission.Current.Location != null && CampaignMission.Current.Location.StringId == "castle_partyroom"))
            {
                IEnumerable<MusicianGroup> enumerable = mission2.MissionObjects.FindAllWithType<MusicianGroup>();
                Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
                foreach (MusicianGroup musicianGroup in enumerable)
                {
                    List<SettlementMusicData> playList = CreateRandomPlayList(settlement);
                    musicianGroup.SetPlayList(playList);
                    //InformationManager.DisplayMessage(new InformationMessage($"Patched{playList.Count}"));
                }
            }
        }

        static List<SettlementMusicData> CreateRandomPlayList(Settlement settlement)
        {
            List<string> listOfLocationTags = new List<string>();
            string stringId = CampaignMission.Current.Location.StringId;

            listOfLocationTags.Add("tavern");
            listOfLocationTags.Add("lordshall");

            Dictionary<CultureObject, float> dictionary = new Dictionary<CultureObject, float>();
            List<CultureObject> objectTypeList = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
            Town town = settlement.Town;
            float num;
            if (town == null)
            {
                Village village = settlement.Village;
                num = ((village != null) ? village.Bound.Town.Loyalty : 100f);
            }
            else
            {
                num = town.Loyalty;
            }
            float num2 = num * 0.01f;
            float num3 = 0f;
            using (List<CultureObject>.Enumerator enumerator = objectTypeList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CultureObject c = enumerator.Current;
                    dictionary.Add(c, 0f);
                    float num4 = Kingdom.All.Sum(delegate (Kingdom k)
                    {
                        if (c != k.Culture)
                        {
                            return 0f;
                        }
                        return k.TotalStrength;
                    });
                    if (num4 > num3)
                    {
                        num3 = num4;
                    }
                }
            }
            foreach (Kingdom kingdom in Kingdom.All)
            {
                float num5 = (Campaign.MapDiagonal - Campaign.Current.Models.MapDistanceModel.GetDistance(kingdom.FactionMidSettlement, settlement.MapFaction.FactionMidSettlement)) / Campaign.MaximumDistanceBetweenTwoSettlements;
                float num6 = num5 * num5 * num5 * 2f;
                num6 += (settlement.MapFaction.IsAtWarWith(kingdom) ? 1f : 2f) * num2;
                dictionary[kingdom.Culture] = TaleWorlds.Library.MathF.Max(dictionary[kingdom.Culture], num6);
            }
            Dictionary<CultureObject, float> dictionary2;
            CultureObject culture;
            foreach (Kingdom kingdom2 in Kingdom.All)
            {
                dictionary2 = dictionary;
                culture = kingdom2.Culture;
                dictionary2[culture] += kingdom2.TotalStrength / num3 * 0.5f;
            }
            foreach (Town town2 in Town.AllTowns)
            {
                float num7 = (Campaign.MapDiagonal - Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, town2.Settlement)) / Campaign.MapDiagonal;
                float num8 = num7 * num7 * num7;
                num8 *= TaleWorlds.Library.MathF.Min(town2.Prosperity, 5000f) * 0.0002f;
                dictionary2 = dictionary;
                culture = town2.Culture;
                dictionary2[culture] += num8;
            }
            dictionary2 = dictionary;
            culture = settlement.Culture;
            dictionary2[culture] += 10f;
            dictionary2 = dictionary;
            culture = settlement.MapFaction.Culture;
            dictionary2[culture] += num2 * 5f;
            List<SettlementMusicData> list = (from x in MBObjectManager.Instance.GetObjectTypeList<SettlementMusicData>()
                                              where listOfLocationTags.Contains(x.LocationId)
                                              select x).ToList<SettlementMusicData>();

            KeyValuePair<CultureObject, float> maxWeightedCulture = TaleWorlds.Core.Extensions.MaxBy((IEnumerable<KeyValuePair<CultureObject, float>>)dictionary, (KeyValuePair<CultureObject, float> x) => x.Value);

            float num9 = (float)list.Count((SettlementMusicData x) => x.Culture == maxWeightedCulture.Key) / maxWeightedCulture.Value;
            List<SettlementMusicData> list2 = new List<SettlementMusicData>();
            foreach (KeyValuePair<CultureObject, float> keyValuePair in dictionary)
            {
                int num10 = MBRandom.RoundRandomized(num9 * keyValuePair.Value);
                if (num10 > 0)
                {
                    PopulatePlayList(list2, list, keyValuePair.Key, num10);
                }
            }
            if (list2.IsEmpty<SettlementMusicData>())
            {
                list2 = list;
            }
            list2.Shuffle<SettlementMusicData>();

            return list2;
        }

        // Token: 0x06000846 RID: 2118 RVA: 0x0003FCCC File Offset: 0x0003FCCC
        static void PopulatePlayList(List<SettlementMusicData> playList, List<SettlementMusicData> settlementMusicDatas, CultureObject culture, int count)
        {
            List<SettlementMusicData> list = (from x in settlementMusicDatas
                                              where x.Culture == culture
                                              select x).ToList<SettlementMusicData>();
            list.Shuffle<SettlementMusicData>();
            int num = 0;
            while (num < count && num < list.Count)
            {
                playList.Add(list[num]);
                num++;
            }
        }
    }
}
