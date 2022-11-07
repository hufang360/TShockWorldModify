using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using TShockAPI;


namespace WorldModify
{
    class BestiaryHelper
    {
        public static string SaveFile;

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void HelpTxt()
            {
                op.SendInfoMessage("/wm bestiary 指令用法：");
                op.SendInfoMessage("/wm be unlock，解锁 全怪物图鉴");
                op.SendInfoMessage("/wm be <id/名称>，解锁 单条记录");
                op.SendInfoMessage("/wm be reset，重置 怪物图鉴");
                op.SendInfoMessage("/wm be import，导入 怪物图鉴");
                op.SendInfoMessage("/wm be backup，备份 怪物图鉴 到 csv文件，解锁和重置前会自动备份");
            }

            if (args.Parameters.Count == 0)
            {
                HelpTxt();
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "unlock":
                    Unlock(op);
                    break;

                case "reset":
                    Reset(op);
                    break;

                case "import":
                    Import(op);
                    break;

                case "backup":
                    Backup();
                    op.SendSuccessMessage($"备份完成（{SaveFile}）");
                    break;

                case "help":
                    HelpTxt();
                    break;

                default:
                    // 解锁单条
                    if (int.TryParse(args.Parameters[0], out int id))
                    {
                        if (id > 0 && id < NPCID.Count)
                        {
                            UnlockOne(id, op);
                        }
                        else
                        {
                            op.SendErrorMessage($"NPC ID 只能在 1~{ItemID.Count} 范围内");
                        }
                    }
                    else
                    {
                        List<NPC> npcs = TShock.Utils.GetNPCByName(args.Parameters[0]);
                        if (npcs.Count == 0)
                        {
                            args.Player.SendErrorMessage("无效的NPC名称!");
                        }
                        else if (npcs.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(npcs.Select(i => $"{i.FullName}({i.netID})"));
                        }
                        else
                        {
                            UnlockOne(npcs[0].netID, op);
                        }
                    }
                    break;
            }

        }

        // 条目的NPCID参考：
        // Terraria.GameContent.Bestiary\BestiaryDatabaseNPCsPopulator.cs
        // AddTownNPCs_Manual();
        static List<int> kills = new List<int>() { };
        static List<int> sights = new List<int>() { };
        static List<int> chats = new List<int>() { 369, 37, 22, 17, 20, 633, 368, 227, 38, 19, 207, 453, 229, 107, 18, 54, 550, 124, 637, 638, 228, 353, 441, 108, 160, 178, 656, 208, 588, 209, 663, 142, 678, 679, 680, 681, 682, 683, 684, 670 };

