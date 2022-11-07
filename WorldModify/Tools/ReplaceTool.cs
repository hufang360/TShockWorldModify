using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 替换工具
    /// </summary>
    class ReplaceTool
    {
        enum Type
        {
            None,
            Replace, // 替换
            Random, // 随机方块
            Config, // 按配置文件进行替换

            Block,   // 图格替换
            Ice,    // 冰河化
            Helt,   // 冰融化
            Lava,    // 岩浆化
            Truffle,  // 蘑菇化
            Desert,  // 沙漠化

            Purify, // 净化
            Corruption, // 腐化
            Crimson,   // 猩红化
            Hallow,   // 神圣化
        };
        public static async void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
                    return;

                List<string> lines = new()
                {
                    "/igen r <id> <id>，图格替换",
                    "/igen r ice，水 → 薄冰",
                    "/igen r melt，薄冰 → 水",
                    "/igen r lava，水 → 岩浆",

                    "/igen r purify，净化",
                    "/igen r corruption，腐化",
                    "/igen r crimson，猩红化",
                    "/igen r hallow，神圣化",

                    "/igen r mushroom，蘑菇化",
                    "/igen r desert，沙漠化",
                    "/igen r random，随机",
                    "/igen r config，按配置进行替换",
                };

                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "/igen replace 指令用法 ({0}/{1})：",
                    FooterFormat = "输入 /igen r help {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }
            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            Type type = Type.None;
            string kw = args.Parameters[0].ToLowerInvariant();
            string[] plist = System.Array.Empty<string>();
            switch (kw)
            {
                case "help": Help(); break;

                default:
                    if (int.TryParse(kw, out int beforeID))
                    {
                        if (args.Parameters.Count < 2 || !int.TryParse(args.Parameters[1], out int afterID))
                        {

                            op.SendErrorMessage("指令用法：/igen r <目标方块id> <替换后的方块id>");
                            return;
                        }

                        if (!IDSet.matchBlockID.Contains(beforeID) || !IDSet.matchBlockID.Contains(afterID))
                        {
                            op.SendErrorMessage("不支持的方块id");
                            return;
                        }

                        type = Type.Replace;
                        plist = new string[] { beforeID.ToString(), afterID.ToString() };
                    }
                    else
                    {
                        Help();
                    }
                    break;

                case "config":
                case "con":
                    bool flag = RetileTool.FirstCreate();
                    if (flag)
                    {
                        op.SendErrorMessage($"{RetileTool.SaveFile} 已创建，按格式编辑后，再次执行本指令");
                        return;
                    }
                    else
                    {
                        RetileTool.Init();
                        type = Type.Config;
                    }
                    break;

                case "desert": type = Type.Desert; break;   // 沙漠化
                case "ice": type = Type.Ice; break; // 冰河化
                case "melt": case "mel": type = Type.Helt; break;    // 冰融化
                case "lava": type = Type.Lava; break;   // 水 → 岩浆
                case "purify": case "pur": type = Type.Purify; break;   // 净化
                case "corruption": case "cor": type = Type.Corruption; break;    // 腐化
                case "crimson": case "cri": type = Type.Crimson; break; // 猩红化
                case "hallow": case "hal": type = Type.Hallow; break;   // 神圣化
                case "mushroom": case "mus": type = Type.Truffle; break; // 蘑菇化
                case "random": case "ran": type = Type.Random; break; // 随机化
            }
            if (type != Type.None)
            {
                Rectangle selection = SelectionTool.GetSelection(op.Index);
                await Action(op, type, selection, plist);
            }
        }

        static Task Action(TSPlayer op, Type type, Rectangle rect, string[] plist)
        {
            int secondLast = Utils.GetUnixTimestamp;
            string GetOpString()
            {
                return type switch
                {
                    Type.Config => "按配置替换",
                    Type.Random => "随机图格",

                    Type.Ice => "冰河化",
                    Type.Helt => "冰融化",
                    Type.Lava => "岩浆化",
                    Type.Truffle => "蘑菇化",
                    Type.Desert => "沙漠化",

                    Type.Purify => "净化",
                    Type.Corruption => "腐化",
                    Type.Crimson => "猩红化",
                    Type.Hallow => "神圣化",
                    _ => "图格替换",
                };
            }
            string opString = GetOpString();

            List<ReTileInfo> replaceInfo = new List<ReTileInfo>();
            int beforeID = -1;
            int afterID = -1;
            switch (type)
            {
                case Type.Replace:
                    beforeID = int.Parse(plist[0]);
                    afterID = int.Parse(plist[1]);
                    break;
                case Type.Config: replaceInfo = RetileTool.Con.replace; break;
                case Type.Desert: replaceInfo = ReplacePlan.GetDesert(); break;
            }

            return Task.Run(() =>
            {
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        switch (type)
                        {
                            case Type.Random: RandomTool.RandomTile(x, y); break;
                            case Type.Replace: RelaceBlock(x, y, beforeID, afterID); break;
                            case Type.Config: Replace(x, y, replaceInfo); break;
                            case Type.Desert: Replace(x, y, replaceInfo); break;

                            case Type.Ice: IceAgeTile(x, y); break;
                            case Type.Helt: IceMeltTile(x, y); break;
                            case Type.Lava: LavaLiquid(x, y); break;

                            case Type.Purify: WorldGen.Convert(x, y, 0); break;
                            case Type.Corruption: WorldGen.Convert(x, y, 1); break;
                            case Type.Crimson: WorldGen.Convert(x, y, 4); break;
                            case Type.Hallow: WorldGen.Convert(x, y, 2); break;
                            case Type.Truffle: WorldGen.Convert(x, y, 3); break;
                        }
                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.FinishGen();
                op.SendSuccessMessage($"{opString} 结束（用时 {Utils.GetUnixTimestamp - secondLast}s）");
            });
        }

        #region 图格替换
        static void Replace(int x, int y, List<ReTileInfo> replaceInfo)
        {
            ITile tile = Main.tile[x, y];
            IEnumerable<ReTileInfo> query;
            bool flag = false;

            // 方块
            if (tile.active())
            {
                query = replaceInfo.Where(info => info.before.type == 0 && info.before.id == tile.type);
                foreach (ReTileInfo info2 in query)
                {
                    RelaceBlock(x, y, info2);
                    flag = true;
                }
            }

            // 墙
            query = replaceInfo.Where(info => info.before.type == 1 && info.before.id == tile.wall);
            foreach (ReTileInfo info in query)
            {
                if (info.after.type == 1)
                {
                    tile.wall = (ushort)info.after.id;
                    flag = true;
                }
            }

            // 液体
            if (tile.liquid > 0)
            {
                int liquidType = tile.liquidType() + 1;
                query = replaceInfo.Where(info => info.before.type == 2 && info.before.id == liquidType);
                foreach (ReTileInfo info in query)
                {
                    // 液体替换成物块
                    if (info.after.type == 0)
                    {
                        if (tile.active())
                        {
                            // 有的宝箱会泡在水里，只把水清除即可
                            tile.liquid = 0;
                        }
                        else
                        {
                            tile.type = (ushort)info.after.id;
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                            tile.liquid = 0;
                        }
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                // 更新状态
                NetMessage.SendTileSquare(-1, x, y);
            }
        }
        // 替换方块
        private static void RelaceBlock(int x, int y, ReTileInfo info2)
        {
            ITile tile = Main.tile[x, y];
            int atype = info2.after.type;
            int aid = info2.after.id;
            int astyle = info2.after.style;

            if (atype == 0)
            {
                if (aid == -1) // 清空图格
                    tile.ClearTile();

                //else if (aid == TileID.PalmTree)  // 棕榈树
                //{
                //    //WorldGen.GrowPalmTree(tileX, tileY);
                //    tile.type = (ushort)aid;
                //    tile.frameX = 66;
                //    tile.frameY = 0;
                //}
                //else if (aid == TileID.OasisPlants) // 绿洲植物
                //{
                //    // WorldGen.PlaceOasisPlant(tileX, tileY); // 无效
                //    int num = WorldGen.genRand.Next(9);
                //    int num2 = 0;
                //    short num3 = (short)(54 * num);
                //    short num4 = (short)(36 * num2);
                //    tile.type = (ushort)aid;
                //    tile.frameX = num3;
                //    tile.frameX = num4;
                //}
                //else if (aid >= 3 && astyle >= 0)  // 带style的图格
                //{
                //    WorldGen.PlaceTile(tileX, tileY, aid, false, true, 0, astyle);
                //    //tile.frameX += 18;
                //    //tile.frameX += 18;
                //}

                else // 直接替换
                {
                    tile.type = (ushort)aid;
                }
            }
        }
        static void RelaceBlock(int x, int y, int before, int after)
        {
            ITile tile = Main.tile[x, y];
            if (tile.type == before)
            {
                tile.type = (ushort)after;
                NetMessage.SendTileSquare(-1, x, y);
            }
        }
        #endregion




        #region 冰河化 / 冰融化
        public static void IceAgeTile(int x, int y)
        {
            // 薄冰 
            ITile tile = Main.tile[x, y];

            if (tile.liquid > 0)
            {
                switch (tile.liquidType())
                {
                    // 水
                    // Tile.Liquid_Water
                    case 0:
                        if (!tile.active())
                        {
                            tile.type = TileID.BreakableIce;
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                        }
                        tile.liquid = 0;
                        break;


                    // 岩浆
                    case 1: break;
                    // 蜂蜜
                    case 2: break;
                }
            }
        }
        public static void LavaLiquid(int x, int y)
        {
            // 薄冰 
            ITile tile = Main.tile[x, y];

            if (tile.liquid > 0)
            {
                switch (tile.liquidType())
                {
                    // 水
                    // Tile.Liquid_Water
                    case 0:
                        if (!tile.active())
                        {
                            tile.lava(lava: true);
                            tile.liquid = byte.MaxValue;
                            WorldGen.SquareTileFrame(x, y);
                            NetMessage.SendTileSquare(-1, x, y);
                        }
                        break;

                    // 岩浆
                    case 1: break;

                    // 蜂蜜
                    case 2: break;
                }
            }
        }




        public static void IceMeltTile(int tileX, int tileY)
        {
            ITile tile = Main.tile[tileX, tileY];
            if (tile.type != TileID.BreakableIce) return;

            tile.active(active: false);
            tile.liquid = byte.MaxValue;
            WorldGen.SquareTileFrame(tileX, tileY);
        }
        #endregion

    }
}