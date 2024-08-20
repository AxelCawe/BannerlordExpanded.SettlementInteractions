using BannerlordExpanded.SettlementInteractions.Inns.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using FaceGen = TaleWorlds.Core.FaceGen;

namespace BannerlordExpanded.SettlementInteractions.Inns.Behaviors
{
    internal class InnBehavior : CampaignBehaviorBase
    {
        public Action<Village> OnInnEntered;
        public Action<Village> OnInnLeft;

        private Location _inn = null;



        public bool IsInInn = false;

        public bool _isInnInitialized = false;

        public override void RegisterEvents()
        {
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.AddGameMenus));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEnter));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(OnSettlementLeft));
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnAfterSettlementEnter));
        }

        public override void SyncData(IDataStore dataStore)
        {
            //dataStore.SyncData("BESI_Inn_IsInInn", ref IsInInn);
            //throw new NotImplementedException();
        }



        void OnAfterSettlementEnter(MobileParty party, Settlement settlement, Hero hero)
        {

        }

        void OnSettlementEnter(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty)
                return;
            if (settlement.IsVillage)
            {
                //Location inn = settlement.LocationComplex.GetLocationWithId("village_inn");
                SetupInn();

            }
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {

            CleanUpInn();
        }

        [GameMenuInitializationHandler("village_inn")]
        public static void InnMenuSoundOnInit(MenuCallbackArgs args)
        {
            args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/tavern");
        }


        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "village_inn", "{=BESI_Inn_GameMenu_GoToInn}Go to the village inn", new GameMenuOption.OnConditionDelegate(CanGoInnOnCondition), delegate
            {
                OnInnEntered.Invoke(Hero.MainHero.CurrentSettlement.Village);
                GameMenu.SwitchToMenu("village_inn");
                IsInInn = true;
                //GlobalSettings<MCMSettings>.Instance.IsInInn = true;
            }, false, 1, false);
            campaignGameStarter.AddGameMenu("village_inn", "{=BESI_Inn_GameMenu_InInn}You are in the village inn", new OnInitDelegate(VillageInnOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters, GameMenu.MenuFlags.None, null);
            campaignGameStarter.AddGameMenuOption("village_inn", "village_inn_visit", "{=BESI_Inn_GameMenu_VisitTheInn}Visit the inn", new GameMenuOption.OnConditionDelegate(VisitInnOnCondition), new GameMenuOption.OnConsequenceDelegate(VisitInnOnConsequence), false, 0, false);
            campaignGameStarter.AddGameMenuOption("village_inn", "village_inn_back", "{=BESI_Inn_GameMenu_Back}Back to village", new GameMenuOption.OnConditionDelegate(BackOnCondition), delegate
            {
                OnInnLeft.Invoke(Hero.MainHero.CurrentSettlement.Village);
                GameMenu.SwitchToMenu("village");
                //GlobalSettings<MCMSettings>.Instance.IsInInn = false;
                IsInInn = false;
            }, true, 5, false);


        }

        #region INN_LOCATION_FUNCS
        public static bool CanGoInnOnCondition(MenuCallbackArgs args)
        {
            SettlementAccessModel settlementAccessModel = Campaign.Current.Models.SettlementAccessModel;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            SettlementAccessModel.AccessDetails accessDetails;
            settlementAccessModel.CanMainHeroEnterSettlement(currentSettlement, out accessDetails);
            if (accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.NoAccess && accessDetails.AccessLimitationReason == SettlementAccessModel.AccessLimitationReason.VillageIsLooted)
            {
                return false;
            }
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            return true;
        }

        private static void VillageInnOnInit(MenuCallbackArgs args)
        {
            //Campaign.Current.GameMenuManager.MenuLocations.Clear();
            //Campaign.Current.GameMenuManager.MenuLocations.Add(LocationComplex.Current.GetLocationWithId("village_inn"));
            Settlement settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty.CurrentSettlement;
            string backgroundMeshName = Settlement.CurrentSettlement.Culture.StringId + "_tavern";
            args.MenuContext.SetBackgroundMeshName(backgroundMeshName);
            //_inn.SetOwnerComplex(settlement.LocationComplex);
            string scenes;
            switch (settlement.Culture.GetCultureCode())
            {
                case CultureCode.Empire:
                    scenes = "empire_house_c_tavern_a";
                    break;
                case CultureCode.Sturgia:
                    scenes = "sturgia_house_b_interior_tavern";
                    break;
                case CultureCode.Aserai:
                    scenes = "arabian_house_new_c_interior_c_tavern";
                    break;
                case CultureCode.Vlandia:
                    scenes = "vlandia_tavern_interior_a";
                    break;
                case CultureCode.Khuzait:
                    scenes = "khuzait_house_g_interior_a_tavern";
                    break;
                case CultureCode.Battania:
                    scenes = "battania_tavern_interior_b";
                    break;
                default:
                    scenes = "empire_house_c_tavern_a";
                    break;
            }
            //Campaign.Current.GetCampaignBehavior<InnBehavior>().SetScenes(scenes);

            args.MenuTitle = new TextObject("{=BESI_Inn_GameMenu_Inn}Inn", null);
        }

        private static bool VisitInnOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Mission;
            return true;
        }

        private void VisitInnOnConsequence(MenuCallbackArgs args)
        {
            if (_inn == null && Campaign.Current.CurrentMenuContext.GameMenu.StringId == "village_inn")
                SetupInn();
            OpenMissionWithSettingPreviousLocation("village_center", "village_inn");
        }

        private static void OpenMissionWithSettingPreviousLocation(string previousLocationId, string missionLocationId)
        {
            Campaign.Current.GameMenuManager.NextLocation = LocationComplex.Current.GetLocationWithId(missionLocationId);
            Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId(previousLocationId);
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, null, null, null);
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;
        }

        private static bool BackOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private void SetScenes(string SceneName)
        {
            for (int i = 0; i < 4; i++)
            {
                _inn.SetSceneName(i, SceneName);
            }
        }

        private void SetupInn()
        {
            if (_inn == null)
            {
                _inn = new Location("village_inn", new TextObject("{=BESI_Inn_GameMenu_Inn}Inn", null), new TextObject("{=BESI_Inn_GameMenu_Inn}Inn", null), 30, true, false, "CanAlways", "CanAlways", "CanAlways", "CanAlways", new string[]
                    {
                "empire_house_c_tavern_a",
                "empire_house_c_tavern_a",
                "empire_house_c_tavern_a",
                "empire_house_c_tavern_a"
                }, null);
            }

            Settlement settlement = Settlement.CurrentSettlement;
            FieldInfo field = LocationComplex.Current.GetType().GetField("_locations", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<string, Location> dictionary = (Dictionary<string, Location>)field.GetValue(LocationComplex.Current);
            if (dictionary.ContainsKey("village_inn"))
            {
                dictionary.Remove("village_inn");
            }
            //_inn.RemoveAllCharacters();
            _inn.SetOwnerComplex(settlement.LocationComplex);
            switch (settlement.Culture.GetCultureCode())
            {
                case CultureCode.Empire:
                    _inn.SetSceneName(0, "empire_house_c_tavern_a");
                    goto IL_14A;
                case CultureCode.Sturgia:
                    _inn.SetSceneName(0, "sturgia_house_a_interior_tavern");
                    goto IL_14A;
                case CultureCode.Aserai:
                    _inn.SetSceneName(0, "arabian_house_new_c_interior_b_tavern");
                    goto IL_14A;
                case CultureCode.Vlandia:
                    _inn.SetSceneName(0, "vlandia_tavern_interior_a");
                    goto IL_14A;
                case CultureCode.Khuzait:
                    _inn.SetSceneName(0, "khuzait_tavern_a");
                    goto IL_14A;
                case CultureCode.Battania:
                    _inn.SetSceneName(0, "battania_tavern_interior_b");
                    goto IL_14A;
            }
            _inn.SetSceneName(0, "empire_house_c_tavern_a");
        IL_14A:

            var allWanderers = Enumerable.Where<Hero>(settlement.HeroesWithoutParty, (Hero x) => x.IsWanderer && x.CompanionOf == null).ToList();
            foreach (Hero wanderer in allWanderers)
            {
                InnHelper.AddWandererLocationCharacter(wanderer, settlement, _inn);
                //settlement.LocationComplex.ChangeLocation(settlement.LocationComplex.GetFirstLocationCharacterOfCharacter(wanderer.CharacterObject), settlement.LocationComplex.GetLocationWithId("village_center"), _inn);
            }

            dictionary.Add("village_inn", _inn);
            if (field != null)
            {
                field.SetValue(LocationComplex.Current, dictionary);
            }

        }

        private void CleanUpInn()
        {
            if (_isInnInitialized)
            {
                _inn.RemoveAllCharacters();
                _inn.SetOwnerComplex(null);
                _isInnInitialized = false;
            }
        }

        public void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            if (CampaignMission.Current.Location.StringId != "village_inn" || _isInnInitialized)
            {
                return;
            }
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            this.AddPeopleToTownTavern(settlement, unusedUsablePointCount);
            _isInnInitialized = true;
        }

        private void AddPeopleToTownTavern(Settlement settlement, Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = LocationComplex.Current.GetLocationWithId("village_inn");



            List<Hero> allNotables = settlement.Notables.ToList();
            foreach (Hero notable in allNotables)
            {
                InnHelper.AddNotableLocationCharacter(notable, settlement, _inn);
            }

            var allWanderers = Enumerable.Where<Hero>(settlement.HeroesWithoutParty, (Hero x) => x.IsWanderer && x.CompanionOf == null).ToList();
            foreach (Hero wanderer in allWanderers)
            {
                InnHelper.AddWandererLocationCharacter(wanderer, settlement, _inn);
            }

            int num;
            if (unusedUsablePointCount.TryGetValue("spawnpoint_tavernkeeper", out num) && num > 0)
            {
                locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernkeeper), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            }
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
                LocationComplex.Current.GetLocationWithId("village_inn").AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDancer), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num2);
            }
            unusedUsablePointCount.TryGetValue("npc_common", out num);
            if (num > 0)
            {
                int num8 = (int)((float)num * 0.3f);
                if (num8 > 0)
                {
                    locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsManForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num8);
                }
                int num9 = (int)((float)num * 0.3f);
                if (num9 > 0)
                {
                    locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsWomanForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num9);
                }
                int num10 = (int)((float)num * 0.3f);
                if (num10 > 0)
                {
                    locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleChildForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, TaleWorlds.Library.MathF.Ceiling(num10 / 4f));
                    locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleTeenagerForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, TaleWorlds.Library.MathF.Ceiling(num10 / 4f));
                    locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleChildForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, TaleWorlds.Library.MathF.Ceiling(num10 / 4f));
                    locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleTeenagerForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, TaleWorlds.Library.MathF.Ceiling(num10 / 4f));
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
            TextObject value2 = new TextObject("{=BESI_Inn_OwnerName}Innkeeper", null);
            FieldInfo field2 = tavernkeeper.GetType().GetField("_basicName", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field2 != null)
            {
                field2.SetValue(tavernkeeper, value2);
            }
            tavernkeeper.StringId = "inn_keeper";
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
            TextObject value2 = new TextObject("{=BESI_Inn_HelperName}Inn Helper", null);
            FieldInfo field2 = tavernWench.GetType().GetField("_basicName", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field2 != null)
            {
                field2.SetValue(tavernWench, value2);
            }
            tavernWench.StringId = "inn_helper";
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

        private static LocationCharacter CreateTownsManForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject townsman = culture.Villager;
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(townsman.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, townsman.IsFemale, "_villager_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, townsman.IsFemale, "_villager_in_tavern");
            }
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, out minValue, out maxValue, "TavernVisitor");
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townsman, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(18, maxValue)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }

        private static LocationCharacter CreateTownsWomanForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject townswoman = culture.VillageWoman;
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(townswoman.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, townswoman.IsFemale, "_warrior_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, townswoman.IsFemale, "_warrior_in_tavern");
            }
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswoman, out minValue, out maxValue, "TavernVisitor");
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(18, maxValue)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }

        public static LocationCharacter CreateMaleChildForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {

            CharacterObject characterObject = culture.VillagerMaleChild;


            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(characterObject.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_tavern");
            }
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(characterObject, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }

        public static LocationCharacter CreateFemaleChildForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {

            CharacterObject characterObject = culture.VillagerFemaleChild;


            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(characterObject.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_tavern");
            }
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(characterObject, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }

        public static LocationCharacter CreateMaleTeenagerForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {

            CharacterObject characterObject = culture.VillagerMaleTeenager;


            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(characterObject.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_tavern");
            }
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(characterObject, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }
        public static LocationCharacter CreateFemaleTeenagerForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {

            CharacterObject characterObject = culture.VillagerFemaleTeenager;


            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(characterObject.Race, "_settlement_slow");
            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_aserai_tavern");
            }
            else
            {
                actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, characterObject.IsFemale, "_villager_in_tavern");
            }
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(characterObject, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }

        #endregion
    }
}
