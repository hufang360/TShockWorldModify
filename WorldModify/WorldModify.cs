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
            Commands.ChatCommands.Add(new Command(new List<string>() { "worldmodify" }, WMCommand, "worldmodify", "wm") { HelpText = "简易的世界修改器" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "moonphase" }, ChangeMoonPhase, "moonphase", "moon") { HelpText = "月相管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "moonstyle" }, ChangeMoonStyle, "moonstyle", "ms") { HelpText = "月亮样式管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "bossmanage" }, BossHelper.Manage, "bossmanage", "boss") { HelpText = "boss管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "npcmanage" }, NPCHelper.Manage, "npcmanage", "npc") { HelpText = "npc管理" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "igen" }, GenHelper.GenManage, "igen") { HelpText = "建造世界" });

            Commands.ChatCommands.Add(new Command(new List<string>() { "relive" }, NPCHelper.Relive, "relive") { HelpText = "复活NPC" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "bossinfo" }, BossHelper.BossInfo, "bossinfo", "bi") { HelpText = "boss进度信息" });
            Commands.ChatCommands.Add(new Command(new List<string>() { "worldinfo" }, WorldInfo, "worldinfo", "wi") { HelpText = "世界信息" });

            BackupHelper.BackupPath = Path.Combine(SaveDir, "backups");
            Regen.SaveDir = SaveDir;
            RetileHelper.SaveFile = Path.Combine(SaveDir, "retile.json");
            ResearchHelper.SaveFile = Path.Combine(SaveDir, "research.csv");
            BestiaryHelper.SaveFile = Path.Combine(SaveDir, "bestiary.csv");
        }


        private void WMCommand(CommandArgs args)
        {
            TSPlayer op = args.Player;
            #region help
            void ShowHelpText()
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
                    return;

                List<string> lines = new List<string> {
                    "/wm info，查看 世界信息",
                    "/wm name [世界名]，查看/修改 世界名字",
                    "/wm mode [难度]，查看/修改 世界难度",
                    "/wm 2020，开启/关闭 05162020 秘密世界",

                    "/wm 2021，开启/关闭 05162021 秘密世界",
                    "/wm ftw，开启/关闭 for the worthy 秘密世界",
                    "/wm ntb，开启/关闭 not the bees 秘密世界",
                    "/wm dst，开启/关闭 饥荒联动 秘密世界",

                    "/wm seed [种子]，查看/修改 世界种子",
                    "/wm id [id]，查看/修改 世界ID",
                    "/wm uuid [uuid字符|new]，查看/修改 世界uuid",
                    "/wm sundial <on/off | 天数>，开关附魔日晷，修改/查看 附魔日晷冷却天数",

                    "/wm spawn，查看 出生点",
                    "/wm dungeon，查看 地牢点",
                    "/wm surface [深度]，查看/修改 地表深度",
                    "/wm cave [深度]，查看/修改 洞穴深度",

                    "/wm wind，查看 风速",
                    "/wm research help，物品研究",
                    "/wm bestiary help，怪物图鉴",
                    "/wm backup，备份地图",

                    "/moon help，月相管理",
                    "/moonstyle help，月亮样式管理",
                    "/boss help，boss管理",
                    "/npc help，npc管理",
                    "/igen help，建造世界"
                };

                PaginationTools.SendPage(
                    op, pageNumber, lines,
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "帮助 ({0}/{1})：",
                        FooterFormat = "输入 {0}wm help {{0}} 查看更多".SFormat(Commands.Specifier)
                    }
                );
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
                case "help":
                    ShowHelpText();
                    return;

                default:
                    op.SendErrorMessage("语法不正确！输入 /wm help 查询用法");
                    break;

                // 世界信息
                case "info":
                    ShowWorldInfo(args, true);
                    break;


                // 名字
                case "name":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"世界名称: {Main.worldName}\n输入 /wm seed <名称> 可更改世界名称");
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
                        op.SendInfoMessage($"世界难度: {_worldModes.Keys.ElementAt(Main.GameMode)}" +
                            $"\n用法：/wm mode <难度>" +
                            $"\n可用的难度：{string.Join(", ", _worldModes.Keys)}");
                        return;
                    }

                    if (int.TryParse(args.Parameters[1], out int mode))
                    {
                        if (mode < 1 || mode > 4)
                        {
                            op.SendErrorMessage($"语法错误！用法：/wm mode <难度>\n可用的难度：{string.Join(", ", _worldModes.Keys)}");
                            return;
                        }
                    }
                    else if (_worldModes.ContainsKey(args.Parameters[1]))
                    {
                        mode = _worldModes[args.Parameters[1]];
                    }
                    else
                    {
                        op.SendErrorMessage($"语法错误！用法：/wm mode <难度>\n可用的难度：{string.Join(", ", _worldModes.Keys)}");
                        return;
                    }
                    Main.GameMode = mode - 1;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    op.SendSuccessMessage("世界模式已改成 {0}", _worldModes.Keys.ElementAt(mode - 1));
                    break;


                // 种子
                case "seed":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"世界种子: {WorldGen.currentWorldSeed}（{Main.ActiveWorldFileData.GetFullSeedText()}）" +
                            $"\n输入 /wm seed <种子> 可更改世界种子");
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
                        op.SendInfoMessage($"世界ID: {Main.worldID}\n输入 /wm id <id> 可更改世界ID");
                        break;
                    }

                    int worldId;
                    if (int.TryParse(args.Parameters[1], out worldId))
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
                        op.SendInfoMessage($"uuid: {Main.ActiveWorldFileData.UniqueId}\n输入 /wm uuid <uuid> 可更改世界的uuid");
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
                        if (utils.ToGuid(uuid))
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


                # region 附魔日晷
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
                            if (!Main.fastForwardTime)
                            {
                                Main.fastForwardTime = true;
                                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                                op.SendSuccessMessage("附魔日晷 已开启");
                            }
                            else
                            {
                                op.SendSuccessMessage("附魔日晷 已是开启状态");
                            }
                            break;
                        case "off":
                            if (Main.fastForwardTime)
                            {
                                Main.fastForwardTime = false;
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
                #endregion

                // 地表深度
                case "surface":
                    if (args.Parameters.Count == 1)
                    {
                        op.SendInfoMessage($"表层深度: {Main.worldSurface}\n输入 /wm surface <深度> 可修改地表深度");
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
                        op.SendInfoMessage($"洞穴深度: {Main.rockLayer}\n输入 /wm cave <深度> 可修改洞穴深度");
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
                    op.SendInfoMessage($"出生点：{Main.spawnTileX}, {Main.spawnTileY}" +
                        $"\n进入游戏后，输入 /setspawn 设置出生点" +
                        $"\n进入游戏后，输入 /spawn 传送至出生点");
                    break;

                // 地牢点
                case "dungeon":
                case "dun":
                    op.SendInfoMessage($"地牢点：{Main.dungeonX}, {Main.dungeonY}" +
                        $"\n进入游戏后，输入 /setdungeon 设置地牢点" +
                        $"\n进入游戏后，输入 /tpnpc \"Old Man\" 传送至地牢点");
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
                    if (Main.drunkWorld)
                    {
                        Main.drunkWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已关闭 05162020 秘密世界（醉酒世界 / DrunkWorld）");
                    }
                    else
                    {
                        Main.drunkWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已开启 05162020 秘密世界（醉酒世界 / DrunkWorld）");
                    }
                    break;


                // 10周年庆典,tenthAnniversaryWorld
                case "2011":
                case "2021":
                case "5162011":
                case "5162021":
                case "05162011":
                case "05162021":
                case "celebrationmk10":
                    if (Main.tenthAnniversaryWorld)
                    {
                        Main.tenthAnniversaryWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已关闭 05162021 秘密世界（10周年庆典）");
                    }
                    else
                    {
                        Main.tenthAnniversaryWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已开启 05162021 秘密世界（10周年庆典）");
                    }
                    break;


                // ftw
                case "ftw":
                case "for the worthy":
                    if (Main.getGoodWorld)
                    {
                        Main.getGoodWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已关闭 for the worthy 秘密世界");
                    }
                    else
                    {
                        Main.getGoodWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已开启 for the worthy 秘密世界");
                    }
                    break;

                // not the bees
                case "ntb":
                    if (Main.notTheBeesWorld)
                    {
                        Main.notTheBeesWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已关闭 not the bees 秘密世界");
                    }
                    else
                    {
                        Main.notTheBeesWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已开启 not the bees 秘密世界");
                    }
                    break;

                //  饥荒联动
                case "eye":
                case "dst":
                case "constant":
                    if (Main.dontStarveWorld)
                    {
                        Main.dontStarveWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已关闭 永恒领域 秘密世界（饥荒联动）");
                    }
                    else
                    {
                        Main.dontStarveWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已开启 永恒领域 秘密世界（饥荒联动）");
                    }
                    break;
                #endregion

                // 全物品研究
                case "research":
                case "re":
                    args.Parameters.RemoveAt(0);
                    ResearchHelper.Manage(args);
                    break;

                // 怪物图鉴
                case "bestiary":
                case "be":
                    args.Parameters.RemoveAt(0);
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
                    args.Parameters.RemoveAt(0);
                    TileHelper.FindCommand(args);
                    break;
            }
        }

        # region worldinfo
        static Dictionary<string, int> _worldModes = new Dictionary<string, int>
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

            List<string> lines = new List<string> {
                $"名称: {Main.worldName}",
                $"大小: {Main.ActiveWorldFileData.WorldSizeName}（{Main.maxTilesX}x{Main.maxTilesY}）",
                $"难度: {_worldModes.Keys.ElementAt(Main.GameMode)}",
                $"种子: {WorldGen.currentWorldSeed}"
            };
            // if( isSuperAdmin )
            // {
            //     // lines.Add($"ID: {Main.worldID}");
            //     // lines.Add($"UUID: {Main.ActiveWorldFileData.UniqueId}");
            //     // lines.Add($"版本: {Main.curRelease}  {Main.versionNumber}");
            // }

            // 腐化 秘密世界
            string text = GetSecretWorldDescription();
            if (!string.IsNullOrEmpty(text))
                lines.Add(text);
            lines.Add(GetCorruptionDescription(isSuperAdmin));

            // 时间
            if (isSuperAdmin)
            {
                double time = Main.time / 3600.0;
                time += 4.5;
                if (!Main.dayTime)
                    time += 15.0;
                time %= 24.0;
                lines.Add(string.Format("时间：{0}:{1:D2}", (int)Math.Floor(time), (int)Math.Floor((time % 1.0) * 60.0)));
            }

            // 附魔日晷
            text = GetSundial();
            if (!string.IsNullOrEmpty(text))
                lines.Add(GetSundial());

            if (isSuperAdmin)
            {
                lines.Add($"月亮: {_moonPhases.Keys.ElementAt(Main.moonPhase)}");
                lines.Add($"月亮样式: {_moonTypes.Keys.ElementAt(Main.moonType)}");

                string percent;
                if (TShock.ServerSideCharacterConfig.Settings.Enabled && Main.GameMode == 3)
                {
                    int num1 = ResearchHelper.GetSacrificeCompleted();
                    int num2 = ResearchHelper.GetSacrificeTotal();
                    if (num1 > 0)
                    {
                        percent = Terraria.Utils.PrettifyPercentDisplay(num1 / num2, "P2");
                        lines.Add($"物品研究：{percent}（{num1}/{num2}）");
                    }
                }

                BestiaryUnlockProgressReport result = Main.GetBestiaryProgressReport();
                percent = Terraria.Utils.PrettifyPercentDisplay(result.CompletionPercent, "P2");
                if (result.CompletionAmountTotal > 0)
                    lines.Add($"怪物图鉴：{percent}（{result.CompletionAmountTotal}/{result.EntriesTotal}）");


                lines.Add($"出生点：{Main.spawnTileX}, {Main.spawnTileY}");
                lines.Add($"地牢点：{Main.dungeonX}, {Main.dungeonY}");
                lines.Add($"表层深度: {Main.worldSurface}");
                lines.Add($"洞穴深度: {Main.rockLayer}");


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

                // 日食、血月
                // 哥布林军队、海盗入侵
                // 撒旦军队
                // 派对、雨、沙尘暴
                // 史莱姆雨
                // 大风天
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
                    lines.Add($"入侵：{string.Join(", ", text)}");

                // lines.Add($"云量：{Main.numClouds}");
                // lines.Add($"风速：{Main.windSpeedCurrent}");

                // 杂项
                List<string> texts = new List<string>();
                if (BirthdayParty._wasCelebrating) texts.Add("派对");
                if (LanternNight.LanternsUp) texts.Add("灯笼夜");
                if (Star.starfallBoost > 3f) texts.Add("流星雨");
                if (Main.bloodMoon) texts.Add("血月");
                if (Main.eclipse) texts.Add("日食");
                if (Main.raining) texts.Add("雨");
                if (Main.IsItStorming) texts.Add("雷雨");
                if (Main.IsItAHappyWindyDay) texts.Add("大风天");
                if (Sandstorm.Happening) texts.Add("沙尘暴");
                if (Main.slimeRain) texts.Add("史莱姆雨");
                if (texts.Count > 0)
                    lines.Add($"事件：{string.Join(", ", texts)}");

                texts = new List<string>();
                if (WorldGen.spawnMeteor) texts.Add("陨石");
                if (Main.xMas) texts.Add("圣诞节");
                if (Main.halloween) texts.Add("万圣节");
                if (texts.Count > 0)
                    lines.Add($"杂项：{string.Join(", ", texts)}");
            }
            op.SendInfoMessage(string.Join("\n", lines));
        }
        #endregion

        #region moon
        static Dictionary<string, int> _moonPhases = new Dictionary<string, int>
        {
            { "满月", 1 },
            { "亏凸月", 2 },
            { "下弦月", 3 },
            { "残月", 4 },
            { "新月", 5 },
            { "娥眉月", 6 },
            { "上弦月", 7 },
            { "盈凸月", 8 }
        };

        // https://terraria.fandom.com/wiki/Moon_phase
        static Dictionary<string, int> _moonTypes = new Dictionary<string, int>
        {
            { "正常", 1 },
            { "火星样式", 2 },
            { "土星样式", 3 },
            { "秘银风格", 4 },
            { "明亮的偏蓝白色", 5 },
            { "绿色", 6 },
            { "糖果", 7 },
            { "金星样式", 8 },
            { "紫色的三重月亮", 9 }
        };

        // 修改月相
        private void ChangeMoonPhase(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                args.Player.SendInfoMessage("当前月相: {0}", _moonPhases.Keys.ElementAt(Main.moonPhase));
                args.Player.SendInfoMessage("用法：/moon <月相>");
                args.Player.SendInfoMessage("月相：{0} （可用数字 1~8 代替）", String.Join(", ", _moonPhases.Keys));
                return;
            }

            int moon;
            if (int.TryParse(args.Parameters[0], out moon))
            {
                if (moon < 1 || moon > 8)
                {
                    args.Player.SendErrorMessage("语法错误！用法：/moon <月相>");
                    args.Player.SendErrorMessage("月相：{0} （可用数字 1~8 代替）", String.Join(", ", _moonPhases.Keys));
                    return;
                }
            }
            else if (_moonPhases.ContainsKey(args.Parameters[0]))
            {
                moon = _moonPhases[args.Parameters[0]];
            }
            else
            {
                args.Player.SendErrorMessage("语法错误！用法：/moon <月相>");
                args.Player.SendErrorMessage("月相：{0} （可用数字 1~8 代替）", String.Join(", ", _moonPhases.Keys));
                return;
            }

            Main.dayTime = false;
            Main.moonPhase = moon - 1;
            Main.time = 0.0;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
            args.Player.SendSuccessMessage("月相已改为 {0}", _moonPhases.Keys.ElementAt(moon - 1));
        }

        // 修改月亮样式
        private void ChangeMoonStyle(CommandArgs args)
        {
            void helpText()
            {
                args.Player.SendInfoMessage("用法：/moonstyle <月亮样式>");
                args.Player.SendInfoMessage("月亮样式：{0} （可用数字 1~9 代替）", String.Join(", ", _moonTypes.Keys));
            }

            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendInfoMessage("当前月亮样式: {0}", _moonTypes.Keys.ElementAt(Main.moonType));
                helpText();
                return;
            }

            if (args.Parameters[0].ToLowerInvariant() == "help")
            {
                helpText();
                return;
            }

            int moontype;
            if (int.TryParse(args.Parameters[0], out moontype))
            {
                if (moontype < 1 || moontype > 9)
                {
                    helpText();
                    return;
                }
            }
            else if (_moonTypes.ContainsKey(args.Parameters[0]))
            {
                moontype = _moonTypes[args.Parameters[0]];
            }
            else
            {
                helpText();
                return;
            }
            Main.dayTime = false;
            Main.moonType = moontype - 1;
            Main.time = 0.0;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
            args.Player.SendSuccessMessage("月亮样式已改为 {0}", _moonTypes.Keys.ElementAt(moontype - 1));
        }
        #endregion

        #region 附魔日晷
        private string GetSundial()
        {
            // 附魔日晷
            string text = Main.fastForwardTime ? "生效中" : "";
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
        #endregion

        #region get description
        // 获取秘密世界种子状态描述
        private string GetSecretWorldDescription()
        {
            List<string> ss = new List<string>();

            if (Main.getGoodWorld)
                ss.Add("for the worthy");

            if (Main.drunkWorld)
                ss.Add("05162020");

            if (Main.tenthAnniversaryWorld)
                ss.Add("05162021");

            if (Main.dontStarveWorld)
                ss.Add("the constant");

            if (Main.notTheBeesWorld)
                ss.Add("not the bees");

            if (ss.Count > 0)
                return $"彩蛋: {string.Join(", ", ss)}";
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
                    text = $"已摧毁 {WorldGen.shadowOrbCount}个[i:115]";
                else if (type == 2)
                    text = $"已摧毁 {WorldGen.heartCount}个[i:3062]";
                else
                    text = $"已摧毁 {WorldGen.heartCount}个[i:3062] {WorldGen.shadowOrbCount}个[i:115]";

                if (Main.hardMode)
                    text += $" {WorldGen.altarCount}个祭坛";

                string s2 = GetWorldStatusDialog();
                if (!string.IsNullOrEmpty(s2))
                    text += $", {s2}";

                return text;
            }

            if (Main.drunkWorld)
            {
                if (WorldGen.crimson)
                    return $"腐化: 今天是猩红（醉酒世界）, {more(3)}";
                else
                    return $"腐化: 今天是腐化（醉酒世界）, {more(3)}";
            }
            else
            {
                if (WorldGen.crimson)
                    return $"腐化: 猩红, {more(2)}";
                else
                    return $"腐化: 腐化, {more(1)}";
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
                _moonPhases.Clear();
                _moonTypes.Clear();

                BossHelper.Clear();
                NPCHelper.Clear();
                ResearchHelper.Clear();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
