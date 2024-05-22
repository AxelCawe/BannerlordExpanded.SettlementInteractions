using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.InputSystem;
using BannerlordExpanded.SettlementInteractions.TrainCompanions.Behaviors;
using Extensions = TaleWorlds.Core.Extensions;

namespace BannerlordExpanded.SettlementInteractions.TrainCompanions.MissionControllers
{
    internal class CompanionDuelMissionController : MissionLogic
    {
        private CharacterObject _duelCharacter;
        private bool _spawnBothSidesWithHorses;
        private bool _duelHasEnded;
        private BasicMissionTimer _duelEndTimer;
        private Agent _playerAgent;
        private Agent _duelAgent;
        private bool _duelWon;

        public CompanionDuelMissionController(
          CharacterObject duelCharacter,
          bool spawnBothSidesWithHorses)
        {
            _duelCharacter = duelCharacter;
            _spawnBothSidesWithHorses = spawnBothSidesWithHorses;
        }

        public override void AfterStart()
        {
            Mission.SetMissionMode(MissionMode.Battle, true);
            //Mission.SetMissionMode(MissionMode., true);
            _duelHasEnded = false;
            _duelEndTimer = new BasicMissionTimer();
            InitializeMissionTeams();
            MatrixFrame playerSpawnFrame;
            MatrixFrame opponentSpawnFrame;
            //var attackerEntity = Mission.Current.Scene.FindEntityWithTag("spawnpoint_player");
            var attackerEntity = Mission.Current.Scene.FindEntityWithTag("attacker_infantry") ?? Mission.Current.Scene.FindEntityWithName("sp_attacker_infantry"); ;



            Vec3 globalPosition = attackerEntity.GlobalPosition;
            getBattleSpawnFrames(globalPosition.AsVec2, out playerSpawnFrame, out opponentSpawnFrame);
            _playerAgent = SpawnAgent(CharacterObject.PlayerCharacter, playerSpawnFrame);
            Mission.CameraIsFirstPerson = false;
            _duelAgent = SpawnAgent(_duelCharacter, opponentSpawnFrame);
        }

        public override void OnMissionTick(float dt)
        {
            if (!_duelHasEnded)
                return;
            GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameTextFromKeyID(HotKeyManager.GetAllCategories().FirstOrDefault(r => r.GameKeyCategoryId == "Generic").RegisteredGameKeys[4].KeyboardKey.ToString()).ToString());
            MBInformationManager.AddQuickInformation(GameTexts.FindText("str_duel_has_ended", null), 0, null, "");
            _duelEndTimer.Reset();
        }

        public override InquiryData OnEndMissionRequest(out bool canLeave)
        {
            canLeave = true;
            return _duelHasEnded ? null :
                new InquiryData("", GameTexts.FindText("str_give_up_fight", null).ToString(), true, true, GameTexts.FindText("str_ok", null).ToString(), GameTexts.FindText("str_cancel", null).ToString(), new Action(Mission.OnEndMissionResult), null, "");
        }

        public override void OnAgentRemoved(
          Agent affectedAgent,
          Agent affectorAgent,
          AgentState agentState,
          KillingBlow killingBlow)
        {
            if (!affectedAgent.IsHuman)
                return;
            if (affectedAgent == _duelAgent)
                _duelWon = true;
            _duelHasEnded = true;
        }

        public override bool MissionEnded(ref MissionResult missionResult) => false;

        protected override void OnEndMission() => Campaign.Current.GetCampaignBehavior<TrainCompanionsBehavior>().OnDuelEnd(_duelWon);

        private Agent SpawnAgent(CharacterObject character, MatrixFrame spawnFrame)
        {
            AgentBuildData agentBuildData1 = new AgentBuildData(character);
            agentBuildData1.BodyProperties(character.GetBodyPropertiesMax());
            Team team = character == CharacterObject.PlayerCharacter ? Mission.PlayerTeam : Mission.PlayerEnemyTeam;
            Mission mission = Mission;
            AgentBuildData agentBuildData2 = agentBuildData1.Team(team).InitialPosition(spawnFrame.origin);
            Vec2 vec2 = spawnFrame.rotation.f.AsVec2;
            vec2 = vec2.Normalized();
            ref Vec2 local = ref vec2;
            AgentBuildData agentBuildData3 = agentBuildData2.InitialDirection(local).NoHorses(!_spawnBothSidesWithHorses).Equipment(character.FirstBattleEquipment).TroopOrigin(GetAgentOrigin(character)).ClothingColor1(character.Culture.Color).ClothingColor2(character.Culture.Color2);
            Agent agent = mission.SpawnAgent(agentBuildData3, false);
            agent.FadeIn();
            if (character == CharacterObject.PlayerCharacter)
                agent.Controller = (Agent.ControllerType)2;
            else
                agent.Controller = Agent.ControllerType.AI;
            if (agent.IsAIControlled)
            {
                agent.SetWatchState(Agent.WatchState.Alarmed);
            }
            agent.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp);
            return agent;
        }

        private IAgentOriginBase GetAgentOrigin(CharacterObject character) =>
            new SimpleAgentOrigin(character, -1, null, new UniqueTroopDescriptor());

        private void InitializeMissionTeams()
        {
            Mission.Teams.Add(BattleSideEnum.Defender, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, Hero.MainHero.Clan.Banner, true, false, true);
            Mission.Teams.Add(BattleSideEnum.Attacker, _duelCharacter.Culture.Color, _duelCharacter.Culture.Color2, _duelCharacter.HeroObject.Clan.Banner, true, false, true);
            Mission.PlayerTeam = Mission.Teams.Defender;
            foreach (Team team in Mission.Teams)
            {
                if (team != Mission.PlayerTeam)
                    team.SetIsEnemyOf(Mission.PlayerTeam, true);
            }
        }

        private void getBattleSpawnFrames(
          Vec2 spawnPoint,
          out MatrixFrame playerSpawnFrame,
          out MatrixFrame opponentSpawnFrame)
        {
            float num = 0.0f;
            Vec2 vec2_1 = new Vec2(spawnPoint.X, spawnPoint.Y);
            Mission.Scene.GetHeightAtPoint(vec2_1, (BodyFlags)2208137, ref num);
            Vec3 vec3_1 = new Vec3(vec2_1.X, vec2_1.Y, num, -1);
            Mat3 mat3_1 = new Mat3(Vec3.Side, new Vec3(0f, -1f, 0f, -1f), Vec3.Up);
            Vec2 vec2_2 = new Vec2(spawnPoint.X, spawnPoint.Y - 10f);
            Mission.Scene.GetHeightAtPoint(vec2_2, (BodyFlags)2208137, ref num);
            Vec3 vec3_2 = new Vec3(vec2_2.X, vec2_2.Y, num, -1);
            Mat3 mat3_2 = new Mat3(Vec3.Side, Vec3.Forward, Vec3.Up);
            playerSpawnFrame = new MatrixFrame(mat3_1, vec3_1);
            playerSpawnFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
            opponentSpawnFrame = new MatrixFrame(mat3_2, vec3_2);
            opponentSpawnFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
        }

    }
}
