using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace icecold
{
    public class BreakingBlockLakeIce : Vintagestory.GameContent.BlockLakeIce
    {
        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact) 
        {
            if (facing.Axis == EnumAxis.Y && entity?.Pos?.AsBlockPos?.DistanceTo(pos.X, pos.Y, pos.Z) <= 1.5f) 
            {
                float chance = 0.005f;
                float fallChance = 0.2f;

                if (collideSpeed.Y < 0.0f)
                {
                    chance -= (float)collideSpeed.Y * 3;
                    fallChance = 1.0f;
                }

                var eagent = entity as EntityAgent;
                if (eagent?.Controls?.Sprint == true)
                {
                    chance += 0.05f;
                    fallChance += 0.3f;
                }

                ClimateCondition conds = api?.World?.BlockAccessor?.GetClimateAt(pos, EnumGetClimateMode.NowValues);
                if (conds == null) return;

                var tempMod = 20.0f + GameMath.Clamp(conds.Temperature, -20.0f, 0.0f);
                chance *= tempMod/10;

                if (GameMath.RoundRandom(System.Random.Shared, chance) > 0) 
                { 
                    entity?.Api?.World?.BlockAccessor?.BreakBlock(pos, null, 0);
                    if (GameMath.RoundRandom(System.Random.Shared, fallChance) > 0)
                        entity?.TeleportTo(pos);
                }
            }
        }
    }
}