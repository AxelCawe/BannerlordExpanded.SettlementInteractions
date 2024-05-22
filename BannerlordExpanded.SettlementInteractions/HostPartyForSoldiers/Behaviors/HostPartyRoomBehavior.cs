using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.TwoDimension;
using FaceGen = TaleWorlds.Core.FaceGen;

namespace BannerlordExpanded.SettlementInteractions.HostPartyForSoldiers.Behaviors
{
    internal class HostPartyRoomBehavior : CampaignBehaviorBase
    {
        List<string> _dialogues = new List<string>() {
                                                        "{=BESI_HostPartyForSoldiers_Dialog1}Thank you granting us this small period of joy, your highness.",
                                                        "{=BESI_HostPartyForSoldiers_Dialog2}Your highness, after all this fighting, I am grateful for this party that honors us.",
                                                        "{=BESI_HostPartyForSoldiers_Dialog3}Ayeeee, this feast is what we needed!",
                                                     };


        public static Location _partyRoom = new Location("castle_partyroom", new TextObject("{=BESI_HostPartyForSoldiers_RoomName}Party Room", null), new TextObject("{=BESI_HostPartyForSoldiers_RoomName}Party Room", null), 30, true, false, "CanAlways", "CanAlways", "CanAlways", "CanAlways", new string[]
        {
            "empire_house_c_tavern_a",
            "empire_house_c_tavern_a",
            "empire_house_c_tavern_a",
            "empire_house_c_tavern_a"
        }, null);



        public bool _isRoomInitialized = false;

