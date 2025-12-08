using BannerlordExpanded.SettlementInteractions.Inns.Settings;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerlordExpanded.StartYourOwnTournaments.Behaviors
{
    public class StartYourOwnTournamentsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            //throw new NotImplementedException();
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddGameMenu);
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }

        void AddGameMenu(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town_arena", "startyourowntournament_GameMenu_Denars", "{=BESI_GameMenu_StartYourOwnTournament_Denar}Start a new tournament with denars", new GameMenuOption.OnConditionDelegate(CanStartTournamentWithDenar), new GameMenuOption.OnConsequenceDelegate(RequestToStartTournamentWithDenar), false, -1, false);
            campaignGameStarter.AddGameMenuOption("town_arena", "startyourowntournament_GameMenu_Influence", "{=BESI_GameMenu_StartYourOwnTournament_Influence}Start a new tournament with influence", new GameMenuOption.OnConditionDelegate(CanStartTournamentWithInfluence), new GameMenuOption.OnConsequenceDelegate(RequestToStartTournamentWithInfluence), false, -1, false);
        }


        bool CanStartTournamentWithDenar(MenuCallbackArgs args)
        {
            if (Settlement.CurrentSettlement == null || !Settlement.CurrentSettlement.IsTown || Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town) != null) // used to check if the player is in a town and if the town does not have a tournament. If not, return false as this menu shouldnt appear.
                return false;

            if (Hero.MainHero.Gold < GlobalSettings<MCMSettings>.Instance.DenarCostToStartTournament)
            {
                args.IsEnabled = false;

                TextObject notEnoughDenars = new TextObject("{=BESI_GameMenu_StartYourOwnTournament_ExplainThatPlayerDoesNotHaveEnoughDenars}It costs {TOURNAMENT_COST}{GOLD_ICON} to start a tournament. You do not have enough {GOLD_ICON}!");
                notEnoughDenars.SetTextVariable("TOURNAMENT_COST", GlobalSettings<MCMSettings>.Instance.DenarCostToStartTournament);
                args.Tooltip = notEnoughDenars;

            }
            else
            {
                TextObject enoughDenars = new TextObject("{=BESI_GameMenu_StartYourOwnTournament_ExplainDenarPrice}It costs {TOURNAMENT_COST}{GOLD_ICON} to start a tournament.");
                enoughDenars.SetTextVariable("TOURNAMENT_COST", GlobalSettings<MCMSettings>.Instance.DenarCostToStartTournament);
                args.Tooltip = enoughDenars;
            }
            args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
            return true;
        }

        void RequestToStartTournamentWithDenar(MenuCallbackArgs args)
        {
            TextObject confirmationTitle = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_ConfirmTitle}Start Tournament Confirmation");
            TextObject confirmButton = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_ConfirmButton}Confirm");
            TextObject cancelButton = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_CancelButton}Cancel");
            TextObject confirmation = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_ConfirmDenarTournament}Are you sure you want to start a tournament using {TOURNAMENT_COST}{GOLD_ICON}?");
            confirmation.SetTextVariable("TOURNAMENT_COST", GlobalSettings<MCMSettings>.Instance.DenarCostToStartTournament);
            InformationManager.ShowInquiry(new InquiryData(confirmationTitle.ToString(), confirmation.ToString(), true, true, confirmButton.ToString(), cancelButton.ToString(), () => StartTournamentWithDenar(), null));
        }

        void StartTournamentWithDenar()
        {
            Campaign.Current.TournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(Settlement.CurrentSettlement.Town));
            GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, Settlement.CurrentSettlement, GlobalSettings<MCMSettings>.Instance.DenarCostToStartTournament);
            GameMenu.SwitchToMenu("town_arena");
        }

        void RequestToStartTournamentWithInfluence(MenuCallbackArgs args)
        {
            TextObject confirmationTitle = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_ConfirmTitle}Start Tournament Confirmation");
            TextObject confirmButton = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_ConfirmButton}Confirm");
            TextObject cancelButton = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_CancelButton}Cancel");
            TextObject confirmation = new TextObject("{=BESI_InquiryMenu_StartYourOwnTournament_ConfirmInfluenceTournament}Are you sure you want to start a tournament using {TOURNAMENT_COST} influence?");
            confirmation.SetTextVariable("TOURNAMENT_COST", GlobalSettings<MCMSettings>.Instance.InfluenceCostToStartTournament);
            InformationManager.ShowInquiry(new InquiryData(confirmationTitle.ToString(), confirmation.ToString(), true, true, confirmButton.ToString(), cancelButton.ToString(), () => StartTournamentWithInfluence(), null));
        }

        void StartTournamentWithInfluence()
        {
            Campaign.Current.TournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(Settlement.CurrentSettlement.Town));
            Clan.PlayerClan.Influence -= GlobalSettings<MCMSettings>.Instance.InfluenceCostToStartTournament;
            TextObject influenceSpentAnnouncement = new TextObject("{=BESI_Announcement_StartYourOwnTournament_UsedInfluence}You have used {INFLUENCE_COST} influence to start a tournament!");
            influenceSpentAnnouncement.SetTextVariable("INFLUENCE_COST", (GlobalSettings<MCMSettings>.Instance.InfluenceCostToStartTournament));
            InformationManager.DisplayMessage(new InformationMessage(influenceSpentAnnouncement.ToString()));
            GameMenu.SwitchToMenu("town_arena");
        }

        bool CanStartTournamentWithInfluence(MenuCallbackArgs args)
        {
            if (Settlement.CurrentSettlement == null || !Settlement.CurrentSettlement.IsTown || Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town) != null) // used to check if the player is in a town. If not, return false as this menu shouldnt appear.
                return false;
            if (Settlement.CurrentSettlement.Town.OwnerClan == Clan.PlayerClan
                || Settlement.CurrentSettlement.Town.OwnerClan == null
                || (Settlement.CurrentSettlement.Town.OwnerClan.Kingdom != null && Clan.PlayerClan.Kingdom != null && (Settlement.CurrentSettlement.Town.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom || FactionManager.IsNeutralWithFaction(Settlement.CurrentSettlement.Town.OwnerClan, Clan.PlayerClan)))) // check if player is in same kingdom as the owner
            {
                if (Clan.PlayerClan.Influence < GlobalSettings<MCMSettings>.Instance.InfluenceCostToStartTournament)
                {
                    args.IsEnabled = false;
                    TextObject notEnoughDenars = new TextObject("{=BESI_GameMenu_StartYourOwnTournament_ExplainThatPlayerDoesNotHaveEnoughInfluence}It costs {TOURNAMENT_COST} influence to start a tournament. You do not have enough influence!");
                    notEnoughDenars.SetTextVariable("TOURNAMENT_COST", GlobalSettings<MCMSettings>.Instance.InfluenceCostToStartTournament);
                    args.Tooltip = notEnoughDenars;
                }
                else
                {
                    TextObject enoughDenars = new TextObject("{=BESI_GameMenu_StartYourOwnTournament_ExplainInfluencePrice}It costs {TOURNAMENT_COST} influence to start a tournament.");
                    enoughDenars.SetTextVariable("TOURNAMENT_COST", GlobalSettings<MCMSettings>.Instance.InfluenceCostToStartTournament);
                    args.Tooltip = enoughDenars;
                }
            }
            else
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=BESI_GameMenu_StartYourOwnTournament_ExplainThatPlayerIsNotAffiliatedWithTown}You are not affiliated with this town.");
            }
            args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
            return true;
        }
    }
}
