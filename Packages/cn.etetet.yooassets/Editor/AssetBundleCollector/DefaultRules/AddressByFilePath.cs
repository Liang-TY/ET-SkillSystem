using System.IO;

namespace YooAsset.Editor
{
    [DisplayName("定位地址: 相对路径（不含扩展名）")]
    public class AddressByFilePath : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            // 用 CollectPath 下的相对路径作为地址（去扩展名）
            // 例：character/swordman/animation/stay.ani.bytes → character/swordman/animation/stay.ani
            string relativePath = Path.GetRelativePath(data.CollectPath, data.AssetPath);
            return Path.ChangeExtension(relativePath, null);
        }
    }
}
