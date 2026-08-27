using System;

namespace ET
{
    [EnableClass]
    [Serializable]
    public class AnimClipData
    {
        public bool loop;
        public int frameMax;
        public AnimFrameData[] frames;
        public int totalDuration;
    }

    [Serializable]
    public struct AnimFrameData
    {
        public int index;
        public AnimFrameImage image;
        public AnimFramePos imagePos;
        public int delay;
        public int graphicEffect;       // 0=无, 1=LINEARDODGE(加法混合发光)——视图层消费，逻辑层不用
        public int rgba;                // 帧染色 packed ARGB（0xAARRGGBB）；0 = 无染色（缺省）——视图层消费

        // 受击/身体盒（单数 = damageBoxes[0]，兼容旧 JSON/现有采样代码）
        public AnimBox damageBox;

        // 受击盒全量：DNF 一帧可有多个 [DAMAGE BOX]（实测文件均为 1 个；旧 JSON 无此字段 → null）
        public AnimBox[] damageBoxes;

        // 攻击盒：DNF 一帧可有多个 [ATTACK BOX]（如 kneekick 帧 1-3 各 2 个；无攻击帧 → null）
        public AnimBox[] attackBoxes;
    }

    [Serializable]
    public struct AnimFrameImage
    {
        public string path;          // 新版：完整 NPK 虚拟路径（sprite/character/...）；旧版：纯文件名
        public string pathLegacy;    // 旧版兼容：纯文件名（新版 JSON 有此字段，旧版无=空）
        public int index;
    }

    [Serializable]
    public struct AnimFramePos
    {
        public int x;
        public int y;
    }

    [Serializable]
    public struct AnimVec3
    {
        public int x;
        public int y;
        public int z;
    }

    [Serializable]
    public struct AnimBox
    {
        public AnimVec3 min;
        public AnimVec3 max;
    }
}
