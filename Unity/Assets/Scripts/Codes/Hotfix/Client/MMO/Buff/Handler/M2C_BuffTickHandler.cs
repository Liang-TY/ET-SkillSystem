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
    public class M2C_BuffTickHandler : AMHandler<M2C_BuffTick>
    {
        protected override async ETTask Run(Session session, M2C_BuffTick message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.UnitId} 触发了 BUFF tick({message.BuffId}) ");

            //buff移除，状态移除在客户端buffcomponent，回收特效，ui特效等
            Unit unit = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(message.UnitId);

            if (unit != null)
            {
                EventSystem.Instance.Publish
                (
                    zoneScene,
                    new BuffTick()
                    {
                        unit = unit,
                        BuffId = message.BuffId
                    }
                );
            }








            await ETTask.CompletedTask;
        }

    }


}
