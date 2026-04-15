using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSAnimResComponent))]
    public static partial class LSAnimResComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimResComponent self)
        {
            self.Sprites = new Dictionary<int, Sprite>();
            self.Textures = new List<Texture2D>();
        }

        [EntitySystem]
        private static void Destroy(this LSAnimResComponent self)
        {
            self.Sprites.Clear();
            foreach (Texture2D tex in self.Textures)
            {
                UnityEngine.Object.Destroy(tex);
            }
            self.Textures.Clear();
        }

        public static async ETTask InitAsync(this LSAnimResComponent self)
        {
            Room room = self.Room();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            // string assetsName = $"Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab";
            // GameObject bundleGameObject = await 
            //         room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            // 1. Load and parse IMG sprite atlas
            string imgPath = "Packages/cn.etetet.lockstep/Bundles/AnimRes/bantuamazones.img.bytes";
            TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>(imgPath);
            Log.Info($"[LSAnimRes] IMG loaded, size: {imgAsset.bytes.Length} bytes");
            
            NpkSprite[] npkSprites = NpkImgParser.Parse(imgAsset.bytes);
            Log.Info($"[LSAnimRes] Parsed {npkSprites.Length} sprites from IMG");
            
            // 2. Convert each NpkSprite to Unity Texture2D + Sprite
            foreach (NpkSprite npkSprite in npkSprites)
            {
                if (npkSprite.ArgbData == null) continue;
            
                Texture2D tex = new Texture2D(npkSprite.Width, npkSprite.Height, TextureFormat.ARGB32, false);
                Color[] colors = new Color[npkSprite.ArgbData.Length];
                for (int i = 0; i < npkSprite.ArgbData.Length; i++)
                {
                    int argb = npkSprite.ArgbData[i];
                    colors[i] = new Color32(
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF),
                        (byte)((argb >> 24) & 0xFF)
                    );
                }
                tex.SetPixels(colors);
                tex.Apply();
            
                Sprite sprite = Sprite.Create(tex,
                    new Rect(0, 0, npkSprite.Width, npkSprite.Height),
                    new Vector2(0.5f, 0.5f), 100f);
            
                self.Sprites[npkSprite.Index] = sprite;
                self.Textures.Add(tex);
            }
            Log.Info($"[LSAnimRes] Created {self.Sprites.Count} Unity Sprites");

            // 3. Load and register animation configs
            string stayPath = "Packages/cn.etetet.lockstep/Bundles/AnimRes/stay.json";//Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab
            TextAsset stayAsset = await resLoader.LoadAssetAsync<TextAsset>(stayPath);
            AnimClipData stayData = JsonUtility.FromJson<AnimClipData>(stayAsset.text);
            AnimConfigRegistry.Register(AnimId.Idle, stayData);
            Log.Info($"[LSAnimRes] Idle config registered: {stayData.frames.Length} frames, loop={stayData.loop}, totalDuration={stayData.totalDuration}ms");

            string movePath = "Packages/cn.etetet.lockstep/Bundles/AnimRes/move.json";
            TextAsset moveAsset = await resLoader.LoadAssetAsync<TextAsset>(movePath);
            AnimClipData moveData = JsonUtility.FromJson<AnimClipData>(moveAsset.text);
            AnimConfigRegistry.Register(AnimId.Walk, moveData);
            Log.Info($"[LSAnimRes] Walk config registered: {moveData.frames.Length} frames, loop={moveData.loop}, totalDuration={moveData.totalDuration}ms");
        }

        public static Sprite GetSprite(this LSAnimResComponent self, int imgIndex)
        {
            self.Sprites.TryGetValue(imgIndex, out Sprite sprite);
            return sprite;
        }
    }
}
