using System.IO;

namespace YooAsset.Editor
{
    [DisplayName("定位地址: 相对路径（不含扩展名）")]
    public class AddressByFilePath : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            // CollectPath 下的相对路径，统一用 / 分隔，去掉 .bytes 后缀
            // 例：character/swordman/animation/stay.ani.bytes → character/swordman/animation/stay.ani
            string relativePath = data.AssetPath.Substring(data.CollectPath.Length)
                .TrimStart('/', '\\', ' ')
                .Replace('\\', '/');

            // 去掉 .bytes 后缀（保留 .ani/.als/.til 等原后缀）
            if (relativePath.EndsWith(".bytes"))
                relativePath = relativePath[..^".bytes".Length];

            return relativePath;
        }
    }
}
