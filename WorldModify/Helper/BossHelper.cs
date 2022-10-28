using System.Collections.Generic;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    class BossHelper
    {
        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                op.SendInfoMessage("/boss info, 查看boss进度");
                op.SendInfoMessage("/boss sb, sb 召唤指令备注（SpawnBoss boss召唤指令）");
                op.SendInfoMessage("/boss <boss名>, 切换boss击败状态");
                op.SendInfoMessage("/boss list, 查看支持切换击败状态的boss名");

                return;
            }

            string param = args.Parameters[0].ToLowerInvariant();
            switch (param)
            {
                default:
                    // 标记进度
                    bool isPass = ToggleBoss(op, param);
                    if (!isPass)
                        op.SendErrorMessage("语法不正确！");
                    break;


                // boss
                case "sb":
                case "spawn":
                    ShowSpawnTips(args);
                    break;


                // 查看boss进度
                case "info":
                    BossInfo(args);
                    break;
            }

        }

        private static void ShowSpawnTips(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
                return;

            List<string> lines = new List<string> {
                "史莱姆王, {0}ks, {0}\"king slime\", {0}king",
                "克苏鲁之眼, {0}eoc, {0}\"eye of cthulhu\", {0}eye（时间会调到晚上）",
                "克苏鲁之脑, {0}boc, {0}\"brain of cthulhu\", {0}brain（不在 猩红之地 会脱战）",
                "世界吞噬怪, {0}eow, {0}\"eater of worlds\", {0}eater（不在 腐化之地 会脱战）",

                "蜂王, {0}qb, {0}\"queen bee\"",
                "骷髅王, {0}skeletron（时间会调到晚上）",
                "鹿角怪, {0}deerclops",
                "血肉墙, {0}wof, {0}\"wall of flesh\"（得在地狱）",

                "史莱姆皇后, {0}qs, {0}\"queen slime\"",
                "机械骷髅王, {0}prime",
                "双子魔眼, {0}twins（时间会调到晚上）",
                "毁灭者, {0}destroyer（时间会调到晚上）",

                "世纪之花, {0}plantera",
                "石巨人, {0}golem",
                "光之女皇, {0}eol, {0}\"empress of light\", {0}empress（不会将时间调到晚上）",
                "猪龙鱼公爵, {0}duke, {0}\"duke fishron\", {0}fishron",

                "哀木, {0}\"mourning wood\"（不会将时间调到晚上，白天会脱战）",
                "南瓜王, {0}pumpking（不会将时间调到晚上，白天会脱战）",
                "常绿尖叫怪, {0}everscream（不会将时间调到晚上，白天会脱战）",
                "冰雪女王, {0}\"ice queen\"（不会将时间调到晚上，白天会脱战）",

                "圣诞坦克, {0}santa（不会将时间调到晚上，白天会脱战）",
                "荷兰飞盗船, {0}\"flying dutchman\", {0}flying, {0}dutchman",
                $"火星飞碟, {0}\"martian saucer\", {Commands.Specifier}sm 395",
                "双足翼龙, {0}betsy（可召唤多只，玩家死亡不脱战）",

                "日耀柱, {0}\"solar pillar\"",
                "星旋柱, {0}\"vortex pillar\"",
                "星云柱, {0}\"nebula pillar\"",
                "星尘柱, {0}\"stardust pillar\"",

                "拜月教邪教徒, {0}lc, {0}\"lunatic cultist\", {0}lunatic, {0}cultist",
                "月亮领主, {0}moon, {0}\"moon lord\", {0}ml"
            };

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = string.Format(lines[i], $"{Commands.Specifier}sb ");
            }
            PaginationTools.SendPage(
                op, pageNumber, lines,
                new PaginationTools.Settings
                {
                    HeaderFormat = Commands.Specifier + "spawnboss 指令 贴士 ({0}/{1})：",
                    FooterFormat = "输入 {0}boss sb {{0}} 查看更多".SFormat(Commands.Specifier)
                }
            );
        }

        /// <summary>
        /// 切换BOSS击败状态
        /// </summary>
        private static bool ToggleBoss(TSPlayer op, string param)
        {
            switch (param)
            {
                case "list":
                    string[] li1 = {
                        "史莱姆王",
                        "克苏鲁之眼",
                        "鹿角怪",
                        "世界吞噬怪",
                        "克苏鲁之脑",
                        "蜂王",
                        "骷髅王",
                        "血肉墙"
                    };

                    string[] li2 = {
                        "毁灭者",
                        "双子魔眼",
                        "机械骷髅王",
                        "世纪之花",
                        "石巨人",
                        "史莱姆皇后",
                        "光之女皇",
                        "猪龙鱼公爵",
                        "拜月教邪教徒",
                        "月亮领主"
                    };

                    string[] li3 = {
                        // "小丑",
                        "哥布林军队",
                        "海盗入侵",
                        "火星暴乱",
                        "哀木",
                        "南瓜王",
                        "冰雪女王",
                        "常绿尖叫怪",
                        "圣诞坦克",
                        "日耀柱",
                        "星旋柱",
                        "星云柱",
                        "星尘柱"
                    };

                    op.SendInfoMessage("支持切换的BOSS击败状态的有");
                    op.SendInfoMessage("肉前：{0}", string.Join(", ", li1));
                    op.SendInfoMessage("肉后：{0}", string.Join(", ", li2));
                    op.SendInfoMessage("事件：{0}", string.Join(", ", li3));
                    break;

                // 史莱姆王
                case "史莱姆王":
                case "史莱姆国王":
                case "king slime":
                case "king":
                case "ks":
                    NPC.downedSlimeKing = !NPC.downedSlimeKing;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedSlimeKing)
                        op.SendSuccessMessage("已标记 史莱姆王 为已击败");
                    else
                        op.SendSuccessMessage("已标记 史莱姆王 为未击败");
                    break;


                //  鹿角怪
                case "鹿角怪":
                case "deerclops":
                case "deer":
                case "独眼巨鹿":
                case "巨鹿":
                    NPC.downedDeerclops = !NPC.downedDeerclops;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedDeerclops)
                        op.SendSuccessMessage("已标记 鹿角怪 为已击败");
                    else
                        op.SendSuccessMessage("已标记 鹿角怪 为未击败");
                    break;


                // 克苏鲁之眼
                case "克苏鲁之眼":
                case "克眼":
                case "eye of cthulhu":
                case "eye":
                case "eoc":
                    NPC.downedBoss1 = !NPC.downedBoss1;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedBoss1)
                        op.SendSuccessMessage("已标记 克苏鲁之眼 为已击败");
                    else
                        op.SendSuccessMessage("已标记 克苏鲁之眼 为未击败");
                    break;


                // 世界吞噬怪 或 克苏鲁之脑
                case "世界吞噬怪":
                case "世吞":
                case "黑长直":
                case "克苏鲁之脑":
                case "克脑":
                case "brain of cthulhu":
                case "boc":
                case "brain":
                case "eater of worlds":
                case "eow":
                case "eater":
                case "boss2":
                    NPC.downedBoss2 = !NPC.downedBoss2;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    string boss2Name2 = "";
                    if (Main.ActiveWorldFileData.HasCrimson && Main.ActiveWorldFileData.HasCorruption)
                        boss2Name2 = "世界吞噬怪 或 克苏鲁之脑";
                    else if (Main.ActiveWorldFileData.HasCrimson)
                        boss2Name2 = "克苏鲁之脑";
                    else if (Main.ActiveWorldFileData.HasCorruption)
                        boss2Name2 = "世界吞噬怪";

                    if (NPC.downedBoss1)
                        op.SendSuccessMessage("已标记 {0} 为已击败", boss2Name2);
                    else
                        op.SendSuccessMessage("已标记 {0} 为未击败", boss2Name2);
                    break;


                // 骷髅王
                case "骷髅王":
                case "skeletron":
                case "boss3":
                    NPC.downedBoss3 = !NPC.downedBoss3;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedBoss3)
                        op.SendSuccessMessage("已标记 骷髅王 为已击败");
                    else
                        op.SendSuccessMessage("已标记 骷髅王 为未击败");
                    break;


                // 蜂王
                case "蜂王":
                case "蜂后":
                case "queen bee":
                case "qb":
                    NPC.downedQueenBee = !NPC.downedQueenBee;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedQueenBee)
                        op.SendSuccessMessage("已标记 蜂王 为已击败");
                    else
                        op.SendSuccessMessage("已标记 蜂王 为未击败");
                    break;


                // 血肉墙
                case "血肉墙":
                case "血肉之墙":
                case "肉山":
                case "wall of flesh":
                case "wof":
                    if (Main.hardMode)
                    {
                        Main.hardMode = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        op.SendSuccessMessage("已标记 血肉墙 为未击败（困难模式 已关闭）");
                    }
                    else if (!TShock.Config.Settings.DisableHardmode)
                    {
                        WorldGen.StartHardmode();
                        op.SendSuccessMessage("已标记 血肉墙 为已击败（困难模式 已开启）");
                    }
                    break;


                // 毁灭者
                case "毁灭者":
                case "铁长直":
                case "destroyer":
                    NPC.downedMechBoss1 = !NPC.downedMechBoss1;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedMechBoss1)
                        op.SendSuccessMessage("已标记 毁灭者 为已击败");
                    else
                        op.SendSuccessMessage("已标记 毁灭者 为未击败");
                    break;


                // 双子魔眼
                case "双子魔眼":
                case "twins":
                    NPC.downedMechBoss2 = !NPC.downedMechBoss2;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedMechBoss2)
                        op.SendSuccessMessage("已标记 双子魔眼 为已击败");
                    else
                        op.SendSuccessMessage("已标记 双子魔眼 为未击败");
                    break;


                // 机械骷髅王
                case "机械骷髅王":
                case "skeletron prime":
                case "prime":
                    NPC.downedMechBoss3 = !NPC.downedMechBoss3;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedMechBoss3)
                        op.SendSuccessMessage("已标记 机械骷髅王 为已击败");
                    else
                        op.SendSuccessMessage("已标记 机械骷髅王 为未击败");
                    break;


                // 世纪之花
                case "世纪之花":
                case "plantera":
                    NPC.downedPlantBoss = !NPC.downedPlantBoss;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedPlantBoss)
                        op.SendSuccessMessage("已标记 世纪之花 为已击败");
                    else
                        op.SendSuccessMessage("已标记 世纪之花 为未击败");
                    break;


                // 石巨人
                case "石巨人":
                case "golem":
                    NPC.downedGolemBoss = !NPC.downedGolemBoss;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedGolemBoss)
                        op.SendSuccessMessage("已标记 石巨人 为已击败");
                    else
                        op.SendSuccessMessage("已标记 石巨人 为未击败");
                    break;


                // 史莱姆皇后
                case "史莱姆皇后":
                case "史莱姆女王":
                case "史莱姆王后":
                case "queen slime":
                case "qs":
                    NPC.downedQueenSlime = !NPC.downedQueenSlime;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedQueenSlime)
                        op.SendSuccessMessage("已标记 史莱姆皇后 为已击败");
                    else
                        op.SendSuccessMessage("已标记 史莱姆皇后 为未击败");
                    break;


                // 光之女皇
                case "光之女皇":
                case "光女":
                case "光之女神":
                case "光之皇后":
                case "empress of light":
                case "empress":
                case "eol":
                    NPC.downedEmpressOfLight = !NPC.downedEmpressOfLight;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedEmpressOfLight)
                        op.SendSuccessMessage("已标记 光之女皇 为已击败");
                    else
                        op.SendSuccessMessage("已标记 光之女皇 为未击败");
                    break;


                // 猪龙鱼公爵
                case "猪龙鱼公爵":
                case "猪鲨":
                case "duke fishron":
                case "duke":
                case "fishron":
                    NPC.downedFishron = !NPC.downedFishron;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedFishron)
                        op.SendSuccessMessage("已标记 猪龙鱼公爵 为已击败");
                    else
                        op.SendSuccessMessage("已标记 猪龙鱼公爵 为未击败");
                    break;



                // 拜月教邪教徒
                case "拜月教邪教徒":
                case "拜月教":
                case "邪教徒":
                case "lunatic cultist":
                case "lunatic":
                case "cultist":
                case "lc":
                    NPC.downedAncientCultist = !NPC.downedAncientCultist;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedAncientCultist)
                        op.SendSuccessMessage("已标记 拜月教邪教徒 为已击败");
                    else
                        op.SendSuccessMessage("已标记 拜月教邪教徒 为未击败");
                    break;


                // 月亮领主
                case "月亮领主":
                case "月总":
                case "moon lord":
                case "moon":
                case "ml":
                    NPC.downedMoonlord = !NPC.downedMoonlord;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedMoonlord)
                        op.SendSuccessMessage("已标记 月亮领主 为已击败");
                    else
                        op.SendSuccessMessage("已标记 月亮领主 为未击败");
                    break;


                // // 小丑
                // case "小丑":
                // case "clown":
                //     NPC.downedClown = !NPC.downedClown;
                //     TSPlayer.All.SendData(PacketTypes.WorldInfo);
                //     if (NPC.downedClown)
                //         op.SendSuccessMessage("已标记 小丑 为已击败");
                //     else
                //         op.SendSuccessMessage("已标记 小丑 为未击败");
                //     break;


                // 哥布林军队
                case "哥布林军队":
                case "哥布林":
                case "goblin":
                case "goblins":
                    NPC.downedGoblins = !NPC.downedGoblins;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedGoblins)
                        op.SendSuccessMessage("已标记 哥布林军队 为已击败");
                    else
                        op.SendSuccessMessage("已标记 哥布林军队 为未击败");
                    break;


                // 海盗入侵
                case "海盗入侵":
                case "荷兰飞盗船":
                case "海盗船":
                case "pirate":
                case "pirates":
                case "flying dutchman":
                case "flying":
                case "dutchman":
                    NPC.downedPirates = !NPC.downedPirates;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedPirates)
                        op.SendSuccessMessage("已标记 海盗入侵 为已击败");
                    else
                        op.SendSuccessMessage("已标记 海盗入侵 为未击败");
                    break;


                // 火星暴乱
                case "火星暴乱":
                case "火星人入侵":
                case "火星飞碟":
                case "ufo":
                case "martian saucer":
                case "martian":
                case "martians":
                    NPC.downedMartians = !NPC.downedMartians;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedMartians)
                        op.SendSuccessMessage("已标记 火星暴乱 为已击败");
                    else
                        op.SendSuccessMessage("已标记 火星暴乱 为未击败");
                    break;


                // 哀木
                case "哀木":
                case "mourning wood":
                case "wood":
                case "halloween tree":
                case "ht":
                    NPC.downedHalloweenTree = !NPC.downedHalloweenTree;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedHalloweenTree)
                        op.SendSuccessMessage("已标记 哀木 为已击败");
                    else
                        op.SendSuccessMessage("已标记 哀木 为未击败");
                    break;


                // 南瓜王
                case "南瓜王":
                case "pumpking":
                case "halloween king":
                case "hk":
                    NPC.downedHalloweenKing = !NPC.downedHalloweenKing;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedHalloweenKing)
                        op.SendSuccessMessage("已标记 南瓜王 为已击败");
                    else
                        op.SendSuccessMessage("已标记 南瓜王 为未击败");
                    break;


                // 冰雪女王
                case "冰雪女王":
                case "冰雪皇后":
                case "ice queen":
                    NPC.downedChristmasIceQueen = !NPC.downedChristmasIceQueen;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedChristmasIceQueen)
                        op.SendSuccessMessage("已标记 冰雪女王 为已击败");
                    else
                        op.SendSuccessMessage("已标记 冰雪女王 为未击败");
                    break;


                // 常绿尖叫怪
                case "常绿尖叫怪":
                case "everscream":
                    NPC.downedChristmasTree = !NPC.downedChristmasTree;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedChristmasTree)
                        op.SendSuccessMessage("已标记 常绿尖叫怪 为已击败");
                    else
                        op.SendSuccessMessage("已标记 常绿尖叫怪 为未击败");
                    break;


                // 圣诞坦克
                case "圣诞坦克":
                case "santa":
                case "santa-nk1":
                case "tank":
                    NPC.downedChristmasSantank = !NPC.downedChristmasSantank;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedChristmasSantank)
                        op.SendSuccessMessage("已标记 圣诞坦克 为已击败");
                    else
                        op.SendSuccessMessage("已标记 圣诞坦克 为未击败");
                    break;


                // 日耀柱
                case "日耀柱":
                case "日耀":
                case "日曜柱":
                case "日曜":
                case "solar pillar":
                case "solar":
                    NPC.downedTowerSolar = !NPC.downedTowerSolar;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedTowerSolar)
                        op.SendSuccessMessage("已标记 日曜柱 为已击败");
                    else
                        op.SendSuccessMessage("已标记 日曜柱 为未击败");
                    break;


                // 星旋柱
                case "星旋柱":
                case "星旋":
                case "vortex pillar":
                case "vortex":
                    NPC.downedTowerVortex = !NPC.downedTowerVortex;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedTowerVortex)
                        op.SendSuccessMessage("已标记 星旋柱 为已击败");
                    else
                        op.SendSuccessMessage("已标记 星旋柱 为未击败");
                    break;


                // 星云柱
                case "星云柱":
                case "星云":
                case "nebula pillar":
                case "nebula":
                    NPC.downedTowerNebula = !NPC.downedTowerNebula;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedTowerNebula)
                        op.SendSuccessMessage("已标记 星云柱 为已击败");
                    else
                        op.SendSuccessMessage("已标记 星云柱 为未击败");
                    break;


                // 星尘柱
                case "星尘柱":
                case "星尘":
                case "stardust pillar":
                case "stardust":
                    NPC.downedTowerStardust = !NPC.downedTowerStardust;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.downedTowerStardust)
                        op.SendSuccessMessage("已标记 星尘柱 为已击败");
                    else
                        op.SendSuccessMessage("已标记 星尘柱 为未击败");
                    break;


                // 双足翼龙
                case "betsy":
                    op.SendErrorMessage("暂时不支持标记 双足翼龙");
                    break;


                default:
                    // op.SendErrorMessage("语法不正确！，请使用 /boss toggle help, 进行查询");
                    return false;
            }
            return true;
        }

        public static void BossInfo(CommandArgs args)
        {
            args.Player.SendInfoMessage(string.Join("\n", ShowBossInfo()));
        }

        private static List<string> ShowBossInfo(bool isSuperAdmin = false)
        {
            List<string> li = new List<string>();
            List<string> li1 = new List<string>();
            List<string> li2 = new List<string>();
            List<string> li3 = new List<string>();

            li1.Add(utils.CFlag(NPC.downedSlimeKing, "史莱姆王"));
            li1.Add(utils.CFlag(NPC.downedBoss1, "克苏鲁之眼"));

            string boss2Name = "";
            if (Main.drunkWorld)
                boss2Name = "世界吞噬怪 或 克苏鲁之脑";
            else
                boss2Name = WorldGen.crimson ? "克苏鲁之脑" : "世界吞噬怪";
            li1.Add(utils.CFlag(NPC.downedBoss2, boss2Name));

            li1.Add(utils.CFlag(NPC.downedDeerclops, "鹿角怪"));
            li1.Add(utils.CFlag(NPC.downedBoss3, "骷髅王"));
            li1.Add(utils.CFlag(NPC.downedQueenBee, "蜂王"));
            li1.Add(utils.CFlag(Main.hardMode, "血肉墙"));


            li2.Add(utils.CFlag(NPC.downedMechBoss1, "毁灭者"));
            li2.Add(utils.CFlag(NPC.downedMechBoss2, "双子魔眼"));
            li2.Add(utils.CFlag(NPC.downedMechBoss3, "机械骷髅王"));

            li2.Add(utils.CFlag(NPC.downedPlantBoss, "世纪之花"));
            li2.Add(utils.CFlag(NPC.downedGolemBoss, "石巨人"));

            li2.Add(utils.CFlag(NPC.downedQueenSlime, "史莱姆皇后"));
            li2.Add(utils.CFlag(NPC.downedEmpressOfLight, "光之女皇"));

            li2.Add(utils.CFlag(NPC.downedFishron, "猪龙鱼公爵"));
            li2.Add(utils.CFlag(NPC.downedAncientCultist, "拜月教邪教徒"));
            li2.Add(utils.CFlag(NPC.downedMoonlord, "月亮领主"));


            // li3.Add(utils.CFlag(NPC.downedClown, "小丑") );
            li3.Add(utils.CFlag(NPC.downedGoblins, "哥布林军队"));
            li3.Add(utils.CFlag(NPC.downedPirates, "海盗入侵"));
            li3.Add(utils.CFlag(NPC.downedMartians, "火星暴乱"));

            li3.Add(utils.CFlag(NPC.downedHalloweenTree, "哀木"));
            li3.Add(utils.CFlag(NPC.downedHalloweenKing, "南瓜王"));

            li3.Add(utils.CFlag(NPC.downedChristmasIceQueen, "冰雪女王"));
            li3.Add(utils.CFlag(NPC.downedChristmasTree, "常绿尖叫怪"));
            li3.Add(utils.CFlag(NPC.downedChristmasSantank, "圣诞坦克"));

            li3.Add(utils.CFlag(NPC.downedTowerSolar, "日耀柱"));
            li3.Add(utils.CFlag(NPC.downedTowerVortex, "星旋柱"));
            li3.Add(utils.CFlag(NPC.downedTowerNebula, "星云柱"));
            li3.Add(utils.CFlag(NPC.downedTowerStardust, "星尘柱"));

            return new List<string>(){
                string.Format("肉前：{0}", string.Join(", ", li1)),
                string.Format("肉后：{0}", string.Join(", ", li2)),
                string.Format("事件：{0}", string.Join(", ", li3))
            };
        }
    }
}