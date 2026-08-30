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
using TShockAPI.DB.Queries;

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
                            args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.type})"));
                        }
                        else
                        {
                            UnlockOne(items[0].type, op);
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
            int total = TShock.ResearchDatastore.SacrificeItem(id, needNum, op);
            // 1.4.5.7+ 权威同步模块，客户端直接 SetSacrificeCountDirectly（所有玩家都能应用，不要求队友关系）
            NetManager.Instance.SendToClient(MakeUnlockPacket(id, total), op.Index);
            op.SendErrorMessage($"{Lang.GetItemName(id)} 已研究。id:{id} 研究数:{needNum}");
        }

        /// <summary>
        /// 构造 NetCreativeUnlocksModule 研究同步包
        /// 1458 原版 SerializeItemSacrifice 声明容量为3却写入4字节(short+ushort)，SendData 时 ShrinkToFit 会抛
        /// IndexOutOfRangeException("Overwrite on supplied Length")；这里手动用正确尺寸(4)构造同载荷包。
        /// </summary>
        private static NetPacket MakeUnlockPacket(int itemId, int sacrificeCount)
        {
            NetPacket packet = new(NetManager.Instance.GetId<NetCreativeUnlocksModule>(), 4);
            packet.Writer.Write((short)itemId);
            packet.Writer.Write((ushort)sacrificeCount);
            return packet;
        }

        // 解锁全部
        private static async void UnlockAll(TSPlayer op)
        {
            int playerId = op.Index;
            await Task.Run(() =>
            {
                isTasking = true;
                try
                {
                    Backup();
                    op.SendInfoMessage("正在解锁，请稍等……");
                    Dictionary<int, int> dic = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId;

                    // 批量写库（一次性多值INSERT），失败自动回退逐条写入，均保持 TShock 内存缓存一致
                    List<(int key, int amount)> items = new(dic.Count);
                    foreach (KeyValuePair<int, int> item in dic)
                        items.Add((item.Key, item.Value));

                    List<(int id, int total)> unlocks = new(items.Count);
                    if (!BulkAddResearch(op.Account?.ID ?? -1, Main.worldID, items, unlocks))
                    {
                        foreach (var (key, amount) in items)
                            unlocks.Add((key, TShock.ResearchDatastore.SacrificeItem(key, amount, op)));
                    }

                    // 网络栈必须在主线程操作：合并为单次入队，一次性批量发包（与 TShock 登录恢复研究数据的方式一致）
                    Main.QueueMainThreadAction(() =>
                    {
                        foreach (var (id, total) in unlocks)
                            NetManager.Instance.SendToClient(MakeUnlockPacket(id, total), playerId);
                    });

                    op.SendSuccessMessage($"已解锁 {unlocks.Count} 个物品研究");
                }
                finally
                {
                    isTasking = false;
                }
            });
        }

        /// <summary>
        /// 批量写入研究数据：多值INSERT分块单语句入库（避免每物品一次数据库连接），
        /// 写库成功后原地同步 TShock 内存缓存（GetSacrificedItems 返回的是同一字典实例），
        /// 保证 备份/信息统计/再次解锁 读到一致数据。返回 false 表示失败，由调用方回退为逐条写入。
        /// </summary>
        /// <param name="accountId">操作者账号ID（-1 表示无账号，直接失败回退，与原行为一致）</param>
        private static bool BulkAddResearch(int accountId, int worldId,
            List<(int key, int amount)> items, List<(int id, int total)> unlocks)
        {
            if (accountId < 0 || items.Count == 0)
                return false;
            try
            {
                var cache = TShock.ResearchDatastore.GetSacrificedItems();

                // 先按缓存旧值计算每条的新累计（暂不改缓存，待写库成功后统一同步）
                var rows = new List<(int key, int amount, int total)>(items.Count);
                foreach (var (key, amount) in items)
                {
                    cache.TryGetValue(key, out int old);
                    rows.Add((key, amount, old + amount));
                    unlocks.Add((key, old + amount));
                }

                DateTime now = DateTime.Now;
                var args = new List<object>(800 * 5);
                var sb = new StringBuilder(800 * 5 * 8);
                const int chunk = 800; // 800行*5参数=4000，兼容 SQLite(32766)/MySQL(65535) 参数上限
                for (int i = 0; i < rows.Count; i += chunk)
                {
                    int n = Math.Min(chunk, rows.Count - i);
                    sb.Length = 0;
                    sb.Append("INSERT INTO Research (WorldId, PlayerId, ItemId, AmountSacrificed, TimeSacrificed) VALUES ");
                    args.Clear();
                    for (int r = 0; r < n; r++)
                    {
                        if (r > 0) sb.Append(',');
                        int p = args.Count;
                        sb.Append($"(@{p},@{p + 1},@{p + 2},@{p + 3},@{p + 4})");
                        args.Add(worldId); args.Add(accountId);
                        args.Add(rows[i + r].key); args.Add(rows[i + r].amount); args.Add(now);
                    }
                    TShock.DB.Query(sb.ToString(), args.ToArray());
                }

                // 写库全部成功后，原地同步 TShock 内存缓存（与逐条写入的最终结果一致）
                foreach (var (key, amount) in items)
                {
                    cache.TryGetValue(key, out int old);
                    cache[key] = old + amount;
                }
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"批量写入研究数据失败，回退为逐条写入：{ex}");
                unlocks.Clear(); // 防止回退时与已预填的条目重复
                return false;
            }
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
                isTasking = true;
                try
                {
                    op.SendInfoMessage("正在导入，请稍等……");

                    // 先解析 CSV
                    List<(int key, int amount)> items = new();
                    foreach (string s in File.ReadAllLines(SaveFile))
                    {
                        string[] arr = s.Split(',');
                        if (arr.Length < 2) continue;
                        if (int.TryParse(arr[0], out int key) && int.TryParse(arr[1], out int value))
                            items.Add((key, value));
                    }

                    // 批量写库（失败回退逐条），再一次性主线程发包
                    int playerId = op.Index;
                    List<(int id, int total)> unlocks = new(items.Count);
                    if (!BulkAddResearch(op.Account?.ID ?? -1, Main.worldID, items, unlocks))
                    {
                        foreach (var (key, amount) in items)
                            unlocks.Add((key, TShock.ResearchDatastore.SacrificeItem(key, amount, op)));
                    }
                    Main.QueueMainThreadAction(() =>
                    {
                        foreach (var (id, total) in unlocks)
                            NetManager.Instance.SendToClient(MakeUnlockPacket(id, total), playerId);
                    });

                    op.SendSuccessMessage($"已导入 {unlocks.Count} 个物品研究");
                }
                finally
                {
                    isTasking = false;
                }
            });
        }

        // 重置
        private static async void Reset(TSPlayer op, bool superReset = false)
        {
            await Task.Run(() =>
            {
                isTasking = true;
                try
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
                        ? new SqliteQueryBuilder()
                        : new MysqlQueryBuilder());
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
                }
                finally
                {
                    isTasking = false;
                }
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