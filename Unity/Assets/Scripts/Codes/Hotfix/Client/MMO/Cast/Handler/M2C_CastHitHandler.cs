using ET.EventType;
using NLog;
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
    public class M2C_CastHitHandler : AMHandler<M2C_CastHit>
    {
        protected override async ETTask Run(Session session, M2C_CastHit message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.CasterId} 技能{message.CastId} 命中了{message.TargetsId.ListToString()} ");
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
