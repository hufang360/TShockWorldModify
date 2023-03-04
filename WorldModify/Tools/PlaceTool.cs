using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify.Tools
{
    /// <summary>
    /// 放置工具
    /// </summary>
    class PlaceTool
    {
        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            var FT = Mapping.FlagTile;
            void Help()
            {
                List<string> lines = new()
                {
                    "/igen p <id/名称> [样式]，放置 指定图格（不一定成功）",
                    $"/igen p lc，放置 {FT("生命水晶")}",
                    $"/igen p lf，放置 {FT("生命果")}",
                    $"/igen p sword，放置 {FT("附魔剑")}",

                    $"/igen p orb，放置 {FT("暗影珠")}",
                    $"/igen p heart，放置 {FT("猩红之心")}",
                    $"/igen p demon，放置 {FT("恶魔祭坛")}",
                    $"/igen p crimson，放置 {FT("猩红祭坛")}",

                    $"/igen p altar，放置 {FT("祭坛")}（自动判断腐化类型）",
                    $"/igen p bulb，放置 {FT("花苞")}",
                    $"/igen p larva，放置 {FT("幼虫")}",
                    $"/igen p tulip，放置 {FT("发光郁金香")}",

                    "/igen p pot，放置 罐子",
                };
                Utils.Pagination(args, ref lines, "/igen place");
            }
            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            string kw = Mapping.AliasSubCMD(args.Parameters[0].ToLowerInvariant()); // 转换别名
            int x = op.TileX;
            int y = op.TileY + 2;
            bool flag;
            switch (kw)
            {
                case "生命水晶":
                    PreparePlace(x, y, 2, 2, true);
                    WorldGen.AddLifeCrystal(x, y);
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendSuccessMessage($"已放置1个{FT(kw)}");
                    break;

                case "暗影珠":
                    flag = WorldGen.crimson;
                    WorldGen.crimson = false;
                    WorldGen.AddShadowOrb(x, y);
                    WorldGen.crimson = flag;
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendSuccessMessage($"已放置1个{FT(kw)}");
                    break;

                case "猩红之心":
                    flag = WorldGen.crimson;
                    WorldGen.crimson = true;
                    WorldGen.AddShadowOrb(x, y);
                    WorldGen.crimson = flag;
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendSuccessMessage($"已放置1个{FT(kw)}");
                    break;

                case "恶魔祭坛":
                    PreparePlace(x, y, 3, 2, true, 25);
                    WorldGen.Place3x2(x, y, TileID.DemonAltar);
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendInfoMessage($"已放置1个{kw}");
                    break;

                case "猩红祭坛":
                    PreparePlace(x, y, 3, 2, true, 203);
                    WorldGen.Place3x2(x, y, TileID.DemonAltar, 1);
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendSuccessMessage($"已放置1个{kw}");
                    break;

                case "祭坛":
                    if (!WorldGen.crimson)
                    {
                        PreparePlace(x, y, 3, 2, true, 25);
                        WorldGen.Place3x2(x, y, TileID.DemonAltar);
                    }
                    else
                    {
                        PreparePlace(x, y, 3, 2, true, 203);
                        WorldGen.Place3x2(x, y, TileID.DemonAltar, 1);
                    }
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendSuccessMessage($"已放置1个{kw}");
                    break;

                case "附魔剑":
                    PreparePlace(x, y, 3, 2, true);
                    WorldGen.PlaceTile(x, y, TileID.LargePiles2, true, true, -1, 17);
                    NetMessage.SendTileSquare(-1, x, y, 3);
                    op.SendSuccessMessage($"已放置1把{FT(kw)}");
                    break;

                case "罐子":
                    if (!PlacePot(x, y))
                    {
                        op.SendErrorMessage("未能放置罐子");
                    }
                    else
                    {
                        op.SendInfoMessage("已尝试放置1个罐子");
                    }
                    break;

                case "生命果":
                    PlaceFruit(x, y);
                    op.SendSuccessMessage($"已放置1颗{FT(kw)}");
                    break;

                case "幼虫":
                    PreparePlace(x, y - 1, 3, 3, true, 225);
                    PlaceLarva(x, y);
                    op.SendSuccessMessage($"已放置1个{FT(kw)}");
                    break;

                case "花苞":
                    PlaceBulb(x, y);
                    op.SendSuccessMessage($"已放置1个{FT(kw)}");
                    break;

                case "发光郁金香":
                    PlaceTulip(x, y);
                    op.SendSuccessMessage($"已放置1朵{FT(kw)}");
                    break;

                default:
                    // 图格id / 图格名称
                    var tileProp = TileHelper.GetTileByIDOrName(kw);
                    if (tileProp != null)
                    {
                        int tileStyle = 0;
                        if (args.Parameters.Count > 1)
                        {
                            if (!int.TryParse(args.Parameters[1], out tileStyle))
                            {
                                op.SendErrorMessage("图格样式输入错误！");
                                return;
                            }
                        }

                        if (TileHelper.IsPylon(op, tileProp.id)) return;

                        //x = op.TileX;
                        //y = op.TileY + 2;
                        WorldGen.PlaceTile(x, y, tileProp.id, true, true, -1, tileStyle);
                        NetMessage.SendTileSquare(-1, x, y, 20);
                        //TileHelper.GenAfter();

                        var s = tileProp.isFrame ? "（该图格有多种样式）" : "";
                        op.SendSuccessMessage($"已尝试放置1个{tileProp.Desc}，样式{tileStyle}{s}。");
                    }
                    else
                    {
                        op.SendErrorMessage("输入的图格id无效或名称不匹配！");
                    }
                    break;
            }
        }

        /// <summary>
        /// 放置罐子
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static bool PlacePot(int x, int y)
        {
            int style = WorldGen.genRand.Next(0, 4);
            int id = 0;
            int wall = 0;
            if (y < Main.maxTilesY - 5)
            {
                id = Main.tile[x, y + 1].type;
                wall = Main.tile[x, y].wall;
            }
            if (id == 147 || id == 161 || id == 162)
            {
                style = WorldGen.genRand.Next(4, 7);
            }
            if (id == 60)
            {
                style = WorldGen.genRand.Next(7, 10);
            }
            if (Main.wallDungeon[Main.tile[x, y].wall])
            {
                style = WorldGen.genRand.Next(10, 13);
            }
            if (id == 41 || id == 43 || id == 44 || id == 481 || id == 482 || id == 483)
            {
                style = WorldGen.genRand.Next(10, 13);
            }
            if (id == 22 || id == 23 || id == 25)
            {
                style = WorldGen.genRand.Next(16, 19);
            }
            if (id == 199 || id == 203 || id == 204 || id == 200)
            {
                style = WorldGen.genRand.Next(22, 25);
            }
            if (id == 367)
            {
                style = WorldGen.genRand.Next(31, 34);
            }
            if (id == 226)
            {
                style = WorldGen.genRand.Next(28, 31);
            }
            if (wall == 187 || wall == 216)
            {
                style = WorldGen.genRand.Next(34, 37);
            }
            if (y > Main.UnderworldLayer)
            {
                style = WorldGen.genRand.Next(13, 16);
            }
            if (!WorldGen.oceanDepths(x, y) && !Main.tile[x, y].shimmer())
            {
                WorldGen.PlacePot(x, y, 28, style);
                NetMessage.SendTileSquare(-1, x, y, 3);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 放置幼虫
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void PlaceLarva(int x, int y)
        {
            for (int rx = x - 1; rx <= x + 1; rx++)
            {
                for (int ry = y - 2; ry <= y + 1; ry++)
                {
                    if (ry != y + 1)
                    {
                        Main.tile[rx, ry].active(active: false);
                    }
                    else
                    {
                        // 放置蜂巢块
                        //Main.tile[rx, ry].active(active: true);
                        //Main.tile[rx, ry].type = 225;
                        //Main.tile[rx, ry].slope(0);
                        //Main.tile[rx, ry].halfBrick(halfBrick: false);
                    }
                }
            }
            WorldGen.PlaceTile(x, y, 231, mute: true);
            NetMessage.SendTileSquare(-1, x, y, 5);
        }

        /// <summary>
        /// 放置花苞
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void PlaceBulb(int x, int y)
        {
            for (int rx = x; rx <= x + 1; rx++)
            {
                for (int ry = y - 2; ry <= y + 1; ry++)
                {
                    if (ry != y + 1)
                    {
                        Main.tile[rx, ry].active(active: false);
                    }
                    else
                    {
                        // 放置“丛林草”方块
                        Main.tile[rx, ry].active(active: true);
                        Main.tile[rx, ry].type = 60;
                        Main.tile[rx, ry].slope(0);
                        Main.tile[rx, ry].halfBrick(halfBrick: false);
                    }
                }
            }
            WorldGen.PlaceTile(x + 1, y, TileID.PlanteraBulb, mute: true);
            NetMessage.SendTileSquare(-1, x, y, 3);
        }

        /// <summary>
        /// 放置生命果
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void PlaceFruit(int x, int y)
        {
            for (int rx = x; rx <= x + 1; rx++)
            {
                for (int ry = y - 2; ry <= y + 1; ry++)
                {
                    if (ry != y + 1)
                    {
                        Main.tile[rx, ry].active(active: false);
                    }
                    else
                    {
                        // 放置“丛林草”方块
                        Main.tile[rx, ry].active(active: true);
                        Main.tile[rx, ry].type = 60;
                        Main.tile[rx, ry].slope(0);
                        Main.tile[rx, ry].halfBrick(halfBrick: false);
                    }
                }
            }
            WorldGen.PlaceTile(x + 1, y, TileID.LifeFruit, mute: true);
            NetMessage.SendTileSquare(-1, x, y, 3);
        }

        /// <summary>
        /// 放置发光郁金香
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void PlaceTulip(int x, int y)
        {
            for (int rx = x; rx <= x + 1; rx++)
            {
                for (int ry = y - 2; ry <= y + 1; ry++)
                {

                    ITile tile = Main.tile[rx, ry];
                    if (ry != y + 1)
                    {
                        tile.active(active: false);
                    }
                    else
                    {
                        // 发光郁金香只生成在指定图格上面
                        var type = tile.type;
                        if (type != 0 && type != 70 && type != 633 && type != 59 && type != 225 && !TileID.Sets.Conversion.Grass[type] && !TileID.Sets.Conversion.Stone[type] && !Main.tileMoss[type])
                        {
                            // 放置“草”方块
                            tile.active(active: true);
                            tile.type = 2;
                            tile.slope(0);
                            tile.halfBrick(halfBrick: false);
                        }

                    }
                }
            }

            WorldGen.PlaceTile(x, y, TileID.GlowTulip, true);
            NetMessage.SendTileSquare(-1, x, y, 3);
        }


        /// <summary>
        /// 准备放置
        /// </summary>
        /// <param name="makeLand">图格下方放置方块</param>
        /// <param name="landType">垫脚方块的图格id</param>
        static void PreparePlace(int x, int y, int w = 1, int h = 1, bool makeLand = false, ushort landType = 2)
        {
            y--;
            x--;
            for (int rx = x; rx < x + w; rx++)
            {
                ITile tile;
                for (int ry = y; ry < y + h; ry++)
                {
                    Main.tile[rx, ry].active(false);
                }

                //放置草块
                tile = Main.tile[rx, y + h];
                Utils.Log($"斜面：{tile.slope()}");
                if (makeLand)
                {
                    // 补空
                    if (!tile.active())
                    {
                        tile.active(true);
                        tile.type = landType;
                    }

                    // 玩家可以穿过的图格
                    // !Main.tileSolidTop[tile.type]
                    if (!Main.tileSolid[tile.type])
                    {
                        tile.ClearTile();
                        tile.active(true);
                        tile.type = landType;
                    }

                    // 未激活
                    if (tile.inActive())
                    {
                        tile.inActive(false);
                    }

                    // 半砖
                    if (!tile.halfBrick())
                    {
                        tile.halfBrick(false);
                    }

                    // 斜面
                    if (tile.slope() != 0)
                    {
                        tile.slope(0);
                    }
                }
            }
        }
    }
}