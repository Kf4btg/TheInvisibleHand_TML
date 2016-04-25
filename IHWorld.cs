using Terraria.ModLoader;
using InvisibleHand.Items;

namespace InvisibleHand
{
    public class IHWorld : ModWorld
    {
        /// It seems that certain actions (like analyzing recipes) happen too early
        // if done in Mod.Load() (e.g. the recipes haven't even been intialized yet,
        // so checking the ingredients is pointless).
        // Thus, we'll do those actions upon world load to make sure that everything
        // is properly setup the way we need it to be.
        public override void Initialize()
        {
            ItemSets.Initialize();
        }
    }
}
