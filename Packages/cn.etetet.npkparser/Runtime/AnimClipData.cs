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
}
