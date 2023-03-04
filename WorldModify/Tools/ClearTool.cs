using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 橡皮擦工具
    /// </summary>
    class ClearTool
    {
        enum OPType
        {
            None,

            All,
            AllBlock, Block,
            AllWall, Wall,

            Liquid,
            Water, Lava, Honey, Shimmer,

            AllWire,
            Wire1, Wire2, Wire3, Wire4,
        };

        static List<string> OPNames = new() {
            "None",

            "所有",
            "所有方块", "指定方块",
            "所有墙体", "指定墙体",

            "所有电线",
            "红电线", "蓝电线", "绿电线", "黄电线",

            "所有液体",
            "水", "岩浆", "蜂蜜", "微光",
        };

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                List<string> lines = new()
                {
                    "/igen c <id/名称>，清除 指定图格",
                    "/igen c wall，清除 所有墙体",
                    "/igen c wall <id/名称>，清除 指定墙体",
                    "/igen c block，清除 所有方块",

                    "/igen c liquid，清除 所有液体",
                    "/igen c wire，清除 所有电线",
                    "/igen c <water/lava/honey/shimmer>，清除 水/岩浆/蜂蜜/微光",
                    "/igen c <red/blue/green/yellow>，清除 红/蓝/绿/黄电线",

                    "/igen c all，清除 所有",
                };
                Utils.Pagination(args, ref lines, "/igen clear");
            }
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            OPType type = OPType.None;
            int id = -1;
            string name = "";
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "all": type = OPType.All; break;
                case "block": type = OPType.AllBlock; break;

                // 墙
                case "wall":
                case "w":
                case "墙":
                    if (args.Parameters.Count > 1)
                    {
                        // 指定墙
                        var wp = TileHelper.GetWallByIDOrName(args.Parameters[1]);
                        if (wp == null)
                        {
                            op.SendErrorMessage("输入的墙id无效或名称不匹配！");
                            return;
                        }
                        else
                        {
                            type = OPType.Wall;
                            id = wp.id;
                            name = wp.Desc;
                        }
                    }
                    else
                    {
                        // 所有墙体
                        type = OPType.AllWall;
                    }
                    break;

                // 电线
                case "wire": case "电线": type = OPType.AllWire; break;
                case "red": case "红电线": type = OPType.Wire1; break;
                case "blue": case "蓝电线": type = OPType.Wire2; break;
                case "green": case "绿电线": type = OPType.Wire3; break;
                case "yellow": case "黄电线": type = OPType.Wire4; break;

                // 液体
                case "liquid": case "液体": type = OPType.Liquid; break;
                case "water": case "水": type = OPType.Water; break;
                case "lava": case "岩浆": type = OPType.Lava; break;
                case "honey": case "蜂蜜": type = OPType.Honey; break;
                case "shimmer": case "微光": type = OPType.Shimmer; break;

                default:
                    // 图格id / 图格名称
                    var tileProp = TileHelper.GetTileByIDOrName(args.Parameters[0].ToLowerInvariant());
                    if (tileProp != null)
                    {
                        type = OPType.Block;
                        id = tileProp.id;
                        name = tileProp.Desc;

                        if (TileHelper.IsPylon(op, id)) return;
                    }
                    else
                    {
                        op.SendErrorMessage("输入的图格id无效或名称不匹配！");
                        return;
                    }
                    break;
            }

            if (type != OPType.None)
            {
                Action(op, type, id, name);
            }
        }

        static async void Action(TSPlayer op, OPType type, int id, string name)
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
                            // 所有
                            case OPType.All:
                                if (tile.active() || tile.wall > 0 || tile.liquid > 0)
                                {
                                    count++;
                                }
                                tile.ClearEverything();
                                break;

                            // 图格
                            case OPType.AllBlock:
                                if (tile.active())
                                {
                                    count++;
                                    tile.ClearTile();
                                }
                                break;
                            case OPType.Block:
                                if (tile.type == id)
                                {
                                    count++;
                                    tile.ClearTile();
                                }
                                break;

                            // 墙
                            case OPType.AllWall:
                                if (tile.wall > 0)
                                {
                                    count++;
                                    tile.wall = 0;
                                }
                                break;
                            case OPType.Wall:
                                if (tile.wall == id)
                                {
                                    count++;
                                    tile.wall = 0;
                                }
                                break;

                            // 电线
                            case OPType.AllWire:
                                if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4())
                                {
                                    count++;
                                }
                                tile.wire(false);
                                tile.wire2(false);
                                tile.wire3(false);
                                tile.wire4(false);
                                break;
                            case OPType.Wire1:
                                if (tile.wire())
                                {
                                    count++;
                                    tile.wire(false);
                                }
                                break;
                            case OPType.Wire2:
                                if (tile.wire2())
                                {
                                    count++;
                                    tile.wire2(false);
                                }
                                break;
                            case OPType.Wire3:
                                if (tile.wire3())
                                {
                                    count++;
                                    tile.wire3(false);
                                }
                                break;
                            case OPType.Wire4:
                                if (tile.wire4())
                                {
                                    count++;
                                    tile.wire4(false);
                                }
                                break;

                            // 液体
                            case OPType.Liquid:
                            case OPType.Water:
                            case OPType.Lava:
                            case OPType.Honey:
                            case OPType.Shimmer:
                                count += ClearLiquid(x, y, type);
                                break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                if (string.IsNullOrEmpty(name)) name = Mapping.FlagTile(OPNames[(int)type]);
                op.SendSuccessMessage($"已清除{count}格{name}，用时{second}秒。");
            });
        }


        /// <summary>
        /// 清除液体
        /// </summary>
        static int ClearLiquid(int x, int y, OPType type)
        {
            ITile tile = Main.tile[x, y];
            if (tile.liquid > 0)
            {
                bool flag = false;
                switch (type)
                {
                    case OPType.Liquid: flag = true; break;
                    case OPType.Water: flag = tile.liquidType() == 0; break;
                    case OPType.Lava: flag = tile.liquidType() == 1; break;
                    case OPType.Honey: flag = tile.liquidType() == 2; break;
                    case OPType.Shimmer: flag = tile.liquidType() == 3; break;
                }
                if (flag)
                {
                    tile.liquid = 0;
                    return 1;
                }
            }
            return 0;
        }


        /// <summary>
        /// 打洞（清出一块站立区域）
        /// </summary>
        public static void Hole(TSPlayer op)
        {
            Rectangle rect = new(op.TileX - 3, op.TileY - 5, 7, 7);
            int count = 0;
            for (int x = rect.X; x < rect.Right; x++)
            {
                for (int y = rect.Y; y < rect.Bottom; y++)
                {
                    if (y == rect.Y || y == rect.Y + 6)
                    {
                        if (x == rect.X || x == rect.X + 1 || x == rect.X + 5 || x == rect.X + 6)
                            continue;
                    }
                    else if (y == rect.Y + 1 || y == rect.Y + 5)
                    {
                        if (x == rect.X || x == rect.X + 6)
                            continue;
                    }
                    ITile tile = Main.tile[x, y];
                    if (tile.active() || tile.wall > 0 || tile.liquid > 0) count++;
                    tile.ClearEverything();
                }
            }
            if (count > 0)
            {
                NetMessage.SendTileSquare(-1, op.TileX, op.TileY, 14);
                op.SendSuccessMessage($"打洞完成，已清空{count}格。");
            }
            else
            {
                op.SendSuccessMessage("打洞完成，未清空任何东西。");
            }
        }
    }
}