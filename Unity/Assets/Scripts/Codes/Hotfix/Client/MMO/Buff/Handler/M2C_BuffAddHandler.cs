using ET.EventType;
using ET.Server;
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

            if (unit == null)
            {
                return;
            }

            Buff buff = BuffFactory.Create(unit, message.BuffData);
            unit.GetComponent<BuffComponent>().Add(buff);
            //buff添加，状态记录到客户端buffcomponent，显示buff图标，信息，插放buff特效，等等的

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







            await ETTask.CompletedTask;
        }

    }


}
