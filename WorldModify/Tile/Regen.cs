using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Biomes;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using TShockAPI;


namespace WorldModify
{
    class Regen
    {
        #region GenWorld
        /// <summary>
        /// GenWorld
        /// 参考：https://github.com/Illuminousity/WorldRefill/blob/master/WorldRefill/WorldRefill.cs#L997
        /// </summary>
        /// <param name="op"></param>
        /// <param name="seedStr"></param>
        /// <param name="size"></param>
        /// <param name="evil"></param>
        /// <param name="eggStr"></param>
        public static async void GenWorld(TSPlayer op, string seedStr = "", int size = 0, int evil = -1, string eggStr = "")
        {
            if (GenHelper.NeedWaitTask(op)) return;

            BackupHelper.Backup(op, "GenWorld");
            if (!op.RealPlayer)
            {
                Console.WriteLine($"seed:{seedStr}");
                op.SendErrorMessage($"[i:556]世界正在解体~");
            }
            TSPlayer.All.SendErrorMessage("[i:556]世界正在解体~");
            int secondLast = utils.GetUnixTimestamp;

            // 设置创建参数
            ProcessSeeds(seedStr);
            ProcessEggSeeds(eggStr);
            seedStr = seedStr.ToLowerInvariant();
            if (string.IsNullOrEmpty(seedStr) || seedStr == "0")
                seedStr = "random";
            if (Main.ActiveWorldFileData.Seed == 5162020)
                seedStr = "5162020";

            if (seedStr == "random")
                Main.ActiveWorldFileData.SetSeedToRandom();
            else
                Main.ActiveWorldFileData.SetSeed(seedStr);

            // 大小 腐化
            int tilesX = 0;
            int tilesY = 0;
            int rawSize = -1;
            if (Main.maxTilesX == 4200 & Main.maxTilesY == 1200)
                rawSize = 1;
            else if (Main.maxTilesX == 6400 & Main.maxTilesY == 1800)
                rawSize = 2;
            else if (Main.maxTilesX == 8400 & Main.maxTilesY == 2400)
                rawSize = 3;

            if (size == 1)
            {
                tilesX = 4200;
                tilesY = 1200;
            }
            else if (size == 2)
            {
                tilesX = 6400;
                tilesY = 1800;
            }
            else if (size == 3)
            {
                tilesX = 8400;
                tilesY = 2400;
            }
            if (tilesX > 0)
            {
                Main.maxTilesX = tilesX;
                Main.maxTilesY = tilesY;
                Main.ActiveWorldFileData.SetWorldSize(tilesX, tilesY);
            }
            WorldGen.WorldGenParam_Evil = evil;

            // 开始创建
            if (!op.RealPlayer)
                op.SendErrorMessage($"[i:3061]世界正在重建（{WorldGen.currentWorldSeed}）");
            TSPlayer.All.SendErrorMessage($"[i:3061]世界正在重建（{WorldGen.currentWorldSeed}）");
            await AsyncGenerateWorld(Main.ActiveWorldFileData.Seed);
            GenHelper.isTaskRunning = false;

            // 创建完成
            int second = utils.GetUnixTimestamp - secondLast;
            string text = $"[i:3061]世界重建完成 （用时 {second}s, {WorldGen.currentWorldSeed}）；-）";
            TSPlayer.All.SendSuccessMessage(text);
            if (!op.RealPlayer) op.SendErrorMessage(text);

            if (rawSize != -1 && size != 0 && rawSize != size)
            {
                if (Main.ServerSideCharacter)
                {
                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player != null && player.IsLoggedIn && !player.IsDisabledPendingTrashRemoval)
                        {
                            player.SaveServerCharacter();
                        }
                    }
                }
                utils.Log("服务器已关闭：重建后的地图大小和之前不一样，为了稳定起见，请重新开服");
                TShock.Utils.StopServer(true, "服务器已关闭：地图大小和创建前不一样");
            }

