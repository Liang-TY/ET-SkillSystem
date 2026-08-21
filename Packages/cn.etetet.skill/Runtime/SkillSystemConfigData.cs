namespace ET
{
    /// <summary>skillconfig.json 的数据结构（JsonUtility 反序列化用——字段名与 json 键精确对应）。</summary>
    [System.Serializable]
    public class SkillSystemConfigData
    {
        public bool hitFlashEnabled;
        public bool screenShakeEnabled;
        public bool debugDrawHitbox;
        public int rngSeed;
    }
}
