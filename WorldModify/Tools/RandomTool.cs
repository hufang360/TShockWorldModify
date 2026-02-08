using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 随机工具
    /// </summary>
    class RandomTool
    {
        /// <summary>
        /// 全图随机
        /// </summary>
        /// <param name="args"></param>
        public async static void RandomAll(CommandArgs args)
        {
            TSPlayer op = args.Player;
            await AsyncRandomArea(op, Utils.GetWorldArea());
        }

        /// <summary>
        /// 范围随机
        /// </summary>
        static Task AsyncRandomArea(TSPlayer op, Rectangle rect)
        {
            int secondLast = Utils.GetUnixTimestamp;

            op.SendSuccessMessage("全图随机开始……");
            return Task.Run(() =>
            {
                ResetTileMapping();
                for (int cx = rect.X; cx < rect.Right; cx++)
                {
                    for (int cy = rect.Y; cy < rect.Bottom; cy++)
                    {
                        RandomTile(cx, cy);
                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.FinishGen();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"随机完成（用时 {second}s）");
            });
        }

        //private static List<int> RandomSkip()
        //{
        //    var nums = Enumerable.Range(0, 300).ToArray();
        //    var rnd = new Random();

        //    // Shuffle the array
        //    for (int i = 0; i < nums.Length; ++i)
        //    {
        //        int randomIndex = rnd.Next(nums.Length);
        //        int temp = nums[randomIndex];
        //        nums[randomIndex] = nums[i];
        //        nums[i] = temp;
        //    }

        //    return new List<int>(nums.Skip(100));
        //}

        #region 随机方块
        public static int RandomTile(int x, int y)
        {
            // TileID.GrayBrick
            // WallID.GrayBrick
            // TileID.Ore;
            // AllBlocksWithSmoothBordersToResolveHalfBlockIssue
            // Corrupt
            // Hallow
            // Crimson
            ITile tile = Main.tile[x, y];
            //Random rng = new Random((int)DateTime.Now.Ticks);
            //bool needSkip = rng.Next(10) < 2;

            bool flag = false;
            if (tile.active() && matchBlockID.Contains(tile.type) && Mapping.ContainsKey(tile.type))
            {
                tile.type = (ushort)Mapping[tile.type];
                NetMessage.SendTileSquare(-1, x, y);

                flag = true;
            }

            if (tile.wall != 0 && WallMapping.ContainsKey(tile.wall))
            {
                tile.wall = (ushort)WallMapping[tile.wall];
                NetMessage.SendTileSquare(-1, x, y);

                flag = true;
            }

            return flag ? 1 : 0;
        }

        public static void ResetTileMapping()
        {
            Mapping = GetRandomBlockMapping();
            WallMapping = GetRandomWallMapping();
        }


        // 不含活火块
        public static readonly List<int> matchBlockID = new() { 0, 1, 2, 6, 7, 8, 9, 22, 23, 25, 30, 32, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 51, 52, 53, 54, 56, 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 69, 70, 75, 76, 107, 108, 109, 111, 112, 115, 116, 117, 118, 119, 120, 121, 122, 123, 130, 131, 140, 145, 146, 147, 148, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 202, 203, 204, 205, 206, 208, 211, 221, 222, 223, 224, 225, 226, 229, 230, 232, 234, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 273, 274, 284, 311, 312, 313, 321, 322, 325, 326, 327, 328, 329, 330, 331, 332, 333, 345, 346, 347, 348, 350, 351, 352, 357, 367, 368, 369, 370, 371, 379, 381, 382, 383, 384, 385, 396, 397, 398, 399, 400, 401, 402, 403, 404, 407, 408, 409, 415, 416, 417, 418, 426, 429, 430, 431, 432, 433, 434, 445, 446, 447, 448, 449, 450, 451, 458, 459, 460, 472, 473, 474, 477, 478, 479, 481, 482, 483, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 561, 562, 563, 566, 576, 577, 618, 666 };
        public static readonly List<int> randomBlockID = new() { 0, 1, 2, 6, 7, 8, 9, 22, 23, 30, 32, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 51, 52, 53, 54, 56, 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 69, 70, 75, 76, 119, 120, 121, 122, 123, 124, 130, 131, 140, 145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 202, 204, 206, 208, 224, 225, 226, 229, 230, 232, 248, 249, 250, 251, 252, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 273, 274, 284, 311, 312, 313, 321, 322, 325, 326, 327, 328, 329, 345, 346, 347, 348, 350, 351, 357, 367, 368, 369, 370, 371, 379, 381, 382, 383, 384, 385, 396, 397, 404, 407, 415, 416, 417, 418, 426, 429, 430, 431, 432, 433, 434, 445, 446, 447, 448, 449, 450, 451, 458, 459, 460, 472, 473, 474, 477, 478, 479, 481, 482, 483, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 561, 562, 563, 566, 574, 575, 576, 577, 578, 618, 666 };
        private static Dictionary<int, int> Mapping = new();
        private static Dictionary<int, int> WallMapping = new();
        private static Dictionary<int, int> GetRandomBlockMapping()
        {
            Dictionary<int, int> mapping = new();
            Random rng = new((int)DateTime.Now.Ticks);

            List<int> tiles = new(randomBlockID);
            List<int> shuffledTiles = new(randomBlockID);

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
            Dictionary<int, int> mapping = new();
            Random rng = new((int)DateTime.Now.Ticks);

            List<int> tiles = new();
            for (int i = 1; i < WallID.Count; i++)
            {
                tiles.Add(i);
            }

            List<int> shuffledTiles = new(tiles);

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
    }
}