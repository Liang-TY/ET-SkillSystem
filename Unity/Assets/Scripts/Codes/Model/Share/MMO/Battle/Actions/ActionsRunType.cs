using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public static class ActionsType
    {
        public const int NumericChange = 1;//改变目标数值，如果是Buff，删除会还原数值
        public const int Damage = 2;//造成伤害
    }

}