            // 传送到出生点
            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null && plr.Active)
                {
                    plr.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
                }
            }
            FinishGen();
            InformPlayers();
        }
        #endregion

        #region 处理种子
        /// <summary>
        /// 处理秘密世界种子
        /// </summary>
        /// <param name="seed"></param>
        private static void ProcessSeeds(string seed)
        {
            // UIWorldCreation.ProcessSpecialWorldSeeds(seedStr);
            WorldGen.notTheBees = false;
            WorldGen.getGoodWorldGen = false;
            WorldGen.tenthAnniversaryWorldGen = false;
            WorldGen.dontStarveWorldGen = false;
            ToggleSpecialWorld(seed.ToLowerInvariant());
        }

        /// <summary>
        /// 处理彩蛋
        /// </summary>
        /// <param name="seedstr">例如：2020,2021,ftw</param>
        private static void ProcessEggSeeds(string seedstr)
        {
            string[] seeds = seedstr.ToLowerInvariant().Split(',');
            foreach (string newseed in seeds)
            {
                ToggleSpecialWorld(newseed);
            }
        }
        /// <summary>
        /// 开关秘密世界（创建器的属性）
        /// </summary>
        /// <param name="seed"></param>
        private static void ToggleSpecialWorld(string seed)
        {
            switch (seed)
            {
                case "2020":
                case "516":
                case "5162020":
                case "05162020":
                    Main.ActiveWorldFileData._seed = 5162020;
                    break;

                case "2021":
                case "5162011":
                case "5162021":
                case "05162011":
                case "05162021":
                case "celebrationmk10":
                    WorldGen.tenthAnniversaryWorldGen = true;
                    break;

                case "ntb":
                case "not the bees":
                case "not the bees!":
                    WorldGen.notTheBees = true;
                    break;

                case "ftw":
                case "for the worthy":
                    WorldGen.getGoodWorldGen = true;
                    break;

                case "dst":
                case "constant":
                case "theconstant":
                case "the constant":
                case "eye4aneye":
                case "eyeforaneye":
                    WorldGen.dontStarveWorldGen = true;
                    break;

                case "superegg":
                    Main.ActiveWorldFileData._seed = 5162020;

                    WorldGen.notTheBees = true;
                    WorldGen.getGoodWorldGen = true;
                    WorldGen.tenthAnniversaryWorldGen = true;
                    WorldGen.dontStarveWorldGen = true;
                    break;
            }
        }
        #endregion

        #region  AsyncGenerateWorld
        private static Task AsyncGenerateWorld(int seed)
        {
            GenHelper.isTaskRunning = true;
            WorldGen.clearWorld();
            return Task.Run(() => WorldGen.GenerateWorld(seed)).ContinueWith((d) => FinishGen());
        }
        #endregion

        #region 一些方法
        /// <summary>
        /// 完成创建
        /// </summary>
        public static void FinishGen(bool needSound = true)
        {
            GenHelper.isTaskRunning = false;
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
            GenHelper.isTaskRunning = true;
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
            RoomTheme th = RoomTheme.GetGlass();

            ushort tile = th.tile;
            ushort wall = th.wall;
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
                    // 清空区域
                    Main.tile[cx, cy].ClearEverything();

                    // 墙
                    if ((cx > Xstart) && (cy < Ystart - 5) && (cx < Xstart + Width - 1) && (cy > Ystart - height))
                    {
                        Main.tile[cx, cy].wall = wall;
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
                        Main.tile[cx, cy].type = tile;
                        Main.tile[cx, cy].active(true);
                        Main.tile[cx, cy].slope(0);
                        Main.tile[cx, cy].halfBrick(false);
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
            GenHelper.isTaskRunning = true;
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
            GenHelper.isTaskRunning = true;
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
            GenHelper.isTaskRunning = true;
            return Task.Run(() => GenPond(posX, posY)).ContinueWith((d) => FinishGen());
        }
        public static void GenPond(int posX, int posY)
        {
            RoomTheme th = RoomTheme.GetGlass();

            ushort tileID = th.tile;
            ushort wallID = th.wall;
            TileInfo platform = th.platform;
            TileInfo chair = th.chair;
            TileInfo bench = th.bench;
            TileInfo torch = th.torch;

            int Xstart = posX - 6;
            int Ystart = posY;
            int Width = 11 + 4;
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

                    if ((cx == Xstart) || (cx == Xstart + 1) || (cx == Xstart + Width - 1) || (cx == Xstart + Width - 2)
                    || (cy == Ystart + height - 1) || (cy == Ystart + height - 2))
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

            Parallel.For(Xstart + 2, Xstart + Width - 2, (cx) =>
                {
                    Parallel.For(Ystart + 1, Ystart + height - 2, (cy) =>
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
            GenHelper.isTaskRunning = true;
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
            GenHelper.isTaskRunning = true;
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
            GenHelper.isTaskRunning = true;
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


        #region 物块 随机/替换
        public static string SaveDir;
        public static Rectangle lastArea = new Rectangle();
        private static StructureMap structures = new StructureMap();
        private static WorldGenConfiguration configuration = WorldGenConfiguration.FromEmbeddedPath("Terraria.GameContent.WorldBuilding.Configuration.json");

        #region 范围随机
        /// <summary>
        /// 范围随机
        /// </summary>
        /// <param name="style">1 小范围随机 2 大范围随机 3 屏幕范围内随机</param>
        /// <param name="op"></param>
        public static async void AsyncRandomArea(int style = 1, TSPlayer op = null, bool needTP = false)
        {
            GenHelper.isTaskRunning = true;
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

            TileHelper.ResetTileMapping();
            for (int cx = rect.X; cx < rect.Right; cx++)
            {
                for (int cy = rect.Y; cy < rect.Bottom; cy++)
                {
                    TileHelper.RandomTile(cx, cy);
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
            GenHelper.isTaskRunning = true;
            await Task.Run(() =>
            {
                TileHelper.ResetTileMapping();
                for (int cx = 0; cx < Main.maxTilesX; cx++)
                {
                    for (int cy = 0; cy < Main.maxTilesY; cy++)
                    {
                        TileHelper.RandomTile(cx, cy);
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


        #region 图格替换
        public static async void AsyncUniReTile(TSPlayer op, int type = 1, bool needAll = false)
        {
            GenHelper.isTaskRunning = true;
            int secondLast = utils.GetUnixTimestamp;
            string GetOpString()
            {
                switch (type)
                {
                    case 1: return "图格替换";
                    case 2: return "冰河化";
                    case 3: return "冰融化";
                    default: return "图格修改";
                }
            }
            string opString = GetOpString();
            if (needAll) op.SendSuccessMessage($"全图{opString}开始");
            Rectangle rect = needAll ? utils.GetWorldArea() : utils.GetScreen(op);
            await Task.Run(() =>
            {
                ReplaceTile(rect, type, needAll);
            }).ContinueWith((d) =>
            {
                if (needAll)
                {
                    InformPlayers();
                    FinishGen(true);
                    op.SendSuccessMessage($"全图{opString}结束（用时 {utils.GetUnixTimestamp - secondLast}s）");
                }
                else
                {
                    InformSections(rect);
                    FinishGen();
                    op.SendSuccessMessage($"{opString}结束");
                }
            });
        }

        private static void ReplaceTile(Rectangle rect, int type = 1, bool needAll = false)
        {
            if (!needAll) lastArea = rect;
            for (int cx = rect.X; cx < rect.Right; cx++)
            {
                for (int cy = rect.Y; cy < rect.Bottom; cy++)
                {
                    switch (type)
                    {
                        case 1: TileHelper.ReplaceTile(cx, cy); break;
                        case 2: TileHelper.IceAgeTile(cx, cy); break;
                        case 3: TileHelper.IceMeltTile(cx, cy); break;
                    }

                }
            }
        }
        #endregion
        #endregion



        #region 沙漠地形
        public static async void AsyncDesertWorld(TSPlayer op)
        {
            GenHelper.isTaskRunning = true;
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


        # region 字符画
        /// <summary>
        /// 字符画
        /// </summary>
        /// <param name="op"></param>
        public static void CharPaint(TSPlayer op)
        {
            string[] chars = File.ReadAllLines(Path.Combine(SaveDir, "raw_0.txt"));
            int x = op.TileX - 61;
            int y = op.TileY - 30;
            int col;
            int row = 0;
            int maxCol = 0;
            TileHelper.ResetTileMapping();
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
                        TileHelper.RandomTile(x + col, y + row);
                    }
                }
            }
            InformSections(new Rectangle(x, y, maxCol, row));
        }
        #endregion


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

    }
}