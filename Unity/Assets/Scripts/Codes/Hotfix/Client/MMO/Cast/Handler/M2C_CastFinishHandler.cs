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
    public class M2C_CastFinishHandler : AMHandler<M2C_CastFinish>
    {
        protected override async ETTask Run(Session session, M2C_CastFinish message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.CasterId} 技能{message.CastId} 结束了 ");
            //技能结束，播放技能结束后摇，回到idle状态，回收技能特效，模型，ui等资源
            EventSystem.Instance.Publish
            (
                zoneScene.CurrentScene(),
                new CastFinish()
                {
                    CasterId = message.CasterId,
                    CastId = message.CastId,
                }
            );


            await ETTask.CompletedTask;
        }

    }


}
