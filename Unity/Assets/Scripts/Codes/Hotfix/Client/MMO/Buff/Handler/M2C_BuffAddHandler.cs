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
    public class M2C_BuffAddHandler : AMHandler<M2C_BuffAdd>
    {
        protected override async ETTask Run(Session session, M2C_BuffAdd message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.UnitId} 添加了 {message.BuffData.ConfigId} BUFF({message.BuffData.Id}) ");
            Unit unit = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(message.UnitId);

            if (unit != null)
            {
                EventSystem.Instance.Publish
                (
                    zoneScene,
                    new BuffAdd()
                    {
                        Unit = unit,
                        BuffConfigId = message.BuffData.ConfigId,
                        BuffId = message.BuffData.Id
                    }
                );
            }








            await ETTask.CompletedTask;
        }

    }


}
