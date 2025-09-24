using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public struct CastActionTimes
    {
        public int Index;
        public bool IsSelfHit;
    }
    public partial class CastConfig
    {
        public List<int> Times = new List<int>();
        public MultiMap<int , CastActionTimes> TimesDict = new MultiMap<int , CastActionTimes>();

        public override void AfterEndInit()
        {
            for (int i = 0; i < this.SelfHitActionTimes.Length; i++)
            {
                int time = this.SelfHitActionTimes[i];
                if (!this.Times.Contains(time))
                {
                    this.Times.Add(time);
                }
                this.TimesDict.Add(time,new CastActionTimes() { Index = i,IsSelfHit = true});
            }
            this.Times.Sort();
        }
    }
}
