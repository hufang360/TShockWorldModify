using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 橡皮擦工具
    /// </summary>
    class ClearTool
    {
        enum Type
        {
            None,

            All,
            AllBlock,
            Block,
            AllWall,
            Wall,

            Liquid,
            Water,
            Lava,
            Honey,
            Shimmer,

            AllWire,
            Wire1,
            Wire2,
            Wire3,
            Wire4,
        };
        static string Desc(Type type)
        {
            return type switch
            {
                Type.All => "所有",

                Type.AllBlock => "所有方块",
                Type.Block => "指定方块",
                Type.AllWall => "所有墙体",
                Type.Wall => "指定墙体",

                Type.AllWire => "所有电线",
                Type.Wire1 => "红电线",
                Type.Wire2 => "蓝电线",
                Type.Wire3 => "绿电线",
                Type.Wire4 => "黄电线",

                Type.Liquid => "所有液体",
                Type.Water => "水",
                Type.Lava => "岩浆",
                Type.Honey => "蜂蜜",
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
                    "/igen c all，清除 所有",

                    "/igen c wall，清除 所有墙体",
                    "/igen c wall <id/中文名称>，清除 指定墙体",
                    "/igen c block，清除 所有方块",
                    "/igen c <id>，清除 指定方块",

                    "/igen c liquid，清除 所有液体",
                    "/igen c wire，清除 所有电线",
                    "/igen c <water/lava/honey/shimmer>，清除 水/岩浆/蜂蜜/微光",
                    "/igen c <red/blue/green/yellow>，清除 红/蓝/绿/黄 电线",

                };
                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "[c/96FF0A:/igen c]lear 指令用法 ({0}/{1})：",
                    FooterFormat = "输入 /igen c help {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }
            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            Type type = Type.None;
            int id = -1;
            string name = "";
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help": Help(); return;

                case "all": type = Type.All; break;
                case "block": type = Type.AllBlock; break;

                // 墙
                case "wall":
                case "墙":
                    if (args.Parameters.Count > 1)
                    {
                        // 指定墙
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
                    }
                    else
                    {
                        // 所有墙体
                        type = Type.AllWall;
                    }

                    break;

                // 电线
                case "wire": case "电线": type = Type.AllWire; break;
                case "red": case "红": type = Type.Wire1; break;
                case "blue": case "蓝": type = Type.Wire2; break;
                case "green": case "绿": type = Type.Wire3; break;
                case "yellow": case "黄": type = Type.Wire4; break;

                // 液体
                case "liquid": case "液体": type = Type.Liquid; break;
                case "water": case "水": type = Type.Water; break;
                case "lava": case "岩浆": type = Type.Lava; break;
                case "honey": case "蜂蜜": type = Type.Honey; break;
                case "shimmer": case "微光": type = Type.Shimmer; break;

                default:
                    if (int.TryParse(args.Parameters[0], out id))
                    {
                        // 指定图格
                        if (id >= 0 && id < TileID.Count)
                        {
                            type = Type.Block;
                        }
                        else
                        {
                            op.SendErrorMessage($"图格id，有效值为 0~{TileID.Count - 1}");
                            return;
                        }
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
                            // 所有
                            case Type.All:
                                if (tile.active() || tile.wall > 0 || tile.liquid == byte.MaxValue)
                                {
                                    count++;
                                }
                                tile.ClearEverything();
                                break;

                            // 图格
                            case Type.AllBlock:
                                if (tile.active())
                                {
                                    count++;
                                    tile.ClearTile();
                                }
                                break;
                            case Type.Block:
                                if (tile.type == id)
                                {
                                    count++;
                                    tile.ClearTile();
                                }
                                break;

                            // 墙
                            case Type.AllWall:
                                if (tile.wall > 0)
                                {
                                    count++;
                                    tile.wall = 0;
                                }
                                break;
                            case Type.Wall:
                                if (tile.wall == id)
                                {
                                    count++;
                                    tile.wall = 0;
                                }
                                break;

                            // 电线
                            case Type.AllWire:
                                if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4())
                                {
                                    count++;
                                }
                                tile.wire(false);
                                tile.wire2(false);
                                tile.wire3(false);
                                tile.wire4(false);
                                break;
                            case Type.Wire1:
                                if (tile.wire())
                                {
                                    count++;
                                    tile.wire(false);
                                }
                                break;
                            case Type.Wire2:
                                if (tile.wire2())
                                {
                                    count++;
                                    tile.wire2(false);
                                }
                                break;
                            case Type.Wire3:
                                if (tile.wire3())
                                {
                                    count++;
                                    tile.wire3(false);
                                }
                                break;
                            case Type.Wire4:
                                if (tile.wire4())
                                {
                                    count++;
                                    tile.wire4(false);
                                }
                                break;

                            // 液体
                            case Type.Liquid:
                            case Type.Water:
                            case Type.Lava:
                            case Type.Honey:
                            case Type.Shimmer:
                                if (tile.liquid == byte.MaxValue)
                                {
                                    count++;
                                    ClearLiquid(x, y, type);
                                }
                                break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                if (string.IsNullOrEmpty(name)) name = Desc(type);
                op.SendSuccessMessage($"已清除 {count}格 {name}，用时{second}秒");
            });
        }


        // 清除液体
        static void ClearLiquid(int x, int y, Type type)
        {
            ITile tile = Main.tile[x, y];
            if (tile.liquid == byte.MaxValue)
            {
                bool flag = false;
                switch (type)
                {
                    case Type.Liquid: flag = true; break;
                    case Type.Water: flag = tile.liquidType() == 0; break;
                    case Type.Lava: flag = tile.liquidType() == 1; break;
                    case Type.Honey: flag = tile.liquidType() == 2; break;
                    case Type.Shimmer: flag = tile.liquidType() == 3; break;
                }
                if (flag)
                {
                    tile.liquid = 0;
                }
            }
        }


        /// <summary>
        /// 打洞（清出一小块 站立区）
        /// </summary>
        public static void Hole(TSPlayer op)
        {
            Rectangle rect = new(op.TileX, op.TileY - 2, 2, 3);
            for (int x = rect.X; x < rect.Right; x++)
            {
                for (int y = rect.Y; y < rect.Bottom; y++)
                {
                    Main.tile[x, y].ClearEverything();
                    NetMessage.SendTileSquare(-1, x, y);
                }
            }
            op.SendSuccessMessage("打洞完成！");
        }
    }
}