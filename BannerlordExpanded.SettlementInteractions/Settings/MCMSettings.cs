using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;

namespace BannerlordExpanded.SettlementInteractions.Inns.Settings
{
    internal class MCMSettings : AttributeGlobalSettings<MCMSettings>
    {
        public override string Id => "BE_SettlementInteractions";

        public override string DisplayName => "BE - Settlement Interactions";

        public override string FolderName => "BannerlordExpanded.SettlementInteractions";

        public override string FormatType => "xml";

        #region INNS
        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_InnModule}Inn Module", GroupOrder = 0)]
        [SettingPropertyBool("{=BE_SettlementInteractions_Settings_InnModule}Inn Module", IsToggle = true, RequireRestart = true)]
        public bool IsInnEnabled { get; set; } = true;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_InnModule}Inn Module", GroupOrder = 0)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_InnModule_MaxCompanions}Maximum number of wanderers in village", minValue: 0, maxValue: 100, Order = 1, HintText = "{=BE_SettlementInteractions_Settings_InnModule_MaxCompanions_Desc}The maximum number of wanderers that can be in a village at once. Expect lag/temporary freezes if you set a high number. Click on Refresh Wanderers button when changing this setting while in-game!", RequireRestart = false)]
        public int MaxCompanionsPerTown { get; set; } = 2;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_InnModule}Inn Module", GroupOrder = 0)]
        [SettingPropertyBool("{=BE_SettlementInteractions_Settings_InnModule_RefreshWeekly}Auto-refresh Wanderers weekly", Order = 2, HintText = "{=BE_SettlementInteractions_Settings_InnModule_RefreshWeekly_Desc}Automatically refreshes the wanderers at the start of every week. May cause lag!")]
        public bool AutoRefreshEveryWeek { get; set; } = false;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_InnModule}Inn Module", GroupOrder = 0)]
        [SettingPropertyButton("{=BE_SettlementInteractions_Settings_InnModule_Refresh}Refresh Wanderers", Content = "{=BE_SettlementInteractions_Settings_InnModule_Refresh_ButtonText}Press to refresh", Order = 3, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_InnModule_Refresh_Desc}Removes all current wanderers and spawn new ones. Use when changing max number of companions midgame! MAY CAUSE TEMPORARY FREEZE")]
        public Action RefreshWanderers { get; set; }

        #endregion

        #region TOURNAMENTS
        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule}Start Your Own Tournaments", GroupOrder = 1)]
        [SettingPropertyBool("{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule}Start Your Own Tournaments", IsToggle = true, RequireRestart = true)]
        public bool IsStartYourOwnTournamentsEnabled { get; set; } = true;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule}Start Your Own Tournaments", GroupOrder = 1)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule_DenarCost}Denar Cost to start tournaments", minValue: 0, maxValue: 10000000, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule_DenarCost_Desc}How much denars it costs to start a tournament.")]
        public int DenarCostToStartTournament { get; set; } = 10000;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule}Start Your Own Tournaments", GroupOrder = 1)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule_InfluenceCost}Influence Cost to start tournaments", minValue: 0, maxValue: 10000000, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_StartYourOwnTournamentsModule_InfluenceCost_Desc}How much influence it costs to start a tournament.")]
        public int InfluenceCostToStartTournament { get; set; } = 10;
        #endregion

        #region COMPANION_TRAINING
        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_CompanionTrainingModule}Companion Training", GroupOrder = 2)]
        [SettingPropertyBool("{=BE_SettlementInteractions_Settings_CompanionTrainingModule}Companion Training", IsToggle = true, RequireRestart = true)]
        public bool CompanionTrainingEnabled { get; set; } = true;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_CompanionTrainingModule}Companion Training", GroupOrder = 2)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_CompanionTrainingModule_TrainingHours}Hours to train companion", minValue: 1, maxValue: 72, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_CompanionTrainingModule_TrainingHours_Desc}How many hours it takes to train a companion.")]
        public int CompanionTrainingHours { get; set; } = 3;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_CompanionTrainingModule}Companion Training", GroupOrder = 2)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_CompanionTrainingModule_NumSkillsToLearn}Number of random skills for companion to improve", minValue: 0, maxValue: 18, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_CompanionTrainingModule_NumSkillsToLearn_Desc}When a companion finishes training, this setting will determine how many random skills the companion will improve on.")]
        public int CompanionTrainingNumOfRandomSkillsToLearn { get; set; } = 3;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_CompanionTrainingModule}Companion Training", GroupOrder = 2)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_CompanionTrainingModule_XpGainPerLevel}XP gain per skill level", minValue: 0, maxValue: 10000, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_CompanionTrainingModule_XpGainPerLevel_Desc}How much xp per skill level your companion will gain when finishing training.")]
        public int CompanionTrainingXpGain { get; set; } = 3; // scales with player's specific skill level
        #endregion

        #region HOST_PARTY_FOR_SOLDIERS
        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_HostPartyModule}Host Party For Soldiers", GroupOrder = 3)]
        [SettingPropertyBool("{=BE_SettlementInteractions_Settings_HostPartyModule}Host Party For Soldiers", IsToggle = true, RequireRestart = true)]
        public bool HostPartyEnabled { get; set; } = true;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_HostPartyModule}Host Party For Soldiers", GroupOrder = 3)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_HostPartyModule_PartyCost}Party Cost in Denars", minValue: 0, maxValue: 10000000, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_HostPartyModule_PartyCost_Desc}How much denars it cost to host a party.")]
        public int HostPartyCost { get; set; } = 5000;


        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_HostPartyModule}Host Party For Soldiers", GroupOrder = 3)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_HostPartyModule_PartyPreparationHours}Party Preparation Duration in Hours", minValue: 1, maxValue: 72, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_HostPartyModule_PartyPreparationHours_Desc}How many hours it takes to prepare the party.")]
        public int HostPartyPreparationHours { get; set; } = 3;

        [SettingPropertyGroup("{=BE_SettlementInteractions_Settings_HostPartyModule}Host Party For Soldiers", GroupOrder = 3)]
        [SettingPropertyInteger("{=BE_SettlementInteractions_Settings_HostPartyModule_MoraleGain}Party Morale Gain", minValue: 0, maxValue: 100, Order = 0, RequireRestart = false, HintText = "{=BE_SettlementInteractions_Settings_HostPartyModule_MoraleGain_Desc}How much morale is gained from hosting a party.")]
        public int HostPartyMoraleGain { get; set; } = 5;
        #endregion
    }
}
