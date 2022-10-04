using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    class ReGenHelper
    {
        public static bool isTaskRunning { get; set; }
        public static int realcount { get; set; }

        public async static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            void helpText()
            {
                op.SendInfoMessage("/igen world [种子] [腐化] [大小] [彩蛋特性], 重建地图");
                op.SendInfoMessage("/igen room <数量>，生成玻璃小房间（默认生成3个）");
                op.SendInfoMessage("/igen pond，玻璃鱼池");
                op.SendInfoMessage("/igen hell，地狱直通车");
                op.SendInfoMessage("/igen sm <w> <h>，盾构机");
                op.SendInfoMessage("/igen dig <w> <h>，钻井机");
                op.SendInfoMessage("/igen dirt，填土");
                op.SendInfoMessage("========");
                op.SendInfoMessage("/igen random help，随机");

                op.SendInfoMessage("/igen selection help，选区工具");
                op.SendInfoMessage("/igen replace help，替换工具");
                op.SendInfoMessage("/igen fill help，填充工具");
                op.SendInfoMessage("/igen clear help，擦除");
                //op.SendInfoMessage("/igen hammer help，锤子工具");

                //op.SendInfoMessage("/igen place help，放置工具");
                //op.SendInfoMessage("/igen place 附魔剑，放置附魔剑");
                //op.SendInfoMessage("/igen place <id> [style]，放置图格");
            }
            if (args.Parameters.Count == 0)
            {
                helpText();
                return;
            }

            bool isRight;
            int w;
            int h;
            int num;
            Rectangle selection = SelectionTool.GetSelection(op.Index);
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help": helpText(); break;
                default: op.SendErrorMessage("语法错误，请输入 /igen help 查询帮助"); break; ;

                // 重建世界
                case "world":
                case "w":
                    if (NeedWaitTask(op)) return;
                    WorldTool.Manage(args);
                    return;

                #region info
                case "info":
                    if (NeedInGame(op)) return;
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


                // 玻璃小房间
                case "room":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    int total = 3;
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
                    await AsyncGenRoom(op.TileX, op.TileY + 4, total, isRight, true);
                    return;


                // 鱼池
                case "pond":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    await AsyncGenPond(op.TileX, op.TileY + 3);
                    return;


                // 盾构机
                case "shieldmachine":
                case "sm":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
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
                    await AsyncGenShieldMachine(op.TileX, op.TileY + 3, w, h, isRight);
                    return;


                // 挖掘机
                case "dig":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    isRight = op.TPlayer.direction != -1;
                    w = 3;
                    h = 34;
                    if (utils.TryParseInt(args.Parameters, 1, out num)) w = Math.Max(3, num);
                    if (utils.TryParseInt(args.Parameters, 2, out num)) h = Math.Max(34, num);
                    await AsyncDigArea(op.TileX, op.TileY + 3, w, h, isRight);
                    return;


                // 地狱直通车
                case "hell":
                    await AsyncGenHellevator(op.TileX, op.TileY + 3);
                    FinishGen();
                    InformPlayers();
                    op.SendSuccessMessage("创建地狱直通车结束");
                    return;


                // 填土
                case "dirt":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    await AsyncPlaceDirt(op.TileX, op.TileY + 3);
                    return;


                #region 带区域操作
                // 设置选区
                case "selection":
                case "s":
                    if (NeedInGame(op)) return;
                    args.Parameters.RemoveAt(0);
                    SelectionTool.RectMange(args);
                    break;

                // 替换
                case "replace":
                case "r":
                    if (NeedInGame(op)) return;
                    args.Parameters.RemoveAt(0);
                    ReplaceTool.Manage(args);
                    break;

                // 填充
                case "fill":
                case "f":
                    if (NeedInGame(op)) return;
                    args.Parameters.RemoveAt(0);
                    FillTool.Manage(args);
                    break;

                // 清空
                case "clear":
                case "c":
                    if (NeedInGame(op)) return;
                    args.Parameters.RemoveAt(0);
                    ClearTool.Manage(args);
                    break;
                #endregion


                #region 全图沙漠化
                case "egypt":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    //Regen.AsyncDesertWorld(op);
                    op.SendInfoMessage("这个功能还没写好");
                    break;

                case "char":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    //Regen.CharPaint(op);
                    op.SendInfoMessage("这个功能还没写好");
                    break;
                #endregion


                #region 随机
                case "random":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    bool needAll = selection.Width == Main.maxTilesX && selection.Height == Main.maxTilesY;
                    if (needAll)
                        AsyncRandomAll(op);
                    else
                        AsyncRandomArea(3, op);
                    break;
                #endregion


                // 像素画
                case "paint":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    PaintTool.Manage(op);
                    break;
            }

        }


        public static bool NeedWaitTask(TSPlayer op)
        {
            if (isTaskRunning)
            {
                if (op != null) op.SendErrorMessage("另一个创建任务正在执行，请稍后再操作");
            }
            return isTaskRunning;
        }

        public static bool NeedInGame(TSPlayer op)
        {
            return utils.NeedInGame(op);
        }



        #region 一些方法
        // 清理图格
        public static void ClearTile(int x, int y, int w = 1, int h = 1)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Main.tile[x + i, y + j].ClearTile();
                    NetMessage.SendTileSquare(-1, x + i, y + j);
                }
            }
        }

        // 挖方块
        public static void BreakTile(int x, int y)
        {
            Main.tile[x, y].ClearTile();
            WorldGen.SquareTileFrame(x, y);
            NetMessage.SendTileSquare(-1, x, y);
        }

        // 清除方块
        public static void Clear(int x, int y)
        {
            Main.tile[x, y].ClearEverything();
            WorldGen.SquareTileFrame(x, y);
            NetMessage.SendTileSquare(-1, x, y);
        }

        /// <summary>
        /// 完成创建
        /// </summary>
        public static void FinishGen(bool needSound = true)
        {
            isTaskRunning = false;
            if (needSound)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player != null && (player.Active))
                    {
                        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(player.TPlayer.position, 1, 0, 10, -16));
                    }
                }
            }
            TShock.Utils.SaveWorld();
        }
        // 更新物块
        public static void InformPlayers()
        {
            foreach (TSPlayer person in TShock.Players)
            {
                if ((person != null) && (person.Active))
                {
                    for (int i = 0; i < 255; i++)
                    {
                        for (int j = 0; j < Main.maxSectionsX; j++)
                        {
                            for (int k = 0; k < Main.maxSectionsY; k++)
                            {
                                Netplay.Clients[i].TileSections[j, k] = false;
                            }
                        }
                    }
                }
            }
        }

        public static void InformSections(Rectangle rect)
        {
            int w = Main.sectionWidth;
            int h = Main.sectionHeight;

            int xStart = Math.Max(0, rect.X / w - 1);
            int yStart = Math.Max(0, rect.Y / h - 1);
            int xEnd = (int)Math.Ceiling((float)(rect.Right / w)) + 1;
            int yEnd = (int)Math.Ceiling((float)(rect.Bottom / h)) + 1;
            xEnd = Math.Min(Main.maxTilesX, xEnd);
            yEnd = Math.Min(Main.maxTilesY, yEnd);

            foreach (TSPlayer person in TShock.Players.Where(p => p != null))
            {
                if (person.Active)
                {
                    for (int i = 0; i < 255; i++)
                    {
                        for (int x = xStart; x <= xEnd; x++)
                        {
                            for (int y = yStart; y <= yEnd; y++)
                            {
                                Netplay.Clients[i].TileSections[x, y] = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion


        #region 生成小房间
        public static Task AsyncGenRoom(int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
        {
            isTaskRunning = true;
            return Task.Run(() => GenRooms(posX, posY, total, isRight, needCenter)).ContinueWith((d) => FinishGen());
        }

        public static void GenRooms(int posX, int posY, int total, bool isRight = true, bool needCenter = false)
        {
            int w = 5;
            int roomWidth = 1 + total * w;

            int startX = needCenter ? posX - (roomWidth / 2) : posX;
            Console.WriteLine($"npctotal:{total} posX:{posX} posY:{posY} startX:{startX}");
            for (int i = 0; i < total; i++)
            {
                GenRoom(startX, posY, isRight);
                startX += w;
            }
        }

        public static void GenRoom(int posX, int posY, bool isRight = true)
        {
            RoomTheme th = RoomTheme.GetGray();

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

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart - height, Ystart, (cy) =>
                {
                    ITile tile = Main.tile[cx, cy];
                    // 清空区域
                    tile.ClearEverything();
                    tile.wall = 0;

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
                });
            });

            if (isRight)
            {
                // 椅子
                WorldGen.PlaceTile(Xstart + 1, Ystart - 6, chair.id, false, true, 0, chair.style);
                Main.tile[Xstart + 1, posY - 6].frameX += 18;
                Main.tile[Xstart + 1, posY - 7].frameX += 18;

                // 工作台
                WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.id, false, true, -1, bench.style);

                // 火把
                WorldGen.PlaceTile(Xstart + 4, Ystart - 5, torch.id, false, true, -1, torch.style);
            }
            else
            {
                WorldGen.PlaceTile(Xstart + 4, Ystart - 6, chair.id, false, true, 0, chair.style);
                WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.id, false, true, -1, bench.style);
                WorldGen.PlaceTile(Xstart + 1, Ystart - 5, torch.id, false, true, -1, torch.style);
            }

            InformPlayers();
        }
        #endregion


        #region 盾构机
        public static Task AsyncGenShieldMachine(int posX, int posY, int w, int h, bool isRight = true)
        {
            isTaskRunning = true;
            return Task.Run(() => GenShieldMachine(posX, posY, w, h, isRight)).ContinueWith((d) => FinishGen());
        }
        public static void GenShieldMachine(int posX, int posY, int w, int h, bool isRight = true)
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
                    if (Main.tile[cx, cy].active())
                        Main.tile[cx, cy].ClearEverything();
                });

                WorldGen.PlaceTile(cx, Ystart, 19, false, true, -1, 43);
            });

            InformPlayers();
        }
        #endregion

        #region 挖掘机
        public static Task AsyncDigArea(int posX, int posY, int w, int h, bool isRight = true, bool isHell = false)
        {
            isTaskRunning = true;
            return Task.Run(() => DigArea(posX, posY, w, h, isRight, isHell)).ContinueWith((d) => FinishGen());
        }
        public static void DigArea(int posX, int posY, int w, int h, bool isRight = true, bool isHell = false)
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

            InformPlayers();
        }
        #endregion

        #region 生成鱼池
        public static Task AsyncGenPond(int posX, int posY)
        {
            isTaskRunning = true;
            return Task.Run(() => GenPond(posX, posY)).ContinueWith((d) => FinishGen());
        }
        public static void GenPond(int posX, int posY)
        {
            RoomTheme th = RoomTheme.GetGray();

            ushort tileID = th.tile;
            ushort wallID = th.wall;
            TileInfo platform = th.platform;

            int Xstart = posX - 6;
            int Ystart = posY;
            int Width = 11 + 2;
            int height = 30 + 2;

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, Ystart + height, (cy) =>
                {
                    ITile tile = Main.tile[cx, cy];
                    if (tile.active())
                    {
                        tile.ClearEverything();
                    }
                    tile.wall = 0;

                    if ((cx == Xstart) || (cx == Xstart + Width - 1) || (cy == Ystart + height - 1))
                    {
                        // 方块
                        tile.type = tileID;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                    }
                });

                WorldGen.PlaceTile(cx, Ystart, platform.id, false, true, -1, platform.style);
            });

            Parallel.For(Xstart + 1, Xstart + Width - 1, (cx) =>
            {
                Parallel.For(Ystart + 1, Ystart + height - 1, (cy) =>
                {
                    ITile tile = Main.tile[cx, cy];

                    tile.active(active: false);
                    tile.liquid = byte.MaxValue;
                    WorldGen.SquareTileFrame(cx, cy);

                    //tile.type = TileID.Dirt;
                    //tile.active(true);
                    //tile.slope(0);
                    //tile.halfBrick(false);
                });
            });

            InformPlayers();
        }
        #endregion

        #region 生成地狱直通车
        public static Task AsyncGenHellevator(int posX, int posY)
        {
            isTaskRunning = true;
            return Task.Run(() => GenHellevator(posX, posY));
        }
        private static void GenHellevator(int posX, int posY)
        {
            int hell;
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
            //utils.Log($"地狱的高度：{height}");
            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, hell, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();

                    if (trees.Contains(Main.tile[cx, cy - 1].type))
                        Main.tile[cx, cy - 1].ClearEverything();
                });
            });

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart, hell, (cy) =>
                {
                    if (cx == Xstart + Width / 2)
                    {
                        Main.tile[cx, cy].type = TileID.SilkRope;
                        Main.tile[cx, cy].active(true);
                        Main.tile[cx, cy].slope(0);
                        Main.tile[cx, cy].halfBrick(false);
                    }
                    else if (cx == Xstart || cx == Xstart + Width - 1)
                    {
                        Main.tile[cx, cy].type = TileID.GrayBrick;  // 灰砖 
                        Main.tile[cx, cy].active(true);
                        Main.tile[cx, cy].slope(0);
                        Main.tile[cx, cy].halfBrick(false);
                    }
                });
            });
            // 平台
            WorldGen.PlaceTile(Xstart + 1, Ystart, 19, false, true, -1, 43);
            WorldGen.PlaceTile(Xstart + 2, Ystart, 19, false, true, -1, 43);
            WorldGen.PlaceTile(Xstart + 3, Ystart, 19, false, true, -1, 43);
        }
        #endregion

        #region 填土
        public static Task AsyncPlaceDirt(int posX, int posY)
        {
            isTaskRunning = true;
            return Task.Run(() => PlaceDirt(posX, posY)).ContinueWith((d) => FinishGen());
        }
        public static void PlaceDirt(int posX, int posY)
        {
            int Width = 121;
            int height = 68;
            int Xstart = posX - 60;
            int Ystart = posY;

            Parallel.For(Xstart, Xstart + Width, (cx) =>
            {
                Parallel.For(Ystart - height, Ystart, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();
                });

                Parallel.For(Ystart, Ystart + height, (cy) =>
                {
                    Main.tile[cx, cy].type = TileID.Dirt;
                    Main.tile[cx, cy].active(true);
                    Main.tile[cx, cy].slope(0);
                    Main.tile[cx, cy].halfBrick(false);
                });

            });

            InformPlayers();
        }
        #endregion

        #region 清空区域
        public static Task AsyncClearArea(Rectangle area, Point center, bool ClearAll = false)
        {
            isTaskRunning = true;
            return Task.Run(() => ClearArea(area, center, ClearAll)).ContinueWith((d) => FinishGen());
        }
        private static void ClearArea(Rectangle area, Point center, bool ClearAll = false)
        {
            if (ClearAll)
                utils.Log($"清空全图");
            else
                utils.Log($"清除区域：{area.X},{area.Y} {area.Width}:{area.Height}, {center.X} {center.Y}");
            if (ClearAll)
            {
                Parallel.For(0, Main.maxTilesX, (cx) =>
                {
                    Parallel.For(0, Main.maxTilesY, (cy) =>
                    {
                        Main.tile[cx, cy].ClearEverything();
                    });
                });
                return;
            }
            Parallel.For(area.X, area.Right, (cx) =>
            {
                Parallel.For(area.Y, area.Bottom, (cy) =>
                {
                    Main.tile[cx, cy].ClearEverything();
                });
            });

            Parallel.For(center.X + 1, center.X + 2, (cx) =>
            {
                WorldGen.PlaceTile(cx, center.Y, 19, false, true, -1, 43);
            });
        }
        #endregion

        public static Rectangle lastArea = new Rectangle();

        #region 范围随机
        /// <summary>
        /// 范围随机
        /// </summary>
        /// <param name="style">1 小范围随机 2 大范围随机 3 屏幕范围内随机</param>
        /// <param name="op"></param>
        public static async void AsyncRandomArea(int style = 1, TSPlayer op = null, bool needTP = false)
        {
            ReGenHelper.isTaskRunning = true;
            int w = style == 1 ? 20 : 200;
            int h = style == 1 ? 15 : 150;

            Rectangle rect = new Rectangle(0, 0, w, h);
            if (style != 3)
            {
                Random rd = new Random((int)DateTime.Now.Ticks);
                rect.X = rd.Next(0, Main.maxTilesX);
                rect.Y = rd.Next(Math.Max(10, Main.spawnTileY - 50), Main.maxTilesY);
                if (rect.X > Main.maxTilesX - w) rect.X = Main.maxTilesX - w;
                if (rect.Y > Main.maxTilesY - h) rect.Y = Main.maxTilesY - h;
            }
            else
            {
                // 屏幕范围内随机
                if (op == null) return;
                if (utils.NeedInGame(op)) return;
                rect = new Rectangle(op.TileX - 61, op.TileY - 34 + 2, 122, 68);
            }

            await Task.Run(() => RandomArea(rect)).ContinueWith((d) =>
            {
                if (needTP && op != null && op.RealPlayer)
                {
                    op.Teleport(lastArea.X * 16, lastArea.Y * 16);
                    op.SendInfoMessage($"随机区域 坐标={lastArea.X},{lastArea.Y} 宽高={lastArea.Width}x{lastArea.Height}");
                }
                else if (op != null) op.SendSuccessMessage("区域随机完成");
                InformSections(rect);
                FinishGen();
            });
        }
        private static void RandomArea(Rectangle rect, TSPlayer op = null)
        {
            lastArea = rect;

            RandomTool.ResetTileMapping();
            for (int cx = rect.X; cx < rect.Right; cx++)
            {
                for (int cy = rect.Y; cy < rect.Bottom; cy++)
                {
                    RandomTool.RandomTile(cx, cy);
                }
            }
        }

        //private static List<int> RandomSkip()
        //{
        //    var nums = Enumerable.Range(0, 300).ToArray();
        //    var rnd = new Random();

        //    // Shuffle the array
        //    for (int i = 0; i < nums.Length; ++i)
        //    {
        //        int randomIndex = rnd.Next(nums.Length);
        //        int temp = nums[randomIndex];
        //        nums[randomIndex] = nums[i];
        //        nums[i] = temp;
        //    }

        //    return new List<int>(nums.Skip(100));
        //}

        /// <summary>
        /// 全图随机
        /// </summary>
        /// <param name="op"></param>
        public static async void AsyncRandomAll(TSPlayer op = null)
        {
            ReGenHelper.isTaskRunning = true;
            await Task.Run(() =>
            {
                RandomTool.ResetTileMapping();
                for (int cx = 0; cx < Main.maxTilesX; cx++)
                {
                    for (int cy = 0; cy < Main.maxTilesY; cy++)
                    {
                        RandomTool.RandomTile(cx, cy);
                    }
                }

            }).ContinueWith((d) =>
            {
                InformPlayers();
                FinishGen(true);
                if (op != null) op.SendSuccessMessage("全图随机完成");
            });
        }
        #endregion


        #region 沙漠地形
        public static async void AsyncDesertWorld(TSPlayer op)
        {
            ReGenHelper.isTaskRunning = true;
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
                InformPlayers();
                FinishGen(true);
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
            string[] chars = File.ReadAllLines(Path.Combine(utils.SaveDir, "raw_0.txt"));
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
            InformSections(new Rectangle(x, y, maxCol, row));
        }
        #endregion


        public static void ShowAll(TSPlayer op)
        {
            AsyncShowAll(op);
        }

        public static async void AsyncShowAll(TSPlayer op)
        {
            int secondLast = utils.GetUnixTimestamp;

            await Task.Run(() =>
            {
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        WorldGen.SquareTileFrame(x, y);
                        NetMessage.SendTileSquare(-1, x, y);
                    }
                }
            }).ContinueWith((d) =>
            {
                op.SendInfoMessage("操作完成！");
            });
        }


        #region trees
        public static List<ushort> trees = new List<ushort>{
            TileID.Trees,
            TileID.TreeAmber,
            TileID.TreeAmethyst,
            TileID.TreeDiamond,
            TileID.TreeEmerald,
            TileID.TreeRuby,
            TileID.TreeSapphire,
            TileID.TreeTopaz,
            TileID.MushroomTrees,
            TileID.PalmTree,
            TileID.VanityTreeYellowWillow,
            TileID.VanityTreeSakura
        };
        #endregion

        class RoomTheme
        {
            public ushort tile = TileID.Dirt;
            public ushort wall = WallID.Dirt;
            public TileInfo platform = new TileInfo(TileID.Platforms, 0);
            public TileInfo chair = new TileInfo(TileID.Chairs, 0);
            public TileInfo bench = new TileInfo(TileID.WorkBenches, 0);
            public TileInfo torch = new TileInfo(TileID.Torches, 0);

            public static RoomTheme GetGlass()
            {
                // 玻璃消耗 边框19 墙16-4 平台12-6 椅子1-4 工作台1-10 火把1-1凝胶1木材
                RoomTheme th = new RoomTheme
                {
                    tile = TileID.Glass,
                    wall = WallID.Glass
                };
                th.platform.style = 14;
                th.chair.style = 18;
                th.bench.style = 25;
                th.torch.style = TorchID.White;

                return th;
            }

            public static RoomTheme GetGray()
            {
                // 玻璃消耗 边框19 墙16-4 平台12-6 椅子1-4 工作台1-10 火把1-1凝胶1木材
                RoomTheme th = new RoomTheme
                {
                    tile = TileID.GrayBrick,
                    wall = WallID.GrayBrick
                };

                th.platform.style = 43;
                return th;
            }

            public static RoomTheme GetWood()
            {
                RoomTheme th = new RoomTheme
                {
                    tile = TileID.WoodBlock,
                    wall = WallID.Wood,
                };
                //th.platform.style = 0;
                //th.chair.style = 0;
                //th.bench.style = 0;
                //th.torch.style = 0;

                return th;
            }
        }

    }
}