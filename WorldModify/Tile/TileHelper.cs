using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TShockAPI;


namespace WorldModify
{
    class TileHelper
    {
        private static readonly Dictionary<string, FindData> FindList = new Dictionary<string, FindData>()
        {
            {"附魔剑", new FindData(187,5, 3,2, 918) },
            {"花苞", new FindData(238,-1, 2,2) },

            {"暗影珠", new FindData(31,0, 2,2, 0) },
            {"猩红之心", new FindData(31,1, 2,2, 36) },

            {"生命水晶", new FindData(12,-1, 2,2) },
            {"生命果", new FindData(236,-1, 2,2) },

            {"幼虫", new FindData(231,-1, 3,3) },
            {"丛林蜥蜴祭坛", new FindData(237,-1, 3,2) },

            {"地狱熔炉", new FindData(77,-1, 3,2) },
            {"提炼机", new FindData(219,-1, 3,3) },
            {"织布机", new FindData(86,-1, 3,2) },

            {"恶魔祭坛", new FindData(26,0, 3,2) },
            {"猩红祭坛", new FindData(26,1, 3,2) },
        };
        public static void FindCommand(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0)
            {
                op.SendErrorMessage($"请输入要查找的名字，例如：附魔剑，可供查找的有：\n{string.Join(", ", FindList.Keys)}");
                return;
            }

            ResetSkip();
            string keyw = args.Parameters[0].ToLowerInvariant();
            if (FindList.ContainsKey(keyw))
            {
                ListedExtra(op, keyw, FindList[keyw]);
            }
            else
            {
                op.SendErrorMessage($"要查找的名字不对 或 不支持，可供查找的有：\n{string.Join(", ", FindList.Keys)}");
            }
        }

