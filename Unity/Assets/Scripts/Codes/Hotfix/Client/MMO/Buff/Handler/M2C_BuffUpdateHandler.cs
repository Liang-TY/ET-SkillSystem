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

    //Tick是每隔一段时间执行一次，而Update是执行一次用于更新Buff的数据

    //在这里的sceneType.Client就相当于et6.0的zoneScene
    [MessageHandler(SceneType.Client)]
    public class M2C_BuffUpdateHandler : AMHandler<M2C_BuffUpdate>
    {
        protected override async ETTask Run(Session session, M2C_BuffUpdate message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.UnitId} 更新了{message.BuffData.ConfigId}  BUFF({message.BuffData.Id}) ");

            //buff上信息的更新，各自根据更新的逻辑进行处理
            Unit unit = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(message.UnitId);

            if (unit == null)
            {
                return;
            }

            ClientBuff buff = unit.GetComponent<ClientBuffComponent>().Get(message.BuffData.Id);
            if (buff == null)
            {
                return;
            }

            unit.GetComponent<ClientBuffComponent>().Update(message.BuffData);
            //buff上信息的更新，各自根据更新的逻辑进行处理

            EventSystem.Instance.Publish
            (
                zoneScene,
                new BuffUpdate()
                {
                    unit = unit,
                    BuffConfigId = message.BuffData.ConfigId,
                    BuffId = message.BuffData.Id
                }
            );








            await ETTask.CompletedTask;
        }

    }


}
