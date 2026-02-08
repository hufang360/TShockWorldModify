using System;
using System.Threading.Tasks;
using Terraria;
using Terraria.WorldBuilding;
using TShockAPI;

namespace WorldModify
{
    class WorldTool
    {
        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 1)
            {
                op.SendErrorMessage("参数不够，用法如下");
                op.SendErrorMessage("/igen world <种子> [腐化] [大小] [彩蛋特性], 重建地图");
                op.SendErrorMessage("种子：输入任意种子名，0表示随机");
                op.SendErrorMessage("腐化：腐化/猩红 或 1/2, 0表示随机");
                op.SendErrorMessage("大小：小/中/大 或 1/2/3, 0表示忽略");
                op.SendErrorMessage("彩蛋特性：种子名中间输入英文逗号，例如 2020,ftw");
                return;
            }
            if (TileHelper.NeedWaitTask(op)) return;
            string seedStr = args.Parameters.Count > 1 ? args.Parameters[1] : "";
            string evilStr = args.Parameters.Count > 2 ? args.Parameters[2] : "";
            string sizeStr = args.Parameters.Count > 3 ? args.Parameters[3] : "";

            string eggStr = "";
            if (args.Parameters.Count > 4)
            {
                args.Parameters.RemoveAt(0);
                args.Parameters.RemoveAt(0);
                args.Parameters.RemoveAt(0);
                args.Parameters.RemoveAt(0);
                eggStr = string.Join(" ", args.Parameters);
            }

            int size = 0;
            if (sizeStr == "小" || sizeStr == "1")
                size = 1;
            else if (sizeStr == "中" || sizeStr == "2")
                size = 2;
            else if (sizeStr == "大" || sizeStr == "3")
                size = 3;

            int evil = -1;
            if (evilStr == "腐化" || evilStr == "1")
                evil = 0;
            else if (evilStr == "猩红" || evilStr == "2")
                evil = 1;

            GenWorld(op, seedStr, size, evil, eggStr);
        }

        /// <summary>
        /// GenWorld
        /// 参考：https://github.com/Illuminousity/WorldRefill/blob/master/WorldRefill/WorldRefill.cs#L997
        /// </summary>
        /// <param name="op"></param>
        /// <param name="seedStr"></param>
        /// <param name="size"></param>
        /// <param name="evil"></param>
        /// <param name="eggStr"></param>
        private static async void GenWorld(TSPlayer op, string seedStr = "", int size = 0, int evil = -1, string eggStr = "")
        {
            BackupHelper.Backup(op, "GenWorld");
            if (!op.RealPlayer)
            {
                Console.WriteLine($"seed:{seedStr}");
                op.SendErrorMessage($"[i:556]世界正在解体~");
            }
            TSPlayer.All.SendErrorMessage("[i:556]世界正在解体~");
            int secondLast = Utils.GetUnixTimestamp;

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
            string seedText = Main.ActiveWorldFileData.SeedText;
            if (!op.RealPlayer)
                op.SendErrorMessage($"[i:3061]世界正在重建（{seedText}）");
            TSPlayer.All.SendErrorMessage($"[i:3061]世界正在重建（{seedText}）");
            await AsyncGenerateWorld();

            // 创建完成
            int second = Utils.GetUnixTimestamp - secondLast;
            string text = $"[i:3061]世界重建完成 （用时 {second}s, {seedText}）；-）";
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
                Utils.Log("服务器已关闭：重建后的地图大小和之前不一样，为了稳定起见，请重新开服");
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
        }

        private static Task AsyncGenerateWorld()
        {
            TileHelper.isTaskRunning = true;
            WorldGen.clearWorld();
            return Task.Run(() =>
            {
                WorldGen.GenerateWorld(new GenerationProgress());
            }).ContinueWith((d) =>
            {
                TileHelper.GenAfter();
            });
        }


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

    }
}