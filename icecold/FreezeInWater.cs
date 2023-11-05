using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace icecold
{
    class FreezeInWater : EntityBehavior
    {
        ITreeAttribute tempTree;
        ICoreAPI api;
        EntityAgent eagent;

        BlockPos plrpos = new BlockPos();

        bool isEnclosedRoom = false;
        float slowaccum;
        float accum;

        public float CurBodyTemperature
        {
            get { return tempTree.GetFloat("bodytemp"); }
            set { tempTree.SetFloat("bodytemp", value); entity?.WatchedAttributes?.MarkPathDirty("bodyTemp"); }
        }

        public double FreezeInWaterUpdateTotalHours
        {
            get { return tempTree.GetDouble("freezeInWaterUpdateTotalHours"); }
            set { tempTree.SetDouble("freezeInWaterUpdateTotalHours", value); entity?.WatchedAttributes?.MarkPathDirty("bodyTemp"); }
        }

        public float Wetness
        {
            get { return entity?.WatchedAttributes?.GetFloat("wetness") ?? 0.0f; }
            set { entity?.WatchedAttributes?.SetFloat("wetness", value); }
        }

        public float NormalBodyTemperature;

        public FreezeInWater(Entity entity) : base(entity) 
        {
            eagent = entity as EntityAgent;
        }

        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            base.Initialize(properties, typeAttributes);

            api = entity.World?.Api;

            tempTree = entity.WatchedAttributes?.GetTreeAttribute("bodyTemp");
            FreezeInWaterUpdateTotalHours = api?.World?.Calendar?.TotalHours ?? 0;

            NormalBodyTemperature = typeAttributes["defaultBodyTemperature"]?.AsFloat(37) ?? 37;
        }

        public override void OnGameTick(float deltaTime)
        {
            if (entity == null) return;

            accum += deltaTime;
            if (accum < 1) return;
            accum = 0;

            if (!api?.World?.Config?.GetString("harshWinters")?.ToBool(true) ?? true) return;
            var eplr = entity as EntityPlayer;
            IPlayer plr = eplr?.Player;
            if (plr?.WorldData?.CurrentGameMode == EnumGameMode.Creative || plr?.WorldData?.CurrentGameMode == EnumGameMode.Spectator) return;

            plrpos?.Set((int)entity.Pos?.X, (int)entity.Pos?.Y, (int)entity.Pos?.Z);
            ClimateCondition conds = api?.World?.BlockAccessor?.GetClimateAt(plrpos, EnumGetClimateMode.NowValues);
            if (conds == null) return;

            slowaccum += deltaTime;
            if (slowaccum > 3)
            {
                Room room = api?.ModLoader?.GetModSystem<RoomRegistry>()?.GetRoomForPosition(plrpos);
                // Check whether it is a proper room, or something like a room i?.e?. with a roof, for exaample a natural cave
                isEnclosedRoom = room?.ExitCount == 0 || room?.SkylightCount < room?.NonSkylightCount;
            }

            float tempUpdateHoursPassed = (float)(api?.World?.Calendar?.TotalHours - FreezeInWaterUpdateTotalHours);

            if (tempUpdateHoursPassed > 0.01f)
            {
                var temperature = conds?.Temperature ?? 37;
                FreezeInWaterUpdateTotalHours = api?.World?.Calendar?.TotalHours ?? 0;
                if (entity.FeetInLiquid && temperature <= 0.0f && !isEnclosedRoom)
                {
                    float damage = 0.2f - temperature / 10.0f;
                    entity.ReceiveDamage(new DamageSource() { DamageTier = 0, Source = EnumDamageSource.Weather, Type = EnumDamageType.Frost }, damage);
                    Wetness = GameMath.Max(Wetness, 0.5f);
                }

                if (api?.Side != EnumAppSide.Server) return;
                if ((plr as IServerPlayer)?.ConnectionState != EnumClientState.Playing) return;

                if (entity.FeetInLiquid && temperature <= 0.0f && !isEnclosedRoom)
                {
                    float currentDeficit = NormalBodyTemperature - CurBodyTemperature;
                    float targetDeficit = 4.0f;
                    float timeToTarget = 3.0f * (-20.0f / temperature - 5.0f);
                    float step = GameMath.Clamp((targetDeficit - currentDeficit) / timeToTarget, 0.0f, 1.0f);
                    CurBodyTemperature -= step * (tempUpdateHoursPassed / 0.01f);
                }
            }
        }

        public override string PropertyName()
        {
            return "FreezeInWater";
        }
    }
}