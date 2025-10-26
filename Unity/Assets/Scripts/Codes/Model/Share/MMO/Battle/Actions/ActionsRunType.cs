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
        public const int CastBullet = 3;//创建子弹
        public const int MoveToTarget = 3;//向目标移动n米，无目标则向前移动
    }

}
