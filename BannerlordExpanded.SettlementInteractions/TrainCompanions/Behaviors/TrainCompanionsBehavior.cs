using BannerlordExpanded.SettlementInteractions.Inns.Settings;
using BannerlordExpanded.SettlementInteractions.TrainCompanions.MissionControllers;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerlordExpanded.SettlementInteractions.TrainCompanions.Behaviors
{
    class TrainCompanionsBehavior : CampaignBehaviorBase
    {
        float _trainingDurationLeft = 0f;
        Hero _companionInTraining = null;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, AddMenus);
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
            dataStore.SyncData("BE_SettlementInteractions_TrainCompanions_TrainingDurationLeft", ref _trainingDurationLeft);

            if (dataStore.IsSaving)
            {
                dataStore.SyncData("BE_SettlementInteractions_TrainCompanions_CompanionInTraining", ref _companionInTraining);

            }
            else
            {
                // Data type check before loading
                object companion = null;
                dataStore.SyncData("BE_SettlementInteractions_TrainCompanions_CompanionInTraining", ref companion);
                if (companion as Hero != null)
                    _companionInTraining = companion as Hero;
            }



        }


        void AddMenus(CampaignGameStarter gameStarter)
        {
            gameStarter.AddGameMenuOption("castle", "besi_castle_traincompanions", "{=BESI_TrainCompanions}Train Clan Members", null, (MenuCallbackArgs args) => { args.optionLeaveType = GameMenuOption.LeaveType.Submenu; GameMenu.SwitchToMenu("besi_castle_companiontraining"); });

            TextObject menuText = new TextObject("{=BESI_TrainCompanions_CastleMenuText}The castle is a good area to train up your companions. It will take {NUM_TRAINING_HOURS} hours.");
            menuText.SetTextVariable("NUM_TRAINING_HOURS", MCMSettings.Instance.CompanionTrainingHours);
            gameStarter.AddGameMenu("besi_castle_companiontraining", menuText.ToString(), companiontraining_menu_on_init);
            gameStarter.AddGameMenuOption("besi_castle_companiontraining", "besi_castle_companiontraining_start", "{=BESI_TrainCompanions_StartTraining}Start Training", companiontraining_starttraining_condition, companiontraining_starttraining_consequence);
            gameStarter.AddGameMenuOption("besi_castle_companiontraining", "besi_castle_companiontraining_back", "{=BESI_TrainCompanions_Back}Back", back_on_condition, (MenuCallbackArgs args) => { GameMenu.SwitchToMenu("castle"); }, true);


            gameStarter.AddWaitGameMenu("besi_castle_companiontraining_inprogress", "{=BESI_TrainCompanions_Midway}You are training your companion...",
                                        (MenuCallbackArgs args) => { args.MenuContext.GameMenu.EndWait(); },
                                        (MenuCallbackArgs args) => { return true; },
                                        (MenuCallbackArgs args) => { args.MenuContext.GameMenu.StartWait(); OnStopTraining(); },
                                        OnTrainingTick,
                                        type: GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption,
                                        overlay: TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                                        targetWaitHours: MCMSettings.Instance.CompanionTrainingHours
                                        );
            gameStarter.AddGameMenuOption("besi_castle_companiontraining_inprogress", "besi_castle_companiontraining_inprogress_back", "{=BESI_TrainCompanions_Back}Back", back_on_condition, (MenuCallbackArgs args) => { GameMenu.SwitchToMenu("besi_castle_companiontraining"); }, true);

            gameStarter.AddGameMenu("besi_castle_companiontraining_ended",
                "{=BESI_TrainCompanions_EndedMenuText}You put {TRAINEE.NAME} through the basics of soldiering, discipline and obedience. But it's not over yet, {?TRAINEE.GENDER}she{?}he{\\?} seems enthusiastic to show-off what he learnt from you and is asking for a friendly fully-armed duel. However, having such a duel on castle grounds will cause too much trouble!",
                (args) =>
                {
                    if (_companionInTraining == null)
                    {
                        GameMenu.SwitchToMenu("besi_castle_companiontraining");
                        return;
                    }
                    StringHelpers.SetCharacterProperties("TRAINEE", _companionInTraining.CharacterObject, null, false);
                });
            gameStarter.AddGameMenuOption("besi_castle_companiontraining_ended", "besi_castle_companiontraining_ended_duelstart", "{=BESI_TrainCompanions_EndedMenuStartDuel}Head outside the castle to start the duel.", (args) => { args.optionLeaveType = GameMenuOption.LeaveType.PracticeFight; return true; }, (args) => { StartDuel(); });
            gameStarter.AddGameMenuOption("besi_castle_companiontraining_ended", "besi_castle_companiontraining_ended_skipduel", "{=BESI_TrainCompanions_EndedMenuSkipDuel}Reject the idea of the duel.", (args) => { args.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; }, (args) => { GameMenu.SwitchToMenu("besi_castle_companiontraining"); });


            gameStarter.AddGameMenu("besi_castle_companiontraining_afterduel", "{BESI_TRAINCOMPANIONS_AFTERDUEL}", (args) => { StringHelpers.SetCharacterProperties("TRAINEE", _companionInTraining.CharacterObject, null, false); });
            gameStarter.AddGameMenuOption("besi_castle_companiontraining_afterduel", "besi_castle_companiontraining_afterduel_leave", "{=BESI_TrainCompanions_AfterDuel_Leave}Leave", back_on_condition, (MenuCallbackArgs args) => { GameMenu.SwitchToMenu("besi_castle_companiontraining"); }, true);
        }


        public void StartTraining(Hero companion)
        {
            _trainingDurationLeft = MCMSettings.Instance.CompanionTrainingHours;
            _companionInTraining = companion;
            //InformationManager.DisplayMessage(new InformationMessage("Training has started"));
            GameMenu.SwitchToMenu("besi_castle_companiontraining_inprogress");
        }

        public void OnTrainingTick(MenuCallbackArgs args, CampaignTime dt)
        {
            _trainingDurationLeft -= (float)dt.ToHours;
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu((MCMSettings.Instance.CompanionTrainingHours - _trainingDurationLeft) / MCMSettings.Instance.CompanionTrainingHours);
            //InformationManager.DisplayMessage(new InformationMessage(dt.ToHours.ToString()));
            //_trainingDurationLeft -= (float)dt.ToHours;
        }

        public void OnStopTraining()
        {
            MBFastRandom random = new MBFastRandom();
            MBReadOnlyList<SkillObject> skills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
            StringHelpers.SetCharacterProperties("TRAINEE", _companionInTraining.CharacterObject, null, false);
            for (int i = 0; i < MCMSettings.Instance.CompanionTrainingNumOfRandomSkillsToLearn; ++i)
            {
                SkillObject selectedSkill = skills[random.Next(0, skills.Count)];
                int xpGain = MCMSettings.Instance.CompanionTrainingXpGain * (Hero.MainHero.GetSkillValue(selectedSkill) + 1);
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BESI_TrainCompanions_XPResults}[Bannerlord Expanded]: {TRAINEE.NAME} has gained {AMOUNT_XP} XP in {SKILL_NAME}.",
                                                                                        new Dictionary<string, object> { { "AMOUNT_XP", xpGain.ToString() }, { "SKILL_NAME", selectedSkill.ToString() } }).ToString()));
                _companionInTraining.AddSkillXp(selectedSkill, xpGain);
            }


            GameMenu.SwitchToMenu("besi_castle_companiontraining_ended");

        }




        #region START_TRAINING
        static void companiontraining_menu_on_init(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=BESI_TrainCompanions_CastleMenuTitle}Castle Training Grounds", null);


        }

        static bool companiontraining_starttraining_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.PracticeFight;

            if (Hero.MainHero.CompanionsInParty.Count() == 0)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=BESI_TrainCompanions_StartTraining_Rejected_NoCompanions}You have no companions to train!");
                return true;
            }
            SettlementAccessModel.AccessDetails accessDetails;
            Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterLordsHall(Settlement.CurrentSettlement, out accessDetails);
            if (accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.LimitedAccess && accessDetails.LimitedAccessSolution == SettlementAccessModel.LimitedAccessSolution.Bribe)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=BESI_TrainCompanions_StartTraining_Rejected_NoRenown}Your renown is not high enough! The castle owner does not allow you to train at their castle!");
                return true;
            }

            return true;
        }

        static void companiontraining_starttraining_consequence(MenuCallbackArgs args)
        {
            List<InquiryElement> selectableCompanions = new List<InquiryElement>();
            foreach (Hero hero in Campaign.Current.AliveHeroes)
            {
                if (hero.Clan == Hero.MainHero.Clan && hero.PartyBelongedTo != null && hero.PartyBelongedTo == MobileParty.MainParty && hero != Hero.MainHero)
                    selectableCompanions.Add(new InquiryElement(hero.Id, hero.Name.ToString(), new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject))));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=BESI_TrainCompanions_StartTraining_ChooseCompanionTitle}Your Companions").ToString()
                                                                                        , new TextObject("{=BESI_TrainCompanions_StartTraining_ChooseCompanionDesc}Choose a companion to train.").ToString()
                                                                                        , selectableCompanions
                                                                                        , true, 1, 1
                                                                                        , new TextObject("{=BESI_TrainCompanions_StartTraining_ChooseCompanionConfirm}Confirm").ToString()
                                                                                        , new TextObject("{=BESI_TrainCompanions_StartTraining_ChooseCompanionCancel}Cancel").ToString()
                                                                                        , (List<InquiryElement> elements) => { Hero companion = Hero.FindFirst(H => H.Id == (MBGUID)elements[0].Identifier); Campaign.Current.GetCampaignBehavior<TrainCompanionsBehavior>().StartTraining(companion); }
                                                                                        , (List<InquiryElement> elements) => { GameMenu.SwitchToMenu("castle"); }));
        }

        #endregion


        #region START_DUEl
        private void StartDuel()
        {
            string scene;
            var loc = PlayerEncounter.LocationEncounter;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            //scene = currentSettlement.LocationComplex.GetLocationWithId("center").GetSceneName(currentSettlement.IsCastle ? currentSettlement.Town.GetWallLevel() : 1);
            scene = PlayerEncounter.GetBattleSceneForMapPatch(Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D));
            CompanionDuelMission.OpenDuelMission(scene, _companionInTraining.CharacterObject, false);
        }

        public void OnDuelEnd(bool playerVictory)
        {

            if (playerVictory)
                MBTextManager.SetTextVariable("BESI_TRAINCOMPANIONS_AFTERDUEL", "{=BESI_TrainCompanions_AfterDuelVictory}{TRAINEE.NAME} has lost and accepted his defeat. However you are confident that {?TRAINEE.GENDER}she{?}he{\\?} has improved tremendously from before.");
            else
                MBTextManager.SetTextVariable("BESI_TRAINCOMPANIONS_AFTERDUEL", "{=BESI_TrainCompanions_AfterDuelLost}You have lost and accepted your defeat. It seems that {TRAINEE.NAME} has grown stronger and surpassed you.");
            GameMenu.SwitchToMenu("besi_castle_companiontraining_afterduel");
        }

        #endregion

        static bool back_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }
    }
}
