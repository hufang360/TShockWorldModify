using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;
using TShockAPI;
using WorldModify.Gen;
using WorldModify.Tools;

namespace WorldModify
{
    /// <summary>
    /// 建造
    /// </summary>
    class iGen
    {
        public async static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            void Help()
            {
                List<string> lines = new()
                {
                    "/igen room [数量]，小房间（默认生成3个）",
                    "/igen hotel，NPC小旅馆",
                    "/igen hell，地狱直通车",
                    "/igen pond [water/lava/honey/shimmer]，鱼池",

                    "/igen dirt，填土",
                    "/igen sm [宽] [高]，盾构机（默认宽61高34）",
                    "/igen drill [宽] [高]，钻井机（默认宽3高34）",
                    "/igen place help，放置工具",

                    "/igen selection help，选区工具",
                    "/igen replace help，替换工具（选区内）",
                    "/igen fill help，填充工具（选区内）",
                    "/igen clear help，清除工具（选区内）",
                    "/igen copy help，复制工具（选区内）",

                    "/igen stats help，统计工具（选区内）",
                    "/igen world help，重建世界",
                    "/igen random，全图随机",

                    "/igen <关键词>，查询图格",
                    "/igen wall <关键词>，查询墙",
                };
                Utils.Pagination(args, ref lines, "/igen");
                //op.SendInfoMessage("/igen hammer help，锤子工具");
            }
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            bool NeedInGame() { return Utils.NeedInGame(op); }
            bool NeedWaitTask() { return TileHelper.NeedWaitTask(op); }

            bool isRight;
            int w;
            int h;
            int num;
            int total;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 图格数据查询
                default:
                    args.Player.SendErrorMessage($"未知指令/参数，输入{Utils.Highlight("/igen help")}查询指令用法");
                    break;

                // 重建世界
                case "world":
                    WorldTool.Manage(args);
                    return;

                case "info":
                    if (NeedInGame()) return;
                    InfoTool.UnderInfo(op);
                    break;

                #region 玻璃小房间
                case "room":
                    if (NeedInGame() || NeedWaitTask()) return;
                    total = 3;
                    if (args.Parameters.Count > 1)
                    {
                        if (!int.TryParse(args.Parameters[1], out total))
                        {
                            op.SendErrorMessage("输入的房间数量不对");
                            return;
                        }
                        if (total < 1 || total > 1000)
                        {
                            total = 3;
                        }
                    }
                    isRight = op.TPlayer.direction != -1;
                    await Room.AsyncGenRoom(op, op.TileX, op.TileY + 3, total, isRight, true);
                    return;

                // 小旅馆
                case "hotel":
                    if (NeedInGame() || NeedWaitTask()) return;
                    isRight = op.TPlayer.direction != -1;
                    total = Math.Max(3, NPCHelper.GetRelive().Count);
                    await Room.AsyncGenHotel(op, op.TileX, op.TileY + 3, total, isRight, true);
                    return;
                #endregion


                #region 鱼池
                case "pond":
                    if (NeedInGame() || NeedWaitTask()) return;
                    int type = 0;
                    if (args.Parameters.Count > 1)
                    {
                        type = Tools.LiquidMan.GetLiquidID(args.Parameters[1].ToLowerInvariant());
                        if (type == -1)
                        {
                            op.SendErrorMessage("鱼池风格不对");
                            return;
                        }
                    }
                    await Pond.AsyncGenPond(op, op.TileX, op.TileY + 3, type);
                    return;
                #endregion

                #region 盾构机
                case "shieldmachine":
                case "sm":
                    if (NeedInGame() || NeedWaitTask()) return;
                    isRight = op.TPlayer.direction != -1;
                    w = 61;
                    h = 34;
                    if (args.Parameters.Count > 1)
                    {
                        if (int.TryParse(args.Parameters[1], out num))
                            w = Math.Max(3, num);
                    }
                    if (args.Parameters.Count > 2)
                    {
                        if (int.TryParse(args.Parameters[2], out num))
                            h = Math.Max(3, num);
                    }
                    await AsyncGenShieldMachine(op, op.TileX, op.TileY + 3, w, h, isRight);
                    return;
                #endregion


                #region 挖掘机
                case "drill":
                    if (NeedInGame() || NeedWaitTask()) return;
                    isRight = op.TPlayer.direction != -1;
                    w = 3;
                    h = 34;
                    if (Utils.TryParseInt(args.Parameters, 1, out num)) w = Math.Max(3, num);
                    if (Utils.TryParseInt(args.Parameters, 2, out num)) h = Math.Max(34, num);
                    await AsyncDigArea(op, op.TileX, op.TileY + 3, w, h, isRight);
                    return;
                #endregion


