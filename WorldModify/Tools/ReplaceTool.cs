using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using WorldModify.Tools;

namespace WorldModify;

/// <summary>
/// 替换工具
/// </summary>
class ReplaceTool
{
    enum OPType
    {
        None,

        Block, // 替换
        Wall, // 替换墙
        Liquid, // 替换液体
        Random, // 随机方块
        Config, // 按配置文件进行替换

        Ice, Helt, Truffle, Desert,
        Purify, Corruption, Crimson, Hallow,
    };

    static List<string> OPNames = new() {
        "None",

        "替换方块",
        "替换墙",
        "替换液体",
        "随机图格",
        "按配置替换",

        "冰河化", "冰融化", "蘑菇化", "沙漠化",
        "净化", "腐化", "猩红化", "神圣化",
    };


    public static void Manage(CommandArgs args)
    {
        args.Parameters.RemoveAt(0);
        TSPlayer op = args.Player;
        void Help()
        {
            List<string> lines = new()
            {
                "/igen r <id/名称> <id/名称>，替换方块",
                "/igen r <液体名称> <液体名称>，替换液体（水/water, 岩浆/lava, 蜂蜜/honey, 微光/shimmer）",
                "/igen r wall <id/名称> <id/名称>，替换墙体",
                "/igen r purify，净化",

                "/igen r corruption，腐化",
                "/igen r crimson，猩红化",
                "/igen r hallow，神圣化",
                "/igen r mushroom，蘑菇化",

                "/igen r desert，沙漠化",
                "/igen r ice，水 → 薄冰",
                "/igen r melt，薄冰 → 水",
                "/igen r random，随机",

                "/igen r config，按配置替换",
            };
            Utils.Pagination(args, ref lines, "/igen replace");
        }
        if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
        {
            Help();
            return;
        }

        OPType type = OPType.None;
        string kw = args.Parameters[0].ToLowerInvariant();
        int beforeID = -1;
        int afterID = -1;
        string name = "";
        switch (kw)
        {
            default:
                // 替换方块
                if (args.Parameters.Count < 2)
                {
                    args.Player.SendInfoMessage("请输入要替换的方块id/名称");
                    Help();
                    return;
                }

                // 替换液体
                beforeID = LiquidMan.GetLiquidID(args.Parameters[0].ToLowerInvariant());
                if (beforeID != -1)
                {
                    afterID = LiquidMan.GetLiquidID(args.Parameters[1].ToLowerInvariant());
                    if (afterID == -1)
                    {
                        op.SendErrorMessage("输入的第二个液体名称无效！");
                        return;
                    }

                    type = OPType.Liquid;
                    name = $"将{LiquidMan.GetName((short)beforeID)}替换成{LiquidMan.GetName((short)afterID)}";
                }
                else
                {
                    var tileProp1 = TileHelper.GetTileByIDOrName(args.Parameters[0].ToLowerInvariant());
                    if (tileProp1 == null)
                    {
                        args.Player.SendInfoMessage("输入的第一个方块id无效/名称不匹配！");
                        return;
                    }

                    var tileProp2 = TileHelper.GetTileByIDOrName(args.Parameters[1].ToLowerInvariant());
                    if (tileProp2 == null)
                    {
                        args.Player.SendInfoMessage("输入的第二个方块id无效/名称不匹配！");
                        return;
                    }

                    type = OPType.Block;
                    beforeID = tileProp1.id;
                    afterID = tileProp2.id;
                    name = $"将{tileProp1.Desc}替换成{tileProp2.Desc}";
                }

                if (beforeID == afterID)
                {
                    op.SendInfoMessage("要替换的目标相同，什么也没做~");
                    return;
                }
                break;


            case "wall":
            case "w":
            case "墙":
                if (args.Parameters.Count < 3)
                {
                    op.SendErrorMessage("指令用法：/igen r wall <目标id> <替换后的id>");
                    return;
                }

                var wp1 = TileHelper.GetWallByIDOrName(args.Parameters[1]);
                if (wp1 == null)
                {
                    op.SendErrorMessage("输入的第一个id无效/名称不匹配！");
                    return;
                }

                var wp2 = TileHelper.GetWallByIDOrName(args.Parameters[2]);
                if (wp2 == null)
                {
                    op.SendErrorMessage("输入的第二个id无效/名称不匹配！");
                    return;
                }

                type = OPType.Wall;
                beforeID = wp1.id;
                afterID = wp2.id;
                name = $"将{wp1.Desc}替换成{wp2.Desc}";
                if (beforeID == afterID)
                {
                    op.SendInfoMessage("要替换的目标相同，什么也没做~");
                    return;
                }
                break;

            case "liquid":
            case "l":
                if (args.Parameters.Count < 3)
                {
                    op.SendErrorMessage("指令用法：/igen r liquid <液体名称> <液体名称>");
                    return;
                }

                beforeID = Tools.LiquidMan.GetLiquidID(args.Parameters[1].ToLowerInvariant());
                afterID = Tools.LiquidMan.GetLiquidID(args.Parameters[2].ToLowerInvariant());
                if (beforeID == -1)
                {
                    op.SendErrorMessage("输入的第一个液体名称无效！");
                    return;
                }

                if (afterID == -1)
                {
                    op.SendErrorMessage("输入的第二个液体名称无效！");
                    return;
                }

                type = OPType.Liquid;
                name = $"将{Tools.LiquidMan.GetName((short)beforeID)}替换成{Tools.LiquidMan.GetName((short)afterID)}";
                if (beforeID == afterID)
                {
                    op.SendInfoMessage("要替换的目标相同，什么也没做~");
                    return;
                }
                break;

            case "config":
            case "con":
                bool flag = RetileTool.FirstCreate();
                if (flag)
                {
                    op.SendErrorMessage($"{RetileTool.SaveFile} 已创建，按格式编辑后，再次执行本指令");
                    return;
                }
                else
                {
                    RetileTool.Init();
                    type = OPType.Config;
                }
                break;

            case "desert": type = OPType.Desert; break;   // 沙漠化
            case "ice": type = OPType.Ice; break; // 冰河化
            case "melt": case "mel": type = OPType.Helt; break;    // 冰融化
            case "purify": case "pur": type = OPType.Purify; break;   // 净化
            case "corruption": case "cor": type = OPType.Corruption; break;    // 腐化
            case "crimson": case "cri": type = OPType.Crimson; break; // 猩红化
            case "hallow": case "hal": type = OPType.Hallow; break;   // 神圣化
            case "mushroom": case "mus": type = OPType.Truffle; break; // 蘑菇化
            case "random": case "ran": type = OPType.Random; break; // 随机化
        }
        if (type != OPType.None)
        {
            if (TileHelper.IsPylon(op, beforeID)) return;
            if (TileHelper.IsPylon(op, afterID)) return;
            Action(op, type, beforeID, afterID, name);
        }
    }

