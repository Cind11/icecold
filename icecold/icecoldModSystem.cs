using Vintagestory.API.Common;

namespace icecold
{
    public class icecoldModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterEntityBehaviorClass("freezeInWater", typeof(FreezeInWater));
            api.RegisterBlockClass("BreakingBlockLakeIce", typeof(BreakingBlockLakeIce));
            api.RegisterBlockClass("CollideThroughSnowLayer", typeof(CollideThroughSnowLayer));
        }
    }
}
