using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 查找工具
    /// </summary>
    class WMFindTool
    {
        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                var FT = Mapping.FlagTile;
                List<string> lines = new()
                {
                    "/wm f <id/名称>，查找指定图格（未能区分多样式图格）",
                    $"/wm f tomb，查找 {FT("墓碑")}（全类型）",
                    $"/wm f dress，查找 {FT("梳妆台")}（全类型）",
                    $"/wm f sword，查找 {FT("附魔剑")}",

                    $"/wm f lc，查找 {FT("生命水晶")}",
                    $"/wm f lf，查找 {FT("生命果")}",
                    $"/wm f lcb，查找 {FT("巨型生命水晶")}",
                    $"/wm f bb，查找 {FT("弹力巨石")}",

                    $"/wm f wb，查找 {FT("水矢")}",
                    $"/wm f gc，查找 {FT("明胶水晶")}",
                    $"/wm f orb，查找 {FT("暗影珠")}",
                    $"/wm f heart，查找 {FT("猩红之心")}",

                    $"/wm f demon，查找 {FT("恶魔祭坛")}",
                    $"/wm f crimson，查找 {FT("猩红祭坛")}",
                    $"/wm f la，查找 {FT("丛林蜥蜴祭坛")}",
                    $"/wm f hf，查找 {FT("地狱熔炉")}",

                    $"/wm f larva，查找 {FT("幼虫")}",
                    $"/wm f bulb，查找 {FT("花苞")}",
                    $"/wm f ex，查找 {FT("提炼机")}",
                    $"/wm f loom，查找 {FT("织布机")}",

                    $"/wm f dirtiest，查找 {FT("最脏的块")}",
                    $"/wm f tulip，查找 {FT("发光郁金香")}",
                    $"/wm f <森林晶塔/海洋晶塔……>，查找 {FT("晶塔")}",
                };

                Utils.Pagination(args, ref lines, "/wm find");
            }

            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            ResetSkip();
            string kw = Mapping.AliasSubCMD(args.Parameters[0].ToLowerInvariant());
            if (FindList.ContainsKey(kw))
            {
                ListedExtra(op, Mapping.FlagTile(kw), FindList[kw], true);
            }
            else
            {
                var tileProp = TileHelper.GetTileByIDOrName(args.Parameters[0].ToLowerInvariant());
                if (tileProp != null)
                {
                    ListedExtra(op, tileProp.Desc, new FindInfo(tileProp.id, 0), !tileProp.isFrame);
                }
                else
                {
                    op.SendErrorMessage("输入的图格id无效或名称不匹配！");
                }
            }
        }

        public static readonly Dictionary<string, FindInfo> FindList = new()
        {
            { "附魔剑", new FindInfo(187,5) },
            { "花苞", new FindInfo(238) },
            { "暗影珠", new FindInfo(31,0) },
            { "猩红之心", new FindInfo(31,1) },

            { "生命水晶", new FindInfo(12,-1, 2,2) },
            { "生命果", new FindInfo(236) },
            { "巨型生命水晶", new FindInfo(665) },
            { "弹力巨石", new FindInfo(664) },

            { "明胶水晶", new FindInfo(129) },
            { "幼虫", new FindInfo(231) },
            { "丛林蜥蜴祭坛", new FindInfo(237) },
            { "地狱熔炉", new FindInfo(77) },

            { "提炼机", new FindInfo(219) },
            { "织布机", new FindInfo(86) },
            { "恶魔祭坛", new FindInfo(26,0) },
            { "猩红祭坛", new FindInfo(26,1) },

            { "墓碑", new FindInfo(85) },
            { "梳妆台", new FindInfo(88) },
            { "最脏的块", new FindInfo(668) },
            { "发光郁金香", new FindInfo(656) },
            { "水矢", new FindInfo(50,5,1,1,90,0) },

            {"森林晶塔", new FindInfo(597,1, 3,4, 0, 0) },
            {"丛林晶塔", new FindInfo(597,2, 3,4, 54,0)},
            {"神圣晶塔", new FindInfo(597,3, 3,4, 108,0)},
            {"洞穴晶塔", new FindInfo(597,4, 3,4, 162,0)},
            {"海洋晶塔", new FindInfo(597,5, 3,4, 216,0)},
            {"沙漠晶塔", new FindInfo(597,6, 3,4, 270,0)},
            {"雪原晶塔", new FindInfo(597,7, 3,4, 324,0)},
            {"蘑菇晶塔", new FindInfo(597,8, 3,4, 378,0)},
            {"万能晶塔", new FindInfo(597,9, 3,4, 432,0)},
        };

        /// <summary>
        /// 列出图格
        /// </summary>
        /// <param name="op"></param>
        /// <param name="opName"></param>
        /// <param name="fd"></param>
        /// <param name="accurate">是否精确（除已知的图格外，暂时无法匹配多样式图格）</param>
        static void ListedExtra(TSPlayer op, string opName, FindInfo fd, bool accurate = true)
        {
            ResetSkip();
            List<Point16> found = new();

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    if (CheckSprite(x, y, fd))
                    {
                        found.Add(new Point16(x, y));
                    }
                }
            }
            if (found.Count == 0)
            {
                op.SendInfoMessage($"未找到{opName}！");
                return;
            }
            Point16 p = found[0];
            var points = found.GetRange(0, Math.Min(20, found.Count));
            var pstrings = Utils.WarpLines(Point2String(points));

            var HL = Utils.Highlight;
            var s1 = $"/tppos {p.X} {p.Y}";
            var s2 = "/wm gps <x> <y>";
            var s3 = accurate ? "" : "*该图格有多种样式，本次查找结果为多个样式的汇总\n";
            List<string> lines = new() {
                $"找到{found.Count}个{opName}:",
                $"前往首个坐标：{HL(s1)}, {Utils.GetLocationDesc(p.X, p.Y)}",
                $"前20个的坐标：{string.Join("\n", pstrings)}",
                $"{s3}*输入{HL(s2)}查询坐标对应的GPS信息"
            };

            op.SendInfoMessage(string.Join("\n", lines));
        }

        /// <summary>
        /// 坐标转字符串
        /// </summary>
        static List<string> Point2String(List<Point16> points)
        {
            List<string> result = new();
            foreach (var p in points)
            {
                result.Add(p.ToString());
            }
            return result;
        }

        /// <summary>
        /// 查找精灵/图格
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="fd"></param>
        /// <returns></returns>
        public static bool CheckSprite(int tileX, int tileY, FindInfo fd)
        {
            ITile tile = Main.tile[tileX, tileY];
            int frameX = tile.frameX;
            int frameY = tile.frameY;

            int id = fd.id;
            int w = fd.w;
            int h = fd.h;

            if (!tile.active() || tile.type != fd.id)
                return false;

            bool check()
            {
                bool pass = true;
                for (int rx = tileX; rx < tileX + w; rx++)
                {
                    for (int ry = tileY; ry < tileY + h; ry++)
                    {
                        if (ContainsSkip(rx, ry) || !Main.tile[rx, ry].active() || Main.tile[rx, ry].type != id)
                        {
                            pass = false;
                            break;
                        }
                    }
                }
                if (pass)
                {
                    skip.Add(new Rectangle(tileX, tileY, w, h));
                }
                return pass;
            }

            // 明胶水晶
            if (id == TileID.Crystals && frameX >= 324)
            {
                return true;
            }

            bool flagX = fd.frameX == -1 || frameX == fd.frameX;
            bool flagY = fd.frameY == -1 || frameY == fd.frameY;

            if (w == 1 && h == 1)
                return flagX && flagY && tile.active() && tile.type == id;
            else
                return flagX && flagY && check();
        }
        private static List<Rectangle> skip = new();
        public static void ResetSkip() { skip.Clear(); }
        private static bool ContainsSkip(int x, int y)
        {
            foreach (Rectangle r in skip) { if (r.Contains(x, y)) return true; }
            return false;
        }

    }

}