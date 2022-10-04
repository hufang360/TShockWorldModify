using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using TShockAPI;


namespace WorldModify
{
    class NPCHelper
    {
        #region NPC管理
        /// <summary>
        /// NPC 管理
        /// </summary>
        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                op.SendInfoMessage("/npc info, 查看npc解救情况");
                op.SendInfoMessage("/npc <解救npc名 或 猫/狗/兔 >, 切换NPC解救状态");
                op.SendInfoMessage("/npc list, 查看支持切换解救状态的NPC名");
                op.SendInfoMessage("/npc clear <NPC名>, 移除一个NPC");
                op.SendInfoMessage("/npc unique, NPC去重");
                op.SendInfoMessage("/npc tphere <NPC名|town>, 将NPC传送到你身边");
                op.SendInfoMessage("/npc relive, 复活NPC（根据怪物图鉴记录）");
                op.SendInfoMessage("/npc sm, sm召唤指令备注（SpawnMob NPC召唤指令）");
                op.SendInfoMessage("/npc mq, 召唤美杜莎boss");
                return;
            }


            string param = args.Parameters[0].ToLowerInvariant();
            switch (param)
            {
                default:
                    // 标记进度
                    if (!ToggleNPC(op, param)) op.SendErrorMessage("语法不正确！");
                    break;

                case "sm":
                case "spawn":
                    List<string> names = _NPCTypes.Keys.ToList<string>();
                    List<string> newStrs = new List<string>();
                    for (int i = 0; i < names.Count; i++)
                    {
                        newStrs.Add(string.Format("/sm {0} ({1})", _NPCTypes[names[i]], names[i]));
                    }
                    op.SendInfoMessage("以下是NPC生成指令, sm = spawnmob：");
                    op.SendInfoMessage(string.Join(", ", newStrs));
                    break;

                // 查看npc解救情况
                case "info": NPCInfo(args); break;

                // 将npc传到你身边
                case "tphere":
                case "th":
                    TPHereNPC(args);
                    break;

                // 移除一个npc
                case "clear": ClearNPC(args); break;

                // NPC去重
                case "unique": UniqueNPC(args); break;

                // NPC重生
                case "relive": Relive(args); break;

                // 召唤美杜莎boss
                case "mq": NPC.SpawnMechQueen(op.Index); break;
            }
        }
        #endregion

        #region NPC信息
        /// <summary>
        /// NPC信息
        /// </summary>
        private static void NPCInfo(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 1)
            {
                List<string> li1 = new List<string>();
                List<string> li2 = new List<string>();
                List<string> li3 = new List<string>();
                List<string> li4 = new List<string>();

                int npcTotal = 0;
                int townTotal = 0;
                List<int> npcIds = new List<int>();
                List<string> npcNames = new List<string>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!Main.npc[i].active)
                        continue;

                    npcTotal++;

                    if (Main.npc[i].townNPC)
                    {
                        npcIds.Add(Main.npc[i].netID);
                        npcNames.Add(Main.npc[i].FullName);

                        int id = Main.npc[i].netID;
                        if (id != 453 && id != 368 && id != 37)
                            townTotal++;
                    }
                }

                li1.Add(utils.CFlag(npcIds.Contains(22), "向导"));
                li1.Add(utils.CFlag(npcIds.Contains(17), "商人"));
                li1.Add(utils.CFlag(npcIds.Contains(19), "军火商"));
                li1.Add(utils.CFlag(npcIds.Contains(18), "护士"));
                li1.Add(utils.CFlag(npcIds.Contains(369), "渔夫"));
                li1.Add(utils.CFlag(npcIds.Contains(227), "油漆工"));
                li1.Add(utils.CFlag(npcIds.Contains(124), "机械师"));
                li1.Add(utils.CFlag(npcIds.Contains(107), "哥布林工匠"));

                li2.Add(utils.CFlag(npcIds.Contains(228), "巫医"));
                li2.Add(utils.CFlag(npcIds.Contains(20), "树妖"));
                li2.Add(utils.CFlag(npcIds.Contains(207), "染料商"));
                li2.Add(utils.CFlag(npcIds.Contains(54), "服装商"));
                li2.Add(utils.CFlag(npcIds.Contains(353), "发型师"));
                li2.Add(utils.CFlag(npcIds.Contains(38), "爆破专家"));
                li2.Add(utils.CFlag(npcIds.Contains(633), "动物学家"));
                li2.Add(utils.CFlag(npcIds.Contains(208), "派对女孩"));
                li2.Add(utils.CFlag(npcIds.Contains(550), "酒馆老板"));
                li2.Add(utils.CFlag(npcIds.Contains(588), "高尔夫球手"));

                li3.Add(utils.CFlag(npcIds.Contains(108), "巫师"));
                li3.Add(utils.CFlag(npcIds.Contains(229), "海盗"));
                li3.Add(utils.CFlag(npcIds.Contains(209), "机器侠"));
                li3.Add(utils.CFlag(npcIds.Contains(441), "税收官"));
                li3.Add(utils.CFlag(npcIds.Contains(160), "松露人"));
                li3.Add(utils.CFlag(npcIds.Contains(142), "圣诞老人"));
                li3.Add(utils.CFlag(npcIds.Contains(178), "蒸汽朋克人"));
                li3.Add(utils.CFlag(npcIds.Contains(663), "公主"));

                li4.Add(utils.CFlag(npcIds.Contains(637), "猫咪"));
                li4.Add(utils.CFlag(npcIds.Contains(638), "狗狗"));
                li4.Add(utils.CFlag(npcIds.Contains(656), "兔兔"));
                li4.Add(utils.CFlag(npcIds.Contains(37), "老人"));
                li4.Add(utils.CFlag(npcIds.Contains(368), "旅商"));
                li4.Add(utils.CFlag(npcIds.Contains(453), "骷髅商人"));
                List<string> texts = new List<string>()
                {
                    $"共{npcTotal}个NPC, {townTotal}个城镇NPC。",
                    $"肉前：{string.Join(", ", li1)}",
                    $"肉前：{string.Join(", ", li2)}",
                    $"肉后：{string.Join(", ", li3)}",
                    $"其它：{string.Join(", ", li4)}"
                };


                li2 = new List<string>();
                li3 = new List<string>();

                li1 = NPC.savedAngler ? li2 : li3;
                li1.Add("渔夫");

                li1 = NPC.savedWizard ? li2 : li3;
                li1.Add("巫师");

                li1 = NPC.savedMech ? li2 : li3;
                li1.Add("机械师");

                li1 = NPC.savedStylist ? li2 : li3;
                li1.Add("发型师");

                li1 = NPC.savedTaxCollector ? li2 : li3;
                li1.Add("税收官");

                li1 = NPC.savedBartender ? li2 : li3;
                li1.Add("酒馆老板");

                li1 = NPC.savedGoblin ? li2 : li3;
                li1.Add("哥布林工匠");

                li1 = NPC.savedGolfer ? li2 : li3;
                li1.Add("高尔夫球手");


                //if (li2.Count > 0)
                //    texts.Add($"已解救：{string.Join(", ", li2)}");
                if (li3.Count > 0)
                    texts.Add($"待解救：{string.Join(", ", li3)}");


                // 城镇宠物
                li2 = new List<string>();
                li3 = new List<string>();

                li1 = NPC.boughtCat ? li2 : li3;
                li1.Add("猫咪");

                li1 = NPC.boughtDog ? li2 : li3;
                li1.Add("狗狗");

                li1 = NPC.boughtBunny ? li2 : li3;
                li1.Add("兔兔");
                //if (li2.Count > 0)
                //    texts.Add($"已使用：{string.Join("许可证, ", li2)}许可证");
                if (li3.Count > 0)
                    texts.Add($"待使用：{string.Join("许可证, ", li3)}许可证");
                op.SendInfoMessage(string.Join("\n", texts));
            }
            else
            {
                var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[1]);
                if (npcs.Count == 0)
                    args.Player.SendErrorMessage("找不到对应的 NPC");
                else if (npcs.Count > 1)
                    args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
                else
                {
                    int total = CoundNPCByID(npcs[0].netID);
                    if (total > 0)
                    {
                        int index = FindNPCByID(npcs[0].netID);
                        NPC npc2 = Main.npc[index];

                        int num1 = (int)((npc2.position.X + (float)(npc2.width / 2)) * 2f / 16f - (float)Main.maxTilesX);
                        string text2 = ((num1 > 0) ? $"{num1}以东" : ((num1 >= 0) ? "中心" : $"{-num1}以西"));

                        int num2 = (int)((double)((npc2.position.Y + (float)npc2.height) * 2f / 16f) - Main.worldSurface * 2.0);
                        num2 = Math.Abs(num2);
                        float num3 = Main.maxTilesX / 4200;
                        num3 *= num3;
                        int num4 = 1200;
                        float num5 = (float)((double)(npc2.Center.Y / 16f - (65f + 10f * num3)) / (Main.worldSurface / 5.0));
                        string text3 = ((npc2.position.Y > (Main.maxTilesY - 204) * 16) ? "地狱" : ((npc2.position.Y > Main.rockLayer * 16.0 + (num4 / 2) + 16.0) ? "洞穴" : ((num2 > 0) ? "地下" : ((!(num5 >= 1f)) ? "太空" : "地表"))));
                        string text4 = ((num2 != 0) ? $"{num2}的" : "级别");
                        string text5 = text4 + text3;

                        op.SendSuccessMessage($"名称：{npcs[0].FullName}" +
                            $"\n数量：{total}" +
                            $"\n坐标：{text2} {text5}" +
                            $"\n输入 /tppos {(int)npc2.position.X / 16} {(int)npc2.position.Y / 16} 进行传送");
                    }
                    else
                    {
                        op.SendSuccessMessage($"未找到 {npcs[0].FullName}");

                    }
                }
            }
        }
        #endregion

        #region 切换NPC解救状态
        /// <summary>
        /// 切换NPC解救状态
        /// </summary>
        private static bool ToggleNPC(TSPlayer op, string param)
        {
            switch (param)
            {
                case "list":
                    string[] li = {
                    "渔夫",
                    "哥布林工匠",
                    "机械师",
                    "发型师",
                    "酒馆老板",
                    "高尔夫球手",
                    "巫师",
                    "税收官",
                    "猫",
                    "狗",
                    "兔"
                };
                    op.SendInfoMessage("支持切换的NPC拯救/购买状态的有: ");
                    op.SendInfoMessage("{0}", string.Join(", ", li));
                    break;


                // 渔夫
                case "渔夫":
                case "沉睡渔夫":
                case "angler":
                    NPC.savedAngler = !NPC.savedAngler;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedAngler)
                        op.SendSuccessMessage("沉睡渔夫 已解救");
                    else
                        op.SendSuccessMessage("渔夫 已标记为 未解救");
                    break;


                // 哥布林工匠
                case "哥布林工匠":
                case "受缚哥布林":
                case "goblin":
                    NPC.savedGoblin = !NPC.savedGoblin;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedGoblin)
                        op.SendSuccessMessage("受缚哥布林 已解救");
                    else
                        op.SendSuccessMessage("哥布林工匠 已标记为 未解救");
                    break;


                // 机械师
                case "机械师":
                case "受缚机械师":
                case "mech":
                case "mechanic":
                    NPC.savedMech = !NPC.savedMech;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedMech)
                        op.SendSuccessMessage("受缚机械师 已解救");
                    else
                        op.SendSuccessMessage("机械师 已标记为 未解救");
                    break;


                // 发型师
                case "发型师":
                case "受缚发型师":
                case "stylist":
                    NPC.savedStylist = !NPC.savedStylist;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedStylist)
                        op.SendSuccessMessage("被网住的发型师 已解救");
                    else
                        op.SendSuccessMessage("发型师 已标记为 未解救");
                    break;


                // 酒馆老板
                case "酒馆老板":
                case "昏迷男子":
                case "酒保":
                case "bartender":
                case "tavernkeep":
                case "tavern":
                    NPC.savedBartender = !NPC.savedBartender;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedBartender)
                        op.SendSuccessMessage("昏迷男子 已解救");
                    else
                        op.SendSuccessMessage("酒馆老板 已标记为 未解救");
                    break;


                // 高尔夫球手
                case "高尔夫球手":
                case "高尔夫":
                case "golfer":
                    NPC.savedGolfer = !NPC.savedGolfer;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedGolfer)
                        op.SendSuccessMessage("高尔夫球手 已解救");
                    else
                        op.SendSuccessMessage("高尔夫球手 已标记为 未解救");
                    break;


                // 巫师
                case "巫师":
                case "受缚巫师":
                case "wizard":
                    NPC.savedWizard = !NPC.savedWizard;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedWizard)
                        op.SendSuccessMessage("受缚巫师 已解救");
                    else
                        op.SendSuccessMessage("巫师 已标记为 未解救");
                    break;


                // 税收官
                case "税收官":
                case "痛苦亡魂":
                case "tax":
                case "tax collector":
                    NPC.savedTaxCollector = !NPC.savedTaxCollector;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.savedTaxCollector)
                        op.SendSuccessMessage("痛苦亡魂 已净化");
                    else
                        op.SendSuccessMessage("税收官 已标记为 未解救");
                    break;


                // 猫
                case "猫":
                case "cat":
                    NPC.boughtCat = !NPC.boughtCat;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.boughtCat)
                        op.SendSuccessMessage("猫咪许可证 已生效");
                    else
                        op.SendSuccessMessage("猫咪许可证 已标记为 未使用");
                    break;


                // 狗
                case "狗":
                case "dog":
                    NPC.boughtDog = !NPC.boughtDog;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.boughtDog)
                        op.SendSuccessMessage("狗狗许可证 已生效");
                    else
                        op.SendSuccessMessage("狗狗许可证 已标记为 未使用");
                    break;


                // 兔
                case "兔子":
                case "兔兔":
                case "兔":
                case "bunny":
                case "rabbit":
                    NPC.boughtBunny = !NPC.boughtBunny;
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    if (NPC.boughtBunny)
                        op.SendSuccessMessage("兔兔许可证 已生效");
                    else
                        op.SendSuccessMessage("兔兔许可证 已标记为 未使用");
                    break;

                default:
                    // op.SendErrorMessage("语法不正确！，请使用 /npc toggle help, 进行查询");
                    return false;
            }
            return true;
        }
        #endregion

        #region 清理NPC
        /// <summary>
        /// 清理NPC
        /// </summary>
        private static void ClearNPC(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                args.Player.SendInfoMessage("语法不正确");
                args.Player.SendInfoMessage("/npc clear <NPC名>, 清除指定NPC");
                args.Player.SendInfoMessage("/npc clear, 清理敌怪但保留友善NPC");
                return;
            }

            // 清除指定NPC/敌怪
            switch (args.Parameters[1].ToLowerInvariant())
            {
                case "help":
                    args.Player.SendInfoMessage("语法不正确");
                    args.Player.SendInfoMessage("/npc clear <NPC名>, 清除指定NPC");
                    args.Player.SendInfoMessage("/npc clear, 清理敌怪但保留友善NPC");
                    break;

                default:
                    var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[1]);
                    if (npcs.Count == 0)
                        args.Player.SendErrorMessage("找不到对应的 NPC");
                    else if (npcs.Count > 1)
                        args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
                    else
                        args.Player.SendSuccessMessage("清理了 {0} 个 {1}", ClearNPCByID(npcs[0].netID), npcs[0].FullName);
                    break;
            }
        }
        #endregion

        #region NPC去重
        /// <summary>
        // NPC去重
        /// <summary>
        private static void UniqueNPC(CommandArgs args)
        {
            // List<int> ids = new List<int>() {22,33};
            List<int> founds = new List<int>();
            int cleared = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active)
                    continue;

                if (!Main.npc[i].townNPC)
                    continue;

                int num = Main.npc[i].type;
                if (founds.Contains(num))
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    cleared++;
                }
                else
                {
                    founds.Add(num);
                }
            }

            if (cleared > 0)
                args.Player.SendSuccessMessage($"已清除 {cleared} 个重复的NPC");
            else
                args.Player.SendInfoMessage("没有可清除的 重复的NPC");
        }
        #endregion

        #region 复活NPC
        /// <summary>
        // 复活NPC
        /// <summary>
        public static void Relive(CommandArgs args)
        {
            List<int> found = new List<int>();
            TSPlayer op = args.Player;

            // 解救状态
            if (NPC.savedAngler) found.Add(369);
            if (NPC.savedGoblin) found.Add(107);
            if (NPC.savedMech) found.Add(124);
            if (NPC.savedStylist) found.Add(353);
            if (NPC.savedBartender) found.Add(550);
            if (NPC.savedGolfer) found.Add(588);
            if (NPC.savedWizard) found.Add(108);
            if (NPC.savedTaxCollector) found.Add(441);  // 税收管
            if (NPC.boughtCat) found.Add(637);// 猫
            if (NPC.boughtDog) found.Add(638);
            if (NPC.boughtBunny) found.Add(656);

            // 怪物图鉴解锁情况
            List<int> remains = new List<int>() {
                22, //向导
                19, //军火商
                54, //服装商
                38, //爆破专家
                20, //树妖
                207, //染料商
                17, //商人
                18, //护士
                227, //油漆工
                208, //派对女孩
                228, //巫医
                633, //动物学家
                209, //机器侠
                229, //海盗
                178, //蒸汽朋克人
                160, //松露人
                663 //公主
                // 453, //骷髅商人
                // 368, //旅商
                // 37, // 老人
            };
            // 142, //圣诞老人
            if (Main.xMas) remains.Add(142);

            foreach (int npcID1 in remains)
            {
                if (DidDiscoverBestiaryEntry(npcID1)) found.Add(npcID1);
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || !Main.npc[i].townNPC || !found.Contains(Main.npc[i].type)) continue;
                found.Remove(Main.npc[i].type);
            }

            // 生成npc
            List<string> names = new List<string>();
            foreach (int npcID in found)
            {
                NPC npc = new NPC();
                npc.SetDefaults(npcID);
                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, op.TileX, op.TileY, 5, 2);

                if (names.Count != 0 && names.Count % 10 == 0)
                    names.Add("\n" + npc.FullName);
                else
                    names.Add(npc.FullName);
            }

            // 找家
            // for (int i = 0; i < Main.maxNPCs; i++)
            // {
            //     if( !Main.npc[i].active || !Main.npc[i].townNPC )
            //         continue;

            //     if( found.Contains(Main.npc[i].type) )
            //         WorldGen.QuickFindHome(i);
            // }

            if (found.Count > 0)
            {
                string text = $"{op.Name} 复活了 {found.Count}个 NPC:\n{string.Join("、", names)}";
                TSPlayer.All.SendInfoMessage(text);
                if (!op.RealPlayer)
                    op.SendInfoMessage(text);
            }
            else
            {
                op.SendInfoMessage("入住过的NPC都活着");
            }
        }
        public static bool DidDiscoverBestiaryEntry(int npcId)
        {
            return Main.BestiaryDB.FindEntryByNPCID(npcId).UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0;
        }
        #endregion

        #region 将NPC传送到你身边
        /// <summary>
        /// 将NPC传送到你身边
        /// </summary>
        /// <param name="args"></param>
        public static void TPHereNPC(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 1)
            {
                op.SendInfoMessage("语法不正确，输入 /npc tphere <NPC名>, 将指定NPC传到你身边");
                return;
            }

            switch (args.Parameters[1].ToLowerInvariant())
            {
                case "help":
                    op.SendInfoMessage("语法不正确，输入 /npc tphere <NPC名>, 将指定NPC传到你身边");
                    break;

                default:
                    Vector2 newPos;
                    if (op.RealPlayer)
                        newPos = new Vector2(op.TPlayer.position.X, op.TPlayer.position.Y);
                    else
                        newPos = new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16);

                    if (args.Parameters[1].ToLowerInvariant() == "town")
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && npc.townNPC && npc.netID != 453 && npc.netID != 368 && npc.netID != 37)
                                npc.Teleport(newPos);
                        }

                        if (op.RealPlayer)
                            op.SendSuccessMessage("已将所有城镇NPC传到你身边");
                        else
                            op.SendSuccessMessage("已将所有城镇NPC传回出生点");
                    }
                    else
                    {
                        bool isNum = int.TryParse(args.Parameters[1], out int npcID);
                        if (_NPCTypes.ContainsKey(args.Parameters[1]))
                        {
                            npcID = _NPCTypes[args.Parameters[1]];
                            isNum = true;
                        }

                        int index = -1;
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            //Console.WriteLine($"FullName:{Main.npc[i].FullName} TypeName:{Main.npc[i].TypeName}");
                            if (!Main.npc[i].active)
                                continue;

                            if (Main.npc[i].TypeName.ToLowerInvariant() == args.Parameters[1])
                            {
                                index = i;
                                break;
                            }

                            if (isNum && Main.npc[i].netID == npcID)
                            {
                                index = i;
                                break;
                            }

                        }
                        if (index == -1)
                        {
                            op.SendErrorMessage("找不到对应的 NPC");
                        }
                        else
                        {

                            NPC npc = Main.npc[index];
                            npc.Teleport(newPos);

                            if (op.RealPlayer)
                                op.SendSuccessMessage("已将 {0} 传到你身边", npc.FullName);
                            else
                                op.SendSuccessMessage("已将 {0} 传回出生点", npc.FullName);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region 一些方法
        /// <summary>
        /// 清理指定id的NPC
        /// </summary>
        private static int ClearNPCByID(int npcID)
        {
            int cleared = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].netID == npcID)
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    cleared++;
                }
            }
            return cleared;
        }
        /// <summary>
        /// 通过id查找NPC
        /// </summary>
        private static int FindNPCByID(int npcID)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].netID == npcID)
                    return i;
            }
            return -1;
        }
        private static int CoundNPCByID(int npcID)
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].netID == npcID)
                    count++;
            }
            return count;
        }
        private static int CoundTownNPC()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].townNPC)
                    count++;
            }
            return count;
        }
        private static int CoundNPC()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                    count++;
            }
            return count;
        }
        private static List<string> GetNPCName()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].townNPC)
                    names.Add(Main.npc[i].FullName);
            }
            return names;
        }
        static Dictionary<string, int> _NPCTypes = new Dictionary<string, int>
        {
            { "商人", 17 },
            { "护士", 18 },
            { "军火商", 19 },
            { "树妖", 20 },
            { "向导", 22 },
            { "老人", 37 },
            { "爆破专家", 38 },
            { "服装商", 54 },
            { "受缚哥布林", 105 },
            { "受缚巫师", 106 },
            { "哥布林工匠", 107 },
            { "巫师", 108 },
            { "受缚机械师", 123 },
            { "机械师", 124 },
            { "圣诞老人", 142 },
            { "松露人", 160 },
            { "蒸汽朋克人", 178 },
            { "染料商", 207 },
            { "派对女孩", 208 },
            { "机器侠", 209 },
            { "油漆工", 227 },
            { "巫医", 228 },
            { "海盗", 229 },
            { "发型师", 353 },
            { "受缚发型师", 354 },
            { "旅商", 368 },
            { "渔夫", 369 },
            { "税收官", 441 },
            { "骷髅商人", 453 },
            { "酒馆老板", 550 },
            { "高尔夫球手", 588 },
            { "高尔夫球手待拯救", 589 },
            { "动物学家", 633 },
            { "公主", 663 },
        };
        public static void Clear()
        {
            _NPCTypes.Clear();
        }
        #endregion

    }
}