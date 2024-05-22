using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerlordExpanded.SettlementInteractions.Inns.Save
{
    internal class InnWanderersSaveDefiner : SaveableTypeDefiner
    {
        public InnWanderersSaveDefiner() : base(43254323)
        {
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<Hero>));
            ConstructContainerDefinition(typeof(Dictionary<Village, List<Hero>>));
        }
    }
}