                #region 地狱直通车
                case "hell":
                    if (NeedInGame() || NeedWaitTask()) return;
                    await Hellevator.AsyncGenHellevator(op, op.TileX, op.TileY + 3);
                    return;
                #endregion


                #region 全图随机
                case "random":
                    if (NeedWaitTask()) return;
                    if (args.Parameters.Count > 1 && args.Parameters[1].ToLowerInvariant() == "true")
                        RandomTool.RandomAll(args);
                    else
                        op.SendErrorMessage("本操作比较危险，将对全图的图格和背景墙进行随机，如确定此操作，请输入 /igen random true");
                    break;
                #endregion

                #region 填土
                case "dirt":
                    if (NeedInGame() || NeedWaitTask()) return;
                    await AsyncPlaceDirt(op);
                    return;
                #endregion

                #region 打洞
                case "hole":
                    if (NeedInGame() || NeedWaitTask()) return;
                    ClearTool.Hole(op);
                    break;
                #endregion

                #region 放置工具
                case "place":
                case "p":
                    if (NeedInGame()) return;
                    PlaceTool.Manage(args);
                    break;
                #endregion

                #region ==带区域操作==
                // 设置选区
                case "selection":
                case "s":
                    if (NeedInGame()) return;
                    SelectionTool.Mange(args);
                    break;

                // 替换
                case "replace":
                case "r":
                    if (NeedInGame() || NeedWaitTask()) return;
                    ReplaceTool.Manage(args);
                    break;

                // 填充
                case "fill":
                case "f":
                    if (NeedInGame() || NeedWaitTask()) return;
                    FillTool.Manage(args);
                    break;

                // 清空
                case "clear":
                case "c":
                    if (NeedInGame() || NeedWaitTask()) return;
                    ClearTool.Manage(args);
                    break;

                //// 挖掘工具
                //case "dig":
                //case "d":
                //    if (NeedInGame() || NeedWaitTask()) return;
                //    DigTool.Manage(args);
                //    break;

                // 统计
                case "stats":
                    StatsTool.Manage(args);
                    break;

                // 复制
                case "copy":
                    if (NeedInGame() || NeedWaitTask()) return;
                    TileScaner.CopyUnder(args.Player);
                    break;
                // 粘贴
                case "paste":
                    if (NeedInGame() || NeedWaitTask()) return;
                    if (args.Parameters.Count < 2)
                    {
                        op.SendErrorMessage("需要输入剪贴板名称，例如 /igen paste 001");
                        return;
                    }
                    TileScaner.Paste(args.Player, args.Parameters[1]);
                    break;
                    #endregion


                    #region 未完成
                    //case "report":
                    //    //ReportTool.Manage(args);
                    //    op.SendInfoMessage("这个功能还没写好");
                    //    break;
                    //// 全图沙漠化
                    //case "egypt":
                    //    if (NeedInGame() || NeedWaitTask()) return;
                    //    //Regen.AsyncDesertWorld(op);
                    //    op.SendInfoMessage("这个功能还没写好");
                    //    break;

                    //case "char":
                    //    if (NeedInGame() || NeedWaitTask()) return;
                    //    //CharPaint(op);
                    //    op.SendInfoMessage("这个功能还没写好");
                    //    break;

