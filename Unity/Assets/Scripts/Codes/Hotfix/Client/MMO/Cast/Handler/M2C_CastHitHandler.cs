using ET.EventType;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    //在这里的sceneType.Client就相当于et6.0的zoneScene
    [MessageHandler(SceneType.Client)]
    [FriendOfAttribute(typeof(ET.Client.Cast))]
    public class M2C_CastHitHandler : AMHandler<M2C_CastHit>
    {
        protected override async ETTask Run(Session session, M2C_CastHit message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.CasterId} 技能{message.CastId} 命中了{message.TargetsId.ListToString()} ");


            Unit caster = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(message.CasterId);
            if (caster == null)
            {
                return;
            }
            Cast cast = caster.GetComponent<CastComponent>().Get(message.CastId);
            if (cast == null)
            {
                return;
            }

            cast.TargetsId.Clear();
            foreach (var targetId in message.TargetsId)
            {
                Unit target = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(targetId);
                if (target == null || target.IsDisposed)
                {
                    continue;
                }
                cast.TargetsId.Add(targetId);
            }


            //技能命中，播放命中特效，和前摇配合校正技能位置等等
            foreach (var targetId in message.TargetsId)
            {
                EventSystem.Instance.Publish
                (
                    zoneScene.CurrentScene(),
                    new CastHit()
                    {
                        CasterId = message.CasterId,
                        CastId = message.CastId,
                        TargetId = targetId
                    }
                );
            }


            await ETTask.CompletedTask;
        }

    }


}
