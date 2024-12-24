using Modding.Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwordMod
{
    public class Sword_Perk : CustomPerk
    {
        public const string PERK_ID = "unl-sword_perk";
        public override string ID => PERK_ID;

        //public override string Group //"Other Perks" by default

        public override string DisplayName => "Sword";

        public override string Description => "Spawns a sword for each player at the start of each cycle.";

        public override string ManualDescription => "Spawns a sword for each player at the start of each cycle.";

        public override string SpriteName => "Symbol_SwordMod_Sword";

        //public override Color Color //= Color.White

        public override bool UnlockedByDefault => true;

        //stuff
        public Sword_Perk() : base()
        {

        }
        
    }
}
