using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace icecold
{
    public class CollideThroughSnowLayer : Vintagestory.GameContent.BlockSnowLayer
    {
        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            if (facing.Axis == EnumAxis.Y)
            {
                var posBelow = pos;
                posBelow = pos.DownCopy();
                world?.BlockAccessor?.GetBlock(posBelow)?.OnEntityCollide(world, entity, posBelow, facing, collideSpeed, isImpact);
            }
        }
    }
}