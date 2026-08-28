using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// NPK 加载器：启动时从 Bundles/ImagePacks2/ 加载所有 .npk.bytes 并挂载到 NpkMountManager。
    /// 替代旧管线的"逐个手工提取 .img.bytes → YooAsset 加载"。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class NpkLoaderComponent: Entity, IAwake, IDestroy
    {
        public NpkMountManager Manager = new NpkMountManager();
        public List<string> LoadedArchiveNames = new List<string>();
    }
}
