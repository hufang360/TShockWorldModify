using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 填充工具
    /// </summary>
    class FillTool
    {
        enum Type
        {
            None,

            Block,  // 填充图格
            Wall,  // 填充墙体

            Dirt,   // 填充土块
            Mud,    // 填充泥块
            Water,  // 填水
            Honey,  // 填充蜂蜜
            Lava,   // 填充岩浆
            Shimmer,   // 填充微光
        };
        static string TypeDesc(Type type)
        {
            return type switch
            {
                Type.Block => "方块",
                Type.Wall => "墙体",

                Type.Dirt => "土块",
                Type.Mud => "泥块",
                Type.Water => "水",
                Type.Honey => "蜂蜜",
                Type.Lava => "岩浆",
                Type.Shimmer => "微光",
                _ => "图格",
            };
        }

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
                    return;

                List<string> lines = new()
                {
                    "/igen f <id>，填充指定图格",
                    "/igen f wall <墙的 id/中文名称>，填充指定墙体",
                    "/igen f dirt，填充土块",
                    "/igen f mud，填充泥块",

                    "/igen f water，填充水",
                    "/igen f honey，填充蜂蜜",
                    "/igen f lava，填充岩浆",
                    "/igen f shimmer，填充微光",
                };

                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "/igen fill 指令用法 ({0}/{1})：",
                    FooterFormat = "输入 /igen f help {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            Type type = Type.None;
            int id = -1;
            string name = "";
            string kw = args.Parameters[0].ToLowerInvariant();
            switch (kw)
            {
                case "dirt":
                case "土":
                case "土块":
                    type = Type.Dirt;
                    break;

                case "mud":
                case "泥":
                case "泥块":
                    type = Type.Mud;
                    break;

                case "water":
                case "水":
                    type = Type.Water;
                    break;

                case "honey":
                case "蜂蜜":
                    type = Type.Honey;
                    break;

                case "lava":
                case "岩浆":
                    type = Type.Lava;
                    break;

                case "shimmer":
                case "微光":
                    type = Type.Shimmer; break;

                case "wall":
                case "w":
                case "墙体":
                case "墙":
                    if (args.Parameters.Count < 2)
                    {
                        op.SendErrorMessage("需要输入墙体ID，/igen f wall <墙的 id/中文名称>");
                        return;
                    }
                    var wp = ResHelper.GetWallByIDOrName(args.Parameters[1]);
                    if (wp == null)
                    {
                        op.SendErrorMessage("墙的 id/中文名称 错误");
                        return;
                    }
                    else
                    {
                        type = Type.Wall;
                        id = wp.id;
                        name = wp.name;
                    }
                    break;

                default:
                    if (int.TryParse(kw, out id))
                    {
                        if (!IDSet.matchBlockID.Contains(id))
                        {
                            op.SendErrorMessage("输入的图格ID无效！目前仅支持填充一些方块");
                            return;
                        }

                        //TileObjectData tileData = TileObjectData.GetTileData(id, 0);
                        //utils.Log($"tileData:{tileData.Width}");
                        //Point p = TileHelper.GetTileWH(id);
                        //if (p.X>1 ||p.Y>1)
                        //{
                        //    op.SendErrorMessage("仅支持填充1x1的图格，不支持填充家具等！");
                        //    return;
                        //}
                        type = Type.Block;
                    }
                    else
                    {
                        Help();
                    }
                    break;
            }
            if (type != Type.None)
            {
                Action(op, type, id, name);
            }
        }

        static async void Action(TSPlayer op, Type type, int id, string name)
        {
            Rectangle rect = SelectionTool.GetSelection(op.Index);
            int secondLast = Utils.GetUnixTimestamp;
            int count = 0;
            await Task.Run(() =>
            {
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {

                        ITile tile = Main.tile[x, y];
                        switch (type)
                        {
                            // 图格
                            case Type.Block:
                            case Type.Dirt:
                            case Type.Mud:
                                if (!tile.active() && tile.liquid == 0)
                                {
                                    count++;
                                    Fill(x, y, type, id);
                                }
                                break;

                            // 墙
                            case Type.Wall:
                                if (tile.wall == 0)
                                {
                                    count++;
                                    tile.wall = (ushort)id;
                                }
                                break;
                            
                            // 液体
                            case Type.Water:
                            case Type.Honey:
                            case Type.Lava:
                            case Type.Shimmer:
                                if (tile.liquid == 0)
                                {
                                    count++;
                                    FillLiquid(x, y, type);
                                }
                                break;
                        }
                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                if (string.IsNullOrEmpty(name)) name = TypeDesc(type);
                op.SendSuccessMessage($"已填充 {count}格 {name}，用时{second}秒");
            });
        }

        /// <summary>
        /// 填充图格
        /// </summary>
        private static void Fill(int x, int y, Type type, int replaceID = -1)
        {
            ITile tile = Main.tile[x, y];
            int id = -1;
            switch (type)
            {
                case Type.Block: if (replaceID != -1) id = replaceID; break;
                case Type.Dirt: id = TileID.Dirt; break;
                case Type.Mud: id = TileID.Mud; break;
            }
            if (id != -1)
            {
                tile.type = (ushort)id;
                tile.active(active: true);
                //WorldGen.SquareTileFrame(x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
            }
        }

        /// <summary>
        /// 填充液体
        /// </summary>
        private static void FillLiquid(int x, int y, Type type)
        {
            ITile tile = Main.tile[x, y];
            if (tile.active() || tile.liquid != byte.MaxValue) return;

            int id = -1;
            switch (type)
            {
                // Tile.Liquid_Water
                case Type.Water: id = 0; break;
                case Type.Lava: id = 1; break;
                case Type.Honey: id = 2; break;
                case Type.Shimmer: id = 3; break;
            }
            if (id != -1)
            {
                tile.liquid = byte.MaxValue;
                tile.liquidType(id);
                //WorldGen.SquareTileFrame(x, y);
                //NetMessage.SendTileSquare(-1, x, y);
            }
        }

    }
}