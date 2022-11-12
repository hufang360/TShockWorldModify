using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using TerrariaApi.Server;
using TShockAPI;


namespace WorldModify
{
    [ApiVersion(2, 1)]
    public class WorldModify : TerrariaPlugin
    {
        public override string Author => "hufang360";

        public override string Description => "简易的世界修改器";

        public override string Name => "WorldModify";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static readonly string SaveDir = Path.Combine(TShock.SavePath, "WorldModify");


        public WorldModify(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("worldmodify", WMCommand, "worldmodify", "wm") { HelpText = "简易的世界修改器" });
            Commands.ChatCommands.Add(new Command("moonphase", MoonHelper.ChangeMoonPhase, "moonphase", "moon") { HelpText = "月相管理" });
            Commands.ChatCommands.Add(new Command("moonstyle", MoonHelper.ChangeMoonStyle, "moonstyle", "ms") { HelpText = "月亮样式管理" });
            Commands.ChatCommands.Add(new Command("bossmanage", BossHelper.Manage, "bossmanage", "boss") { HelpText = "boss管理" });
            Commands.ChatCommands.Add(new Command("npcmanage", NPCHelper.Manage, "npcmanage", "npc") { HelpText = "npc管理" });
            Commands.ChatCommands.Add(new Command("igen", iGen.Manage, "igen") { HelpText = "建造世界" });

            Commands.ChatCommands.Add(new Command("worldinfo", WorldInfo, "worldinfo", "wi") { HelpText = "世界信息" });
            Commands.ChatCommands.Add(new Command("bossinfo", BossHelper.BossInfo, "bossinfo", "bi") { HelpText = "boss进度信息" });
            Commands.ChatCommands.Add(new Command("relive", NPCHelper.Relive, "relive") { HelpText = "复活NPC" });
            Commands.ChatCommands.Add(new Command("cleartomb", ClearToolWM.ClearTomb, "cleartomb", "ct") { HelpText = "清理墓碑" });

            Utils.SaveDir = SaveDir;
            BackupHelper.BackupPath = Utils.CombinePath("backups");
            RetileTool.SaveFile = Utils.CombinePath("retile.json");
            ReportTool.SaveFile = Utils.CombinePath("report.json");
            ResearchHelper.SaveFile = Utils.CombinePath("research.csv");
            BestiaryHelper.SaveFile = Utils.CombinePath("bestiary.csv");
        }


