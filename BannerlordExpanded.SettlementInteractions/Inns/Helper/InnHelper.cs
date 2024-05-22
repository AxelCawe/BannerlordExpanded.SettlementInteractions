using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace BannerlordExpanded.SettlementInteractions.Inns.Helper
{
    public static class InnHelper
    {
        public static void AddWandererLocationCharacter(Hero wanderer, Settlement settlement, Location location)
        {
            bool flag = settlement.Culture.StringId.ToLower() == "aserai" || settlement.Culture.StringId.ToLower() == "khuzait";
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(wanderer.CharacterObject.Race, "_settlement");
            string actionSetCode = flag ? ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, wanderer.IsFemale, "_warrior_in_aserai_tavern") : ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, wanderer.IsFemale, "_warrior_in_tavern");
            LocationCharacter locationCharacter = new LocationCharacter(new AgentData(new PartyAgentOrigin(null, wanderer.CharacterObject, -1, default(UniqueTroopDescriptor), false)).Monster(monsterWithSuffix).NoHorses(true).Age((int)wanderer.Age), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, LocationCharacter.CharacterRelations.Neutral, actionSetCode, true, false, null, false, false, true);
            location.AddCharacter(locationCharacter);
        }

        public static void AddNotableLocationCharacter(Hero notable, Settlement settlement, Location location)
        {
            string suffix = notable.IsArtisan ? "_villager_artisan" : (notable.IsMerchant ? "_villager_merchant" : (notable.IsPreacher ? "_villager_preacher" : (notable.IsGangLeader ? "_villager_gangleader" : (notable.IsRuralNotable ? "_villager_ruralnotable" : (notable.IsFemale ? "_lord" : "_villager_merchant")))));
            string text = notable.IsArtisan ? "sp_notable_artisan" : (notable.IsMerchant ? "sp_notable_merchant" : (notable.IsPreacher ? "sp_notable_preacher" : (notable.IsGangLeader ? "sp_notable_gangleader" : (notable.IsRuralNotable ? "sp_notable_rural_notable" : ((notable.GovernorOf == notable.CurrentSettlement.Town) ? "sp_governor" : "sp_notable")))));
            IReadOnlyList<Workshop> ownedWorkshops = notable.OwnedWorkshops;
            if (Enumerable.Count<Workshop>(ownedWorkshops) != 0)
            {
                for (int i = 0; i < Enumerable.Count<Workshop>(ownedWorkshops); i++)
                {
                    if (!ownedWorkshops[i].WorkshopType.IsHidden)
                    {
                        text = text + "_" + ownedWorkshops[i].Tag;
                        break;
                    }
                }
            }
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(notable.CharacterObject.Race, "_settlement");
            AgentData agentData = new AgentData(new PartyAgentOrigin(null, notable.CharacterObject, -1, default(UniqueTroopDescriptor), false)).Monster(monsterWithSuffix).NoHorses(true);
            LocationCharacter locationCharacter = new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, LocationCharacter.CharacterRelations.Neutral, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, notable.IsFemale, suffix), true, false, null, false, false, true);

            location.AddCharacter(locationCharacter);
            return;
        }
    }
}
