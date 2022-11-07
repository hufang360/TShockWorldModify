using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;
using TShockAPI;


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
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber)) return;

                List<string> lines = new()
                {
                    "/igen room [数量]，小房间（默认生成3个）",
                    "/igen hotel，NPC小旅馆",
                    "/igen hell，地狱直通车",
                    "/igen pond [水/蜂蜜/岩浆/微光]，鱼池",

                    "/igen dirt，填土",
                    "/igen place help，放置",
                    "/igen sm [宽] [高]，盾构机（默认 宽61 高34）",
                    "/igen dig [宽] [高]，钻井机（默认 宽3 高34）",

                    "/igen selection help，选区工具",
                    "/igen replace help，替换工具",
                    "/igen fill help，填充工具",
                    "/igen clear help，清除工具",

                    "/igen count help，统计工具",

                    "/igen world help，重建世界",
                    "/igen random，全图随机",
                    "",
                    "",
                };
                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "/igen 指令用法 ({0}/{1})：",
                    FooterFormat = "输入 /igen help {{0}} 查看更多".SFormat(Commands.Specifier)
                });

                //op.SendInfoMessage("/igen hammer help，锤子工具");
            }
            if (args.Parameters.Count == 0)
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
                case "help": Help(); break;
                default: op.SendErrorMessage("语法错误，请输入 /igen help 查询帮助"); break; ;

                // 重建世界
                case "world":
                case "w":
                    WorldTool.Manage(args);
                    return;

                #region info
                case "info":
                    if (NeedInGame()) return;
                    int cx = op.TileX;
                    int cy = op.TileY + 3;
                    op.SendInfoMessage($"pos:{op.TileX},{op.TileY} || {op.TPlayer.position.X},{op.TPlayer.position.Y}");
                    op.SendInfoMessage($"type:{Main.tile[cx, cy].type}");
                    op.SendInfoMessage($"wall:{Main.tile[cx, cy].wall}");
                    op.SendInfoMessage($"frameX:{Main.tile[cx, cy].frameX}");
                    op.SendInfoMessage($"frameY:{Main.tile[cx, cy].frameY}");
                    op.SendInfoMessage($"blockType:{Main.tile[cx, cy].blockType()}");
                    op.SendInfoMessage($"slope:{Main.tile[cx, cy].slope()}");
                    break;
                #endregion


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
                    await AsyncGenRoom(op, op.TileX, op.TileY + 3, total, isRight, true);
                    return;

                // 小旅馆
                case "hotel":
                    if (NeedInGame() || NeedWaitTask()) return;
                    isRight = op.TPlayer.direction != -1;
                    total = Math.Max(3, NPCHelper.GetRelive().Count);
                    await AsyncGenHotel(op, op.TileX, op.TileY + 3, total, isRight, true);
                    return;
                #endregion


                #region 鱼池
                case "pond":
                    if (NeedInGame() || NeedWaitTask()) return;
                    int type = 0;
                    if (args.Parameters.Count > 1)
                    {
                        switch (args.Parameters[1].ToLowerInvariant())
                        {
                            case "水": case "water": type = 0; break;
                            case "岩浆": case "lava": type = 1; break;
                            case "蜂蜜": case "honey": type = 2; break;
                            case "微光": case "shimmer": type = 3; break;

                            default:
                                op.SendErrorMessage("鱼池风格不对");
                                return;
                        }
                    }
                    await AsyncGenPond(op, op.TileX, op.TileY + 3, type);
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
                case "dig":
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
                    await AsyncGenHellevator(op, op.TileX, op.TileY + 3);
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
                    await AsyncPlaceDirt(op, op.TileX, op.TileY + 3);
                    return;
                #endregion

                #region 打洞
                case "hole":
                    if (NeedInGame() || NeedWaitTask()) return;
                    ClearTool.Hole(op);
                    break;
                #endregion


                #region 放置
                case "place":
                case "p":
                    if (NeedInGame() || NeedWaitTask()) return;
                    PlaceTool.Manage(args);
                    return;
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

                // 打破
                case "broken":
                case "b":
                    if (NeedInGame() || NeedWaitTask()) return;
                    BrokenTool.Manage(args);
                    break;

                // 统计
                case "count":
                    CountTool.Manage(args);
                    break;
                #endregion

                #region 未完成
                case "report":
                    //ReportTool.Manage(args);
                    op.SendInfoMessage("这个功能还没写好");
                    break;
                // 全图沙漠化
                case "egypt":
                    if (NeedInGame() || NeedWaitTask()) return;
                    //Regen.AsyncDesertWorld(op);
                    op.SendInfoMessage("这个功能还没写好");
                    break;

                case "char":
                    if (NeedInGame() || NeedWaitTask()) return;
                    //Regen.CharPaint(op);
                    op.SendInfoMessage("这个功能还没写好");
                    break;

                // 像素画
                case "paint":
                    if (NeedInGame() || NeedWaitTask()) return;
                    PaintTool.Manage(op);
                    break;
                    #endregion
            }

        }


        #region 小房间
        /// <summary>
        /// NPC旅馆
        /// </summary>
        static Task AsyncGenHotel(TSPlayer op, int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                int w = 5;
                int row = 6;
                int roomWidth = 1 + Math.Min(row, total) * w;

                int fixPosX = needCenter ? posX - (roomWidth / 2) : posX;
                int startX;
                int startY;
                for (int i = 0; i < total; i++)
                {
                    startX = fixPosX + i % row * w;
                    startY = posY - (int)Math.Floor((float)(i / row)) * 10;
                    GenRoom(startX, startY, isRight);
                }
                int x1 = fixPosX - 1;
                int x2 = fixPosX + roomWidth;
                int y = posY - 1;
                WorldGen.PlaceTile(x1, y, 19, false, true, -1, 14);
                WorldGen.PlaceTile(x2, y, 19, false, true, -1, 14);
                Main.tile[x1, y].slope(2);
                Main.tile[x2, y].slope(1);
                WorldGen.SquareTileFrame(x1, y);
                WorldGen.SquareTileFrame(x2, y);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"已生成 NPC小旅馆（共{total}间 用时 {second}s）");
            });
        }

        /// <summary>
        /// 小房间
        /// </summary>
        static Task AsyncGenRoom(TSPlayer op, int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                int w = 5;
                int roomWidth = 1 + total * w;

                int startX = needCenter ? posX - (roomWidth / 2) : posX;
                //Console.WriteLine($"npctotal:{total} posX:{posX} posY:{posY} startX:{startX}");
                for (int i = 0; i < total; i++)
                {
                    GenRoom(startX, posY, isRight);
                    startX += w;
                }
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"已生成 {total} 个小房间 （用时 {second}s）");
            });
        }
        static void GenRoom(int posX, int posY, bool isRight = true)
        {
            RoomTheme th = new();
            th.SetGlass();

            ushort tileID = th.tile;
            ushort wallID = th.wall;
            TileInfo platform = th.platform;
            TileInfo chair = th.chair;
            TileInfo bench = th.bench;
            TileInfo torch = th.torch;

            int Xstart = posX;
            int Ystart = posY;
            int Width = 6;
            int height = 10;

            if (!isRight)
                Xstart += 2;

            for (int cx = Xstart; cx < Xstart + Width; cx++)
            {
                for (int cy = Ystart - height; cy < Ystart; cy++)
                {
                    ITile tile = Main.tile[cx, cy];
                    tile.ClearEverything();

                    // 墙
                    if ((cx > Xstart) && (cy < Ystart - 5) && (cx < Xstart + Width - 1) && (cy > Ystart - height))
                    {
                        tile.wall = wallID;
                    }

                    if ((cx == Xstart && cy > Ystart - 5)
                    || (cx == Xstart + Width - 1 && cy > Ystart - 5)
                    || (cy == Ystart - 1))
                    {
                        // 平台
                        WorldGen.PlaceTile(cx, cy, platform.id, false, true, -1, platform.style);
                    }

                    else if ((cx == Xstart) || (cx == Xstart + Width - 1)
                    || (cy == Ystart - height)
                    || (cy == Ystart - 5))
                    {
                        // 方块
                        tile.type = tileID;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                    }
                }
            }

            if (isRight)
            {
                WorldGen.PlaceTile(Xstart + 1, Ystart - 6, chair.id, false, true, 0, chair.style);  // 椅子
                Main.tile[Xstart + 1, posY - 6].frameX += 18;
                Main.tile[Xstart + 1, posY - 7].frameX += 18;

                WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.id, false, true, -1, bench.style); // 工作台
                WorldGen.PlaceTile(Xstart + 4, Ystart - 5, torch.id, false, true, -1, torch.style); // 火把
            }
            else
            {
                WorldGen.PlaceTile(Xstart + 4, Ystart - 6, chair.id, false, true, 0, chair.style);
                WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.id, false, true, -1, bench.style);
                WorldGen.PlaceTile(Xstart + 1, Ystart - 5, torch.id, false, true, -1, torch.style);
            }
        }
        #endregion

        #region 鱼池
        static Task AsyncGenPond(TSPlayer op, int posX, int posY, int style)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                GenPond(posX, posY, style);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                string desc;
                switch (style)
                {
                    case 1: desc = "岩浆"; break;
                    case 2: desc = "蜂蜜"; break;
                    case 3: desc = "微光"; break;
                    default: desc = "普通"; break;
                }
                op.SendSuccessMessage($"已生成 {desc}鱼池（用时 {second}s）");
            });
        }
        static void GenPond(int posX, int posY, int style)
        {
            PondTheme th = new PondTheme();
            switch (style)
            {
                default: th.SetGlass(); break;
                case 1: th.SetObsidian(); break;
                case 2: th.SetHoney(); break;
                case 3: th.SetGray(); break;
            }

            ushort tileID = th.tile;
            ushort wallID = th.wall;
            TileInfo platform = th.platform;

            int Xstart = posX - 6;
            int Ystart = posY;
            int Width = 11 + 2;
            int height = 30 + 2;

            for (int cx = Xstart; cx < Xstart + Width; cx++)
            {
                for (int cy = Ystart; cy < Ystart + height; cy++)
                {
                    ITile tile = Main.tile[cx, cy];
                    tile.ClearEverything();

                    if ((cx == Xstart) || (cx == Xstart + Width - 1) || (cy == Ystart + height - 1))
                    {
                        tile.type = tileID;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                    }
                }

                WorldGen.PlaceTile(cx, Ystart, platform.id, false, true, -1, platform.style);
            }

            for (int cx = Xstart + 1; cx < Xstart + Width - 1; cx++)
            {
                for (int cy = Ystart + 1; cy < Ystart + height - 1; cy++)
                {
                    ITile tile = Main.tile[cx, cy];
                    tile.active(active: false);
                    switch (style)
                    {
                        case 2: tile.honey(honey: true); break;
                        case 3: tile.lava(lava: true); break;
                        case 4: tile.shimmer(shimmer: true); break;
                    }
                    tile.liquid = byte.MaxValue;
                }
            }
        }
        #endregion

        #region 地狱直通车
        static Task AsyncGenHellevator(TSPlayer op, int posX, int posY)
        {
            int secondLast = Utils.GetUnixTimestamp;
            int height = 0;
            return Task.Run(() =>
            {
                height = GenHellevator(posX, posY);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"已生成 地狱直通车（高{height}格  用时 {second}s）");
            });
        }
        static int GenHellevator(int posX, int posY)
        {
            int hell = 0;
            int xtile;
            for (hell = Main.UnderworldLayer + 10; hell <= Main.maxTilesY - 100; hell++)
            {
                xtile = posX;
                Parallel.For(posX, posX + 8, (cwidth, state) =>
                {
                    if (Main.tile[cwidth, hell].active() && !Main.tile[cwidth, hell].lava())
                    {
                        state.Stop();
                        xtile = cwidth;
                        return;
                    }
                });

                if (!Main.tile[xtile, hell].active()) break;
            }

            int Width = 5;
            int height = hell;
            int Xstart = posX - 2;
            int Ystart = posY;

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, hell, (cy) =>
                {
                    ITile tile = Main.tile[cx, cy];
                    tile.ClearEverything();

                    if (cx == Xstart + Width / 2)
                    {
                        tile.type = TileID.SilkRope;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                    }
                    else if (cx == Xstart || cx == Xstart + Width - 1)
                    {
                        tile.type = TileID.ObsidianBrick;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                    }
                });
            });

            // 平台
            WorldGen.PlaceTile(Xstart + 1, Ystart, 19, false, true, -1, 13);
            WorldGen.PlaceTile(Xstart + 2, Ystart, 19, false, true, -1, 13);
            WorldGen.PlaceTile(Xstart + 3, Ystart, 19, false, true, -1, 13);

            return hell;
        }
        #endregion

        #region 填土
        static Task AsyncPlaceDirt(TSPlayer op, int posX, int posY)
        {
            int secondLast = Utils.GetUnixTimestamp;
            return Task.Run(() =>
            {
                PlaceDirt(posX, posY);
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
                int second = Utils.GetUnixTimestamp - secondLast;
                op.SendSuccessMessage($"填土完成（用时 {second}s）");
            });
        }
        static void PlaceDirt(int posX, int posY)
        {
            int Width = 121;
            int height = 33;
            int Xstart = posX - 60;
            int Ystart = posY;

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart - height - 2, Ystart, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();
                });

                Parallel.For(Ystart, Ystart + height, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();

                    Main.tile[cx, cy].type = TileID.Dirt;
                    Main.tile[cx, cy].active(true);
                    Main.tile[cx, cy].slope(0);
                    Main.tile[cx, cy].halfBrick(false);
                });

            });
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
                op.SendSuccessMessage($"盾构完成 （{w}x{h}格 用时 {second}s）");
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
                op.SendSuccessMessage($"挖掘完成 （{w}x{h}格 用时 {second}s）");
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
        /// <summary>
        /// 字符画
        /// </summary>
        /// <param name="op"></param>
        public static void CharPaint(TSPlayer op)
        {
            string[] chars = File.ReadAllLines(Path.Combine(Utils.SaveDir, "raw_0.txt"));
            int x = op.TileX - 61;
            int y = op.TileY - 30;
            int col;
            int row = 0;
            int maxCol = 0;
            RandomTool.ResetTileMapping();
            foreach (string s in chars)
            {
                row++;
                col = 0;
                foreach (char c in s)
                {
                    col++;
                    if (maxCol < col) maxCol = col;
                    if (c != ' ')
                    {
                        RandomTool.RandomTile(x + col, y + row);
                    }
                }
            }
            TileHelper.InformPlayers();
        }
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