        private static async void Unlock(TSPlayer op)
        {
            await Task.Run(() =>
            {
                Backup();
                if (kills.Count == 0) ReadEnmeyAndCritter();
                foreach (int id in kills)
                {
                    UnlockKill(id);
                }

                foreach (int id in sights)
                {
                    UnlockSight(id);
                }

                foreach (int id in chats)
                {
                    UnlockChat(id);
                }

                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                        Main.BestiaryTracker.OnPlayerJoining(i);
                }

                BestiaryUnlockProgressReport result = Main.GetBestiaryProgressReport();
                op.SendSuccessMessage($"怪物图鉴 已全部解锁 ;-) {result.CompletionAmountTotal}/{result.EntriesTotal}");
            });
        }
        private static void UnlockOne(int id, TSPlayer op)
        {
            if (kills.Count == 0) ReadEnmeyAndCritter();
            bool flag = false;
            if (kills.Contains(id)) flag = UnlockKill(id);
            else if (sights.Contains(id)) flag = UnlockSight(id);
            else if (chats.Contains(id)) flag = UnlockChat(id);

            if (flag)
            {
                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                        Main.BestiaryTracker.OnPlayerJoining(i);
                }
                op.SendSuccessMessage($"已将 {TShock.Utils.GetNPCById(id).FullName} 加入怪物图鉴");
            }
            else
            {
                op.SendSuccessMessage($"此条目已经解锁过了");
            }
        }
        private static bool UnlockKill(int id)
        {
            string key = GetBestiaryCreditId(id);
            Dictionary<string, int> dic = Main.BestiaryTracker.Kills._killCountsByNpcId;
            if (dic.ContainsKey(key))
            {
                if (dic[key] < 50)
                {
                    dic[key] = 50;
                    return true;
                }
            }
            else
            {
                dic.Add(key, 50);
                return true;
            }
            return false;
        }
        private static bool UnlockSight(int id)
        {
            string key = GetBestiaryCreditId(id);
            if (!Main.BestiaryTracker.Sights._wasNearPlayer.Contains(key))
            {
                Main.BestiaryTracker.Sights._wasNearPlayer.Add(key);
                return true;
            }
            return false;
        }
        private static bool UnlockChat(int id)
        {
            string key = GetBestiaryCreditId(id);
            if (!Main.BestiaryTracker.Chats._chattedWithPlayer.Contains(key))
            {
                Main.BestiaryTracker.Chats._chattedWithPlayer.Add(key);
                return true;
            }
            return false;
        }


        private static async void Reset(TSPlayer op)
        {
            await Task.Run(() =>
            {
                Backup();
                Main.BestiaryTracker.Reset();
                TSPlayer.All.SendData(PacketTypes.WorldInfo);

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                        Main.BestiaryTracker.OnPlayerJoining(i);
                }
                op.SendSuccessMessage("怪物图鉴 已清空，重进游戏后生效");
            });
        }

        // 备份现有的记录
        private static void Backup()
        {
            StringBuilder str = new StringBuilder();
            int id;
            foreach (var obj in Main.BestiaryTracker.Kills._killCountsByNpcId)
            {
                // 1.4.4.x 已经没有这个名字
                if (obj.Key == "DeerclopsLeg") continue;
                id = GetNPCId(obj.Key);
                if (id == 0) continue;
                if (id == 195) // 迷失女孩
                {
                    if (!Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey("Nymph"))
                        str.Append($"{196},{obj.Value},{Lang.GetNPCName(id)}\n");
                }
                else if (id == 196) //宁芙
                {
                    if (!Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey("LostGirl"))
                        str.Append($"{195},{obj.Value},{Lang.GetNPCName(id)}\n");
                }
                str.Append($"{id},{obj.Value},{Lang.GetNPCName(id)}\n");
            }
            foreach (string s in Main.BestiaryTracker.Sights._wasNearPlayer)
            {
                id = GetNPCId(s);
                str.Append($"{GetNPCId(s)},{Lang.GetNPCName(id)}\n");
            }
            foreach (string s in Main.BestiaryTracker.Chats._chattedWithPlayer)
            {
                id = GetNPCId(s);
                str.Append($"{GetNPCId(s)},{Lang.GetNPCName(id)}\n");
            }
            Utils.SafeSave(SaveFile, str.ToString());
        }


        private static async void Import(TSPlayer op)
        {
            if (!File.Exists(SaveFile))
            {
                op.SendInfoMessage($"{SaveFile}文件不存在，无法导入，解锁/清空 全怪物图鉴 会自动生成该文件。");
                return;
            }
            await Task.Run(() =>
            {
                op.SendInfoMessage("正在导入，请稍等……");
                if (kills.Count == 0) ReadEnmeyAndCritter();

                int count = 0;
                string key = "";
                foreach (string s in File.ReadAllLines(SaveFile))
                {
                    string[] arr = s.Split(',');
                    if (!int.TryParse(arr[0], out int id)) continue;

                    int num = 0;
                    if (arr.Length > 1)
                    {
                        if (kills.Contains(id) && int.TryParse(arr[1], out num))
                        {
                            key = GetBestiaryCreditId(id);
                            if (Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey(key))
                                Main.BestiaryTracker.Kills._killCountsByNpcId[key] = num;
                            else
                                Main.BestiaryTracker.Kills._killCountsByNpcId.Add(key, num);
                            count++;
                            continue;
                        }
                    }

                    if (sights.Contains(id))
                    {
                        key = GetBestiaryCreditId(id);
                        if (!Main.BestiaryTracker.Sights._wasNearPlayer.Contains(key))
                            Main.BestiaryTracker.Sights._wasNearPlayer.Add(key);
                        count++;
                        continue;
                    }

                    if (chats.Contains(id))
                    {
                        key = GetBestiaryCreditId(id);
                        if (!Main.BestiaryTracker.Chats._chattedWithPlayer.Contains(key))
                            Main.BestiaryTracker.Chats._chattedWithPlayer.Add(key);
                        count++;
                        continue;
                    }
                }

                BestiaryUnlockProgressReport result = Main.GetBestiaryProgressReport();
                string percent = Terraria.Utils.PrettifyPercentDisplay(result.CompletionPercent, "P2");
                op.SendSuccessMessage($"已处理 {count} 条数据。怪物图鉴进度： {percent} {result.CompletionAmountTotal}/{result.EntriesTotal}");
            });
        }

        private static string GetBestiaryCreditId(int netID)
        {
            return ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[netID];
        }
        private static int GetNPCId(string key)
        {
            return ContentSamples.NpcNetIdsByPersistentIds[key];
        }


        // 出处：
        // Terraria.GameContent.Bestiary\BestiaryDatabaseNPCsPopulator.cs
        // AddEmptyEntries_CrittersAndEnemies_Automated();
        private static void ReadEnmeyAndCritter()
        {
            HashSet<int> exclusions = GetExclusions();
            foreach (KeyValuePair<int, NPC> item in ContentSamples.NpcsByNetId)
            {
                if (!exclusions.Contains(item.Key) && !item.Value.isLikeATownNPC)
                {
                    if (item.Value.CountsAsACritter)
                    {
                        sights.Add(item.Key);
                    }
                    else
                    {
                        kills.Add(item.Key);
                    }
                }
            }
        }
        private static HashSet<int> GetExclusions()
        {
            HashSet<int> hashSet = new HashSet<int>();
            List<int> list = new List<int>();
            foreach (KeyValuePair<int, NPCID.Sets.NPCBestiaryDrawModifiers> item in NPCID.Sets.NPCBestiaryDrawOffset)
            {
                if (item.Value.Hide)
                {
                    list.Add(item.Key);
                }
            }
            foreach (int item2 in list)
            {
                hashSet.Add(item2);
            }
            return hashSet;
        }

    }
}