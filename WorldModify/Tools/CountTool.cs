using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 统计工具
    /// </summary>
    class CountTool
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
                Type.AllBlock => "方块",
                Type.Block => "指定方块",
                Type.AllWall => "墙体",
                Type.Wall => "指定墙体",

                Type.AllWire => "电线",
                Type.Wire1 => "红电线",
                Type.Wire2 => "蓝电线",
                Type.Wire3 => "绿电线",
                Type.Wire4 => "黄电线",

                Type.Liquid => "液体",
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
                    "/igen count block，统计 方块数量",
                    "/igen count <id>，统计 指定方块数量",
                    "/igen count wall，统计 墙体数量",
                    "/igen count wall <墙的 id/中文名称>，统计 指定墙体数量",

                    "/igen count liquid，统计 液体数量",
                    "/igen count <water/lava/honey/shimmer>，统计 水/岩浆/蜂蜜/微光 数量",
                    "/igen count wire，统计 电线数量",
                    "/igen count <red/blue/green/yellow>，统计 红/蓝/绿/黄 电线数量",

                };
                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "/igen count 指令用法 ({0}/{1})：",
                    FooterFormat = "输入 /igen count help {{0}} 查看更多".SFormat(Commands.Specifier)
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

                // 图格
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
            Rectangle rect = op.RealPlayer ? SelectionTool.GetSelection(op.Index) : Utils.GetWorldArea();
            int count = 0;
            int wallNum = 0;
            int liquidNum = 0;
            int wireNum = 0;
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
                                if (tile.active()) count++;
                                if (tile.wall > 0) wallNum++;
                                if (tile.liquid == byte.MaxValue) liquidNum++;
                                if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4()) wireNum++;
                                break;

                            // 图格
                            case Type.AllBlock: if (tile.active()) count++; break;
                            case Type.Block: if (tile.active() && tile.type == id) count++; break;

                            // 墙
                            case Type.AllWall: if (tile.wall > 0) count++; break;
                            case Type.Wall: if (tile.wall == id) count++; break;

                            // 电线
                            case Type.AllWire: if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4()) count++; break;
                            case Type.Wire1: if (tile.wire()) count++; break;
                            case Type.Wire2: if (tile.wire2()) count++; break;
                            case Type.Wire3: if (tile.wire3()) count++; break;
                            case Type.Wire4: if (tile.wire4()) count++; break;

                            // 液体
                            case Type.Liquid: if (tile.liquid == byte.MaxValue) count++; break;
                            case Type.Water: if (tile.liquid == byte.MaxValue && tile.liquidType() == 0) count++; break;
                            case Type.Lava: if (tile.liquid == byte.MaxValue && tile.liquidType() == 1) count++; break;
                            case Type.Honey: if (tile.liquid == byte.MaxValue && tile.liquidType() == 2) count++; break;
                            case Type.Shimmer: if (tile.liquid == byte.MaxValue && tile.liquidType() == 3) count++; break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                if (type == Type.All)
                {
                    op.SendInfoMessage($"{Desc(Type.AllBlock)}: {count}格");
                    op.SendInfoMessage($"{Desc(Type.AllWall)}: {wallNum}格");
                    op.SendInfoMessage($"{Desc(Type.Liquid)}: {liquidNum}格");
                    op.SendInfoMessage($"{Desc(Type.AllWire)}: {wireNum}根");
                }
                else
                {
                    if (string.IsNullOrEmpty(name)) name = Desc(type);
                    op.SendInfoMessage($"{name}: {count}格");
                }
            });
        }
    }
}