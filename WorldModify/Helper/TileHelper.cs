using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ObjectData;
using TShockAPI;

namespace WorldModify
{

    /// <summary>
    /// 图格辅助
    /// </summary>
    public class TileHelper
    {
        public static bool isTaskRunning { get; set; }

        /// <summary>
        /// 开始创建
        /// </summary>
        public static void StartGen() { isTaskRunning=true; }

        /// <summary>
        /// 完成创建
        /// </summary>
        public static void FinishGen()
        {
            isTaskRunning = false;
            TShock.Utils.SaveWorld();
        }

        /// <summary>
        /// 有任务正在执行
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool NeedWaitTask(TSPlayer op)
        {
            if (isTaskRunning)
            {
                if (op != null) op.SendErrorMessage("另一个创建任务正在执行，请稍后再操作");
            }
            return isTaskRunning;
        }

        /// <summary>
        /// 更新物块
        /// </summary>
        public static void InformPlayers()
        {
            foreach (TSPlayer person in TShock.Players)
            {
                if ((person != null) && (person.Active))
                {
                    for (int i = 0; i < 255; i++)
                    {
                        for (int j = 0; j < Main.maxSectionsX; j++)
                        {
                            for (int k = 0; k < Main.maxSectionsY; k++)
                            {
                                Netplay.Clients[i].TileSections[j, k] = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 建造完成（标记状态，刷新图格）
        /// </summary>
        public static void GenAfter()
        {
            InformPlayers();
            FinishGen();
        }

        /// <summary>
        /// 清理图格
        /// </summary>
        public static void ClearTile(int x, int y, int w = 1, int h = 1)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    ClearTile(x + i, y + j);
                }
            }
        }
        /// <summary>
        /// 清理图格
        /// </summary>
        public static void ClearTile(int x, int y)
        {
            Main.tile[x, y].ClearTile();
            NetMessage.SendTileSquare(-1, x, y);
        }

        /// <summary>
        /// 挖方块
        /// </summary>
        public static void KillTile(int x, int y)
        {
            Main.tile[x, y].ClearTile();
            WorldGen.SquareTileFrame(x, y);
            NetMessage.SendTileSquare(-1, x, y);
        }


        /// <summary>
        /// 清理液体
        /// </summary>
        public static void ClearLiquid(int x, int y)
        {
            ITile tile = Main.tile[x, y];
            tile.liquid = 0;
            NetMessage.SendTileSquare(-1, x, y);
        }


        /// <summary>
        /// 清除所有
        /// </summary>
        public static void ClearEverything(int x, int y)
        {
            Main.tile[x, y].ClearEverything();
            NetMessage.SendTileSquare(-1, x, y);
        }

        /// <summary>
        /// 设置图格id
        /// </summary>
        public static void SetType(int x, int y, int id)
        {
            Main.tile[x, y].type = (ushort)id;
            Main.tile[x, y].active(true);
            Main.tile[x, y].slope(0);
            Main.tile[x, y].halfBrick(false);
        }
        /// <summary>
        /// 设置图格id
        /// </summary>
        public static void SetType(ITile tile, int id)
        {
            tile.type = (ushort)id;
            tile.active(true);
            tile.slope(0);
            tile.halfBrick(false);
        }

        public static Point GetTileWH(int id, int style = 0)
        {
            utils.Log($"GetTileWH:{id}");
            TileObjectData tileData = TileObjectData.GetTileData(id, style);
            return new Point(tileData.Width, tileData.Height);
        }

    }
}