    static async void Action(TSPlayer op, OPType type, int beforeID = -1, int afterID = -1, string name = "")
    {
        Rectangle rect = SelectionTool.GetSelection(op.Index);
        int secondLast = Utils.GetUnixTimestamp;
        int count = 0;
        List<ReTileInfo> replaceInfo = new();
        switch (type)
        {
            case OPType.Config: replaceInfo = RetileTool.Con.replace; break;
            case OPType.Desert: replaceInfo = ReTileTheme.GetDesert(); break;
        }

        await Task.Run(() =>
        {
            for (int x = rect.X; x < rect.Right; x++)
            {
                for (int y = rect.Y; y < rect.Bottom; y++)
                {
                    switch (type)
                    {
                        case OPType.Random:
                            count += RandomTool.RandomTile(x, y);
                            break;

                        case OPType.Config:
                        case OPType.Desert:
                            count += ReplaceTile(x, y, replaceInfo);
                            break;

                        case OPType.Block: count += ReplaceBlock(x, y, beforeID, afterID); break;
                        case OPType.Wall: count += ReplaceWall(x, y, beforeID, afterID); break;
                        case OPType.Liquid: count += LiquidMan.ReplaceLiquid(x, y, beforeID, afterID); break;

                        case OPType.Ice: count += LiquidMan.IceAgeTile(x, y, true); break;
                        case OPType.Helt: count += LiquidMan.IceMeltTile(x, y, true); break;

                        case OPType.Purify: WorldGen.Convert(x, y, 0); break;
                        case OPType.Corruption: WorldGen.Convert(x, y, 1); break;
                        case OPType.Crimson: WorldGen.Convert(x, y, 4); break;
                        case OPType.Hallow: WorldGen.Convert(x, y, 2); break;
                        case OPType.Truffle: WorldGen.Convert(x, y, 3); break;
                    }
                }
            }
        }).ContinueWith((d) =>
        {
            // 未避免替换过程中出现额外的转换，待替换完成后再统一更新
            if (type == OPType.Liquid)
            {
                TileHelper.InformPlayers();
            }

            TileHelper.FinishGen();
            int second = Utils.GetUnixTimestamp - secondLast;
            if (string.IsNullOrEmpty(name)) name = OPNames[(int)type];
            switch (type)
            {
                case OPType.Purify:
                case OPType.Corruption:
                case OPType.Crimson:
                case OPType.Hallow:
                case OPType.Truffle:
                    op.SendSuccessMessage($"{name}完成，用时{second}秒。");
                    break;

                default:
                    op.SendSuccessMessage($"{name}完成，共{count}格，用时{second}秒。");
                    break;
            }
        });
    }

