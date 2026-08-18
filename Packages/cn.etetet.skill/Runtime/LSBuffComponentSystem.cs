using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Buff 容器系统（ET.Skill：要调 BuffLoader/ActionLoader，放 Hotfix 会循环依赖）。
    /// Route B：Just\* 标记本帧置 true，下帧 LSUpdate 开头清（本组件挂载顺序在 Hitbox 之前，
    /// 命中挂 Buff 设的标记都在清之后）。Removing 的 buff 存活一帧服务视图标记后回收。
    /// </summary>
    [EntitySystemOf(typeof(LSBuffComponent))]
    [LSEntitySystemOf(typeof(LSBuffComponent))]
    [FriendOf(typeof(LSBuffComponent))]
    [FriendOf(typeof(LSBuff))]
    public static partial class LSBuffComponentSystem
    {
        /// <summary>Buff 迭代复用缓冲（tick 动作可能增删 Buff，快照迭代防集合并发修改）</summary>
        [StaticField]
        private static readonly List<LSBuff> buffScratch = new();

        [EntitySystem]
        private static void Awake(this LSBuffComponent self)
        {
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSBuffComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();
            int frameNo = unit.LSWorld().Frame;

            // 1) 回收 Removing 的 buff（标记上帧已被视图读过）+ 清存活 buff 的上一帧标记
            buffScratch.Clear();
            foreach (var kv in self.Children)
            {
                if (kv.Value is LSBuff buff) buffScratch.Add(buff);
            }
            foreach (LSBuff buff in buffScratch)
            {
                if (buff.Removing)
                {
                    buff.Dispose();
                    continue;
                }
                buff.JustAdded = buff.JustRemoved = false;
            }

            // 2) 倒计时 + Tick + 到时移除（快照迭代：tick 动作可安全增删 Buff）
            for (int i = 0; i < buffScratch.Count; i++)
            {
                LSBuff buff = buffScratch[i];
                if (buff.Removing) continue;
                BuffDefinition def = BuffLoader.Get(buff.ConfigId);
                if (def == null) continue;

                if (def.TotalTimeMs > 0)
                {
                    buff.RemainingMs -= LSConstValue.UpdateInterval;
                    if (buff.RemainingMs <= 0)
                    {
                        self.RemoveBuff(buff, def, frameNo);
                        continue;
                    }
                }

                if (def.TickTimeMs > 0)
                {
                    buff.TickTimer += LSConstValue.UpdateInterval;
                    while (buff.TickTimer >= def.TickTimeMs && !buff.Removing)
                    {
                        buff.TickTimer -= def.TickTimeMs;
                        RunActions(unit, buff.SourceId, buff.ConfigId, def.TickActions, frameNo);
                    }
                }
            }
        }

        /// <summary>
        /// 挂 Buff。同 ConfigId 已存在 → 叠层简版：Stack+1 + 刷新时长（不重跑 AddActions，DNF 刷新型燃烧）。
        /// </summary>
        public static LSBuff AddBuff(this LSBuffComponent self, LSUnit source, int buffId)
        {
            BuffDefinition def = BuffLoader.Get(buffId);
            if (def == null)
            {
                Log.Error($"[Buff] 未注册的 buffId={buffId}，跳过");
                return null;
            }

            LSUnit unit = self.GetParent<LSUnit>();

            foreach (var kv in self.Children)
            {
                if (kv.Value is not LSBuff buff || buff.ConfigId != buffId || buff.Removing) continue;
                buff.Stack++;
                buff.RemainingMs = def.TotalTimeMs;   // 刷新型叠层
                buff.JustAdded = true;                 // Route B：视图可感知刷新
                return buff;
            }

            LSBuff newBuff = self.AddChild<LSBuff, int>(buffId);
            newBuff.SourceId = source != null ? source.Id : 0;
            newBuff.RemainingMs = def.TotalTimeMs;
            newBuff.TickTimer = 0;
            newBuff.Stack = 1;
            newBuff.JustAdded = true;

            RunActions(unit, newBuff.SourceId, buffId, def.AddActions, unit.LSWorld().Frame);
            return newBuff;
        }

        /// <summary>移除（外部驱散等）：跑 RemoveActions → 置标记 → 下帧回收</summary>
        public static void RemoveBuff(this LSBuffComponent self, LSBuff buff)
        {
            BuffDefinition def = BuffLoader.Get(buff.ConfigId);
            if (def != null)
            {
                self.RemoveBuff(buff, def, self.GetParent<LSUnit>().LSWorld().Frame);
            }
        }

        private static void RemoveBuff(this LSBuffComponent self, LSBuff buff, BuffDefinition def, int frameNo)
        {
            if (buff.Removing) return;
            buff.Removing = true;
            buff.JustRemoved = true;
            LSUnit unit = self.GetParent<LSUnit>();
            RunActions(unit, buff.SourceId, buff.ConfigId, def.RemoveActions, frameNo);
        }

        /// <summary>执行 action 列表（owner=buff 宿主，source=buff 来源单位）</summary>
        private static void RunActions(LSUnit owner, long sourceId, int buffId, int[] actionIds, int frameNo)
        {
            if (actionIds == null) return;
            LSUnit source = owner.LSWorld().GetComponent<LSUnitComponent>().GetChild<LSUnit>(sourceId);
            foreach (int actionId in actionIds)
            {
                LSAction action = ActionLoader.Get(actionId);
                if (action == null)
                {
                    Log.Error($"[Buff] buffId={buffId} 引用了未注册的 actionId={actionId}，跳过");
                    continue;
                }
                action.Run(new LSActionContext(owner.LSWorld(), owner, source, frameNo));
            }
        }
    }
}
