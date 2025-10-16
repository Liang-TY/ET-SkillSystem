
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public static class BattleHelper
    {
        /// <summary>
        /// 结算战斗
        /// 此处必须是同步的，不可改为异步，否则需求复杂之后，很可能会出问题
        /// 可以用协程，但不可把整个战斗计算改成异步
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="actions"></param>
        public static void CalcAttack(Unit attacker, Unit target, Actions actions)
        {
            // 这里应该是根据各个项目实际情况，攻击力，防御力之类的一堆公式计算，得出一个伤害值
            //现在简化为直接读取固定伤害
            long damage = long.Parse(actions.Config.Param[0]);
            NumericComponent numericComponent = target.GetComponent<NumericComponent>();
            long oldHp = numericComponent[NumericType.Hp];
            long tarHp = numericComponent[NumericType.Hp] + damage;
            numericComponent[NumericType.HpBase] = Math.Clamp(tarHp, 0, numericComponent[NumericType.MaxHp]);
            long newHp = numericComponent[NumericType.Hp];
            long res_damage = newHp - oldHp;


            //广播飘字
            if (res_damage != 0){
                MMOMessageHelper.SendClient(target, new M2C_BattleResult()
                {
                    AttackerId = attacker.Id,
                    TargetId = target.Id,
                    Damage = res_damage
                },NoticeClientType.Broadcast);
            }

            if (oldHp > 0 && newHp == 0)
            {
                //处理死亡逻辑
            }




        }

        // 击杀
        // 负责击杀双方相关的逻辑
        public static void Kill(Unit killer,Unit killed)
        {
            //此处击杀者有很多处理，例如红名，pk值，记录到被杀的仇恨列表，击杀排行榜记录等等的需求
        }


    }
}
