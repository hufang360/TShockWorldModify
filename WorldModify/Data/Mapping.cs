namespace WorldModify
{
    /// <summary>
    /// 数据映射
    /// </summary>
    public class Mapping
    {
        /// <summary>
        /// 子指令别名映射
        /// 目前主要 /wm find、/wm clear、/igen place 指令会用到
        /// </summary>
        /// <returns>未找到就返回自己</returns>
        public static string AliasSubCMD(string name)
        {
            return name switch
            {
                "tomb" => "墓碑",
                "dress" => "梳妆台",
                "sword" => "附魔剑",
                "larva" => "幼虫",
                "bulb" or "世纪之花灯泡" or "世界之花球茎" => "花苞",
                "gelatin crystal" or "gc" => "明胶水晶",
                "life crystal" or "lc" => "生命水晶",
                "life fruit" or "lf" => "生命果",
                "orb" => "暗影珠",
                "heart" => "猩红之心",
                "demon" => "恶魔祭坛",
                "crimson" => "猩红祭坛",
                "altar" => "祭坛",
                "lihzahrd altar" or "la" => "丛林蜥蜴祭坛",
                "hell forge" or "hf" => "地狱熔炉",
                "extractinator" or "ex" => "提炼机",
                "loom" => "织布机",
                "dirtiest" => "最脏的块",
                "tulip" => "发光郁金香",
                "gnome" => "花园侏儒",
                "pot" => "罐子",
                "rolling" => "仙人球",
                "hive" => "马蜂窝",
                "spike" => "尖刺",
                "wooden spike" or "ws" => "尖刺",
                "lcb" => "巨型生命水晶",
                "bb" => "弹力巨石",
                "tnt" => "TNT枪管",
                "water bolt" or "wb" => "水矢",
                "egg" or "chilletegg" => "巨型龙蛋",

                "h" => "help",
                _ => name,
            };
        }

        /// <summary>
        /// 标签化图格（加上物品图标并显示图格id）
        /// </summary>
        /// <param name="name"></param>
        /// <returns>未找到就返回自己</returns>
        public static string FlagTile(string name)
        {
            // 补充物品图标（id）
            var id = GetItemID(name);

            // 补充图格id
            var tileID = GetTileID(name);
            if (tileID != -1)
                name = $"{name}({tileID})";

            name = Utils.Highlight(name);
            if (id != 0)
                return $"[i:{id}]{name}";
            else
                return name;
        }
        /// <summary>
        /// 标签化图格（加上物品图标并显示图格id）
        /// </summary>
        /// <returns>未找到就返回空</returns>
        public static string FlagTile2(int tileID, int style = -1)
        {
            string name = TileHelper.GetTileNameByID(tileID);

            // 匹配已知的多样式数据
            foreach (string k in WMFindTool.FindList.Keys)
            {
                FindInfo fd = WMFindTool.FindList[k];
                if (fd.id == tileID && fd.style == style)
                {
                    name = k;
                }
            }

            string flag;
            if (TileHelper.IsFrame(tileID))
            {
                if (style != -1)
                    flag = $",{style}";
                else
                    flag = ",多样式";
            }
            else
            {
                flag = "";
            }

            // 补充物品图标（id）
            var itemID = GetItemID(name);

            name = Utils.Highlight($"{name}({tileID}{flag})");
            if (itemID != 0)
                return $"[i:{itemID}]{name}";
            else
                return name;
        }

        /// <summary>
        /// 标签化墙（加上物品图标并显示图格id）
        /// </summary>
        /// <param name="name"></param>
        /// <returns>未找到就返回自己</returns>
        public static string FlagWall(string name)
        {
            // 补充物品图标（id）
            var id = GetItemID(name);

            // 补充墙id
            var wallID = GetWallID(name);
            if (wallID != -1)
                name = $"{name}({wallID})";

            name = Utils.Highlight(name);
            if (id != 0)
                return $"[i:{id}]{name}";
            else
                return name;
        }

        /// <summary>
        /// 标签化墙（加上物品图标并显示图格id）
        /// </summary>
        /// <param name="name"></param>
        /// <returns>未找到就返回自己</returns>
        public static string FlagWall2(int wallID)
        {
            var name = TileHelper.GetWallNameByID(wallID);

            // 补充物品图标（id）
            var id = GetItemID(name);
            name = Utils.Highlight($"{name}({wallID})");
            if (id != 0)
                return $"[i:{id}]{name}";
            else
                return name;
        }

        /// <summary>
        /// 获得物品id
        /// </summary>
        /// <param name="name"></param>
        /// <returns>未找到则返回0</returns>
        public static int GetItemID(string name)
        {
            int id = name switch
            {
                "墓石" => 1173, // 1176
                // "梳妆台" => 334,
                "幼虫" => 2108,
                "花苞" => 2109,

                // "仙人球" => 4390,
                "马蜂窝" => 5066,
                "tnt枪管" => 5327,
                //"巨型生命水晶" => 5384,
                //"弹力巨石" => 5383,
                //"木尖刺" => 1150,

                //"附魔剑" => 989,
                //"明胶水晶" => 4988,
                //"生命水晶" => 29,
                //"生命果" => 1291,
                //"暗影珠" => 115,
                //"猩红之心" => 3062,
                //"丛林蜥蜴祭坛" => 1292,
                //"地狱熔炉" => 221,
                //"提炼机" => 997,
                //"织布机" => 332,
                //"最脏的块" => 5400,
                //"发光郁金香" => 5333,
                //"花园侏儒" => 4609,

                "铜币堆" => 71,
                "银币堆" => 72,
                "金币堆" => 73,
                "铂金币堆" => 74,

                "云" => 751,
                "蓝玉" => 177,
                "红玉" => 178,
                "钴矿" => 364,
                "尖刺" => 147,

                "丛林藤蔓" => 210,
                "晶塔" => 4876,

                _ => 0,
            };

            // 查询中文物品名称
            if (id == 0)
            {
                ChineseLanguage.Initialize();
                id = ChineseLanguage.GetItemIDByName(name);
            }
            return id;
        }


        /// <summary>
        /// 获得墙id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static int GetWallID(string name)
        {
            //var id = name switch
            //{
            //    _ => 0,
            //};

            // 查询墙配置
            var id = 0;
            var p = TileHelper.GetWallByIDOrName(name);
            if (p != null)
                id = p.id;
            return id;
        }

        /// <summary>
        /// 在图格名后面添加上图格id
        /// </summary>
        /// <param name="name"></param>
        /// <returns>未找到则返回-1</returns>
        static int GetTileID(string name)
        {
            var id = name switch
            {
                //"生命水晶" => 12,
                //"生命果" => 236,
                //"花苞" => 238,
                //"幼虫" => 231,
                //"暗影珠" => 31,
                //"猩红之心" => 31,
                //"恶魔祭坛" => 26,
                //"猩红祭坛" => 26,
                "祭坛" => 26,
                //"附魔剑" => TileID.LargePiles2,
                //"花园侏儒" => 567,
                //"发光郁金香" => 656,
                //"罐子" => 653,
                //"水矢" => 50,
                //"墓碑" => 85,
                //"明胶水晶" => 129,
                "tnt枪管" => 654,
                "铁砧" or "铅砧" => 16,
                "秘银砧" or "山铜砧" => 134,
                "精金熔炉" or "钛金熔炉" => 133,

                "森林晶塔" or "丛林晶塔" or "神圣晶塔" or "洞穴晶塔" or "海洋晶塔" or "沙漠晶塔" or "雪原晶塔" or "蘑菇晶塔" or "万能晶塔" => 597,

                _ => -1,
            };

            if (id == -1 && WMFindTool.FindList.ContainsKey(name))
            {
                id = WMFindTool.FindList[name].id;
            }

            // 查询图格配置
            if (id == -1)
            {
                var p = TileHelper.GetTileByIDOrName(name);
                if (p != null)
                    id = p.id;
            }
            return id;
        }

        /// <summary>
        /// 将指定图格名转成大写
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string UpperTileName(string name)
        {
            return name switch
            {
                "tnt枪管" => "TNT枪管",
                _ => name,
            };
        }
    }
}
