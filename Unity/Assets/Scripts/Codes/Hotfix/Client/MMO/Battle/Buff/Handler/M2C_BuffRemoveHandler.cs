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
    public class M2C_BuffRemoveHandler : AMHandler<M2C_BuffRemove>
    {
        protected override async ETTask Run(Session session, M2C_BuffRemove message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.UnitId} 移除了 BUFF({message.BuffId}) ");

            //buff移除，状态移除在客户端buffcomponent，回收特效，ui特效等
            Unit unit = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(message.UnitId);

            if(unit == null){
                return;
            }

            ClientBuff buff = unit.GetComponent<ClientBuffComponent>().Get(message.BuffId);
            if (buff == null)
            {
                return;
            }



            EventSystem.Instance.Publish
            (
                zoneScene,
                new BuffRemove()
                {
                    Unit = unit,
                    BuffId = message.BuffId
                }
            );

            unit.GetComponent<ClientBuffComponent>().Remove(message.BuffId);

            await ETTask.CompletedTask;
        }

    }


}
