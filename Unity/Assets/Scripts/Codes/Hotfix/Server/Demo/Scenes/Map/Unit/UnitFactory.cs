using System;
using System.Numerics;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOfAttribute(typeof(ET.Server.BulletComponent))]
    public static class UnitFactory
    {
        public static Unit Create(Scene scene, long id, UnitType unitType)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            switch (unitType)
            {
                case UnitType.Player:
                    {
                        Unit unit = unitComponent.AddChildWithId<Unit, int>(id, 1001);
                        unit.AddComponent<MoveComponent>();
                        unit.Position = new float3(-10, 0, -10);

                        NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
                        numericComponent.Set(NumericType.Speed, 6f); // 速度是6米每秒
                        numericComponent.Set(NumericType.AOI, 15000); // 视野15米

                        unitComponent.Add(unit);
                        //// 加入aoi
                        //unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);

                        unit.AddComponent<CastComponent>();
                        unit.AddComponent<BuffComponent>();
                        unit.AddComponent<SkillStatusComponent>();
                        return unit;
                    }
                default:
                    throw new Exception($"not such unit type: {unitType}");
            }
        }
        /// <summary>
        /// 创建子弹
        /// </summary>
        public static Unit CreateBullet(Scene scene, long ownerId, int unitConfigId, int bulletId, float3 pos, quaternion quaternion)
        {

            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit unit = unitComponent.AddChild<Unit, int>(unitConfigId);
            unit.Position = pos;
            unit.Rotation = quaternion;
            unit.AddComponent<CastComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            numericComponent.Set(NumericType.Speed, 6f);
            numericComponent.Set(NumericType.AOI, 15000);

            BulletComponent bulletComponent = unit.AddComponent<BulletComponent, int>(bulletId);
            bulletComponent.OwnerId = ownerId;
            unitComponent.Add(unit);
            return unit;
        }


        public static Unit CreateMonster(Scene scene, int unitConfigId, float3 pos)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit unit = unitComponent.AddChild<Unit, int>(unitConfigId);
            unit.AddComponent<MoveComponent>();
            unit.Position = pos;
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            numericComponent.Set(NumericType.Speed, 6.0f);
            numericComponent.Set(NumericType.AOI, 15000);
            numericComponent.Set(NumericType.MaxHp, 1000);
            numericComponent.Set(NumericType.Hp, 1000);
            unit.AddComponent<ReliveComponent>();
            unitComponent.Add(unit);
            return unit;
        }
    }
}