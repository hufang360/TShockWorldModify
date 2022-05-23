using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace WorldModify
{
    public class RetileHelper
    {
        private static RetileConfig _config;
        public static string SaveFile;

        public static void Init() { Reload(); }
        public static void Reload() { _config = RetileConfig.Load(SaveFile); }
        public static void Save() { File.WriteAllText(SaveFile, JsonConvert.SerializeObject(_config, Formatting.Indented)); }
        public static RetileConfig Con { get { return _config; } }
    }

    public class RetileConfig
    {
        public List<TileReInfo> replace = new List<TileReInfo>();

        public static RetileConfig Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<RetileConfig>(File.ReadAllText(path), new JsonSerializerSettings()
                {
                    Error = (sender, error) => error.ErrorContext.Handled = true
                });
            }
            else
            {
                var c = new RetileConfig();
                c.InitDefault();
                File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }));
                return c;
            }
        }

        public void InitDefault()
        {
            // 方块
            replace.Add(new TileReInfo(TileID.Dirt, TileID.Sand, 0, 0, "土块 → 沙块"));
            replace.Add(new TileReInfo(TileID.Grass, TileID.Sand, 0, 0, "草块 → 沙块"));
            replace.Add(new TileReInfo(TileID.ClayBlock, TileID.HardenedSand, 0, 0, "粘土块 → 硬化沙块"));
            replace.Add(new TileReInfo(TileID.Stone, TileID.Sandstone, 0, 0, "石块 → 沙岩块"));

            //replace.Add(new TileInfo2(TileID.Trees, TileID.PalmTree));  // 树 → 棕榈树
            //replace.Add(new TileInfo2(TileID.Sunflower, TileID.Cactus));  // 向日葵 → 仙人掌
            replace.Add(new TileReInfo(TileID.Plants, TileID.SeaOats, 0, 0, "高茎草 → 海燕麦"));
            replace.Add(new TileReInfo(TileID.Plants2, TileID.SeaOats, 0, 0, "高茎草 → 海燕麦"));

            replace.Add(new TileReInfo(TileID.Mud, TileID.Sand, 0, 0, "泥块 → 沙块"));
            replace.Add(new TileReInfo(TileID.JungleGrass, TileID.Sand, 0, 0, "丛林草块 → 沙块"));

            // 墙
            replace.Add(new TileReInfo(WallID.Dirt, WallID.Sandstone, 1, 1, "土墙 → 沙岩墙"));
            replace.Add(new TileReInfo(WallID.DirtUnsafe, WallID.Sandstone, 1, 1));
            replace.Add(new TileReInfo(WallID.Stone, WallID.Sandstone, 1, 1, "石墙 → 沙岩墙"));
            replace.Add(new TileReInfo(WallID.GrassUnsafe, WallID.SmoothSandstone, 1, 1));
            replace.Add(new TileReInfo(WallID.Grass, WallID.SmoothSandstone, 1, 1));

            // 液体
            replace.Add(new TileReInfo(1, TileID.BreakableIce, 2, 0, "水 → 薄冰"));
        }
    }

}
