using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ET.Server
{
    public static class MMOMessageHelper
    {
        public static void SendClient(Unit unit, IActorMessage message, NoticeClientType noticeClientType)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            switch (noticeClientType)
            {
                case NoticeClientType.NoNotice:
                    break;
                case NoticeClientType.Self:
                    SendClientSelf(unit,message);
                    break;
                case NoticeClientType.BroadcastNoSelf:
                    SendClientBroadcastNoSelf(unit, message);
                    break;
                case NoticeClientType.Broadcast:
                    SendClientBroadcast(unit, message);
                    break;
            }
        }
        public static void SendClientSelf(Unit unit, IActorMessage message)
        {
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            if (unitGateComponent == null)
            {
                return;
            }
            if (unitGateComponent.GateSessionActorId == 0)
            {
                return;
            }
            MessageHelper.SendActor(unitGateComponent.GateSessionActorId,message);
        }
        public static void SendClientBroadcast(Unit unit, IActorMessage message)
        {
            if (unit.GetComponent<AOIEntity>() == null)
            {
                return;
            }

            Dictionary <long,AOIEntity> dict = unit.GetBeSeePlayers();
            if (dict.Count <= 0)
            {
                return;
            }

            foreach (AOIEntity aoiEntity in dict.Values)
            {
                Unit u = aoiEntity.Unit;
                if (u == null || u.IsDisposed)
                {
                    continue;
                }
                SendClientSelf(u,message);
            }
        }

        public static void SendClientBroadcastNoSelf(Unit unit, IActorMessage message)
        {
            if (unit.GetComponent<AOIEntity>() == null)
            {
                return;
            }
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            if (dict.Count <= 0)
            {
                return;
            }

            foreach (AOIEntity aoiEntity in dict.Values)
            {
                Unit u = aoiEntity.Unit;
                if (u == null || u.IsDisposed)
                {
                    continue;
                }
                if (u.Id == unit.Id)
                {
                    continue;
                }
                SendClientSelf(u, message);
            }
        }

        public static void ForceSetPosition(this Unit unit,float3 newPos , bool sendMsg = false)
        {
            unit.Position = unit.GetComponent<PathfindingComponent>().RecastFindNearestPoint(newPos);
            if (sendMsg)
            {
                M2C_SetPosition msg = new M2C_SetPosition();
                msg.UnitId = unit.Id;
                msg.Position = unit.Position;
                msg.Rotation = unit.Rotation;
                SendClient(unit, msg, NoticeClientType.Broadcast);
            }
        }
    }
}
