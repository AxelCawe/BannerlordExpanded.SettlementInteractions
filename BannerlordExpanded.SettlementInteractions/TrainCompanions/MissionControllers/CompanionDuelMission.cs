using SandBox.Missions.MissionLogics;
using SandBox.View.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.MountAndBlade.View;

namespace BannerlordExpanded.SettlementInteractions.TrainCompanions.MissionControllers
{
    [MissionManager]
    internal class CompanionDuelMission
    {
        public static MissionInitializerRecord CreateDuelMissionInitializerRecord(
          string sceneName,
          string sceneLevels = "",
          bool doNotUseLoadingScreen = false)
        {
            MissionInitializerRecord initializerRecord = new MissionInitializerRecord(sceneName);
            //initializerRecord.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
            initializerRecord.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
            initializerRecord.PlayingInCampaignMode = Campaign.Current.GameMode == CampaignGameMode.Campaign;
            initializerRecord.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
            initializerRecord.SceneLevels = sceneLevels;
            initializerRecord.DoNotUseLoadingScreen = doNotUseLoadingScreen;
            return initializerRecord;
        }

        [MissionMethod]
        public static Mission OpenDuelMission(
          string scene,
          CharacterObject duelCharacter,
          bool spawnBothSidesWithHorse)
        {
            return MissionState.OpenNew("CompanionDuelMission", CreateDuelMissionInitializerRecord(scene), (a) => new MissionBehavior[]
            {
                new MissionCampaignView(),
                new CampaignMissionComponent(),
                new MissionOptionsComponent(),
                new BattleMissionAgentInteractionLogic(),
                new CompanionDuelMissionController(duelCharacter, spawnBothSidesWithHorse),
                ViewCreator.CreateMissionAgentStatusUIHandler(a),
                ViewCreator.CreateMissionMainAgentEquipmentController(a),
                new MissionSingleplayerViewHandler(),
                //(MissionView) new MusicBattleMissionView(false),
                //new MissionBoundaryWallView(),
                //new MissionItemContourControllerView(),
                //new MissionAgentContourControllerView(),
                new MissionGauntletOptionsUIHandler(),
                new AgentHumanAILogic(),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreatePhotoModeView(),
            }, true, true);
        }
    }
}
