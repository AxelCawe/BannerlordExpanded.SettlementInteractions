using BannerlordExpanded.SettlementInteractions.Inns.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace BannerlordExpanded.SettlementInteractions.Inns.Behaviors
{
    internal class InnWanderersBehavior : CampaignBehaviorBase
    {
        List<CharacterObject> _allCompanionTemplates = null;

        Dictionary<Village, List<Hero>> _existingVillageWanderers = new Dictionary<Village, List<Hero>>();

        Random _random = new Random();

        public override void RegisterEvents()
        {
            //CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreatedEvent);
            Campaign.Current.GetCampaignBehavior<InnBehavior>().OnInnEntered += OnInnEntered;
            Campaign.Current.GetCampaignBehavior<InnBehavior>().OnInnLeft += OnInnLeft;
            //CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnAfterSettlementEntered);
            //CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            //CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameLoaded);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
            CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);

            MCMSettings.Instance.RefreshWanderers += RefreshWanderers;
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsVillage)
            {
                DeactivateInnWanderersAtVillage(Settlement.CurrentSettlement.Village);
                dataStore.SyncData("BE_SettlementInteractions_InnWanderers", ref _existingVillageWanderers);
                ActivateInnWanderersAtVillage(Settlement.CurrentSettlement.Village);
            }
            else
            {
                dataStore.SyncData("BE_SettlementInteractions_InnWanderers", ref _existingVillageWanderers);
            }


        }

        void OnNewGameCreatedEvent(CampaignGameStarter gameStarter)
        {
            InitializeCompanionTemplates();
            RefreshWanderers();
        }


        void OnGameLoaded(CampaignGameStarter gameStarter)
        {
            InitializeCompanionTemplates();


        }

        void WeeklyTick()
        {
            InitializeCompanionTemplates();
            if (MCMSettings.Instance.AutoRefreshEveryWeek)
                RefreshWanderers();
        }

        //void OnAfterSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        //{
        //    if ((mobileParty != null && mobileParty.IsMainParty) && settlement.IsVillage)
        //    {
        //        ActivateInnWanderersAtVillage(settlement.Village);
        //    }
        //}

        //void OnSettlementLeft(MobileParty mobileParty, Settlement settlement)
        //{
        //    if (mobileParty.IsMainParty && settlement.IsVillage)
        //    {
        //        DeactivateInnWanderersAtVillage(settlement.Village);
        //    }
        //}



        void InitializeCompanionTemplates()
        {
            _allCompanionTemplates = new MBList<CharacterObject>();
            foreach (CharacterObject characterObject in MBObjectManager.Instance.GetObjectTypeList<CharacterObject>())
            {
                if (characterObject.IsTemplate && characterObject.Occupation == Occupation.Wanderer)
                {
                    _allCompanionTemplates.Add(characterObject);
                }
            }
        }


        void RefreshWanderers()
        {
            //InformationManager.DisplayMessage(new InformationMessage("Refreshing..."));
            foreach (Village village in Village.All)
            {
                List<Hero> wanderersAtSettlement;
                if (_existingVillageWanderers.TryGetValue(village, out wanderersAtSettlement))
                {
                    foreach (Hero wanderer in wanderersAtSettlement)
                    {
                        DeleteWanderer(wanderer);
                    }

                    wanderersAtSettlement.Clear();
                    _existingVillageWanderers.Remove(village);
                }
                wanderersAtSettlement = new List<Hero>();
                _existingVillageWanderers.Add(village, wanderersAtSettlement);

                for (int i = 0; i < MCMSettings.Instance.MaxCompanionsPerTown; ++i)
                    CreateHeroForVillage(village);

            }
        }

        void OnInnEntered(Village village)
        {
            ActivateInnWanderersAtVillage(village);
        }

        void OnInnLeft(Village village)
        {
            DeactivateInnWanderersAtVillage(village);
        }

        void ActivateInnWanderersAtVillage(Village village)
        {
            //InformationManager.DisplayMessage(new InformationMessage("Activating..."));
            List<Hero> wanderersAtSettlement;
            if (_existingVillageWanderers.TryGetValue(village, out wanderersAtSettlement))
            {
                foreach (Hero wanderer in wanderersAtSettlement)
                {
                    //InformationManager.DisplayMessage(new InformationMessage("Adding..."));
                    wanderer.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(wanderer, village.Settlement);

                    //LocationCharacter locCharacter =village.Settlement.LocationComplex.GetLocationCharacterOfHero(wanderer);
                    //locCharacter.IsHidden = false;
                }
            }
        }

        void DeactivateInnWanderersAtVillage(Village village)
        {
            //InformationManager.DisplayMessage(new InformationMessage("De-Activating..."));
            List<Hero> wanderersAtSettlement;

            if (_existingVillageWanderers.TryGetValue(village, out wanderersAtSettlement))
            {
                List<Hero> hiredWanderers = new List<Hero>();
                foreach (Hero wanderer in wanderersAtSettlement)
                {
                    if (!wanderer.IsPlayerCompanion && wanderer.CurrentSettlement != null)
                    {
                        //LocationCharacter locCharacter = village.Settlement.LocationComplex.GetLocationCharacterOfHero(wanderer);
                        //locCharacter.IsHidden = false;
                        LeaveSettlementAction.ApplyForCharacterOnly(wanderer);
                        wanderer.ChangeState(Hero.CharacterStates.NotSpawned);

                    }
                    else
                        hiredWanderers.Add(wanderer);

                }

                foreach (Hero hiredWanderer in hiredWanderers)
                    wanderersAtSettlement.Remove(hiredWanderer);


            }
        }

        private void CreateHeroForVillage(Village village)
        {
            CharacterObject companionTemplate = _allCompanionTemplates[_random.Next(0, _allCompanionTemplates.Count)];

            Hero hero = HeroCreator.CreateSpecialHero(companionTemplate, village.Settlement, null, null, Campaign.Current.Models.AgeModel.HeroComesOfAge + MBRandom.RandomInt(27));
            this.AdjustEquipmentImp(hero.BattleEquipment);
            this.AdjustEquipmentImp(hero.CivilianEquipment);
            hero.SetNewOccupation(Occupation.Wanderer);
            hero.ChangeState(Hero.CharacterStates.NotSpawned);
            AddHeroToWandererList(village, hero);
        }
        private void AdjustEquipmentImp(Equipment equipment)
        {
            ItemModifier @object = MBObjectManager.Instance.GetObject<ItemModifier>("companion_armor");
            ItemModifier object2 = MBObjectManager.Instance.GetObject<ItemModifier>("companion_weapon");
            ItemModifier object3 = MBObjectManager.Instance.GetObject<ItemModifier>("companion_horse");
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumEquipmentSetSlots; equipmentIndex++)
            {
                EquipmentElement equipmentElement = equipment[equipmentIndex];
                if (equipmentElement.Item != null)
                {
                    if (equipmentElement.Item.ArmorComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, @object, null, false);
                    }
                    else if (equipmentElement.Item.HorseComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object3, null, false);
                    }
                    else if (equipmentElement.Item.WeaponComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object2, null, false);
                    }
                }
            }
        }

        private void AddHeroToWandererList(Village village, Hero hero)
        {
            List<Hero> wanderersAtSettlement;
            if (_existingVillageWanderers.TryGetValue(village, out wanderersAtSettlement))
            {
                wanderersAtSettlement.Add(hero);
            }
            else
            {
                _existingVillageWanderers.Add(village, new List<Hero> { hero });
            }
        }

        private void RemoveHeroFromWandererList(Village village, Hero hero)
        {
            List<Hero> wanderersAtSettlement;
            if (_existingVillageWanderers.TryGetValue(village, out wanderersAtSettlement))
            {
                if (wanderersAtSettlement.Contains(hero))
                    wanderersAtSettlement.Remove(hero);
            }

        }

        void DeleteWanderer(Hero hero)
        {
            hero.AddDeathMark(null, KillCharacterAction.KillCharacterActionDetail.Lost);
            hero.ChangeState(Hero.CharacterStates.Dead);
            CompanionsCampaignBehavior behavior = Campaign.Current.GetCampaignBehavior<CompanionsCampaignBehavior>();
            typeof(CompanionsCampaignBehavior).GetMethod("RemoveFromAliveCompanions", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(behavior, new object[] { hero });
            typeof(CampaignObjectManager).GetMethod("UnregisterDeadHero", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Campaign.Current.CampaignObjectManager, new object[] { hero });
        }

    }
}
