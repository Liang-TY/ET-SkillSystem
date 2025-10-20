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
    public class M2C_BuffTickHandler : AMHandler<M2C_BuffTick>
    {
        protected override async ETTask Run(Session session, M2C_BuffTick message)
        {

            Scene zoneScene = session.ClientScene();
            Log.Console($"zone{zoneScene.Zone} ->  玩家 {message.UnitId} 触发了 BUFF tick({message.BuffId}) ");

            //buffTick，播放特效，动面等等的，例如流血动面，飘字之类的
            Unit unit = zoneScene.CurrentScene().GetComponent<UnitComponent>().Get(message.UnitId);


            if (unit == null)
            {
                return;
            }

            Buff buff = unit.GetComponent<BuffComponent>().Get(message.BuffId);
            if (buff == null)
            {
                return;
            }


            EventSystem.Instance.Publish
            (
                zoneScene,
                new BuffTick()
                {
                    unit = unit,
                    BuffId = message.BuffId
                }
            );


            await ETTask.CompletedTask;
        }

    }


}
