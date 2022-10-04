using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 查找工具
    /// </summary>
    class FindTool
    {
        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0)
            {
                op.SendErrorMessage($"请输入要查找的名字，例如：附魔剑，可供查找的有：\n{string.Join(", ", FindList.Keys)}");
                return;
            }

            ResetSkip();
            string keyw = args.Parameters[0].ToLowerInvariant();
            if (FindList.ContainsKey(keyw))
            {
                ListedExtra(op, keyw, FindList[keyw]);
            }
            else
            {
                op.SendErrorMessage($"要查找的名字不对 或 不支持，可供查找的有：\n{string.Join(", ", FindList.Keys)}");
            }
        }

        private static readonly Dictionary<string, FindData> FindList = new Dictionary<string, FindData>()
        {
            {"附魔剑", new FindData(187,5, 3,2, 918) },
            {"花苞", new FindData(238,-1, 2,2) },

            {"暗影珠", new FindData(31,0, 2,2, 0) },
            {"猩红之心", new FindData(31,1, 2,2, 36) },

            {"生命水晶", new FindData(12,-1, 2,2) },
            {"生命果", new FindData(236,-1, 2,2) },

            {"幼虫", new FindData(231,-1, 3,3) },
            {"丛林蜥蜴祭坛", new FindData(237,-1, 3,2) },

            {"地狱熔炉", new FindData(77,-1, 3,2) },
            {"提炼机", new FindData(219,-1, 3,3) },
            {"织布机", new FindData(86,-1, 3,2) },

            {"恶魔祭坛", new FindData(26,0, 3,2) },
            {"猩红祭坛", new FindData(26,1, 3,2) },
            {"墓碑", new FindData(85,-1, 2,2) },
            {"梳妆台", new FindData(88,-1, 3,2) },
            {"最脏的块", new FindData(668,-1, 1,1) }
        };

        private static void ListedExtra(TSPlayer op, string opName, FindData fd)
        {
            ResetSkip();
            List<Point16> found = new List<Point16>();

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    ITile tile = Main.tile[x, y];
                    if (tile.active() && tile.type == fd.id)
                    {
                        if (GetItem(x, y, fd)) found.Add(new Point16(x, y));
                    }
                }
            }
            if (found.Count == 0)
            {
                op.SendInfoMessage($"未找到 {opName}！");
                return;
            }
            op.SendInfoMessage($"{opName} 查找结果（{found.Count}）:\n{string.Join(", ", found.GetRange(0, Math.Min(20, found.Count)))}\n输入 /tppos <x> <y> 进行传送");


        }
        public static bool GetItem(int tileX, int tileY, FindData fd)
        {
            ITile tile = Main.tile[tileX, tileY];
            int frameX = tile.frameX;
            int frameY = tile.frameY;

            bool check(int w, int h)
            {
                bool pass = true;
                for (int i = tileX; i < tileX + w; i++)
                {
                    for (int k = tileY; k < tileY + h; k++)
                    {
                        if (ContainsSkip(i, k) || !Main.tile[i, k].active() || Main.tile[i, k].type != fd.id)
                        {
                            pass = false;
                            break;
                        }
                    }
                }
                if (pass)
                {
                    skip.Add(new Rectangle(tileX, tileY, w, h));

                    //if (type == 187) utils.Log($"type={type}：{tileX},{tileY} frame：{frameX},{frameY}");
                }
                return pass;
            }

            bool flag = fd.frameX == -1 || (frameX == fd.frameX ? true : false);
            bool flag2 = fd.frameY == -1 || (frameY == fd.frameY ? true : false);
            if (flag && flag2 && check(fd.w, fd.h)) return true;
            return false;
        }
        private static List<Rectangle> skip = new List<Rectangle>();
        public static void ResetSkip() { skip.Clear(); }
        private static bool ContainsSkip(int x, int y)
        {
            foreach (Rectangle r in skip) { if (r.Contains(x, y)) return true; }
            return false;
        }
    }


    class FindData
    {
        public int id = 0;
        public int w = 1;
        public int h = 1;
        public int style = -1;
        public int frameX = -1;
        public int frameY = -1;

        public FindData(int _id, int _style, int _w, int _h, int _frameX = -1, int _frameY = -1)
        {
            id = _id;
            w = _w;
            h = _h;
            style = _style;
            frameX = _frameX;
            frameY = _frameY;
        }
    }
}