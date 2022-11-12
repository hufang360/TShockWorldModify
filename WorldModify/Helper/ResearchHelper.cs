using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.Net;
using TShockAPI;
using TShockAPI.DB;

namespace WorldModify
{
    class ResearchHelper
    {
        public static string SaveFile;

        private static bool isTasking = false;

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void HelpTxt()
            {
                op.SendInfoMessage("/wm research 指令用法：");
                op.SendInfoMessage("/wm re unlock, 解锁 全物品研究");
                op.SendInfoMessage("/wm re <id/名称>, 研究单个物品");
                op.SendInfoMessage("/wm re import, 导入 物品研究");
                op.SendInfoMessage("/wm re reset, 重置 物品研究");
                op.SendInfoMessage("/wm re clear, 清空 物品研究（所有地图）");
                op.SendInfoMessage("/wm re backup，备份 物品研究 到 csv文件，解锁和清空前会自动备份");
            }
            if (args.Parameters.Count == 0)
            {
                HelpTxt();
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "unlock":
                    if (isTasking)
                    {
                        op.SendSuccessMessage("有任务正在运行，请稍后再试！");
                        return;
                    }
                    UnlockAll(op);
                    break;

                case "reset":
                    Reset(op);
                    break;

                case "clear":
                    Reset(op, true);
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
                        if (id > 0 && id < ItemID.Count)
                        {
                            UnlockOne(id, op);
                        }
                        else
                        {
                            op.SendErrorMessage($"物品id 只能在 1~{ItemID.Count} 范围内");
                        }
                    }
                    else
                    {
                        List<Item> items = TShock.Utils.GetItemByName(args.Parameters[0]);
                        if (items.Count == 0)
                        {
                            args.Player.SendErrorMessage("无效的物品名!");
                        }
                        else if (items.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.netID})"));
                        }
                        else
                        {
                            UnlockOne(items[0].netID, op);
                        }
                    }
                    break;
            }
        }

        private static void UnlockOne(int id, TSPlayer op)
        {
            if (!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(id))
            {
                op.SendErrorMessage($"id={id}的物品无法研究。");
                return;
            }
            int needNum = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[id];
            TShock.ResearchDatastore.SacrificeItem(id, needNum, op);
            var response = NetCreativeUnlocksModule.SerializeItemSacrifice(id, needNum);
            NetManager.Instance.Broadcast(response);
            op.SendErrorMessage($"{Lang.GetItemName(id)} 已研究。id:{id} 研究数:{needNum}");
        }

        // 解锁全部
        private static async void UnlockAll(TSPlayer op)
        {
            await Task.Run(() =>
            {
                isTasking = true;
                Backup();
                op.SendInfoMessage("正在解锁，请稍等……");
                Dictionary<int, int> dic = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId;
                foreach (KeyValuePair<int, int> item in dic)
                {
                    TShock.ResearchDatastore.SacrificeItem(item.Key, item.Value, op);
                    var response = NetCreativeUnlocksModule.SerializeItemSacrifice(item.Key, item.Value);
                    NetManager.Instance.Broadcast(response);
                }
                op.SendSuccessMessage($"已解锁 {dic.Count} 个物品研究");
                isTasking = false;
            });
        }

        private static void Backup()
        {
            // 将研究进度保存到csv文件
            StringBuilder str = new();
            foreach (var obj in TShock.ResearchDatastore.GetSacrificedItems())
            {
                str.Append($"{obj.Key},{obj.Value},{Lang.GetItemName(obj.Key)}\n");
            }
            Utils.SafeSave(SaveFile, str.ToString());
        }

        // 批量导入
        private static async void Import(TSPlayer op)
        {
            if (!File.Exists(SaveFile))
            {
                op.SendInfoMessage($"{SaveFile}文件不存在，请在此文件的每一行填写好“物品id,物品数量”，然后再导入。");
                return;
            }
            await Task.Run(() =>
            {
                op.SendInfoMessage("正在导入，请稍等……");
                int count = 0;
                foreach (string s in File.ReadAllLines(SaveFile))
                {
                    string[] arr = s.Split(',');
                    if (arr.Length < 2) continue;

                    if (int.TryParse(arr[0], out int key) && int.TryParse(arr[0], out int value))
                    {
                        TShock.ResearchDatastore.SacrificeItem(key, value, op);
                        var response = NetCreativeUnlocksModule.SerializeItemSacrifice(key, value);
                        NetManager.Instance.Broadcast(response);
                        count++;
                    }
                }
                op.SendSuccessMessage($"已导入 {count} 个物品研究");
            });
        }

        // 重置
        private static async void Reset(TSPlayer op, bool superReset = false)
        {
            await Task.Run(() =>
            {
                IDbConnection db = TShock.DB;
                IDbConnection database = db;

                var table = new SqlTable("Research",
                                        new SqlColumn("WorldId", MySqlDbType.Int32),
                                        new SqlColumn("PlayerId", MySqlDbType.Int32),
                                        new SqlColumn("ItemId", MySqlDbType.Int32),
                                        new SqlColumn("AmountSacrificed", MySqlDbType.Int32),
                                        new SqlColumn("TimeSacrificed", MySqlDbType.DateTime)
                    );
                var creator = new SqlTableCreator(db,
                    db.GetSqlType() == SqlType.Sqlite
                        ? new SqliteQueryCreator()
                        : new MysqlQueryCreator());
                try
                {
                    creator.EnsureTableStructure(table);
                }
                catch (DllNotFoundException)
                {
                    Console.WriteLine("Possible problem with your database - is Sqlite3.dll present?");
                    throw new Exception("Could not find a database library (probably Sqlite3.dll)");
                }

                var sql = superReset ? @"DELETE FROM Research WHERE NOT WorldId = @0" : @"DELETE FROM Research WHERE WorldId = @0";

                if (!superReset) Backup();

                try
                {
                    database.Query(sql, Main.worldID);
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                }
                if (superReset)
                    op.SendInfoMessage("历史世界 的 物品研究 已清空");
                else
                    op.SendInfoMessage("当前世界 的 物品研究 已清空，重开服后有效！");
            });
        }


        public static int GetSacrificeTotal()
        {
            return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.Count;
        }

        public static int GetSacrificeCompleted()
        {
            Dictionary<int, int> datas = TShock.ResearchDatastore.GetSacrificedItems();
            int count = 0;
            foreach (int key in datas.Keys)
            {
                int amount = datas[key];
                CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(key, out int amountNeeded);
                if (amount >= amountNeeded)
                {
                    count++;
                }
            }
            return count;
            // op.SendSuccessMessage("研究数据仅保存在服务器上，每张地图的研究数据是分开的");
        }
    }
}