                    //// 像素画
                    //case "paint":
                    //    if (NeedInGame() || NeedWaitTask()) return;
                    //    PaintTool.Manage(op);
                    //    break;
                    #endregion
            }

        }






        #region 填土
        static Task AsyncPlaceDirt(TSPlayer op)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                PlaceDirt(op);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"填土完成，用时{second}秒。");
            });
        }
        static void PlaceDirt(TSPlayer op)
        {
            Rectangle rect = Utils.GetScreen(op);

            for (int x = rect.X; x < rect.Right; x++)
            {
                // 清空区域
                for (int y = rect.Y; y < rect.Bottom; y++)
                {
                    Main.tile[x, y].ClearEverything();
                }

                // 下方一格填充草方块
                Main.tile[x, rect.Y + 35].type = TileID.Grass;
                Main.tile[x, rect.Y + 35].active(true);

                // 下方填土
                for (int cy = rect.Y + 36; cy < rect.Bottom; cy++)
                {
                    Main.tile[x, cy].type = TileID.Dirt;
                    Main.tile[x, cy].active(true);
                    //Main.tile[cx, cy].slope(0);
                    //Main.tile[cx, cy].halfBrick(false);
                }
            }
        }
        #endregion


        #region 盾构机
        static Task AsyncGenShieldMachine(TSPlayer op, int posX, int posY, int w, int h, bool isRight = true)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                GenShieldMachine(posX, posY, w, h, isRight);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"已盾构出{w}x{h}区域，用时{second}秒。");
            });
        }
        static void GenShieldMachine(int posX, int posY, int w, int h, bool isRight = true)
        {
            int Xstart = posX;
            int Xend;
            int Ystart = posY;
            int Width = Math.Max(3, w);
            int height = Math.Max(3, h);

            if (isRight)
            {
                Xend = Xstart + Width;
            }
            else
            {
                Xend = Xstart + 2;
                Xstart -= Width;
            }

            Parallel.For(Xstart, Xend, (cx) =>
            {
                Parallel.For(Ystart - height, Ystart, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();
                });

                WorldGen.PlaceTile(cx, Ystart, 19, false, true, -1, 43);
            });
        }
        #endregion

        #region 挖掘机
        static Task AsyncDigArea(TSPlayer op, int posX, int posY, int w, int h, bool isRight = true, bool isHell = false)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                DigArea(posX, posY, w, h, isRight, isHell);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"已挖掘出{w}x{h}区域，用时{second}秒。");
            });
        }
        static void DigArea(int posX, int posY, int w, int h, bool isRight = true, bool isHell = false)
        {
            int Width = Math.Max(3, w);
            int height = Math.Max(3, h);
            int Xstart = posX - (Width / 2) + 1;
            int Ystart = posY;

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, Ystart + height, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();
                });
            });

            Parallel.For(posX, posX + 2, (cx) =>
            {
                WorldGen.PlaceTile(cx, Ystart, 19, false, true, -1, 43);
            });
        }
        #endregion


        #region 沙漠地形
        public static async void AsyncDesertWorld(TSPlayer op)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    //DesertBiome desertBiome = configuration.CreateBiome<DesertBiome>();
                    //desertBiome.Place(new Point(400+i*700, op.TileY), structures);
                    WorldGen.makeTemple(op.TileX, op.TileY);
                }

            }).ContinueWith((d) =>
            {
                TileHelper.FinishGen();
                TileHelper.InformPlayers();
                op.SendSuccessMessage("创建沙漠地形完成");
            });
        }

        private static UnifiedRandom genRand = WorldGen.genRand;

        #endregion


        #region 字符画
        ///// <summary>
        ///// 字符画
        ///// </summary>
        ///// <param name="op"></param>
        //public static void CharPaint(TSPlayer op)
        //{
        //    string[] chars = File.ReadAllLines(Path.Combine(Utils.SaveDir, "raw_0.txt"));
        //    int x = op.TileX - 61;
        //    int y = op.TileY - 30;
        //    int col;
        //    int row = 0;
        //    int maxCol = 0;
        //    RandomTool.ResetTileMapping();
        //    foreach (string s in chars)
        //    {
        //        row++;
        //        col = 0;
        //        foreach (char c in s)
        //        {
        //            col++;
        //            if (maxCol < col) maxCol = col;
        //            if (c != ' ')
        //            {
        //                RandomTool.RandomTile(x + col, y + row);
        //            }
        //        }
        //    }
        //    TileHelper.InformPlayers();
        //}
        #endregion


        //public static void ShowAll(TSPlayer op)
        //{
        //    AsyncShowAll(op);
        //}

        //public static async void AsyncShowAll(TSPlayer op)
        //{
        //    int secondLast = utils.GetUnixTimestamp;

        //    await Task.Run(() =>
        //    {
        //        for (int x = 0; x < Main.maxTilesX; x++)
        //        {
        //            for (int y = 0; y < Main.maxTilesY; y++)
        //            {
        //                WorldGen.SquareTileFrame(x, y);
        //                NetMessage.SendTileSquare(-1, x, y);
        //            }
        //        }
        //    }).ContinueWith((d) =>
        //    {
        //        op.SendInfoMessage("操作完成！");
        //    });
        //}


        //#region trees
        //public static List<ushort> trees = new List<ushort>{
        //    TileID.Trees,
        //    TileID.TreeAmber,
        //    TileID.TreeAmethyst,
        //    TileID.TreeDiamond,
        //    TileID.TreeEmerald,
        //    TileID.TreeRuby,
        //    TileID.TreeSapphire,
        //    TileID.TreeTopaz,
        //    TileID.MushroomTrees,
        //    TileID.PalmTree,
        //    TileID.VanityTreeYellowWillow,
        //    TileID.VanityTreeSakura
        //};
        //#endregion

    }
}