        private static void ListedExtra(TSPlayer op, string opName, FindData fd)
        {
            ResetSkip();
            List<Point16> found = new List<Point16>();

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    ITile tile = Main.tile[x, y];
                    if (tile.active() && tile.type == fd.id)
                    {
                        if (GetItem(x, y, fd)) found.Add(new Point16(x, y));
                    }
                }
            }
            if (found.Count == 0)
            {
                op.SendInfoMessage($"未找到 {opName}！");
                return;
            }
            op.SendInfoMessage($"{opName} 查找结果（{found.Count}）:\n{string.Join(", ", found.GetRange(0, Math.Min(20, found.Count)))}\n输入 /tppos <x> <y> 进行传送");


        }
        private static bool GetItem(int tileX, int tileY, FindData fd)
        {
            ITile tile = Main.tile[tileX, tileY];
            int frameX = tile.frameX;
            int frameY = tile.frameY;

            bool check(int w, int h)
            {
                bool pass = true;
                for (int i = tileX; i < tileX + w; i++)
                {
                    for (int k = tileY; k < tileY + h; k++)
                    {
                        if (ContainsSkip(i, k) || !Main.tile[i, k].active() || Main.tile[i, k].type != fd.id)
                        {
                            pass = false;
                            break;
                        }
                    }
                }
                if (pass)
                {
                    skip.Add(new Rectangle(tileX, tileY, w, h));

                    //if (type == 187) utils.Log($"type={type}：{tileX},{tileY} frame：{frameX},{frameY}");
                }
                return pass;
            }

            bool flag = fd.frameX == -1 || (frameX == fd.frameX ? true : false);
            bool flag2 = fd.frameY == -1 || (frameY == fd.frameY ? true : false);
            if (flag && flag2 && check(fd.w, fd.h)) return true;
            return false;
        }
        private static List<Rectangle> skip = new List<Rectangle>();
        private static void ResetSkip() { skip.Clear(); }
        private static bool ContainsSkip(int x, int y)
        {
            foreach (Rectangle r in skip) { if (r.Contains(x, y)) return true; }
            return false;
        }

        #region 随机方块
        public static void RandomTile(int tileX, int tileY)
        {
            // TileID.GrayBrick
            // WallID.GrayBrick
            // TileID.Ore;
            // AllBlocksWithSmoothBordersToResolveHalfBlockIssue
            // Corrupt
            // Hallow
            // Crimson
            ITile tile = Main.tile[tileX, tileY];
            //Random rng = new Random((int)DateTime.Now.Ticks);
            //bool needSkip = rng.Next(10) < 2;
            if (tile.active() && matchBlockID.Contains(tile.type) && Mapping.ContainsKey(tile.type))
            {
                tile.type = (ushort)Mapping[tile.type];
            }

            if (tile.wall != 0 && WallMapping.ContainsKey(tile.wall))
            {
                tile.wall = (ushort)WallMapping[tile.wall];
            }
        }

        public static void ResetTileMapping()
        {
            Mapping = GetRandomBlockMapping();
            WallMapping = GetRandomWallMapping();
        }

        // 不含活火块
        public static readonly List<int> matchBlockID = new List<int>() { 0, 1, 2, 6, 7, 8, 9, 22, 23, 25, 30, 32, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 51, 52, 53, 54, 56, 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 69, 70, 75, 76, 107, 108, 109, 111, 112, 115, 116, 117, 118, 119, 120, 121, 122, 123, 130, 131, 140, 145, 146, 147, 148, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 202, 203, 204, 205, 206, 208, 211, 221, 222, 223, 224, 225, 226, 229, 230, 232, 234, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 273, 274, 284, 311, 312, 313, 321, 322, 325, 326, 327, 328, 329, 330, 331, 332, 333, 345, 346, 347, 348, 350, 351, 352, 357, 367, 368, 369, 370, 371, 379, 381, 382, 383, 384, 385, 396, 397, 398, 399, 400, 401, 402, 403, 404, 407, 408, 409, 415, 416, 417, 418, 426, 429, 430, 431, 432, 433, 434, 445, 446, 447, 448, 449, 450, 451, 458, 459, 460, 472, 473, 474, 477, 478, 479, 481, 482, 483, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 561, 562, 563, 566, 576, 577, 618 };
        public static readonly List<int> randomBlockID = new List<int>() { 0, 1, 2, 6, 7, 8, 9, 22, 23, 30, 32, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 51, 52, 53, 54, 56, 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 69, 70, 75, 76, 119, 120, 121, 122, 123, 124, 130, 131, 140, 145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 202, 204, 206, 208, 224, 225, 226, 229, 230, 232, 248, 249, 250, 251, 252, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 273, 274, 284, 311, 312, 313, 321, 322, 325, 326, 327, 328, 329, 345, 346, 347, 348, 350, 351, 357, 367, 368, 369, 370, 371, 379, 381, 382, 383, 384, 385, 396, 397, 404, 407, 415, 416, 417, 418, 426, 429, 430, 431, 432, 433, 434, 445, 446, 447, 448, 449, 450, 451, 458, 459, 460, 472, 473, 474, 477, 478, 479, 481, 482, 483, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 561, 562, 563, 566, 574, 575, 576, 577, 578, 618 };

        private static Dictionary<int, int> Mapping = new Dictionary<int, int>();
        private static Dictionary<int, int> WallMapping = new Dictionary<int, int>();

        private static Dictionary<int, int> GetRandomBlockMapping()
        {
            Dictionary<int, int> mapping = new Dictionary<int, int>();
            Random rng = new Random((int)DateTime.Now.Ticks);

            List<int> tiles = new List<int>(randomBlockID);
            List<int> shuffledTiles = new List<int>(tiles);

            int n = shuffledTiles.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = shuffledTiles[n];
                shuffledTiles[n] = shuffledTiles[k];
                shuffledTiles[k] = temp;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                mapping.Add(tiles[i], shuffledTiles[i]);
            }
            return mapping;
        }

        private static Dictionary<int, int> GetRandomWallMapping()
        {
            Dictionary<int, int> mapping = new Dictionary<int, int>();
            Random rng = new Random((int)DateTime.Now.Ticks);

            List<int> tiles = new List<int>();
            for (int i = 1; i < WallID.Count; i++)
            {
                tiles.Add(i);
            }

            List<int> shuffledTiles = new List<int>(tiles);

            int n = shuffledTiles.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = shuffledTiles[n];
                shuffledTiles[n] = shuffledTiles[k];
                shuffledTiles[k] = temp;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                mapping.Add(tiles[i], shuffledTiles[i]);
            }
            return mapping;
        }
        #endregion


        #region 图格替换


        public static void ReplaceTile(int tileX, int tileY, List<TileReInfo> replaceInfo = null)
        {
            ITile tile = Main.tile[tileX, tileY];
            IEnumerable<TileReInfo> query;

            if (replaceInfo == null) replaceInfo = RetileHelper.Con.replace;

            // 方块
            if (tile.active())
            {
                query = replaceInfo.Where(info => info.before.type == 0);
                query = query.Where(info => info.before.id == tile.type);
                foreach (TileReInfo info2 in query)
                {
                    RelaceBlock(tileX, tileY, info2);
                }
            }

            // 墙
            query = replaceInfo.Where(info => info.before.type == 1);
            query = query.Where(info => info.before.id == tile.wall);
            foreach (TileReInfo info in query)
            {
                if (info.after.type == 1) tile.wall = (ushort)info.after.id;
            }

            // 液体
            if (tile.liquid > 0)
            {
                int liquidType = tile.liquidType() + 1;
                query = replaceInfo.Where(res => res.before.type == 2);
                query = query.Where(info => info.before.id == liquidType);
                foreach (TileReInfo info in query)
                {
                    // 液体替换成物块
                    if (info.after.type == 0)
                    {
                        if (tile.active())
                        {
                            // 有的宝箱会泡在水里，只把水清除即可
                            tile.liquid = 0;
                        }
                        else
                        {
                            tile.type = (ushort)info.after.id;
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                            tile.liquid = 0;
                        }
                    }
                }
            }

        }
        // 替换方块
        private static void RelaceBlock(int tileX, int tileY, TileReInfo info2)
        {
            ITile tile = Main.tile[tileX, tileY];
            int atype = info2.after.type;
            int aid = info2.after.id;
            int astyle = info2.after.style;

            if (atype == 0)
            {
                if (aid == -1) // 清空图格
                    tile.ClearTile();

                //else if (aid == TileID.PalmTree)  // 棕榈树
                //{
                //    //WorldGen.GrowPalmTree(tileX, tileY);
                //    tile.type = (ushort)aid;
                //    tile.frameX = 66;
                //    tile.frameY = 0;
                //}
                //else if (aid == TileID.OasisPlants) // 绿洲植物
                //{
                //    // WorldGen.PlaceOasisPlant(tileX, tileY); // 无效
                //    int num = WorldGen.genRand.Next(9);
                //    int num2 = 0;
                //    short num3 = (short)(54 * num);
                //    short num4 = (short)(36 * num2);
                //    tile.type = (ushort)aid;
                //    tile.frameX = num3;
                //    tile.frameX = num4;
                //}
                //else if (aid >= 3 && astyle >= 0)  // 带style的图格
                //{
                //    WorldGen.PlaceTile(tileX, tileY, aid, false, true, 0, astyle);
                //    //tile.frameX += 18;
                //    //tile.frameX += 18;
                //}

                else // 直接替换
                {
                    tile.type = (ushort)aid;
                }
            }
        }
        #endregion


        #region 冰河化 / 冰融化
        public static void IceAgeTile(int tileX, int tileY)
        {
            // 薄冰 
            ITile tile = Main.tile[tileX, tileY];

            if (tile.liquid > 0)
            {
                switch (tile.liquidType())
                {
                    // 水
                    // Tile.Liquid_Water
                    case 0:
                        if (!tile.active())
                        {
                            tile.type = TileID.BreakableIce;
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                        }
                        tile.liquid = 0;
                        break;


                    // 岩浆
                    case 1: break;
                    // 蜂蜜
                    case 2: break;
                }
            }
        }


        public static void IceMeltTile(int tileX, int tileY)
        {
            ITile tile = Main.tile[tileX, tileY];
            if (tile.type != TileID.BreakableIce) return;

            tile.active(active: false);
            tile.liquid = byte.MaxValue;
            WorldGen.SquareTileFrame(tileX, tileY);
        }
        #endregion


    }
}