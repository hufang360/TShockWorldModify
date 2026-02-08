using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 统计工具
    /// </summary>
    class StatsTool
    {
        enum OPType
        {
            None,

            All, AllBlock, Block, AllWall, Wall,
            Liquid, Water, Lava, Honey, Shimmer,
            AllWire, Wire1, Wire2, Wire3, Wire4,
        };


        static readonly List<string> OPNames = new() {
            "None",

            "方块", "指定方块", "墙体", "指定墙体", "液体",
            "水", "岩浆", "蜂蜜", "微光",
            "电线", "红电线", "蓝电线", "绿电线", "黄电线",
        };

        static string GetOPName(OPType t) { return OPNames[(int)t]; }

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                List<string> lines = new()
                {
                    "/igen stats block，统计方块的数量",
                    "/igen stats <id/名称>，统计指定方块的数量",
                    "/igen stats wall，统计墙的数量",
                    "/igen stats wall <id/名称>，统计指定墙的数量",

                    "/igen stats liquid，统计液体的数量",
                    "/igen stats <water/lava/honey/shimmer>，水/岩浆/蜂蜜/微光",
                    "/igen stats wire，统计电线的数量",
                    "/igen stats <red/blue/green/yellow>，红/蓝/绿/黄电线",

                };
                Utils.Pagination(args, ref lines, "/igen stats");
            }
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            OPType type;
            int id = -1;
            string name = "";
            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 图格
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
                            op.SendErrorMessage("输入的id/名称无效！");
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
                            case OPType.All:
                                if (tile.active()) count++;
                                if (tile.wall > 0) wallNum++;
                                if (tile.liquid == byte.MaxValue) liquidNum++;
                                if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4()) wireNum++;
                                break;

                            // 图格
                            case OPType.AllBlock: if (tile.active()) count++; break;
                            case OPType.Block: if (tile.active() && tile.type == id) count++; break;

                            // 墙
                            case OPType.AllWall: if (tile.wall > 0) count++; break;
                            case OPType.Wall: if (tile.wall == id) count++; break;

                            // 电线
                            case OPType.AllWire: if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4()) count++; break;
                            case OPType.Wire1: if (tile.wire()) count++; break;
                            case OPType.Wire2: if (tile.wire2()) count++; break;
                            case OPType.Wire3: if (tile.wire3()) count++; break;
                            case OPType.Wire4: if (tile.wire4()) count++; break;

                            // 液体
                            case OPType.Liquid: if (tile.liquid > 0) count++; break;
                            case OPType.Water: if (tile.liquid > 0 && tile.liquidType() == 0) count++; break;
                            case OPType.Lava: if (tile.liquid > 0 && tile.liquidType() == 1) count++; break;
                            case OPType.Honey: if (tile.liquid > 0 && tile.liquidType() == 2) count++; break;
                            case OPType.Shimmer: if (tile.liquid > 0 && tile.liquidType() == 3) count++; break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                if (Utils.IsWorldArea(rect))
                    op.SendInfoMessage("整个世界内的图格统计：");
                else
                    op.SendInfoMessage("区域内的图格统计：");
                if (type == OPType.All)
                {
                    op.SendInfoMessage($"{GetOPName(OPType.AllBlock)}: {count}格" +
                        $"\n{GetOPName(OPType.AllWall)}: {wallNum}格" +
                        $"\n{GetOPName(OPType.Liquid)}: {liquidNum}格" +
                        $"\n{GetOPName(OPType.AllWire)}: {wireNum}根");
                }
                else
                {
                    if (string.IsNullOrEmpty(name)) name = GetOPName(type);
                    op.SendInfoMessage($"{name}: {count}格");
                }
            });
        }
    }
}