  不需要 Entity 的 — 纯数据容器                                                                                          
   ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
  像 AnimClipData 这种，只是从 JSON 反序列化出来的数据，不需要框架管理生命周期，不需要                                    Update，不参与帧同步序列化。用普通 class/struct 就行：

  // 就是纯数据，够用了
  [Serializable]
  public class AnimClipData
  {
      public bool loop;
      public AnimFrameData[] frames;
  }

  [Serializable]
  public struct AnimFrameData
  {
      public int index;
      public int delay;
  }

  需要 Entity 的 — 有生命周期、需要框架管理

  如果这个数据需要在 ECS 中存活、响应事件、每帧更新、或者参与帧同步快照/回滚，才需要定义成
  Entity/Component。比如游戏中的一个单位：

  [ComponentOf(typeof(LSUnit))]
  public class LSUnitComponent : LSEntity, IAwake, IDestroy
  {
      // 需要帧同步序列化的状态
  }

  多层嵌套的 JSON 怎么处理

  方案一：还是用普通数据类嵌套

  public class SkillData
  {
      public string name;
      public SkillEffectData[] effects;   // 嵌套
  }

  public class SkillEffectData
  {
      public int damage;
      public BuffData buff;               // 再嵌套
  }

  JSON 反序列化一次就拿到完整结构，不需要每一层都是 Entity。

  方案二：只有最外层是 Entity，内部数据用普通类

  [ComponentOf(typeof(LSUnit))]
  public class LSSkillComponent : LSEntity, IAwake
  {
      public Dictionary<int, SkillData> Skills; // SkillData 是普通类
  }

  方案三：真的需要每层都是 Entity

  只有当嵌套的每一层都独立需要 ECS 生命周期管理时（比如独立的 buff 需要自己的 Update 倒计时），才用 Entity 的父子关系：

  LSUnit (Entity)
    └── LSBuffComponent (Entity, 管理所有 buff)
          ├── LSBuff (Child Entity, 有自己的 ILSUpdate)
          └── LSBuff (Child Entity, 有自己的 ILSUpdate)

  判断标准

  ┌─────────────────────────────┬───────────────────┬──────────────────┐
  │                             │ 普通 class/struct │ Entity/Component │
  ├─────────────────────────────┼───────────────────┼──────────────────┤
  │ 只是存数据                  │ 用                │ 不用             │
  ├─────────────────────────────┼───────────────────┼──────────────────┤
  │ 需要 Awake/Destroy 生命周期 │ 不用              │ 用               │
  ├─────────────────────────────┼───────────────────┼──────────────────┤
  │ 需要 Update 每帧执行        │ 不用              │ 用               │
  ├─────────────────────────────┼───────────────────┼──────────────────┤
  │ 需要参与帧同步快照回滚      │ 不用              │ 用               │
  ├─────────────────────────────┼───────────────────┼──────────────────┤
  │ 需要发布/订阅事件           │ 不用              │ 用               │
  └─────────────────────────────┴───────────────────┴──────────────────┘

  简单原则：数据用 class，行为用 Entity。 大多数 JSON 配置表、动画数据、技能配置都只是数据，不需要变成 Entity。