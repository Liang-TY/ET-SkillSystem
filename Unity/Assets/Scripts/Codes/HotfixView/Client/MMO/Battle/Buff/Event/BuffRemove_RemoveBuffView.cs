using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class BuffRemove_RemoveBuffView : AEvent<BuffRemove>
    {
        protected override async ETTask Run(Scene scene, BuffRemove a)
        {
            await ETTask.CompletedTask;
        }
    }
}
