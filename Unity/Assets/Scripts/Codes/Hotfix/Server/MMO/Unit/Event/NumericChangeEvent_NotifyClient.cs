using ET.EventType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [Event(SceneType.None)]
    public class NumericChangeEvent_NotifyClient:AEvent<ET.EventType.NumbericChange>
    {
        protected override async ETTask Run(Scene scene,NumbericChange a)
        {
            M2C_NumericChange m2CNumericChange = new M2C_NumericChange()
            {
                KV = new Dictionary<int, long>()
            };

            m2CNumericChange.UnitId = a.Unit.Id;
            m2CNumericChange.KV.Add(a.NumericType, a.New);
            MMOMessageHelper.SendClient(a.Unit, m2CNumericChange, NoticeClientType.Broadcast);
            await ETTask.CompletedTask;
        }
    }
}
