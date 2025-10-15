using dnlib.DotNet;
using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [Event(SceneType.Map)]
    public class BuffTimeOut_RemoveBuff : AEvent<BuffTimeOut>
    {
        protected override async ETTask Run(Scene scene, BuffTimeOut a)
        {
            a.Unit?.GetComponent<BuffComponent>()?.Remove(a.BuffId);
            await ETTask.CompletedTask;
        }

    }


}
