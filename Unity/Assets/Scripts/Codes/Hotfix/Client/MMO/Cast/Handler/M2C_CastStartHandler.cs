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
    public class M2C_CastStartHandler : AMHandler<M2C_CastStart>
    {
        protected override async ETTask Run(Session session, M2C_CastStart message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.CasterId} 开始释放 {message.CastConfigId} 技能({message.CastId}) ");
            //技能释放流程的开始，此处可以自行接入行为树或状态机之类的
            //开始播放技能前摇，播放技能特效，音效等等


            EventSystem.Instance.Publish
                (
                    zoneScene.CurrentScene(),
                    new CastStart()
                    {
                        CasterId = message.CasterId,
                        CastConfigId = message.CastConfigId,
                        CastId = message.CasterId
                    }
                );






            await ETTask.CompletedTask;
        }

    }


}
