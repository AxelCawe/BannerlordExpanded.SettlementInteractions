using BannerlordExpanded.SettlementInteractions.HostPartyForSoldiers.Behaviors;
using BannerlordExpanded.SettlementInteractions.Inns.Behaviors;
using BannerlordExpanded.SettlementInteractions.Inns.Settings;
using BannerlordExpanded.SettlementInteractions.TrainCompanions.Behaviors;
using BannerlordExpanded.StartYourOwnTournaments.Behaviors;
using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;


namespace BannerlordExpanded.SettlementInteractions
{
    public class SubModule : MBSubModuleBase
    {
        bool harmonyPatched = false;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (!harmonyPatched)
            {
                Harmony harmony = new Harmony("BannerlordExpanded.SettlementInteractions");
                if (MCMSettings.Instance.IsInnEnabled)
                    harmony.PatchCategory(Assembly.GetExecutingAssembly(), "InnModule");
                if (MCMSettings.Instance.HostPartyEnabled)
                    harmony.PatchCategory(Assembly.GetExecutingAssembly(), "HostPartyForSoldiersModule");

                harmonyPatched = true;
            }
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (gameStarter as CampaignGameStarter != null)
            {
                AddBehaviors(gameStarter as CampaignGameStarter);
            }



        }

        private void AddBehaviors(CampaignGameStarter gameStarter)
        {
            //gameStarter.AddBehavior(new SaveDataBehaviour());
            if (gameStarter != null)
            {
                #region INNS
                if (MCMSettings.Instance.IsInnEnabled)
                {
                    gameStarter.AddBehavior(new InnBehavior());
                    gameStarter.AddBehavior(new InnWanderersBehavior());
                }
                #endregion

                #region START_YOUR_OWN_TOURNAMENTS
                if (MCMSettings.Instance.IsStartYourOwnTournamentsEnabled)
                {
                    gameStarter.AddBehavior(new StartYourOwnTournamentsBehavior());
                }
                #endregion

                #region COMPANION_TRAINING
                if (MCMSettings.Instance.CompanionTrainingEnabled)
                {
                    gameStarter.AddBehavior(new TrainCompanionsBehavior());
                }
                #endregion

                #region HOST_PARTY_FOR_SOLDIERS
                if (MCMSettings.Instance.HostPartyEnabled)
                {
                    gameStarter.AddBehavior(new HostPartyForSoldiersBehavior());
                    gameStarter.AddBehavior(new HostPartyRoomBehavior());
                }
                #endregion
            }
        }
    }
}