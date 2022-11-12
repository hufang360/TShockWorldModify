using Microsoft.Xna.Framework;
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
                op.SendInfoMessage("/npc info, NPC信息");
                op.SendInfoMessage("/npc unique, 城镇NPC去重");
                op.SendInfoMessage("/npc clear help, 移除指定NPC");
                op.SendInfoMessage("/npc tphere help, 将NPC传送到你身边");
                op.SendInfoMessage("/npc relive, 复活NPC（根据怪物图鉴记录）");
                op.SendInfoMessage("/npc find <id/名称>, 查询指定NPC的信息");
                op.SendInfoMessage("/npc <id/名称>, 切换NPC解救状态");
                op.SendInfoMessage("/npc list, 查看支持切换解救状态的NPC");
                op.SendInfoMessage("/npc sm, sm召唤指令备注（SpawnMob NPC召唤指令）");
                op.SendInfoMessage("/npc mq, 召唤美杜莎boss");
                return;
            }

            string param = args.Parameters[0].ToLowerInvariant();
            switch (param)
            {
                default:
                    if (!ToggleNPC(op, param)) op.SendErrorMessage("语法不正确！");
                    break;

                case "sm":
                case "spawn":
                case "spawnmob":
                    List<string> newStrs = new();
                    int count = 0;
                    foreach (int id in NPCIDHelper.smIDs)
                    {
                        string tFlag = count != 0 && count % 5 == 0 ? "\n" : "";
                        newStrs.Add($"{tFlag}{NPCIDHelper.GetNameByID(id)}=/sm {id}");
                        count++;
                    }
                    op.SendInfoMessage($"以下是城镇NPC生成指令参考（共{NPCIDHelper.smIDs.Length}个）：");
                    op.SendInfoMessage(string.Join(", ", newStrs));
                    break;

                // 查看npc解救情况
                case "info": NPCInfo(args); break;

                // 将npc传到你身边
                case "tphere":
                case "th":
                    TPHereNPC(args);
                    break;

                // 查询NPC
                case "find": FindNPC(args); break;

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

            int npcTotal = 0;
            int townTotal = 0;
            List<int> npcIds = new();
            List<string> npcNames = new();
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

            List<string> li1 = new();
            List<string> li2 = new();
            List<string> li3 = new();
            List<string> li4 = new();
            List<string> li5 = new();

            int[] ids = new int[] {
                22, // 向导
                17, // 商人
                19, // 军火商
                18, // 护士
                369, // 渔夫
                227, // 油漆工
                124, // 机械师
                107, // 哥布林工匠
            };
            foreach (int id in ids)
            {
                li1.Add(Utils.CFlag(npcIds.Contains(id), NPCIDHelper.GetNameByID(id)));
            }

            ids = new int[] {
                228, // 巫医
                20, // 树妖
                207, // 染料商
                54, // 服装商
                353, // 发型师
                38, // 爆破专家
                633, // 动物学家
                208, // 派对女孩
                550, // 酒馆老板
                588, // 高尔夫球手
            };
            foreach (int id in ids)
            {
                li2.Add(Utils.CFlag(npcIds.Contains(id), NPCIDHelper.GetNameByID(id)));
            }


            ids = new int[] {
                108, // 巫师
                229, // 海盗
                209, // 机器侠
                441, // 税收官
                160, // 松露人
                142, // 圣诞老人
                178, // 蒸汽朋克人
                663, // 公主
            };
            foreach (int id in ids)
            {
                li3.Add(Utils.CFlag(npcIds.Contains(id), NPCIDHelper.GetNameByID(id)));
            }

            ids = new int[] {
                637, // 猫咪
                638, // 狗狗
                656, // 兔兔
                37, // 老人
                368, // 旅商
                453, // 骷髅商人
            };
            foreach (int id in ids)
            {
                li4.Add(Utils.CFlag(npcIds.Contains(id), NPCIDHelper.GetNameByID(id)));
            }

            ids = new int[] {
                670, // 呆瓜
                678, // 冷酷
                679, // 年长
                680, // 笨拙
                681, // 唱将
                682, // 粗暴
                683, // 神秘
                684, // 侍卫
            };
            foreach (int id in ids)
            {
                li5.Add(Utils.CFlag(npcIds.Contains(id), NPCIDHelper.GetNameByID(id)));
            }

            List<string> texts = new()
            {
                $"共{npcTotal}个NPC（含敌怪）, {townTotal}个城镇NPC。",
                $"肉前：{string.Join(", ", li1)}",
                $"肉前：{string.Join(", ", li2)}",
                $"肉后：{string.Join(", ", li3)}",
                $"其它：{string.Join(", ", li4)}",
                $"史莱姆：{string.Join(", ", li5)}",
            };


            li2.Clear();
            li3.Clear();

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
            li2.Clear();
            li3.Clear();

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

            // 城镇史莱姆
            li2.Clear();
            li3.Clear();

            li1 = NPC.unlockedSlimeBlueSpawn ? li2 : li3;
            li1.Add("呆瓜史莱姆");

            li1 = NPC.unlockedSlimeGreenSpawn ? li2 : li3;
            li1.Add("冷酷史莱姆");

            li1 = NPC.unlockedSlimeOldSpawn ? li2 : li3;
            li1.Add("年长史莱姆");

            li1 = NPC.unlockedSlimePurpleSpawn ? li2 : li3;
            li1.Add("笨拙史莱姆");

            li1 = NPC.unlockedSlimeRainbowSpawn ? li2 : li3;
            li1.Add("唱将史莱姆");

            li1 = NPC.unlockedSlimeRedSpawn ? li2 : li3;
            li1.Add("粗暴史莱姆");

            li1 = NPC.unlockedSlimeYellowSpawn ? li2 : li3;
            li1.Add("神秘史莱姆");

            li1 = NPC.unlockedSlimeCopperSpawn ? li2 : li3;
            li1.Add("侍卫史莱姆");

            if (li3.Count > 0)
                texts.Add($"待解锁：{string.Join(", ", li3)}");
            op.SendInfoMessage(string.Join("\n", texts));
        }

        // 查询NPC状态
        private static void FindNPC(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                op.SendInfoMessage("/npc find 指令用法：");
                op.SendInfoMessage("/npc find <id/名称>, 查询指定NPC的信息");
                return;
            }


            var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
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
                    NPC npc = Main.npc[index];

                    int x = (int)(npc.Center.X / 16);
                    int y = (int)(npc.Center.Y / 16);

                    op.SendSuccessMessage($"名称：{npcs[0].FullName}（id={npcs[0].netID}）" +
                        $"\n数量：{total}" +
                        $"\n坐标：{Utils.PointToLocationDesc(npc)}   {Utils.PointToLocationDesc(x, y)}" +
                        $"\n输入 /tppos {(int)npc.position.X / 16} {(int)npc.position.Y / 16} 进行传送");
                }
                else
                {
                    op.SendSuccessMessage($"未找到 {npcs[0].FullName}");
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
            string UFlag(bool _value, string _str)
            {
                return _value ? $"{_str} 已解锁" : $"{_str} 已标记为 未解锁";
            }

            string text = "";
            switch (param)
            {
                case "list":
                    int[] li = new[] { 369, 107, 124, 353, 550, 588, 108, 441, 637, 638, 656, 670, 678, 679, 680, 681, 682, 683, 684 };
                    List<string> texts = new();
                    int count = 0;
                    foreach (int id in li)
                    {
                        string tFlag = count != 0 && count % 5 == 0 ? "\n" : "";
                        texts.Add($"{tFlag}{NPCIDHelper.GetNameByID(id)}={id}");
                        count++;
                    }
                    op.SendInfoMessage("支持切换的NPC拯救/购买状态的有: ");
                    op.SendInfoMessage("{0}", string.Join(", ", texts));
                    break;


                // 渔夫
                case "渔夫":
                case "angler":
                case "369":
                case "沉睡渔夫":
                case "376":
                    NPC.savedAngler = !NPC.savedAngler;
                    text = NPC.savedAngler ? "沉睡渔夫 已解救" : "渔夫 已标记为 未解救";
                    break;


                // 哥布林工匠
                case "哥布林工匠":
                case "107":
                case "受缚哥布林":
                case "goblin":
                case "105":
                    NPC.savedGoblin = !NPC.savedGoblin;
                    text = NPC.savedGoblin ? "受缚哥布林 已解救" : "哥布林工匠 已标记为 未解救";
                    break;


                // 机械师
                case "机械师":
                case "123":
                case "受缚机械师":
                case "124":
                case "mech":
                case "mechanic":
                    NPC.savedMech = !NPC.savedMech;
                    text = NPC.savedMech ? "受缚机械师 已解救" : "机械师 已标记为 未解救";
                    break;


                // 发型师
                case "发型师":
                case "353":
                case "受缚发型师":
                case "354":
                case "stylist":
                    NPC.savedStylist = !NPC.savedStylist;
                    text = NPC.savedStylist ? "被网住的发型师 已解救" : "发型师 已标记为 未解救";
                    break;


                // 酒馆老板
                case "酒馆老板":
                case "550":
                case "昏迷男子":
                case "579":
                case "酒保":
                case "bartender":
                case "tavernkeep":
                case "tavern":
                    NPC.savedBartender = !NPC.savedBartender;
                    text = NPC.savedBartender ? "昏迷男子 已解救" : "酒馆老板 已标记为 未解救";
                    break;


                // 高尔夫球手
                case "高尔夫球手":
                case "588":
                case "589":
                case "高尔夫":
                case "golfer":
                    NPC.savedGolfer = !NPC.savedGolfer;
                    text = NPC.savedGolfer ? "高尔夫球手 已解救" : "高尔夫球手 已标记为 未解救";
                    break;


                // 巫师
                case "巫师":
                case "106":
                case "受缚巫师":
                case "108":
                case "wizard":
                    NPC.savedWizard = !NPC.savedWizard;
                    text = NPC.savedWizard ? "受缚巫师 已解救" : "巫师 已标记为 未解救";
                    break;


                // 税收官
                case "税收官":
                case "441":
                case "痛苦亡魂":
                case "534":
                case "tax":
                case "tax collector":
                    NPC.savedTaxCollector = !NPC.savedTaxCollector;
                    text = NPC.savedTaxCollector ? "痛苦亡魂 已净化" : "税收官 已标记为 未解救";
                    break;


                // 猫
                case "猫":
                case "637":
                case "cat":
                    NPC.boughtCat = !NPC.boughtCat;
                    text = NPC.boughtCat ? "猫咪许可证 已生效" : "猫咪许可证 已标记为 未使用";
                    break;


                // 狗
                case "狗":
                case "638":
                case "dog":
                    NPC.boughtDog = !NPC.boughtDog;
                    text = NPC.boughtDog ? "狗狗许可证 已生效" : "狗狗许可证 已标记为 未使用";
                    break;


                // 兔
                case "兔子":
                case "兔兔":
                case "656":
                case "兔":
                case "bunny":
                case "rabbit":
                    NPC.boughtBunny = !NPC.boughtBunny;
                    text = NPC.boughtBunny ? "兔兔许可证 已生效" : "兔兔许可证 已标记为 未使用";
                    break;

                // 1.4.4.x
                case "呆瓜史莱姆":
                case "670":
                case "呆瓜":
                    NPC.unlockedSlimeBlueSpawn = !NPC.unlockedSlimeBlueSpawn;
                    text = UFlag(NPC.unlockedSlimeBlueSpawn, "呆瓜史莱姆");
                    break;

                case "冷酷史莱姆":
                case "678":
                case "冷酷":
                    NPC.unlockedSlimeGreenSpawn = !NPC.unlockedSlimeGreenSpawn;
                    text = UFlag(NPC.unlockedSlimeGreenSpawn, "冷酷史莱姆");
                    break;

                case "年长史莱姆":
                case "679":
                case "年长":
                    NPC.unlockedSlimeOldSpawn = !NPC.unlockedSlimeOldSpawn;
                    text = UFlag(NPC.unlockedSlimeOldSpawn, "年长史莱姆");
                    break;

                case "笨拙史莱姆":
                case "680":
                case "笨拙":
                    NPC.unlockedSlimePurpleSpawn = !NPC.unlockedSlimePurpleSpawn;
                    text = UFlag(NPC.unlockedSlimePurpleSpawn, "笨拙史莱姆");
                    break;

                case "唱将史莱姆":
                case "681":
                case "唱将":
                    NPC.unlockedSlimeRainbowSpawn = !NPC.unlockedSlimeRainbowSpawn;
                    text = UFlag(NPC.unlockedSlimeRainbowSpawn, "唱将史莱姆");
                    break;

                case "粗暴史莱姆":
                case "682":
                case "粗暴":
                    NPC.unlockedSlimeRedSpawn = !NPC.unlockedSlimeRedSpawn;
                    text = UFlag(NPC.unlockedSlimeRedSpawn, "粗暴史莱姆");
                    break;

                case "神秘史莱姆":
                case "683":
                case "神秘":
                    NPC.unlockedSlimeYellowSpawn = !NPC.unlockedSlimeYellowSpawn;
                    text = UFlag(NPC.unlockedSlimeYellowSpawn, "神秘史莱姆");
                    break;

                case "侍卫史莱姆":
                case "684":
                case "侍卫":
                    NPC.unlockedSlimeCopperSpawn = !NPC.unlockedSlimeCopperSpawn;
                    text = UFlag(NPC.unlockedSlimeCopperSpawn, "侍卫史莱姆");
                    break;

                default:
                    // op.SendErrorMessage("语法不正确！，请使用 /npc toggle help, 进行查询");
                    return false;
            }

            if (!string.IsNullOrEmpty(text))
            {
                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                op.SendSuccessMessage(text);
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
            args.Parameters.RemoveAt(0);
            void Help()
            {
                args.Player.SendInfoMessage("/npc clear 指令用法：");
                args.Player.SendInfoMessage("/npc clear <id/名称>, 清除指定NPC");
                args.Player.SendInfoMessage("/npc clear enemy, 清除所有敌怪，保留友善NPC");
            }
            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            // 清除指定NPC/敌怪
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    Help();
                    break;

                case "enemy":
                    int cleared = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && !Main.npc[i].friendly)
                        {
                            Main.npc[i].active = false;
                            Main.npc[i].type = 0;
                            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                            cleared++;
                        }
                    }
                    args.Player.SendSuccessMessage($"已清除 {cleared}个 敌怪");
                    break;

                default:
                    var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
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
            List<int> founds = new();
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
            TSPlayer op = args.Player;
            List<int> found = GetRelive();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || !Main.npc[i].townNPC || !found.Contains(Main.npc[i].type)) continue;
                found.Remove(Main.npc[i].type);
            }

            // 生成npc
            List<string> names = new();
            foreach (int npcID in found)
            {
                NPC npc = new();
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
        public static List<int> GetRelive()
        {
            List<int> found = new();

            // 怪物图鉴解锁情况
            List<int> remains = new() {
                17, // 商人
                18, // 护士
                19, // 军火商
                20, // 树妖
                22, // 向导
                38, // 爆破专家
                54, // 服装商
                107, // 哥布林工匠
                108, // 巫师
                124, // 机械师
                160, // 松露人
                178, // 蒸汽朋克人
                207, // 染料商
                208, // 派对女孩
                209, // 机器侠
                227, // 油漆工
                228, // 巫医
                229, // 海盗
                353, // 发型师
                369, // 渔夫
                441, // 税收官
                550, // 酒馆老板
                588, // 高尔夫球手
                633, // 动物学家
                663, // 公主
                637, // 猫咪
                638, // 狗狗
                656, // 兔兔
                670, // 呆瓜史莱姆
                678, // 冷酷史莱姆
                679, // 年长史莱姆
                680, // 笨拙史莱姆
                681, // 唱将史莱姆
                682, // 粗暴史莱姆
                683, // 神秘史莱姆
                684, // 侍卫史莱姆
                // 453, //骷髅商人
                // 368, //旅商
                // 37, // 老人
            };
            // 142, //圣诞老人
            if (Main.xMas) remains.Add(142);

            foreach (int id in remains)
            {
                if (DidDiscoverBestiaryEntry(id)) found.Add(id);
            }
            return found;
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
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                op.SendInfoMessage("/npc tphere 指令用法：");
                op.SendInfoMessage("/npc th <id/名称>, 将指定NPC传到你身边");
                op.SendInfoMessage("/npc th town, 将所有城镇NPC传到你身边");
                return;
            }

            string kw = args.Parameters[0].ToLowerInvariant();
            switch (kw)
            {
                default:
                    Vector2 newPos;
                    if (op.RealPlayer)
                        newPos = new Vector2(op.TPlayer.position.X, op.TPlayer.position.Y);
                    else
                        newPos = new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16);

                    if (kw == "town")
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
                        bool isNum = false;
                        if (int.TryParse(kw, out int npcID))
                        {
                            isNum = true;
                        }
                        else
                        {
                            int id2 = NPCIDHelper.GetIDByName(kw);
                            if (id2 > 0)
                            {
                                npcID = id2;
                                isNum = true;
                            }
                        }

                        int index = -1;
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            //Console.WriteLine($"FullName:{Main.npc[i].FullName} TypeName:{Main.npc[i].TypeName}");
                            if (!Main.npc[i].active)
                                continue;

                            if (Main.npc[i].TypeName.ToLowerInvariant() == kw)
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
        //private static int CoundTownNPC()
        //{
        //    int count = 0;
        //    for (int i = 0; i < Main.maxNPCs; i++)
        //    {
        //        if (Main.npc[i].active && Main.npc[i].townNPC)
        //            count++;
        //    }
        //    return count;
        //}
        //private static int CoundNPC()
        //{
        //    int count = 0;
        //    for (int i = 0; i < Main.maxNPCs; i++)
        //    {
        //        if (Main.npc[i].active)
        //            count++;
        //    }
        //    return count;
        //}
        //private static List<string> GetNPCName()
        //{
        //    List<string> names = new List<string>();
        //    for (int i = 0; i < Main.maxNPCs; i++)
        //    {
        //        if (Main.npc[i].active && Main.npc[i].townNPC)
        //            names.Add(Main.npc[i].FullName);
        //    }
        //    return names;
        //}
        #endregion

    }
}