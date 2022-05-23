using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.NetModules;
using Terraria.Net;
using TShockAPI;
using TShockAPI.DB;

namespace WorldModify
{
    class ResearchHelper
    {
        public static string SaveFile;

        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0)
            {
                UnlockAll(op);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
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
                    op.SendInfoMessage("/wm re, 解锁 全物品研究");
                    op.SendInfoMessage("/wm re import, 导入 物品研究");
                    op.SendInfoMessage("/wm re reset, 清空 当前世界 的 物品研究");
                    op.SendInfoMessage("/wm re clear, 清空 历史世界 的 物品研究");
                    op.SendInfoMessage("/wm re backup，备份 物品研究 到 csv文件，解锁和清空前会自动备份");
                    break;
            }
        }

        // 解锁全部
        private static async void UnlockAll(TSPlayer op)
        {
            await Task.Run(() =>
            {
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
            });
        }

        private static void Backup()
        {
            // 将研究进度保存到csv文件
            StringBuilder str = new StringBuilder();
            foreach (var obj in TShock.ResearchDatastore.GetSacrificedItems())
            {
                str.Append($"{obj.Key},{obj.Value}\n");
            }
            utils.SaveAndBack(SaveFile, str.ToString());
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
                        ? (IQueryBuilder)new SqliteQueryCreator()
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

        public static void Clear() { }
    }
}