using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
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
        public static void StartGen() { isTaskRunning = true; }

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

        /// <summary>
        /// 是否为晶塔
        /// </summary>
        /// <param name="op"></param>
        /// <param name="tileID"></param>
        /// <returns></returns>
        public static bool IsPylon(TSPlayer op, int tileID)
        {
            if (tileID == TileID.TeleportationPylon)
            {
                op.SendInfoMessage("目前无法安全清除晶塔，请进游戏用镐子进行操作！");
                return true;
            }
            return false;
        }



        #region 图格数据查询
        public static Dictionary<int, TileProp> Tiles = new();
        /// <summary>
        /// 加载内嵌的图格配置文件（.csv）
        /// </summary>
        public static void LoadTile()
        {
            if (Tiles.Any()) return;

            // csv文件列头
            // id,name,w,h,isFrame,color
            foreach (string line in Utils.FromEmbeddedPath("Tile.csv").Split('\n').Skip(2))
            {
                var arr = line.Split(',');
                int id = int.Parse(arr[0]);
                Tiles.Add(id, new TileProp
                {
                    id = id,
                    name = arr[1],
                    w = int.Parse(arr[2]),
                    h = int.Parse(arr[3]),
                    isFrame = arr[4] == "1",
                    color = arr[5],
                });
            }
        }

        /// <summary>
        /// 获得图格属性
        /// </summary>
        /// <param name="idOrName"></param>
        /// <returns>查找失败时返回 null</returns>
        public static TileProp GetTileByIDOrName(string idOrName)
        {
            LoadTile();
            if (!int.TryParse(idOrName, out int id))
            {
                // 匹配大小写
                idOrName = Mapping.UpperTileName(idOrName);

                foreach (var w in Tiles.Values)
                {
                    if (w.name == idOrName)
                    {
                        id = w.id;
                        break;
                    }
                }
            }

            if (id == 0)
            {
                if (idOrName == "0" || idOrName == Tiles[0].name)
                {
                    return Tiles[id];
                }
                else
                {
                    // 匹配已知的多样式图格（id）
                    if (WMFindTool.FindList.ContainsKey(idOrName))
                    {
                        id = WMFindTool.FindList[idOrName].id;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (Tiles.ContainsKey(id))
            {
                var p = Tiles[id];
                if (WMFindTool.FindList.ContainsKey(idOrName))
                {
                    var p2 = WMFindTool.FindList[idOrName];
                    var p3 = p.Clone();
                    p3.name = idOrName;
                    p3.frameX = p2.frameX;
                    p3.frameY = p2.frameY;
                    return p3;
                }

                return p;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获得图格id
        /// <param name="fuzzy">是否模糊匹配</param>
        /// <returns>{tileID,tileStyle}</returns>
        public static Dictionary<int, int> GetTileIDByIdOrName(string idOrName, bool fuzzy = true)
        {
            Dictionary<int, int> dict = new();
            int id;
            FindInfo fd;

            LoadTile();
            if (!IsTileID(idOrName))
            {
                // 匹配大小写
                idOrName = Mapping.UpperTileName(idOrName);

                if (fuzzy)
                {
                    // 模糊匹配
                    foreach (var w in Tiles.Values)
                    {
                        var name = w.name;
                        if (name == idOrName || name.StartsWith(idOrName) || name.EndsWith(idOrName))
                        {
                            dict.Add(w.id, -1);
                        }
                    }

                    // 匹配已知的多样式图格
                    foreach (var name in WMFindTool.FindList.Keys)
                    {
                        if (name == idOrName || name.StartsWith(idOrName) || name.EndsWith(idOrName))
                        {
                            fd = WMFindTool.FindList[name];
                            if (!dict.ContainsKey(fd.id))
                            {
                                dict.Add(fd.id, fd.style);
                            }
                            else
                            {
                                dict[fd.id] = fd.style;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var w in Tiles.Values)
                    {
                        if (w.name == idOrName)
                        {
                            dict.Add(w.id, -1);
                            break;
                        }
                    }

                    if (WMFindTool.FindList.ContainsKey(idOrName))
                    {
                        fd = WMFindTool.FindList[idOrName];
                        if (!dict.ContainsKey(fd.id))
                            dict.Add(fd.id, fd.style);
                        else
                            dict[fd.id] = fd.style;
                    }
                }
            }
            else
            {
                id = int.Parse(idOrName);
                dict.Add(id, -1);
            }

            return dict;
        }

        public static TileProp GetTileByID(int id)
        {
            LoadTile();
            return Tiles.ContainsKey(id) ? Tiles[id] : null;
        }

        /// <summary>
        /// 获得图格描述
        /// </summary>
        /// <param name="idOrName"></param>
        /// <returns>未找到则返回空</returns>
        public static string GetTileDescByIDOrName(string idOrName)
        {
            var p = GetTileByIDOrName(idOrName);
            return p != null ? p.Desc : "";
        }

        /// <summary>
        /// 获得图格名称
        /// </summary>
        public static string GetTileNameByID(int id)
        {
            LoadTile();
            return Tiles.ContainsKey(id) ? Tiles[id].name : "";
        }

        /// <summary>
        /// 图格id是否有效
        /// </summary>
        public static bool IsTileID(int id)
        {
            return id >= 0 && id <= TileID.Count;
        }

        /// <summary>
        /// 图格id是否有效
        /// </summary>
        public static bool IsTileID(string value)
        {
            if (int.TryParse(value, out int id))
                return IsTileID(id);
            else
                return false;
        }

        /// <summary>
        /// 是否为多样式图格
        /// </summary>
        /// <returns></returns>
        public static bool IsFrame(int id)
        {
            if (Tiles.ContainsKey(id))
            {
                return Tiles[id].isFrame;
            }
            return false;
        }
        #endregion


        #region 墙数据查询
        public static Dictionary<int, WallProp> Walls = new();
        /// <summary>
        /// 加载内嵌的墙体配置文件（.csv）
        /// </summary>
        public static void LoadWall()
        {
            if (Walls.Any()) return;

            // csv文件列头
            // id,name,color
            foreach (string line in Utils.FromEmbeddedPath("Wall.csv").Split('\n').Skip(2))
            {
                var arr = line.Split(',');
                int id = int.Parse(arr[0]);
                Walls.Add(id, new WallProp
                {
                    id = id,
                    name = arr[1],
                    color = arr[2]
                });
            }
        }

        /// <summary>
        /// 获得墙属性
        /// </summary>
        /// <param name="idOrName"></param>
        /// <returns>查找失败时返回 null</returns>
        public static WallProp GetWallByIDOrName(string idOrName)
        {
            LoadWall();
            if (!int.TryParse(idOrName, out int id))
            {
                foreach (var w in Walls.Values)
                {
                    if (w.name == idOrName)
                    {
                        id = w.id;
                        break;
                    }
                }
            }
            return Walls.ContainsKey(id) ? Walls[id] : null;
        }
        public static WallProp GetWallByID(ushort id)
        {
            return GetWallByIDOrName(id.ToString());
        }

        public static string GetWallNameByID(int id)
        {
            LoadWall();
            return Walls.ContainsKey(id) ? Walls[id].name : "";
        }

        /// <summary>
        /// 获得墙描述
        /// </summary>
        /// <param name="idOrName"></param>
        /// <returns>未找到则返回空</returns>
        public static string GetWallDescByIDOrName(string idOrName)
        {
            var p = GetWallByIDOrName(idOrName);
            return p != null ? p.Desc : "";
        }

        /// <summary>
        /// 获得墙id
        /// <param name="fuzzy">是否模糊匹配</param>
        public static List<int> GetWallIDByIdOrName(string idOrName, bool fuzzy = true)
        {
            List<int> ids = new();

            LoadWall();
            if (!IsWallID(idOrName))
            {
                if (fuzzy)
                {
                    // 模糊匹配
                    foreach (var w in Walls.Values)
                    {
                        var name = w.name;
                        if (name == idOrName || name.StartsWith(idOrName) || name.EndsWith(idOrName))
                        {
                            ids.Add(w.id);
                        }
                    }
                }
                else
                {
                    foreach (var w in Walls.Values)
                    {
                        if (w.name == idOrName)
                        {
                            ids.Add(w.id);
                            break;
                        }
                    }
                }
            }
            else
            {
                ids.Add(int.Parse(idOrName));
            }

            return ids;
        }


        /// <summary>
        /// 墙id是否有效
        /// </summary>
        public static bool IsWallID(int id)
        {
            return id > 0 && id <= WallID.Count;
        }

        /// <summary>
        /// 墙id是否有效
        /// </summary>
        public static bool IsWallID(string value)
        {
            if (int.TryParse(value, out int id))
                return IsWallID(id);
            else
                return false;
        }
        #endregion
    }
}
