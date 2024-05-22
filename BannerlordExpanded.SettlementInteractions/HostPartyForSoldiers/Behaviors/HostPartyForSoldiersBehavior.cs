
using BannerlordExpanded.SettlementInteractions.Inns.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerlordExpanded.SettlementInteractions.HostPartyForSoldiers.Behaviors
{
    public class HostPartyForSoldiersBehavior : CampaignBehaviorBase
    {
        float _preparationTimeSpent = 0f;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddMenus);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("BE_SettlementInteractions_HostPartyForSoldiers_PreparationTimeSpent", ref _preparationTimeSpent);
        }

        public void AddMenus(CampaignGameStarter gameStarter)
        {
            gameStarter.AddGameMenuOption("castle", "besi_castle_hostpartyforsoldiers", "{=BESI_HostPartyForSoldiers}Host party for soldiers", host_party_for_soldiers_condition, (MenuCallbackArgs args) => { GameMenu.SwitchToMenu("besi_castle_hostpartyforsoldiers"); });

            TextObject menuText = new TextObject("{=BESI_HostPartyForSoldiersMenu}You found a suitable room to host a party for your soldiers. It will take about {NUM_OF_HOURS} hours and {NUM_OF_DENARS} to prepare.");
            menuText.SetTextVariable("NUM_OF_HOURS", MCMSettings.Instance.HostPartyPreparationHours);
            menuText.SetTextVariable("NUM_OF_DENARS", MCMSettings.Instance.HostPartyCost);
            gameStarter.AddGameMenu("besi_castle_hostpartyforsoldiers", menuText.ToString(), host_party_for_soldiers_menu_init);

            gameStarter.AddGameMenuOption("besi_castle_hostpartyforsoldiers", "besi_castle_hostpartyforsoldiers_start", "{=BESI_HostPartyForSoldiers_StartPreparation}Start Preparations", host_party_for_soldiers_startpreparation_condition, host_party_for_soldiers_startpreparation_consequence);
            gameStarter.AddGameMenuOption("besi_castle_hostpartyforsoldiers", "besi_castle_hostpartyforsoldiers_back", "{=BESI_HostPartyForSoldiers_Back}Back", back_on_condition, (MenuCallbackArgs args) => { GameMenu.SwitchToMenu("castle"); }, true);

            gameStarter.AddWaitGameMenu("besi_castle_hostpartyforsoldiers_inprogress", "{=BESI_HostPartyForSoldiers_Preparation}You gathered all available people from your castle to help out with the preparations. Preparations are ongoing...",
                                        (MenuCallbackArgs args) => { args.MenuContext.GameMenu.EndWait(); },
                                        (MenuCallbackArgs args) => { return true; },
                                        (MenuCallbackArgs args) => { args.MenuContext.GameMenu.StartWait(); GameMenu.SwitchToMenu("besi_castle_hostpartyforsoldiers_preparationfinished"); PartyRoomMenuSoundOnInit(args); },
                                        OnPreparationTick,
                                        type: GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption,
                                        overlay: TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                                        targetWaitHours: MCMSettings.Instance.HostPartyPreparationHours
                                        );

            gameStarter.AddGameMenu("besi_castle_hostpartyforsoldiers_preparationfinished", "{=BESI_HostPartyForSoldiers_PreparationFinished}The preparations are finished! Temporary tavern maids are on standby while your men are walking in.", (args) => { Hero.MainHero.Gold = Hero.MainHero.Gold - MCMSettings.Instance.HostPartyCost; args.MenuTitle = new TextObject("{=BESI_HostPartyForSoldiersMenu_PreparationFinishedTitle}Party Preparations Finished!"); });
            gameStarter.AddGameMenuOption("besi_castle_hostpartyforsoldiers_preparationfinished", "besi_castle_hostpartyforsoldiers_joinparty", "{=BESI_HostPartyForSoldiers_JoinParty}Join the party!", (args) => { args.optionLeaveType = GameMenuOption.LeaveType.Mission; return true; }, (args) => { OnPreparationFinished(); });

            gameStarter.AddGameMenuOption("besi_castle_hostpartyforsoldiers_inprogress", "besi_castle_hostpartyforsoldiers_inprogress_back", "{=BESI_HostPartyForSoldiers_Back}Back", back_on_condition, (MenuCallbackArgs args) => { GameMenu.SwitchToMenu("besi_castle_hostpartyforsoldiers"); }, true);

        }


        [GameMenuInitializationHandler("castle_partyroom")]
        public static void PartyRoomMenuSoundOnInit(MenuCallbackArgs args)
        {
            args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/tavern");
        }



        bool host_party_for_soldiers_condition(MenuCallbackArgs args)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement == null)
                return false;

            if (Settlement.CurrentSettlement.Owner != Hero.MainHero)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=BESI_HostPartyForSoldiers_NotOwnerOfCastle}You are not the owner of this castle!");
                return true;
            }

            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            args.IsEnabled = true;
            return true;
        }

        void host_party_for_soldiers_menu_init(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=BESI_HostPartyForSoldiersMenuTitle}Host Party For Your Soldiers");
        }

        bool host_party_for_soldiers_startpreparation_condition(MenuCallbackArgs args)
        {
            if (Hero.MainHero.Gold < MCMSettings.Instance.HostPartyCost)
            {
                args.Tooltip = new TextObject("{=BESI_HostPartyForSoldiers_NotEnoughDenars}You do not have enough denars.");
                args.IsEnabled = false;
                return true;
            }
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            args.IsEnabled = true;
            return true;
        }



        void host_party_for_soldiers_startpreparation_consequence(MenuCallbackArgs args)
        {
            _preparationTimeSpent = 0f;
            GameMenu.SwitchToMenu("besi_castle_hostpartyforsoldiers_inprogress");
        }



        void OnPreparationTick(MenuCallbackArgs args, CampaignTime dt)
        {
            _preparationTimeSpent += (float)dt.ToHours;
            float progress = _preparationTimeSpent / MCMSettings.Instance.HostPartyPreparationHours;
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(progress);
        }
        public void OnPreparationFinished()
        {
            MobileParty.MainParty.RecentEventsMorale += 5;
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BESI_HostPartyForSoldiers_MoraleBoosted}Your party's morale has increased!").ToString()));
            GameMenu.SwitchToMenu("castle");
            Campaign.Current.GetCampaignBehavior<HostPartyRoomBehavior>().EnterPartyRoom();

        }
        static bool back_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }
    }
}
