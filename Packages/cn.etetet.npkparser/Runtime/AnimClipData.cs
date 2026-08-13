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
        public AnimBox damageBox;       // 受击/身体盒（每帧都有，对应 JSON 已有的 damageBox）
        // attackBox（攻击盒）等 attack.json 接入时再加；现在 JSON 没有，JsonUtility 也不便处理 nullable
    }

    [Serializable]
    public struct AnimFrameImage
    {
        public string path;
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
