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
            NULL,
            Block,  // 填充图格
            Wall,  // 填充墙体

            Dirt,   // 填充土块
            Mud,    // 填充泥块
            Water,  // 填水
            Honey,  // 填充蜂蜜
            Lava,   // 填充岩浆
            Shimmer,   // 填充微光
        };


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
                    "/igen f wall <id>，填充指定墙体",
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

            int id = 0;
            Type type = Type.NULL;
            string kw = args.Parameters[0].ToLowerInvariant();
            switch (kw)
            {
                case "dirt": type = Type.Dirt; break;
                case "mud": type = Type.Mud; break;
                case "water": type = Type.Water; break;
                case "honey": type = Type.Honey; break;
                case "lava": type = Type.Lava; break;
                case "shimmer": type = Type.Shimmer; break;

                case "wall":
                case "w":
                    if (args.Parameters.Count < 2)
                    {
                        op.SendErrorMessage("需要输入墙体ID，/igen f wall <id>");
                        return;
                    }
                    if (!int.TryParse(args.Parameters[1], out id))
                    {
                        op.SendErrorMessage($"输入的墙体ID无效，有效值 1~{WallID.Count - 1}");
                        return;
                    }
                    type = Type.Wall;
                    break;

                default:
                    if (int.TryParse(kw, out id))
                    {
                        if (!RandomTool.matchBlockID.Contains(id))
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
            if (type != Type.NULL)
            {
                Rectangle selection = SelectionTool.GetSelection(op.Index);
                Action(op, type, selection, id);
            }
        }


        private static async void Action(TSPlayer op, Type type, Rectangle rect, int id = -1)
        {
            int secondLast = utils.GetUnixTimestamp;
            string GetOpString()
            {
                return type switch
                {
                    Type.Block => "填充方块",
                    Type.Wall => "填充墙体",

                    Type.Dirt => "填充土块",
                    Type.Mud => "填充泥块",
                    Type.Water => "注水",
                    Type.Honey => "注入蜂蜜",
                    Type.Lava => "注入岩浆",
                    Type.Shimmer => "注入微光",
                    _ => "填充图格",
                };
            }
            string opString = GetOpString();


            await Task.Run(() =>
            {
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        ITile tile = Main.tile[x, y];
                        if (tile.active())
                            continue;

                        switch (type)
                        {
                            case Type.Block:
                            case Type.Wall:
                            case Type.Dirt:
                            case Type.Mud:
                            case Type.Water:
                            case Type.Honey:
                            case Type.Lava:
                            case Type.Shimmer:
                                Fill(x, y, type, id);
                                break;
                        }
                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"{opString} 结束（用时 {second}s）");
            });
        }

        private static void Fill(int x, int y, Type type, int id = -1)
        {
            ITile tile = Main.tile[x, y];

            bool isBlock = false;
            bool isLiquid = false;
            switch (type)
            {
                case Type.Block:
                    if (id != -1)
                    {
                        tile.type = (ushort)id;
                        isBlock = true;
                    }
                    break;

                case Type.Wall:
                    if (id != -1)
                        tile.wall = (ushort)id;
                    break;

                case Type.Dirt:
                    tile.type = TileID.Dirt;
                    isBlock = true;
                    break;

                case Type.Mud:
                    tile.type = TileID.Mud;
                    isBlock = true;
                    break;

                case Type.Water:
                    tile.honey(honey: false);
                    tile.lava(lava: false);
                    isLiquid = true;
                    break;

                case Type.Honey:
                    tile.honey(honey: true);
                    isLiquid = true;
                    break;

                case Type.Lava:
                    tile.lava(lava: true);
                    isLiquid = true;
                    break;

                case Type.Shimmer:
                    tile.shimmer(shimmer: true);
                    isLiquid = true;
                    break;
            }
            if (isBlock)
            {
                tile.active(active: true);
                //WorldGen.SquareTileFrame(x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                //NetMessage.SendTileSquare(-1, x, y);
            }
            else if (isLiquid)
            {
                tile.liquid = byte.MaxValue;
                //WorldGen.SquareTileFrame(x, y);
                //NetMessage.SendTileSquare(-1, x, y);
            }
        }
    }
}