    #region 替换图格
    /// <summary>
    /// 替换图格
    /// </summary>
    /// <returns>0表示未替换，1表示已替换</returns>
    static int ReplaceTile(int x, int y, List<ReTileInfo> replaceInfo)
    {
        ITile tile = Main.tile[x, y];
        IEnumerable<ReTileInfo> query;
        bool flag = false;

        // 方块
        if (tile.active())
        {
            query = replaceInfo.Where(info => info.before.type == 0 && info.before.id == tile.type);
            foreach (ReTileInfo info2 in query)
            {
                RelaceBlock(x, y, info2);
                flag = true;
            }
        }

        // 墙
        query = replaceInfo.Where(info => info.before.type == 1 && info.before.id == tile.wall);
        foreach (ReTileInfo info in query)
        {
            if (info.after.type == 1)
            {
                tile.wall = (ushort)info.after.id;
                NetMessage.SendTileSquare(-1, x, y);
                flag = true;
            }
        }

        // 液体
        if (tile.liquid > 0)
        {
            int liquidType = tile.liquidType() + 1;
            query = replaceInfo.Where(info => info.before.type == 2 && info.before.id == liquidType);
            foreach (ReTileInfo info in query)
            {
                // 液体替换成物块
                if (info.after.type == 0)
                {
                    if (tile.active())
                    {
                        // 有的宝箱会泡在水里，只把水清除即可
                        tile.liquid = 0;
                    }
                    else
                    {
                        tile.type = (ushort)info.after.id;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                        tile.liquid = 0;
                    }
                    NetMessage.SendTileSquare(-1, x, y);
                    flag = true;
                }
            }
        }

        if (flag)
        {
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// 替换方块
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="info2"></param>
    /// <returns>0表示未替换，1表示已替换</returns>
    private static int RelaceBlock(int x, int y, ReTileInfo info2)
    {
        ITile tile = Main.tile[x, y];
        int atype = info2.after.type;
        int aid = info2.after.id;
        int astyle = info2.after.style;

        if (atype == 0)
        {
            if (aid == -1) // 清空图格
            {
                tile.ClearTile();
            }

            //else if (aid == TileID.PalmTree)  // 棕榈树
            //{
            //    //WorldGen.GrowPalmTree(tileX, tileY);
            //    tile.type = (ushort)aid;
            //    tile.frameX = 66;
            //    tile.frameY = 0;
            //}
            //else if (aid == TileID.OasisPlants) // 绿洲植物
            //{
            //    // WorldGen.PlaceOasisPlant(tileX, tileY); // 无效
            //    int num = WorldGen.genRand.Next(9);
            //    int num2 = 0;
            //    short num3 = (short)(54 * num);
            //    short num4 = (short)(36 * num2);
            //    tile.type = (ushort)aid;
            //    tile.frameX = num3;
            //    tile.frameX = num4;
            //}
            //else if (aid >= 3 && astyle >= 0)  // 带style的图格
            //{
            //    WorldGen.PlaceTile(tileX, tileY, aid, false, true, 0, astyle);
            //    //tile.frameX += 18;
            //    //tile.frameX += 18;
            //}

            else // 直接替换
            {
                tile.type = (ushort)aid;
            }

            NetMessage.SendTileSquare(-1, x, y);
            return 1;
        }
        return 0;
    }


    /// <summary>
    /// 替换方块
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns>0表示未替换，1表示已替换</returns>
    static int ReplaceBlock(int x, int y, int before, int after)
    {
        ITile tile = Main.tile[x, y];
        if (tile.type == before)
        {
            tile.type = (ushort)after;
            NetMessage.SendTileSquare(-1, x, y);
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// 替换墙体
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns>0表示未替换，1表示已替换</returns>
    static int ReplaceWall(int x, int y, int before, int after)
    {
        ITile tile = Main.tile[x, y];
        if (tile.wall == before)
        {
            tile.wall = (ushort)after;
            NetMessage.SendTileSquare(-1, x, y);
            return 1;
        }
        return 0;
    }
    #endregion

}