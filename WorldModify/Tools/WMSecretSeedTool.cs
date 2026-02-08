using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 秘密种子工具
    /// </summary>
    class WMSecretSeedTool
    {
        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);

            void Help()
            {
                List<string> lines =
                [
                    "/wm s 2020，开启/关闭 05162020 秘密世界",
                    $"/wm s 2021，开启/关闭 05162021 秘密世界",
                    $"/wm s ftw，开启/关闭 for the worthy 秘密世界",
                    $"/wm s ntb，开启/关闭 not the bees 秘密世界",

                    $"/wm s dst，开启/关闭 饥荒联动 秘密世界",
                    $"/wm s remix，开启/关闭 Remix 秘密世界",
                    $"/wm s nt，开启/关闭 No Traps 秘密世界",
                    $"/wm s zenith，开启/关闭 Zenith 秘密世界",

                    $"/wm s sky，开启/关闭 空岛 秘密世界",
                    $"/wm s vampire，开启/关闭 吸血鬼 秘密世界",
                    $"/wm s infected，开启/关闭 infect 秘密世界",
                    $"/wm s team，开启/关闭 team 秘密世界",

                    $"/wm s dual，开启/关闭 双地牢 秘密世界",
                ];

                Utils.Pagination(args, ref lines, "/wm secret");
            }

            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            string kw = args.Parameters[0].ToLowerInvariant();
            switch (kw)
            {
                case "help":
                case "h":
                    Help();
                    break;
                default:
                    SecretSeed(args);
                    break;
            }

        }

        /// <summary>
        /// 开关秘密世界
        /// </summary>
        /// <param name="args"></param>
        public static void SecretSeed(CommandArgs args)
        {
            string kw = args.Parameters[0].ToLowerInvariant();
            TSPlayer op = args.Player;
            switch (kw)
            {
                default:
                    op.SendErrorMessage("请输入 /wm secret help 查询用法！");
                    break;

                #region 秘密世界
                // 醉酒世界
                case "516":
                case "0516":
                case "5162020":
                case "05162020":
                case "2020":
                case "drunk":
                    Main.drunkWorld = !Main.drunkWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.drunkWorld)} 05162020 秘密世界（醉酒世界 / DrunkWorld）");
                    break;


                // 10周年庆典,tenthAnniversaryWorld
                case "2011":
                case "2021":
                case "5162011":
                case "5162021":
                case "05162011":
                case "05162021":
                case "celebrationmk10":
                    Main.tenthAnniversaryWorld = !Main.tenthAnniversaryWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.tenthAnniversaryWorld)} 10周年庆典 秘密世界（05162021）");
                    break;

                // ftw（for the worthy）
                case "ftw":
                case "for the worthy":
                    Main.getGoodWorld = !Main.getGoodWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.getGoodWorld)} for the worthy 秘密世界");
                    break;

                // not the bees
                case "ntb":
                    Main.notTheBeesWorld = !Main.notTheBeesWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.notTheBeesWorld)} not the bees 秘密世界");
                    break;

                // 饥荒联动
                case "eye":
                case "dst":
                case "constant":
                    Main.dontStarveWorld = !Main.dontStarveWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.dontStarveWorld)} 永恒领域 秘密世界（饥荒联动）");
                    break;


                // Remix 种子
                case "remix":
                    Main.remixWorld = !Main.remixWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.remixWorld)} Remix 秘密世界（don't dig up）");
                    break;

                // noTraps 种子
                case "nt":
                case "no traps":
                    Main.noTrapsWorld = !Main.noTrapsWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.noTrapsWorld)} No Traps 秘密世界");
                    break;

                // 天顶种子
                case "zenith":
                case "gfb":
                case "everything":
                    Main.zenithWorld = !Main.zenithWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.zenithWorld)} 天顶 秘密世界（getfixedboi）");
                    break;

                // 空岛
                case "sky":
                case "sky block":
                    Main.skyblockWorld = !Main.skyblockWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.skyblockWorld)} 空岛 秘密世界（getfixedboi）");
                    break;


                // 参考链接： https://terraria.wiki.gg/wiki/Secret_world_seeds
                // 吸血鬼种子
                case "va":
                case "vampire":
                    Main.vampireSeed = !Main.vampireSeed;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.vampireSeed)} 吸血鬼 秘密世界（vampire）");
                    break;

                // infected
                case "infected":
                    Main.infectedSeed = !Main.infectedSeed;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.infectedSeed)} 感染世界 秘密世界（infected）");
                    break;

                // team
                case "team":
                case "team based":
                case "team based spawns":
                    Main.teamBasedSpawnsSeed = !Main.teamBasedSpawnsSeed;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.teamBasedSpawnsSeed)} 团队生成点 秘密世界（team based spawns）");
                    break;

                // 双地牢
                case "dual":
                case "dual dungeons":
                    Main.dualDungeonsSeed = !Main.dualDungeonsSeed;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.dualDungeonsSeed)} 双地牢 秘密世界（dual dungeons）");
                    break;

                // 全秘密世界种子
                case "full":
                case "fullseed":
                    string fullseedText = "1.1.1.0.abandoned manors|arachnophobia|beam me up|bring a towel|double daring dangers|fish mox|hocus pocus|how did i get here|i am error|invisible plane|jagged rocks|jingle all the way|mole people|monochrome|more traps please|negative infinity|night of the living dead|planetoids|pumpkin season|purify this|rainbow road|royale with cheese|does that sparkle|too easy|water park|what a horrible night to have a curse|winter is coming|xray vision|truck stop|sandy britches|save the rainforest|such great heights|the care bears movie|toadstool|we don\'t even test for that";
                    op.SendSuccessMessage($"全彩蛋种子为: {fullseedText}");
                    break;

                #endregion
            }
        }

        #region 秘密世界
        /// <summary>
        /// 是否是秘密世界种子命令（兼容旧的 `/wm 2020` 模式
        /// </summary>
        /// <param name="kw"></param>
        /// <returns></returns>
        public static bool IsSecretSeedCommand(string kw)
        {
            List<string> ss =
            [
                // 2020
                "516",
                "0516",
                "5162020",
                "05162020",
                "2020",
                "drunk",

                // 2021
                "2011",
                "2021",
                "5162011",
                "5162021",
                "05162011",
                "05162021",
                "celebrationmk10",

                //ftw
                "ftw",
                "for the worthy",

                // not the bees
                "ntb",

                // 饥荒
                "eye",
                "dst",
                "constant",

                // remix
                "remix",

                // noTraps 种子
                "nt",
                "no traps",

                // 天顶剑种子
                "zenith",
                "gfb",
                "everything",

                // 空岛
                "sky",
                "sky block",

                // 吸血鬼
                "va",
                "vampire",

                // 感染
                "infect",
                "infected",

                "team",
                "dual",
            ];
            return ss.Contains(kw);
        }
        #endregion

    }

}