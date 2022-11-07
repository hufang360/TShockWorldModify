using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
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

            op.SendSuccessMessage($"全图随机开始……");
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
        public static void RandomTile(int x, int y)
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
            if (tile.active() && IDSet.matchBlockID.Contains(tile.type) && Mapping.ContainsKey(tile.type))
            {
                tile.type = (ushort)Mapping[tile.type];
                NetMessage.SendTileSquare(-1, x, y);
            }

            if (tile.wall != 0 && WallMapping.ContainsKey(tile.wall))
            {
                tile.wall = (ushort)WallMapping[tile.wall];
                NetMessage.SendTileSquare(-1, x, y);
            }
        }

        public static void ResetTileMapping()
        {
            Mapping = GetRandomBlockMapping();
            WallMapping = GetRandomWallMapping();
        }



        private static Dictionary<int, int> Mapping = new Dictionary<int, int>();
        private static Dictionary<int, int> WallMapping = new Dictionary<int, int>();

        private static Dictionary<int, int> GetRandomBlockMapping()
        {
            Dictionary<int, int> mapping = new Dictionary<int, int>();
            Random rng = new Random((int)DateTime.Now.Ticks);

            List<int> tiles = new List<int>(IDSet.randomBlockID);
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
    }
}