        private void WMCommand(CommandArgs args)
        {
            TSPlayer op = args.Player;

            #region help
            void ShowHelpText()
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
                    return;

                List<string> lines = new()
                {
                    "/wm info，查看 世界信息",
                    "/wm name [世界名]，查看/修改 世界名字",
                    "/wm mode [难度]，查看/修改 世界难度",
                    "/wm 2020，开启/关闭 05162020 秘密世界",

                    "/wm 2021，开启/关闭 05162021 秘密世界",
                    "/wm ftw，开启/关闭 for the worthy 秘密世界",
                    "/wm ntb，开启/关闭 not the bees 秘密世界",
                    "/wm dst，开启/关闭 饥荒联动 秘密世界",

                    "/wm remix，开启/关闭 Remix 秘密世界",
                    "/wm nt，开启/关闭 No Traps 秘密世界",
                    "/wm zenith，开启/关闭 Zenith 秘密世界",
                    "/wm seed [种子]，查看/修改 世界种子",

                    "/wm id [id]，查看/修改 世界ID",
                    "/wm uuid [uuid字符 / new]，查看/修改 世界uuid",
                    "/wm sundial [on / off / 天数]，查看/开关 附魔日晷 / 修改 冷却天数",
                    "/wm moondial [on / off / 天数]，查看/开关 附魔月晷 / 修改 冷却天数",

                    "/wm spawn，查看 出生点",
                    "/wm dungeon，查看 地牢点",
                    "/wm surface [深度]，查看/修改 地表深度",
                    "/wm cave [深度]，查看/修改 洞穴深度",

                    "/wm wind，查看 风速",
                    "/wm backup [备注]，备份地图",
                    "/wm research help，物品研究",
                    "/wm bestiary help，怪物图鉴",

                    "/wm clear help，全图清理指定图格",
                    "/moon help，月相管理",
                    "/moonstyle help，月亮样式管理",
                    "/boss help，boss管理",

                    "/npc help，npc管理",
                    "/igen help，建造世界",
                    "/wm docs，生成参考文档"
                };

                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "帮助 ({0}/{1})：",
                    FooterFormat = "输入 /wm help {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }

            if (args.Parameters.Count == 0)
            {
                op.SendErrorMessage("语法错误，输入 /wm help 查询用法");
                return;
            }

            string text;

            #endregion
            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 帮助
                case "help": ShowHelpText(); break;
                default: op.SendErrorMessage("语法不正确！输入 /wm help 查询用法"); break;

                // 世界信息
                case "info": ShowWorldInfo(args, true); break;

                #region 基础信息
                // 名字
                case "name":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"世界名称：{Main.worldName}\n输入 /wm seed <名称> 可更改世界名称");
                        break;
                    }
                    Main.worldName = args.Parameters[1];
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage("世界名称已改成 {0}", args.Parameters[1]);
                    break;

                // 难度/模式 ，经典/专家/大师/旅行
                case "mode":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"世界难度：{_worldModes.Keys.ElementAt(Main.GameMode)}" +
                            $"\n用法：/wm mode <难度>" +
                            $"\n可用的难度：{string.Join(", ", _worldModes.Keys)}");
                        break;
                    }

                    if (int.TryParse(args.Parameters[1], out int mode))
                    {
                        if (mode < 1 || mode > 4)
                        {
                            op.SendErrorMessage($"语法错误！用法：/wm mode <难度>\n可用的难度：{string.Join(", ", _worldModes.Keys)}");
                            break;
                        }
                    }
                    else if (_worldModes.ContainsKey(args.Parameters[1]))
                    {
                        mode = _worldModes[args.Parameters[1]];
                    }
                    else
                    {
                        op.SendErrorMessage($"语法错误！用法：/wm mode <难度>\n可用的难度：{string.Join(", ", _worldModes.Keys)}");
                        break;
                    }
                    Main.GameMode = mode - 1;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage("世界模式已改成 {0}", _worldModes.Keys.ElementAt(mode - 1));
                    break;


                // 种子
                case "seed":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"世界种子：{WorldGen.currentWorldSeed}（{Main.ActiveWorldFileData.GetFullSeedText()}）\n输入 /wm seed <种子> 可更改世界种子");
                        break;
                    }

                    Main.ActiveWorldFileData.SetSeed(args.Parameters[1]);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage("世界种子已改成 {0}", args.Parameters[1]);
                    break;


                // worldId 800875906
                case "id":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"世界ID：{Main.worldID}\n输入 /wm id <id> 可更改世界ID");
                        break;
                    }

                    if (int.TryParse(args.Parameters[1], out int worldId))
                    {
                        Main.worldID = worldId;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("世界的ID已改成 {0}", args.Parameters[1]);
                    }
                    else
                    {
                        op.SendErrorMessage("世界ID只能由数字组成");
                    }
                    break;


                // uuid ee700694-ab04-434e-b872-8a800a527cd7
                case "uuid":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"uuid：{Main.ActiveWorldFileData.UniqueId}\n输入 /wm uuid <uuid> 可更改世界的uuid");
                        break;
                    }

                    string uuid = args.Parameters[1].ToLower();
                    if (uuid == "new")
                    {
                        Main.ActiveWorldFileData.UniqueId = Guid.NewGuid();
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("世界的UUID已改成 {0}", Main.ActiveWorldFileData.UniqueId);
                    }
                    else
                    {
                        if (Utils.StringToGuid(uuid))
                        {
                            Main.ActiveWorldFileData.UniqueId = new Guid(uuid);
                            TSPlayer.All.SendData(PacketTypes.WorldInfo);
                            op.SendSuccessMessage("世界的UUID已改成 {0}", uuid);
                        }
                        else
                        {
                            op.SendErrorMessage("uuid格式不正确！");
                        }
                    }
                    break;
                #endregion


                #region 附魔日晷 附魔月晷
                // 附魔日晷
                case "sd":
                case "sundial":
                    if (args.Parameters.Count == 1)
                    {
                        text = GetSundial();
                        if (string.IsNullOrEmpty(text))
                            text = "附魔日晷：无";
                        op.SendInfoMessage($"{text}" +
                            $"\n输入 /wm sundial <天数> 可修改修改附魔日晷冷却天数" +
                            $"\n输入 /wm sundial <on/off> 可开关附魔日晷");
                        break;
                    }
                    switch (args.Parameters[1].ToLowerInvariant())
                    {
                        case "on":
                            if (!Main.fastForwardTimeToDawn)
                            {
                                Main.fastForwardTimeToDawn = true;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔日晷 已开启");
                            }
                            else
                            {
                                op.SendSuccessMessage("附魔日晷 已是开启状态");
                            }
                            break;
                        case "off":
                            if (Main.fastForwardTimeToDawn)
                            {
                                Main.fastForwardTimeToDawn = false;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔日晷已关闭");
                            }
                            else
                            {
                                op.SendSuccessMessage("附魔日晷 已是关闭状态");
                            }
                            break;
                        default:
                            if (int.TryParse(args.Parameters[1], out int days))
                            {
                                Main.sundialCooldown = days;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔日晷冷却天数 已改成 {0}", days);
                            }
                            else
                            {
                                op.SendErrorMessage("天数输入错误！");
                            }
                            break;
                    }
                    break;

                // 附魔月晷
                case "md":
                case "moondial":
                    if (args.Parameters.Count == 1)
                    {
                        text = GetMoondial();
                        if (string.IsNullOrEmpty(text))
                            text = "附魔月晷：无";
                        op.SendInfoMessage($"{text}" +
                            $"\n输入 /wm moondial <天数> 可修改修改附魔月晷冷却天数" +
                            $"\n输入 /wm moondial <on/off> 可开关附魔月晷");
                        break;
                    }
                    switch (args.Parameters[1].ToLowerInvariant())
                    {
                        case "on":
                            if (!Main.fastForwardTimeToDusk)
                            {
                                Main.fastForwardTimeToDusk = true;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔月晷 已开启");
                            }
                            else
                            {
                                op.SendSuccessMessage("附魔月晷 已是开启状态");
                            }
                            break;
                        case "off":
                            if (Main.fastForwardTimeToDusk)
                            {
                                Main.fastForwardTimeToDusk = false;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔月晷已关闭");
                            }
                            else
                            {
                                op.SendSuccessMessage("附魔月晷 已是关闭状态");
                            }
                            break;
                        default:
                            if (int.TryParse(args.Parameters[1], out int days))
                            {
                                Main.moondialCooldown = days;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔日晷冷却天数 已改成 {0}", days);
                            }
                            else
                            {
                                op.SendErrorMessage("天数输入错误！");
                            }
                            break;
                    }
                    break;
                #endregion

                // 地表深度
                case "surface":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"表层深度：{Main.worldSurface}\n输入 /wm surface <深度> 可修改地表深度");
                        break;
                    }
                    if (int.TryParse(args.Parameters[1], out int surface))
                    {
                        Main.worldSurface = surface;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("表层深度 已改成 {0}", surface);
                    }
                    else
                    {
                        op.SendErrorMessage("深度输入错误！");
                    }
                    break;

                // 洞穴深度
                case "cave":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"洞穴深度：{Main.rockLayer}\n输入 /wm cave <深度> 可修改洞穴深度");
                        break;
                    }
                    if (int.TryParse(args.Parameters[1], out int cave))
                    {
                        Main.rockLayer = cave;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("洞穴深度 已改成 {0}", cave);
                    }
                    else
                    {
                        op.SendErrorMessage("深度输入错误！");
                    }
                    break;

                // 出生点
                case "spawn":
                    op.SendInfoMessage($"出生点：{Main.spawnTileX}, {Main.spawnTileY} \n进入游戏后，输入 /setspawn 设置出生点 \n进入游戏后，输入 /spawn 传送至出生点");
                    break;

                // 地牢点
                case "dungeon":
                case "dun":
                    op.SendInfoMessage($"地牢点：{Main.dungeonX}, {Main.dungeonY} \n进入游戏后，输入 /setdungeon 设置地牢点 \n进入游戏后，输入 /tpnpc \"Old Man\" 传送至地牢点");
                    break;

                // 风速
                case "wind":
                    op.SendInfoMessage($"风速：{Main.windSpeedCurrent}\n输入 /wind <速度> 可调节风速");
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

                //  饥荒联动
                case "eye":
                case "dst":
                case "constant":
                    Main.dontStarveWorld = !Main.dontStarveWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.dontStarveWorld)} 永恒领域 秘密世界（饥荒联动）");
                    break;


                //  Remix 种子
                case "remix":
                    Main.remixWorld = !Main.remixWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.remixWorld)} Remix 秘密世界（don't dig up）");
                    break;

                //  noTraps 种子
                case "nt":
                case "no traps":
                    Main.noTrapsWorld = !Main.noTrapsWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.noTrapsWorld)} No Traps 秘密世界");
                    break;

                //  天顶种子
                case "zenith":
                case "gfb":
                case "everything":
                    Main.zenithWorld = !Main.zenithWorld;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage($"{Utils.BFlag(Main.zenithWorld)} 天顶 秘密世界（getfixedboi）");
                    break;
                #endregion

                // 全物品研究
                case "research":
                case "re":
                    ResearchHelper.Manage(args);
                    break;

                // 怪物图鉴
                case "bestiary":
                case "be":
                    BestiaryHelper.Manage(args);
                    break;

                // 备份
                case "backup":
                    string notes = "";
                    if (args.Parameters.Count > 1)
                    {
                        args.Parameters.RemoveAt(0);
                        notes = string.Join(" ", args.Parameters);
                    }
                    BackupHelper.Backup(op, notes);
                    break;

                // 查找地形
                case "find":
                    FindTool.Manage(args);
                    break;

                // 清理目标
                case "clear":
                    ClearToolWM.Manage(args);
                    break;

                //// 点亮全图
                //case "showall":
                //    op.SendInfoMessage("未实现！");
                //    //ShowAll(op);
                //    break;

                // 生成参考文档
                case "docs":
                case "refer":
                case "dump":
                    DocsHelper.GenDocs(op);
                    break;

                //// 解析并导出xml数据
                //case "xml":
                //    ResHelper.DumpXML();
                //    break;


                // 测试 debug用
                case "debug":
                    TShock.Config.Settings.DebugLogs = !TShock.Config.Settings.DebugLogs;
                    op.SendInfoMessage($"debug模式:{TShock.Config.Settings.DebugLogs}");
                    break;
            }
        }

        # region worldinfo
        static readonly Dictionary<string, int> _worldModes = new()
        {
            { "经典", 1 },
            { "专家", 2 },
            { "大师", 3 },
            { "旅行", 4 }
        };
        private void WorldInfo(CommandArgs args)
        {
            ShowWorldInfo(args);
        }

        private void ShowWorldInfo(CommandArgs args, bool isSuperAdmin = false)
        {
            TSPlayer op = args.Player;

            List<string> lines = new() {
                $"名称：{Main.worldName}",
                $"大小：{GetWorldSize()}",
                $"难度：{_worldModes.Keys.ElementAt(Main.GameMode)}",
                $"种子：{WorldGen.currentWorldSeed}"
            };
            if (isSuperAdmin)
            {
                lines.Add($"ID：{Main.worldID}");
                lines.Add($"UUID：{Main.ActiveWorldFileData.UniqueId}");
                lines.Add($"版本：{Main.curRelease}  {Main.versionNumber}");
            }

            // 腐化 秘密世界
            string text = GetSecretWorldDescription();
            if (!string.IsNullOrEmpty(text))
                lines.Add(text);
            lines.Add(GetCorruptionDescription(isSuperAdmin));


            HashSet<string> texts = new();
            if (NPC.combatBookWasUsed) texts.Add("[i:4382]");
            if (NPC.combatBookVolumeTwoWasUsed) texts.Add("[i:5336]");
            if (NPC.peddlersSatchelWasUsed) texts.Add("[i:5343]");
            if (texts.Any())
                lines.Add($"增强：{string.Join(",", texts)}");

            // 时间
            //if (isSuperAdmin)
            //{
            //    double time = Main.time / 3600.0;
            //    time += 4.5;
            //    if (!Main.dayTime)
            //        time += 15.0;
            //    time %= 24.0;
            //    lines.Add(string.Format("时间：{0}:{1:D2}", (int)Math.Floor(time), (int)Math.Floor((time % 1.0) * 60.0)));
            //}

            // 附魔日晷 / 月晷
            texts.Clear();
            if (Main.fastForwardTimeToDawn) texts.Add("日晷生效中");
            if (Main.sundialCooldown > 0) texts.Add($"日晷冷却：{Main.sundialCooldown}天");
            if (Main.fastForwardTimeToDusk) texts.Add("月晷生效中");
            if (Main.moondialCooldown > 0) texts.Add($"月晷冷却：{Main.moondialCooldown}天");
            if (texts.Any())
                lines.Add($"时间：{string.Join(", ", texts)}");

            if (isSuperAdmin)
            {
                texts.Clear();
                texts.Add(MoonHelper.MoonPhaseDesc);
                texts.Add(MoonHelper.MoonTypeDesc);
                if (Main.bloodMoon) texts.Add("血月");
                if (Main.eclipse) texts.Add("日食");
                lines.Add($"月相：{string.Join(", ", texts)}");

                texts.Clear();
                if (Main.raining) texts.Add("雨天");
                if (Main.IsItStorming) texts.Add("雷雨天");
                if (Main.IsItAHappyWindyDay) texts.Add("大风天");
                if (Sandstorm.Happening) texts.Add("沙尘暴");
                lines.Add($"天气：云量：{Main.numClouds}  风力：{Main.windSpeedCurrent}  {string.Join("  ", texts)}");

                string percent;
                if (TShock.ServerSideCharacterConfig.Settings.Enabled && Main.GameMode == 3)
                {
                    int num1 = ResearchHelper.GetSacrificeCompleted();
                    int num2 = ResearchHelper.GetSacrificeTotal();
                    percent = Terraria.Utils.PrettifyPercentDisplay(num1 / num2, "P2");
                    lines.Add($"物品研究：{percent}（{num1}/{num2}）");
                }

                BestiaryUnlockProgressReport result = Main.GetBestiaryProgressReport();
                percent = Terraria.Utils.PrettifyPercentDisplay(result.CompletionPercent, "P2");
                lines.Add($"图鉴：{percent}（{result.CompletionAmountTotal}/{result.EntriesTotal}）");

                if (DD2Event.DownedInvasionT1)
                    text = "已通过 T1难度";
                else if (DD2Event.DownedInvasionT2)
                    text = "已通过 T2难度";
                else if (DD2Event.DownedInvasionT3)
                    text = "已通过 T3难度";
                else
                    text = "";
                if (!string.IsNullOrEmpty(text))
                    lines.Add($"撒旦军队：{text}");

                // 哥布林军队、海盗入侵
                // 撒旦军队
                // 派对、雨、沙尘暴
                // 史莱姆雨
                // 南瓜月、雪人军团、霜月、火星暴乱
                // 月亮事件
                string textSize = $"（已清理{Main.invasionProgress}%波）（规模：{Main.invasionSize} ）";
                string textSize2 = $"（第{Main.invasionProgressWave}波：{Main.invasionProgress}%）";
                if (Main.invasionType == 1)
                    text = $"哥布林入侵（{textSize}）";
                else if (Main.invasionType == 2)
                    text = $"霜月（{textSize2}）";
                else if (Main.invasionType == 3)
                    text = $"海盗入侵（{textSize}）";
                else if (Main.invasionType == 4)
                    text = $"火星暴乱（{textSize}）";
                else
                {
                    // NPC.BusyWithAnyInvasionOfSorts
                    if (Main.pumpkinMoon)
                        text = $"南瓜月（{textSize2}）";
                    else if (Main.snowMoon)
                        text = $"雪人军团（{textSize}）";
                    else if (DD2Event.Ongoing)
                        text = $"撒旦军队（{textSize2}）";
                    else
                        text = "";
                }
                if (!string.IsNullOrEmpty(text))
                    lines.Add($"入侵：{text}");

                // 杂项
                texts.Clear();
                if (BirthdayParty._wasCelebrating) texts.Add("派对");
                if (LanternNight.LanternsUp) texts.Add("灯笼夜");
                if (Star.starfallBoost > 3f) texts.Add("流星雨");
                if (Main.slimeRain) texts.Add("史莱姆雨");
                if (WorldGen.spawnMeteor) texts.Add("陨石");
                if (texts.Any())
                    lines.Add($"事件：{string.Join(", ", texts)}");

                texts.Clear();
                if (Main.xMas) texts.Add("圣诞节");
                if (Main.halloween) texts.Add("万圣节");
                if (texts.Any())
                    lines.Add($"节日：{string.Join(", ", texts)}");

                texts.Clear();
                texts.Add($"表层深度：{Main.worldSurface}, 洞穴深度：{Main.rockLayer}, 出生点：{Main.spawnTileX},{Main.spawnTileY}, 地牢点：{Main.dungeonX},{Main.dungeonY}");
                if (TShock.Config.Settings.RequireLogin) texts.Add("已开启需要登录");
                if (TShock.ServerSideCharacterConfig.Settings.Enabled) texts.Add("已开启SSC");
                lines.Add($"杂项：{string.Join(", ", texts)}");
            }
            op.SendInfoMessage(string.Join("\n", lines));
        }
        private static string GetWorldSize()
        {
            if (Main.maxTilesX == 8400 && Main.maxTilesY == 2400) return "小（4200x1200）";
            else if (Main.maxTilesX == 6400 && Main.maxTilesY == 1800) return "中（6400x1800）";
            else if (Main.maxTilesX == 4200 && Main.maxTilesY == 1200) return "大（8400x2400）";
            else return "未知";
        }
        #endregion



        #region 附魔日晷
        private string GetSundial()
        {
            // 附魔日晷
            string text = Main.IsFastForwardingTime() ? "生效中" : "";
            string text2 = Main.sundialCooldown > 0 ? $"{Main.sundialCooldown}天后可再次使用" : "";
            if (string.IsNullOrEmpty(text))
                text = text2;
            else
            {
                if (!string.IsNullOrEmpty(text2))
                    text = $"{text} {text2}";
            }
            if (!string.IsNullOrEmpty(text))
                return $"附魔日晷：{text}";
            else
                return "";
        }
        private string GetMoondial()
        {
            // 附魔日晷
            string text = Main.IsFastForwardingTime() ? "生效中" : "";
            string text2 = Main.moondialCooldown > 0 ? $"{Main.moondialCooldown}天后可再次使用" : "";
            if (string.IsNullOrEmpty(text))
                text = text2;
            else
            {
                if (!string.IsNullOrEmpty(text2))
                    text = $"{text} {text2}";
            }
            if (!string.IsNullOrEmpty(text))
                return $"附魔月晷：{text}";
            else
                return "";
        }
        #endregion

        #region GetSecretWorldDescription
        // 获取秘密世界种子状态描述
        private string GetSecretWorldDescription()
        {
            List<string> ss = new();

            if (Main.getGoodWorld) ss.Add("for the worthy");
            if (Main.drunkWorld) ss.Add("05162020");
            if (Main.tenthAnniversaryWorld) ss.Add("05162021");
            if (Main.dontStarveWorld) ss.Add("the constant");
            if (Main.notTheBeesWorld) ss.Add("not the bees");
            if (Main.remixWorld) ss.Add("Remix");
            if (Main.noTrapsWorld) ss.Add("No Traps");
            if (Main.zenithWorld) ss.Add("Zenith");

            if (ss.Count > 0)
                return $"彩蛋：{string.Join(", ", ss)}";
            else
                return "";
        }


        // 获取腐化类型描述
        private string GetCorruptionDescription(bool isSuperAdmin = false)
        {
            // Main.ActiveWorldFileData.HasCrimson
            // Main.ActiveWorldFileData.HasCorruption
            string more(int type)
            {
                if (isSuperAdmin)
                    type = 3;

                string text;
                if (type == 1)
                    text = $"已摧毁 暗影珠x{WorldGen.shadowOrbCount} ";
                else if (type == 2)
                    text = $"已摧毁 猩红之心x{WorldGen.heartCount} ";
                else
                    text = $"已摧毁 猩红之心x{WorldGen.heartCount} 暗影珠x{WorldGen.shadowOrbCount} ";

                if (Main.hardMode)
                    text += $"祭坛x{WorldGen.altarCount}";

                string s2 = GetWorldStatusDialog();
                if (!string.IsNullOrEmpty(s2))
                    text += $" （{s2}）";

                return text;
            }

            if (Main.drunkWorld)
            {
                if (WorldGen.crimson)
                    return $"腐化：今天是猩红[i:3016], {more(3)}";
                else
                    return $"腐化：今天是腐化[i:3015], {more(3)}";
            }
            else
            {
                if (WorldGen.crimson)
                    return $"腐化：猩红[i:880], {more(2)}";
                else
                    return $"腐化：腐化[i:56], {more(1)}";
            }
        }


        public string GetWorldStatusDialog()
        {
            int tGood = WorldGen.tGood;
            int tEvil = WorldGen.tEvil;
            int tBlood = WorldGen.tBlood;

            if (tGood > 0 && tEvil > 0 && tBlood > 0)
                return $"{tGood}%神圣 {tEvil}%腐化 {tBlood}%猩红";

            else if (tGood > 0 && tEvil > 0)
                return $"{tGood}%神圣 {tEvil}%腐化";

            else if (tGood > 0 && tBlood > 0)
                return $"{tGood}%神圣 {tBlood}%猩红";

            else if (tEvil > 0 && tBlood > 0)
                return $"{tEvil}%腐化 {tBlood}%猩红";

            else if (tEvil > 0)
                return $"{tEvil}%腐化";

            else if (tBlood > 0)
                return $"{tBlood}%猩红";

            else
                return "";
        }
        #endregion

        #region dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _worldModes.Clear();
            }
            SelectionTool.dispose();
            base.Dispose(disposing);
        }
        #endregion
    }
}
