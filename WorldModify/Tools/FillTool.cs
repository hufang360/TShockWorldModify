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

            Block,
            Wall,

            Dirt, Mud,
            Water, Honey, Lava, Shimmer,
            Wire1, Wire2, Wire3, Wire4,
        };

        static readonly string[] Desc = new string[] {
            "",
            "方块",
            "墙体",

            "土块", "泥块",
            "水", "蜂蜜", "岩浆", "微光",
            "红电线", "蓝电线", "绿电线", "黄电线",
        };

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                List<string> lines = new()
                {
                    "/igen f <id/名称>，填充图格",
                    "/igen f wall <id/名称>，铺墙",
                    "/igen f <dirt/mud>，填充土块/泥块",
                    "/igen f <water/lava/honey/shimmer>，填充水/岩浆/蜂蜜/微光",
                    "/igen f <red/blue/green/yellow>，铺设红/蓝/绿/黄电线",
                };
                Utils.Pagination(args, ref lines, "/igen fill");
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
                    id = TileID.Dirt;
                    break;

                case "mud":
                case "泥":
                case "泥块":
                    type = Type.Mud;
                    id = TileID.Mud;
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
                    type = Type.Shimmer;
                    break;

                case "wall":
                case "w":
                case "墙":
                    if (args.Parameters.Count < 2)
                    {
                        op.SendErrorMessage("需要输入墙体ID，/igen f wall <id/名称>");
                        return;
                    }
                    var wp = TileHelper.GetWallByIDOrName(args.Parameters[1]);
                    if (wp == null)
                    {
                        op.SendErrorMessage("输入的墙id无效或名称不匹配！");
                        return;
                    }
                    else
                    {
                        type = Type.Wall;
                        id = wp.id;
                        name = wp.Desc;
                    }
                    break;

                // 电线
                case "red":
                case "红电线":
                case "电线":
                    type = Type.Wire1;
                    id = 1;
                    break;

                case "blue":
                case "蓝电线":
                    type = Type.Wire2;
                    id = 2;
                    break;

                case "green":
                case "绿电线":
                    type = Type.Wire3;
                    id = 3;
                    break;

                case "yellow":
                case "黄电线":
                    type = Type.Wire4;
                    id = 4;
                    break;

                default:
                    // 图格id / 图格名称
                    var tileProp = TileHelper.GetTileByIDOrName(kw);
                    if (tileProp != null)
                    {
                        type = Type.Block;
                        id = tileProp.id;
                        name = tileProp.Desc;
                    }
                    else
                    {
                        op.SendErrorMessage("输入的图格id无效或图格名称不匹配！");
                        return;
                    }
                    break;
            }
            if (type != Type.None)
            {
                if (TileHelper.IsPylon(op, id)) return;
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
                                if (tile.liquid == 0 && tile.wall == 0 && !tile.active())
                                {
                                    count++;
                                    FillLiquid(x, y, type);
                                }
                                break;
                        }
                    }
                }

                switch (type)
                {
                    // 电线
                    case Type.Wire1:
                    case Type.Wire2:
                    case Type.Wire3:
                    case Type.Wire4:
                        for (int x = rect.X; x < rect.Right; x++)
                        {
                            ITile tile = Main.tile[x, rect.Bottom - 1];
                            if (id == 1) tile.wire(true);
                            else if (id == 2) tile.wire2(true);
                            else if (id == 3) tile.wire3(true);
                            else if (id == 4) tile.wire4(true);

                            count++;
                        }
                        for (int y = rect.Y; y < rect.Bottom; y++)
                        {
                            ITile tile = Main.tile[rect.Right - 1, y];
                            if (id == 1) tile.wire(true);
                            else if (id == 2) tile.wire2(true);
                            else if (id == 3) tile.wire3(true);
                            else if (id == 4) tile.wire4(true);
                            count++;
                        }
                        break;
                }
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                if (string.IsNullOrEmpty(name)) name = Desc[(int)type];

                string text = "";
                switch (type)
                {
                    case Type.Water:
                    case Type.Honey:
                    case Type.Lava:
                    case Type.Shimmer:
                        text = $"，如液体不流动，可输入{Utils.Highlight("/settle")}进行平衡";
                        break;
                }
                op.SendSuccessMessage($"已填充{count}格{name}，用时{second}秒{text}。");
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
            if (tile.active() || tile.liquid > 0) return;

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