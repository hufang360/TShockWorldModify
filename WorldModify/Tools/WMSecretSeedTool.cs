using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.WorldBuilding;
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
                    "/wm s 2021，开启/关闭 05162021 秘密世界",
                    "/wm s ftw，开启/关闭 for the worthy 秘密世界",
                    "/wm s ntb，开启/关闭 not the bees 秘密世界",

                    "/wm s dst，开启/关闭 饥荒联动 秘密世界",
                    "/wm s remix，开启/关闭 Remix 秘密世界",
                    "/wm s nt，开启/关闭 No Traps 秘密世界",
                    "/wm s zenith，开启/关闭 Zenith 秘密世界",

                    "/wm s sky，开启/关闭 空岛 秘密世界",
                    "/wm s vampire，开启/关闭 吸血鬼 秘密世界",
                    "/wm s infected，开启/关闭 infect 秘密世界",
                    "/wm s team，开启/关闭 team 秘密世界",

                    "/wm s dual，开启/关闭 双地牢 秘密世界",
                    "/wm s rain，开启/关闭 一年的雨量 特性",

                    "/wm s random，随机开启一个秘密世界特性（2020/2021/rain/full，已开启的会自动跳过，最多随机3次）",
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
                case "random":
                    RandomSecret(args);
                    break;
                default:
                    SecretSeed(args);
                    break;
            }

        }

        /// <summary>
        /// 随机开启一个秘密世界特性（2020/2021/rain/full）
        /// 若随机到当前世界已开启的特性，则继续随机，最多随机3次
        /// </summary>
        /// <param name="args"></param>
        private static void RandomSecret(CommandArgs args)
        {
            TSPlayer op = args.Player;

            // 随机池：2020 / 2021 / rain / full
            // full 无开关状态（仅显示全彩蛋种子），始终可执行
            List<(string kw, string name, bool isOn)> pool =
            [
                ("2020", "05162020 醉酒世界", Main.drunkWorld),
                ("2021", "05162021 10周年庆典", Main.tenthAnniversaryWorld),
                ("rain", "一年的雨量", Main.IsRainingForever),
                ("full", "全彩蛋种子", false),
            ];

            Random rnd = new Random();
            for (int i = 1; i <= 3; i++)
            {
                var pick = pool[rnd.Next(pool.Count)];
                if (pick.isOn)
                {
                    op.SendInfoMessage($"第{i}次随机到 [{pick.name}]，当前世界已开启该特性，继续随机");
                    continue;
                }

                args.Parameters[0] = pick.kw;
                SecretSeed(args);
                op.SendSuccessMessage($"本次随机结果：{pick.name}");
                return;
            }

            op.SendErrorMessage("连续3次随机均命中已开启的特性，本次未开启新特性");
        }

        /// <summary>
        /// 开关秘密世界
        /// </summary>
        /// <param name="args"></param>
        public static void SecretSeed(CommandArgs args)
        {
            string kw = args.Parameters[0].ToLowerInvariant();
            TSPlayer op = args.Player;

            if (!TryParseState(args.Parameters, op, out bool? state)) return;

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
                    Main.drunkWorld = ResolveState(Main.drunkWorld, state, op);
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
                    Main.tenthAnniversaryWorld = ResolveState(Main.tenthAnniversaryWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.tenthAnniversaryWorld)} 10周年庆典 秘密世界（05162021）");
                    break;

                // ftw（for the worthy）
                case "ftw":
                case "for the worthy":
                    Main.getGoodWorld = ResolveState(Main.getGoodWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.getGoodWorld)} for the worthy 秘密世界");
                    break;

                // not the bees
                case "ntb":
                    Main.notTheBeesWorld = ResolveState(Main.notTheBeesWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.notTheBeesWorld)} not the bees 秘密世界");
                    break;

                // 饥荒联动
                case "eye":
                case "dst":
                case "constant":
                    Main.dontStarveWorld = ResolveState(Main.dontStarveWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.dontStarveWorld)} 永恒领域 秘密世界（饥荒联动）");
                    break;


                // Remix 种子
                case "remix":
                    Main.remixWorld = ResolveState(Main.remixWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.remixWorld)} Remix 秘密世界（don't dig up）");
                    break;

                // noTraps 种子
                case "nt":
                case "no traps":
                    Main.noTrapsWorld = ResolveState(Main.noTrapsWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.noTrapsWorld)} No Traps 秘密世界");
                    break;

                // 天顶种子
                case "zenith":
                case "gfb":
                case "everything":
                    Main.zenithWorld = ResolveState(Main.zenithWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.zenithWorld)} 天顶 秘密世界（getfixedboi）");
                    break;

                // 空岛
                case "sky":
                case "sky block":
                    Main.skyblockWorld = ResolveState(Main.skyblockWorld, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.skyblockWorld)} 空岛 秘密世界（getfixedboi）");
                    break;


                // 参考链接： https://terraria.wiki.gg/wiki/Secret_world_seeds
                // 吸血鬼种子
                case "va":
                case "vampire":
                    Main.vampireSeed = ResolveState(Main.vampireSeed, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.vampireSeed)} 吸血鬼 秘密世界（vampire）");
                    break;

                // infected
                case "infected":
                    Main.infectedSeed = ResolveState(Main.infectedSeed, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.infectedSeed)} 感染世界 秘密世界（infected）");
                    break;

                // team[试验]
                case "team":
                    Main.teamBasedSpawnsSeed = ResolveState(Main.teamBasedSpawnsSeed, state, op);
                    if (Main.teamBasedSpawnsSeed)
                    {
                        ExtraSpawnPointManager.PrepareExtraSpawns();
                        ExtraSpawnPointManager.settings = new ExtraSpawnSettings
                        {
                            spawnType = ExtraSpawnType.TeamBased,
                            surface = !GenVars.worldSpawnHasBeenRandomized && Main.isThereAWorldSurface,
                            remix = Main.remixWorld,
                            roundLandmass = WorldGen.SecretSeed.roundLandmasses.Enabled,
                            skyblock = Main.skyblockWorld,
                            extraLiquid = WorldGen.SecretSeed.extraLiquid.Enabled
                        };
                        ExtraSpawnPointManager.GenerateExtraSpawns();
                        op.SendSuccessMessage($"{Utils.Points2String(ExtraSpawnPointManager.extraSpawnPoints)}");
                    }
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.teamBasedSpawnsSeed)} 团队生成点 秘密世界（team based spawns）");
                    break;

                // 双地牢
                case "dual":
                    Main.dualDungeonsSeed = ResolveState(Main.dualDungeonsSeed, state, op);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.dualDungeonsSeed)} 双地牢 秘密世界（dual dungeons）");
                    break;

                // 一年的雨量
                case "rain":
                    {
                        bool rainState = ResolveState(Main.IsRainingForever, state, op);
                        if (rainState && !Main.IsRainingForever)
                        {
                            WorldGen.SecretSeed.DoRainsForAYear();
                        }
                        else if (!rainState && Main.IsRainingForever)
                        {
                            Main.raining = false;
                            Main.rainTime = 0;
                            Main.numClouds = 0;
                        }
                    }
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.IsRainingForever)} 一年的雨量 秘密世界（rainsForAYear）");
                    break;

                // 全秘密世界种子
                case "full":
                    string fullseedText = "1.1.1.0.abandoned manors|arachnophobia|beam me up|bring a towel|double daring dangers|fish mox|hocus pocus|how did i get here|i am error|invisible plane|jagged rocks|jingle all the way|mole people|monochrome|more traps please|negative infinity|night of the living dead|planetoids|pumpkin season|purify this|rainbow road|royale with cheese|does that sparkle|too easy|water park|what a horrible night to have a curse|winter is coming|xray vision|truck stop|sandy britches|save the rainforest|such great heights|the care bears movie|toadstool|we don\'t even test for that";
                    op.SendSuccessMessage($"全彩蛋种子为: {fullseedText}");
                    break;

                #endregion
            }
        }

        /// <summary>
        /// 计算目标状态，若已处于目标状态则给出提示
        /// </summary>
        /// <param name="current">当前状态</param>
        /// <param name="state">目标状态（null=切换）</param>
        /// <param name="op">操作者</param>
        /// <returns>最终状态</returns>
        private static bool ResolveState(bool current, bool? state, TSPlayer op)
        {
            if (state.HasValue && current == state.Value)
                op.SendInfoMessage(state.Value ? "已经是开启状态" : "已经是关闭状态");
            return state ?? !current;
        }

        /// <summary>
        /// 解析可选的开关状态参数
        /// </summary>
        /// <param name="parameters">命令参数列表</param>
        /// <param name="op">操作者</param>
        /// <param name="state">null=切换, true=开启, false=关闭</param>
        /// <returns>参数是否有效</returns>
        private static bool TryParseState(List<string> parameters, TSPlayer op, out bool? state)
        {
            state = null;
            if (parameters.Count < 2) return true;
            string s = parameters[1].ToLowerInvariant();
            switch (s)
            {
                case "on":
                case "true":
                case "1":
                    state = true;
                    return true;
                case "off":
                case "false":
                case "0":
                    state = false;
                    return true;
                default:
                    op.SendErrorMessage("无效的状态参数，请使用 on/off 或 true/false");
                    return false;
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