        public override void RegisterEvents()
        {
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.AddDialog));

        }

        public override void SyncData(IDataStore dataStore)
        {
            //dataStore.SyncData("BESI_Inn_IsInInn", ref IsInInn);
            //throw new NotImplementedException();
        }

        void AddDialog(CampaignGameStarter gameStarter)
        {
            gameStarter.AddDialogLine("besi_hostpartyforsoldiers_dialog", "start", "close_window", "{BESI_RANDOM_DIALOG}", IsTroopInTavern, null, 100, null);
        }
        bool IsTroopInTavern()
        {
            if (Hero.MainHero.CurrentSettlement != null && CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("besi_partyroom_"))
            {
                MBTextManager.SetTextVariable("BESI_RANDOM_DIALOG", _dialogues.GetRandomElement());
                return true;
            }

            return false;
        }



        public void EnterPartyRoom()
        {
            CleanUpRoom();
            //Location inn = settlement.LocationComplex.GetLocationWithId("village_inn");

            FieldInfo field = LocationComplex.Current.GetType().GetField("_locations", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<string, Location> dictionary = (Dictionary<string, Location>)field.GetValue(LocationComplex.Current);
            if (dictionary.ContainsKey("castle_partyroom"))
            {
                dictionary.Remove("castle_partyroom");
            }
            //_inn.RemoveAllCharacters();
            _partyRoom.SetOwnerComplex(Settlement.CurrentSettlement.LocationComplex);
            _partyRoom.SetSceneName(0, "empire_house_c_tavern_a");

            dictionary.Add("castle_partyroom", _partyRoom);
            if (field != null)
            {
                field.SetValue(LocationComplex.Current, dictionary);
            }
            _isRoomInitialized = true;

            Campaign.Current.GameMenuManager.NextLocation = _partyRoom;
            Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("castle");
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(_partyRoom, null, null, null);
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;

        }

        private void CleanUpRoom()
        {
            if (_isRoomInitialized)
            {
                _partyRoom.RemoveAllCharacters();
                _partyRoom.SetOwnerComplex(null);
                _isRoomInitialized = false;
            }
        }

        public void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            if (CampaignMission.Current.Location.StringId != "castle_partyroom")
            {
                return;
            }
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            this.AddPeopleToPartyRoom(settlement, unusedUsablePointCount);
            _isRoomInitialized = true;
        }

        private void AddPeopleToPartyRoom(Settlement settlement, Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = _partyRoom;

            int num;
            if (unusedUsablePointCount.TryGetValue("sp_tavern_wench", out num) && num > 0)
            {
                locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernWench), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            }
            if (unusedUsablePointCount.TryGetValue("musician", out num) && num > 0)
            {
                locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMusician), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num);
            }

            int num2;
            unusedUsablePointCount.TryGetValue("npc_dancer", out num2);
            if (num2 > 0)
            {
                locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDancer), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num2);
            }

            unusedUsablePointCount.TryGetValue("npc_common", out num);
            if (num > 0)
            {

                using (IEnumerator<Hero> enumerator2 = Hero.MainHero.CompanionsInParty.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Hero companion = enumerator2.Current;
                        if (!companion.IsWounded && !PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer.Exists((AccompanyingCharacter x) => x.LocationCharacter.Character.HeroObject == companion))
                        {
                            --num;
                            IFaction mapFaction3 = companion.MapFaction;
                            uint color3 = (mapFaction3 != null) ? mapFaction3.Color : 4291609515U;
                            IFaction mapFaction4 = companion.MapFaction;
                            uint color4 = (mapFaction4 != null) ? mapFaction4.Color : 4291609515U;
                            Monster baseMonsterFromRace2 = FaceGen.GetBaseMonsterFromRace(companion.CharacterObject.Race);
                            AgentData agentData2 = new AgentData(new PartyAgentOrigin(PartyBase.MainParty, companion.CharacterObject, -1, default(UniqueTroopDescriptor), false)).Monster(baseMonsterFromRace2).NoHorses(true).ClothingColor1(color3).ClothingColor2(color4);
                            locationWithId.AddCharacter(new LocationCharacter(agentData2, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddCompanionBehaviors), "sp_notable", true, LocationCharacter.CharacterRelations.Friendly, null, !PlayerEncounter.LocationEncounter.Settlement.IsVillage, false, null, false, true, true));
                        }
                    }
                }

                List<TroopRosterElement> roster = MobileParty.MainParty.MemberRoster.GetTroopRoster();

                IEnumerable<TroopRosterElement> filteredRoster = roster.WhereQ((element) => element.Character.IsSoldier);
                int charactersPerTypeToSpawn = (int)Mathf.Ceil((float)num / (float)(filteredRoster.ToList().Count()));
                int charactersSpawned = 0;
                foreach (TroopRosterElement element in filteredRoster)
                {
                    for (int i = 0; i < charactersPerTypeToSpawn && charactersSpawned < num; ++i)
                    {
                        locationWithId.AddCharacter(CreateTroopForRoom(element.Character, settlement.Culture, LocationCharacter.CharacterRelations.Neutral));
                        ++charactersSpawned;
                    }
                    if (charactersSpawned >= num)
                        break;
                }
            }
        }

        private static LocationCharacter CreateDancer(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject femaleDancer = culture.FemaleDancer;
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(femaleDancer.Race, "_settlement");
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(femaleDancer, out minValue, out maxValue, "Dancer");
            AgentData agentData = new AgentData(new SimpleAgentOrigin(femaleDancer, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue));
            return new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_dancer", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, "_dancer"), true, false, null, false, false, true);
        }

        private static LocationCharacter CreateTavernkeeper(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject tavernkeeper = CharacterObject.CreateFrom(culture.Tavernkeeper);

            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(tavernkeeper.Race, "_settlement");
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(tavernkeeper, out minValue, out maxValue, "");
            AgentData agentData = new AgentData(new SimpleAgentOrigin(tavernkeeper, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue));
            return new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "spawnpoint_tavernkeeper", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, "_tavern_keeper"), true, false, null, false, false, true);
        }
        private static LocationCharacter CreateMusician(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject musician = culture.Musician;
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(musician.Race, "_settlement");
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(musician, out minValue, out maxValue, "");
            AgentData agentData = new AgentData(new SimpleAgentOrigin(musician, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue));
            return new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "musician", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, "_musician"), true, false, null, false, false, true);
        }

        private static LocationCharacter CreateTavernWench(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject tavernWench = CharacterObject.CreateFrom(culture.TavernWench);
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(tavernWench, out minValue, out maxValue, "");
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(tavernWench.Race, "_settlement");
            AgentData agentData = new AgentData(new SimpleAgentOrigin(tavernWench, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue));
            return new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "sp_tavern_wench", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, "_barmaid"), true, false, null, false, false, true)
            {
                PrefabNamesForBones =
                {
                    {
                        agentData.AgentMonster.OffHandItemBoneIndex,
                        "kitchen_pitcher_b_tavern"
                    }
                }
            };
        }

        private static LocationCharacter CreateTroopForRoom(CharacterObject oldTroop, CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject troop = CharacterObject.CreateFrom(oldTroop);
            troop.StringId = "besi_partyroom_" + oldTroop.StringId;
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(troop.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, troop.IsFemale, "_villager_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, troop.IsFemale, "_villager_in_tavern");
            }
            int minValue = (int)troop.GetBodyPropertiesMin().Age;
            int maxValue = (int)troop.GetBodyPropertiesMax().Age;
            if (minValue > maxValue)
            {
                int temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(troop, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }


    }
}
