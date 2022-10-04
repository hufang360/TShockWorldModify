using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 全图清理工具
    /// </summary>
    class ClearToolWM
    {
        enum Type
        {
            Tomb,
            Dress
        };

        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0)
            {
                op.SendInfoMessage("输入 /wm clear help 查看帮助");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    op.SendErrorMessage($"请输入要清理的名字，目前只支持：墓碑，梳妆台（全类型）");
                    return;

                case "tombstone":
                case "tomb":
                case "墓碑":
                    if (ReGenHelper.NeedWaitTask(op)) return;
                    Action(op, Type.Tomb);
                    break;


                case "dressers":
                case "dress":
                    if (ReGenHelper.NeedWaitTask(op)) return;
                    Action(op, Type.Dress);
                    break;
            }
        }

        /// <summary>
        /// 清理墓碑（外部调用）
        /// </summary>
        /// <param name="op"></param>
        public static void ClearTombstone(TSPlayer op)
        {
            if (ReGenHelper.NeedWaitTask(op)) return;
            Action(op, Type.Tomb);
        }

        private static async void Action(TSPlayer op, Type type)
        {
            string GetOpString()
            {
                switch (type)
                {
                    case Type.Tomb: return "墓碑";
                    case Type.Dress: return "梳妆台";
                    default: return "";
                };
            }

            FindTool.ResetSkip();
            int secondLast = utils.GetUnixTimestamp;
            string opString = GetOpString();
            Rectangle rect = utils.GetWorldArea();
            bool flag = false;
            int count = 0;
            await Task.Run(() =>
            {
                op.SendSuccessMessage($"全图 清除{opString} 开始");
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        switch (type)
                        {
                            case Type.Tomb: flag = ClearTomb(x, y); break;
                            case Type.Dress: flag = ClearDress(x, y); break;
                            default: flag = false; break;
                        }
                        if (flag) count++;
                    }
                }
            }).ContinueWith((d) =>
            {
                ReGenHelper.FinishGen(false);
                op.SendSuccessMessage($"全图共清理了{count}个{opString}（用时 {utils.GetUnixTimestamp - secondLast}秒）");
            });
        }

        private static bool ClearTomb(int x, int y)
        {
            int tileID = 85;
            ITile tile = Main.tile[x, y];
            if (!tile.active() || tile.type != tileID) return false;
            FindData fd = new FindData(tileID, -1, 2, 2);
            if (FindTool.GetItem(x, y, fd))
            {
                ReGenHelper.ClearTile(x, y, fd.w, fd.h);
                return true;
            }
            return false;
        }
        private static bool ClearDress(int x, int y)
        {
            int tileID = 88;
            ITile tile = Main.tile[x, y];
            if (!tile.active() || tile.type != tileID) return false;
            FindData fd = new FindData(tileID, -1, 3, 2);
            if (FindTool.GetItem(x, y, fd))
            {
                ReGenHelper.ClearTile(x, y, fd.w, fd.h);
                return true;
            }
            return false;
        }
    }
}