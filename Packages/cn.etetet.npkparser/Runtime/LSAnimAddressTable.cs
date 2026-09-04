using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 动画资源地址表：AnimId → 资源地址 + overlay/切片配置（纯数据，单一真源）。
    /// 运行时 LSAnimClipRegistrar 遍历本表做 YooAsset 异步加载注册；
    /// Editor（ET.Skill.Editor）读同一张表做预览资源定位与引用校验（ISSUE-013 方案 A1）。
    ///
    /// 约定：
    /// - 地址 = AnimRes 收集目录下相对路径（含扩展名），与 YooAsset AddressByFilePath 一致；
    /// - OverlayAliases = overlay 文件（.als/.json）里的特效别名 → AnimId，挂在 OwnerAnimId 上；
    /// - 切片动画（jump/wave）记录在 SlicedEntries：源地址 → 帧段列表。
    /// 新增动画：在此加一条 + AnimId 常量，运行时与 Editor 同时生效；
    /// 目录自动比对（探针 SkillAnimResProbe）会报"表引用了不存在的文件/目录里有但表未引用"。
    /// </summary>
    public static class LSAnimAddressTable
    {
        public sealed class OverlayDef
        {
            public string File;
            public Dictionary<string, int> Aliases;
        }

        public sealed class Entry
        {
            public int AnimId;
            public string Address;
            /// <summary>挂在本条目 AnimId 上的 overlay（至多一个）。</summary>
            public OverlayDef Overlay;
        }

        public sealed class SliceDef
        {
            public string SourceAddress;
            public (int start, int end, int animId)[] Segments;
        }

        [StaticField]
        private static readonly List<Entry> entryList = BuildEntries();

        /// <summary>只读条目（AnimId 唯一）。</summary>
        public static IReadOnlyList<Entry> Entries => entryList;

        [StaticField]
        private static readonly List<SliceDef> slicedEntries = new()
        {
            // jump.ani：两个 10000ms 悬停帧（DNF 物理切帧哨兵）按悬停帧切 2 段，运行时由跳跃物理驱动切换
            new()
            {
                SourceAddress = "character/swordman/animation/jump.ani",
                Segments = new (int, int, int)[]
                {
                    (0, 6, AnimId.JumpUp),      // F0-F6 起跳段 600ms
                    (8, 13, AnimId.JumpFall),   // F8-F13 下落段 520ms（F7/F14 悬停帧丢弃）
                },
            },
            // wave.ani：邪光斩施法 = 切片 F1-F8（F0 10000ms 蓄势哨兵帧丢弃）
            new()
            {
                SourceAddress = "character/swordman/animation/wave.ani",
                Segments = new (int, int, int)[]
                {
                    (1, 8, AnimId.SwordmanWaveCast),   // F1-F8 挥剑 500ms
                },
            },
        };

        /// <summary>切片源地址 → 帧段（运行时 RegisterSliced 与 Editor 显示共用）。</summary>
        public static IReadOnlyList<SliceDef> SlicedEntries => slicedEntries;

        /// <summary>按 AnimId 查条目（Editor 引用跳转用）；无则 null。</summary>
        public static Entry Find(int animId)
        {
            foreach (Entry entry in entryList)
            {
                if (entry.AnimId == animId) return entry;
            }
            return null;
        }

        /// <summary>本表引用的全部资源地址（含 overlay 文件与切片源，去重）——目录比对用。</summary>
        public static HashSet<string> CollectReferencedAddresses()
        {
            HashSet<string> addresses = new();
            foreach (Entry entry in entryList)
            {
                addresses.Add(entry.Address);
                if (entry.Overlay != null) addresses.Add(entry.Overlay.File);
            }
            foreach (SliceDef sliced in slicedEntries)
                addresses.Add(sliced.SourceAddress);
            return addresses;
        }

        // ── 数据本体（与原 LSAnimClipRegistrar.RegisterAll 逐条等价转录）──

        private const string A = "character/swordman/animation";
        private const string E = "character/swordman/effect/animation/bloodboom";
        private const string P = "passiveobject/character/swordman/animation";
        private const string M = "monster/event/bluemarble/bantuamazones/baanimation";

        private static List<Entry> BuildEntries()
        {
            List<Entry> list = new();

            void One(int animId, string address, OverlayDef overlay = null)
                => list.Add(new Entry { AnimId = animId, Address = address, Overlay = overlay });

            // 切片产物（AnimId 属于 SlicedEntries 的 Segment）挂 overlay：无本体地址，只带 overlay
            void OverlayOn(int animId, OverlayDef overlay)
                => list.Add(new Entry { AnimId = animId, Address = null, Overlay = overlay });

            OverlayDef O(string file, Dictionary<string, int> aliases)
                => new OverlayDef { File = file, Aliases = aliases };

            // 怪物动画（bantuamazones）
            One(AnimId.Idle, $"{M}/stay.ani");
            One(AnimId.Walk, $"{M}/move.ani");
            One(AnimId.Attack1, $"{M}/kneekick.ani");
            One(AnimId.Hurt, $"{M}/damage.ani");
            One(AnimId.MonsterLowKick, $"{M}/lowkick.ani");
            One(AnimId.MonsterHighKick, $"{M}/highkick.ani");
            One(AnimId.MonsterIceBreath, $"{M}/icebreath.ani");
            One(AnimId.MonsterDown, $"{M}/down.ani");

            // 鬼剑士动画
            One(AnimId.SwordmanIdle, $"{A}/stay.ani");
            One(AnimId.SwordmanWalk, $"{A}/move.ani");
            One(AnimId.SwordmanAttack1, $"{A}/attack1.ani");
            One(AnimId.SwordmanAttack2, $"{A}/attack2.ani");
            One(AnimId.SwordmanAttack3, $"{A}/attack3.ani");
            One(AnimId.SwordmanHurt, $"{A}/damage1.ani");
            One(AnimId.SwordmanBloodboom, $"{A}/bloodboom.ani",
                O($"{A}/bloodboom.ani.als", new Dictionary<string, int>
                {
                    ["casting_bloodboom_casting_back"] = AnimId.BloodboomCastingBack,
                    ["casting_bloodboom_casting"] = AnimId.BloodboomCasting,
                }));
            One(AnimId.HardAttack, $"{A}/hardattack.ani",
                O("character/swordman/effect/animation/hardattack/hardattack_blade_overlay.json",
                    new Dictionary<string, int>
                    {
                        ["hardattack1"] = AnimId.HardAttackBlade1,
                        ["hardattack2"] = AnimId.HardAttackBlade2,
                    }));

            // 上挑（F2/F3 自带攻击盒 → 帧驱动自动激活；刀光走官方 .als overlay）
            One(AnimId.SwordmanUpAttack, $"{A}/up_attack.ani",
                O($"{A}/up_attack.ani.als", new Dictionary<string, int> { ["sub2"] = AnimId.UpperslashFx }));
            One(AnimId.UpperslashFx, "character/swordman/effect/animation/upperslash1.ani");

            // 三段斩（5 段动画，当前用 1-3；无攻击盒 → 技能手动盒；弧光/扬尘走手组装 overlay）
            string tsFx = "character/swordman/effect/animation/tripleslash";
            for (int i = 1; i <= 5; i++)
            {
                int i1 = i;
                One(AnimId.SwordmanTripleSlash1 + i1 - 1, $"{A}/tripleslash{i1}.ani",
                    O($"{A}/tripleslash{i1}_overlay.json", new Dictionary<string, int>
                    {
                        [$"slash{i1}"] = AnimId.TripleSlashFx1 + i1 - 1,
                        ["move1"] = AnimId.TripleSlashMoveDust1,
                        ["move2"] = AnimId.TripleSlashMoveDust2,
                    }));
            }
            for (int i = 1; i <= 5; i++)
                One(AnimId.TripleSlashFx1 + i - 1, $"{tsFx}/slash{i}.ani");
            One(AnimId.TripleSlashMoveDust1, $"{tsFx}/move1.ani");
            One(AnimId.TripleSlashMoveDust2, $"{tsFx}/move2.ani");

            // 连突刺（本体 F2-F6 自带攻击盒；剑气弹视图）
            One(AnimId.SwordmanDashAttackMultiHit, $"{A}/dashattackmultihit.ani");
            One(AnimId.ThrustBeam, $"{P}/dashattackmultihitsub.ani");

            // 银光落刃（下落刺击本体 + 落地冲击波环/尘土）
            One(AnimId.SwordmanJumpAttack, $"{A}/jumpattack.ani");
            One(AnimId.AshenForkSubRing, $"{P}/ashenforksub.ani");
            One(AnimId.AshenForkSubDust, $"{P}/ashenforksubdust.ani");

            // ---- 第 2 批 ----

            // 空中连斩（融入普攻：空中 X 首击复用 JumpAttack，链斩交替）
            One(AnimId.JumpAttackMultiSlash1, $"{A}/jumpattackmultislash1.ani");
            One(AnimId.JumpAttackMultiSlash2, $"{A}/jumpattackmultislash2.ani");

            // 崩山击（蓄力→前跃下砸 F3-F5 自带盒→落地冲击波）
            One(AnimId.SwordmanHopSmashReady, $"{A}/hopsmashready.ani");
            One(AnimId.SwordmanHopSmash, $"{A}/hopsmash.ani");
            One(AnimId.HopSmashWaveFront, $"{P}/hopsmashsubfront1.ani");
            One(AnimId.HopSmashWaveGlow, $"{P}/hopsmashsubfront2.ani");

            // 裂波斩（上斩→旋转→波轮多段→终结；波轮 F13 悬停帧已钳 80ms）
            One(AnimId.SwordmanVaneSlashTry, $"{A}/vaneslashtry.ani");
            One(AnimId.SwordmanVaneSlash, $"{A}/vaneslash.ani");
            One(AnimId.VaneSlashWheel, $"{P}/vaneslashwheel.ani",
                O($"{P}/vaneslashwheel.ani.als", new Dictionary<string, int>
                {
                    ["light_GrandWave1"] = AnimId.GrandWaveLight1,
                    ["light_GrandWave_light"] = AnimId.GrandWaveLight,
                }));
            One(AnimId.VaneSlashNormal, $"{P}/vaneslashnormal.ani");

            // 十字斩（两刀+血之十字两相位+追击）
            One(AnimId.SwordmanGoreCross, $"{A}/gorecross.ani");
            One(AnimId.GoreCrossFlash, $"{P}/gorecross1.ani");
            One(AnimId.GoreCrossCross, $"{P}/gorecross2.ani");
            One(AnimId.GoreCross3Cross, $"{P}/gorecross3.ani");
            One(AnimId.GoreCross3CrossFade, $"{P}/gorecross4.ani");

            // 里·鬼剑术（太刀 4 段，json 自带攻击盒；刀光走 .als overlay——含 followParent 新节）
            string wc = "character/swordman/effect/animation/weaponcombo";
            One(AnimId.KatananewFx11, $"{wc}/katananew_1_1.ani");
            One(AnimId.KatananewFx12, $"{wc}/katananew_1_2.ani");
            One(AnimId.KatananewFx1m1, $"{wc}/katananew_1-1.ani");
            One(AnimId.UraKatanaEff, $"{wc}/ura_katana_eff.ani");
            One(AnimId.KatananewFx21, $"{wc}/katananew_2_1.ani");
            One(AnimId.KatananewFx22, $"{wc}/katananew_2_2.ani");
            One(AnimId.KatananewFx2m1, $"{wc}/katananew_2-1.ani");
            One(AnimId.KatananewFx2m2, $"{wc}/katananew_2-2.ani");
            One(AnimId.KatanaNew1Under, $"{wc}/katana_new1_under_effect.ani");
            One(AnimId.KatanaNew1Upper, $"{wc}/katana_new1_upper_effect.ani");
            One(AnimId.KatananewFx31, $"{wc}/katananew_3_1.ani");
            One(AnimId.KatananewFx32, $"{wc}/katananew_3_2.ani");
            One(AnimId.KatananewFx3m1, $"{wc}/katananew_3-1.ani");
            One(AnimId.KatananewFx3m2, $"{wc}/katananew_3-2.ani");
            One(AnimId.KatanaNew2Under, $"{wc}/katana_new2_under_effect.ani");
            One(AnimId.KatanaNew2Upper, $"{wc}/katana_new2_upper_effect.ani");
            One(AnimId.SwordmanWeaponComboBlade1, $"{A}/weaponcomboblade1.ani",
                O($"{A}/weaponcomboblade1.ani.als", new Dictionary<string, int>
                {
                    ["katananew_1_1"] = AnimId.KatananewFx11,
                    ["katananew_1_2"] = AnimId.KatananewFx12,
                    ["katananew_1-1"] = AnimId.KatananewFx1m1,
                    ["ura_katana_eff"] = AnimId.UraKatanaEff,
                }));
            One(AnimId.SwordmanWeaponComboBlade2, $"{A}/weaponcomboblade2.ani",
                O($"{A}/weaponcomboblade2.ani.als", new Dictionary<string, int>
                {
                    ["katananew_2_1"] = AnimId.KatananewFx21,
                    ["katananew_2_2"] = AnimId.KatananewFx22,
                    ["katananew_2-1"] = AnimId.KatananewFx2m1,
                    ["katananew_2-2"] = AnimId.KatananewFx2m2,
                    ["katana_new1_under_effect"] = AnimId.KatanaNew1Under,
                    ["katana_new1_upper_effect"] = AnimId.KatanaNew1Upper,
                }));
            One(AnimId.SwordmanWeaponComboBlade3, $"{A}/weaponcomboblade3.ani",
                O($"{A}/weaponcomboblade3.ani.als", new Dictionary<string, int>
                {
                    ["katananew_1_1"] = AnimId.KatananewFx11,
                    ["katananew_1_2"] = AnimId.KatananewFx12,
                    ["katananew_1-1"] = AnimId.KatananewFx1m1,
                    ["ura_katana_eff"] = AnimId.UraKatanaEff,
                }));
            One(AnimId.SwordmanWeaponComboBlade4, $"{A}/weaponcomboblade4.ani",
                O($"{A}/weaponcomboblade4.ani.als", new Dictionary<string, int>
                {
                    ["katananew_3_1"] = AnimId.KatananewFx31,
                    ["katananew_3_2"] = AnimId.KatananewFx32,
                    ["katananew_3-1"] = AnimId.KatananewFx3m1,
                    ["katananew_3-2"] = AnimId.KatananewFx3m2,
                    ["katana_new2_under_effect"] = AnimId.KatanaNew2Under,
                    ["katana_new2_upper_effect"] = AnimId.KatanaNew2Upper,
                }));

            // ---- 第 3 批 ----

            // 月光斩（三段 json 自带盒 → 帧驱动；月牙/满月斩光手组装 overlay）
            string mf = "character/swordman/effect/animation";
            One(AnimId.SwordmanMoonlightSlash1, $"{A}/moonlightslash1.ani",
                O($"{A}/moonlightslash1_overlay.json", new Dictionary<string, int>
                {
                    ["moonlightfx1"] = AnimId.MoonlightSlashFx1,
                }));
            One(AnimId.SwordmanMoonlightSlash2, $"{A}/moonlightslash2.ani",
                O($"{A}/moonlightslash2_overlay.json", new Dictionary<string, int>
                {
                    ["moonlightfx2"] = AnimId.MoonlightSlashFx2,
                }));
            One(AnimId.SwordmanMoonlightSlashFull, $"{A}/moonlightslashfull.ani",
                O($"{A}/moonlightslashfull_overlay.json", new Dictionary<string, int>
                {
                    ["moonlightfxfull"] = AnimId.MoonlightSlashFxFull,
                }));
            One(AnimId.MoonlightSlashFx1, $"{mf}/moonlightslashfx1.ani");
            One(AnimId.MoonlightSlashFx2, $"{mf}/moonlightslashfx2.ani");
            One(AnimId.MoonlightSlashFxFull, $"{mf}/moonlightslashfxfull.ani");

            // 邪光斩（施法 = wave.ani 切片 F1-F8，见 SlicedEntries；overlay 挂切片产物上）
            One(AnimId.GrandWaveWheel, $"{P}/grandwavewheel.ani",
                O($"{P}/grandwavewheel.ani.als", new Dictionary<string, int>
                {
                    ["light_GrandWave1"] = AnimId.GrandWaveLight1,
                    ["light_GrandWave_light"] = AnimId.GrandWaveLight,
                }));
            One(AnimId.GrandWaveLight, $"{P}/grandwave_GrandWave_light_GrandWave_light.ani");
            One(AnimId.GrandWaveLight1, $"{P}/grandwave_GrandWave_light_GrandWave1.ani");
            One(AnimId.GrandWaveFx, $"{mf}/grandwavefx.ani");
            // 切片产物的 overlay 单独登记（条目 AnimId 与切片目标重合，运行时按 OverlayOn 处理）
            OverlayOn(AnimId.SwordmanWaveCast, O($"{A}/wave_overlay.json", new Dictionary<string, int>
            {
                ["grandwavefx"] = AnimId.GrandWaveFx,
            }));

            // 拔刀斩（F0=500 蓄势原帧直用；大波视觉区）
            One(AnimId.SwordmanMomentarySlash, $"{A}/momentaryslash.ani");
            One(AnimId.MomentarySlashWave, $"{P}/momentaryslashwave.ani");
            One(AnimId.MomentarySlashWaveB, $"{P}/momentaryslashwaveb.ani");

            // 破军升龙击（冲撞/上挑 json 自带盒；上挑弧光 = 上挑区视觉）
            One(AnimId.SwordmanChargeCrashDash, $"{A}/chargecrashdash.ani");
            One(AnimId.SwordmanChargeCrashUpper, $"{A}/chargecrashupper.ani");
            One(AnimId.ChargeCrashUpSlash, $"{mf}/chargecrashupslash.ani");
            One(AnimId.ChargeCrashSubBack, $"{P}/chargecrashsubback.ani");

            // 怒气爆发（血柱全套：前段 + 主血柱 .als 挂 blood1-8 + 内圈宽柱）
            One(AnimId.BlastBloodPre, $"{P}/blastbloodpresubback.ani");
            One(AnimId.BlastBloodPreFront, $"{P}/blastbloodpresubfront.ani");
            One(AnimId.BlastBlood1, $"{P}/blastblood1.ani",
                O($"{P}/blastblood1.ani.als", new Dictionary<string, int>
                {
                    ["blood1"] = AnimId.BlastBloodBlood1,
                    ["blood2"] = AnimId.BlastBloodBlood2,
                    ["blood3"] = AnimId.BlastBloodBlood3,
                    ["blood4"] = AnimId.BlastBloodBlood4,
                    ["blood5"] = AnimId.BlastBloodBlood5,
                    ["blood6"] = AnimId.BlastBloodBlood6,
                    ["blood7"] = AnimId.BlastBloodBlood7,
                    ["blood8"] = AnimId.BlastBloodBlood8,
                    ["floor_over"] = AnimId.BlastBloodFloorOver,
                    ["blast_blood_light"] = AnimId.BlastBloodLight,
                }));
            One(AnimId.BlastBloodCore, $"{P}/blastbloodsub.ani");
            for (int i = 1; i <= 8; i++)
                One(AnimId.BlastBloodBlood1 + i - 1, $"{P}/blastblood_blood{i}.ani");
            One(AnimId.BlastBloodFloorOver, $"{P}/blastblood_floor_over.ani");
            One(AnimId.BlastBloodLight, $"{P}/blastblood_blast_blood_light.ani");

            // 鬼斩刀光特效（手组装 overlay；overlay 已并入 HardAttack 条目）
            string ha = "character/swordman/effect/animation/hardattack";
            One(AnimId.HardAttackBlade1, $"{ha}/hardattack1.ani");
            One(AnimId.HardAttackBlade2, $"{ha}/hardattack2.ani");

            // 浴血之怒特效
            One(AnimId.BloodboomCasting, $"{E}/boom1_bloodboom_casting.ani");
            One(AnimId.BloodboomCastingBack, $"{E}/boom1_bloodboom_casting_back.ani");
            One(AnimId.BloodboomBoomFront, $"{E}/boom1_bloodboom_boomfront.ani");
            One(AnimId.BloodboomBoomBack, $"{E}/boom1_bloodboom_boomback.ani");

            // 波动剑
            One(AnimId.NormalWave, $"{P}/normalwave.ani");

            // 冰息弹（手组装，保留旧 JSON，地址仍用文件名）
            One(AnimId.IceBreathBullet1, "icebreath_bullet1.json",
                O("icebreath_bullet_overlay.json", new Dictionary<string, int>
                {
                    ["icebreath2"] = AnimId.IceBreathBullet2,
                }));
            One(AnimId.IceBreathBullet2, "icebreath_bullet2.json");

            return list;
        }
    }
}
