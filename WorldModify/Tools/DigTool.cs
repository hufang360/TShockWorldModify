namespace WorldModify
{
    /// <summary>
    /// 挖掘工具
    /// </summary>
    //class DigTool
    //{
    //    enum Type
    //    {
    //        None,
    //        All,
    //        Block,
    //        Match,
    //        Wall,

    //        Liquid,
    //        Water,
    //        Lava,
    //        Honey,
    //        Shimmer,

    //        Wire,
    //        Wire1,
    //        Wire2,
    //        Wire3,
    //        Wire4,
    //    };

    //    public async static void Manage(CommandArgs args)
    //    {
    //        args.Parameters.RemoveAt(0);
    //        TSPlayer op = args.Player;
    //        void Help()
    //        {
    //            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
    //                return;

    //            List<string> lines = new()
    //            {
    //                "/igen d <id>，清除 指定方块",
    //                "/igen d block，清除 所有方块",
    //                "/igen d all，清除 所有",
    //                "/igen d wall，清除 所有墙体",

    //                "/igen d liquid，清除 所有液体",
    //                "/igen d wire，清除 所有电线",

    //                "/igen d redwire，清除 红电线",
    //                "/igen d bluewire，清除 蓝电线",
    //                "/igen d greenwire，清除 绿电线",
    //                "/igen d yellowwire，清除 黄电线",
    //            };
    //            PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
    //            {
    //                HeaderFormat = "/igen clear 指令用法 ({0}/{1})：",
    //                FooterFormat = "输入 /igen c help {{0}} 查看更多".SFormat(Commands.Specifier)
    //            });

    //            //op.SendInfoMessage("/igen c wall [墙id]，清除 墙");
    //            //op.SendInfoMessage("/igen c wire [red/blue/]，清除 电线");
    //        }
    //        if (args.Parameters.Count == 0)
    //        {
    //            Help();
    //            return;
    //        }

    //        Type type = Type.None;
    //        int tileID = 0;
    //        switch (args.Parameters[0].ToLowerInvariant())
    //        {
    //            case "help": Help(); return;

    //            case "all": type = Type.All; break;
    //            case "block": type = Type.Block; break;
    //            case "wall": type = Type.Wall; break;

    //            case "wire": type = Type.Wire; break;
    //            case "redwire": case "rw": type = Type.Wire1; break;
    //            case "bluewire": case "bw": type = Type.Wire2; break;
    //            case "greenwire": case "gw": type = Type.Wire3; break;
    //            case "yellowwire": case "yw": type = Type.Wire4; break;

    //            case "liquid": type = Type.Liquid; break;
    //            case "water": type = Type.Water; break;
    //            case "lava": type = Type.Lava; break;
    //            case "honey": type = Type.Honey; break;
    //            case "shimmer": type = Type.Shimmer; break;

    //            default:
    //                if (int.TryParse(args.Parameters[0], out tileID))
    //                {
    //                    if (tileID >= 0 && tileID < TileID.Count)
    //                    {
    //                        type = Type.Match;
    //                    }
    //                    else
    //                    {
    //                        op.SendErrorMessage($"图格id，有效值为 0~{TileID.Count - 1}");
    //                    }
    //                }
    //                else
    //                {
    //                    Help();
    //                }
    //                break;
    //        }

    //        if (type != Type.None)
    //        {
    //            Rectangle rect = SelectionTool.GetSelection(op.Index);
    //            if (rect.Width * rect.Height > 122 * 68)
    //            {
    //                op.SendInfoMessage("此操作容易造成服务器卡顿，请将选区设置在一屏内！");
    //                return;
    //            }
    //            await Action(op, type, rect, new int[] { tileID });
    //        }
    //    }


    //    static Task Action(TSPlayer op, Type type, Rectangle rect, int[] plist)
    //    {
    //        int secondLast = Utils.GetUnixTimestamp;
    //        string GetOpString()
    //        {
    //            return type switch
    //            {
    //                Type.Match => "指定图格",
    //                Type.All => "所有",
    //                Type.Block => "所有方块",
    //                Type.Wall => "所有墙体",
    //                Type.Wire => "所有电线",
    //                Type.Liquid => "所有液体",

    //                Type.Wire1 => "红电线",
    //                Type.Wire2 => "蓝电线",
    //                Type.Wire3 => "绿电线",
    //                Type.Wire4 => "黄电线",

    //                Type.Water => "水",
    //                Type.Lava => "岩浆",
    //                Type.Honey => "蜂蜜",
    //                Type.Shimmer => "微光",
    //                _ => "图格",
    //            };
    //        }
    //        string opString = $"收集{GetOpString()}";

    //        return Task.Run(() =>
    //        {
    //            for (int x = rect.X; x < rect.Right; x++)
    //            {
    //                for (int y = rect.Y; y < rect.Bottom; y++)
    //                {
    //                    ITile tile = Main.tile[x, y];
    //                    switch (type)
    //                    {
    //                        //case Type.All: WorldGen.KillTile(x, y); break;

    //                        case Type.Block: WorldGen.KillTile(x, y); break;
    //                        case Type.Match: if (tile.type == plist[0]) WorldGen.KillTile(x, y); break;

    //                        case Type.Wall: WorldGen.KillWall(x, y); break;

    //                        // 电线
    //                        case Type.Wire:
    //                            WorldGen.KillWire(x, y);
    //                            WorldGen.KillWire2(x, y);
    //                            WorldGen.KillWire3(x, y);
    //                            WorldGen.KillWire4(x, y);
    //                            break;
    //                        case Type.Wire1: WorldGen.KillWire(x, y); break;
    //                        case Type.Wire2: WorldGen.KillWire2(x, y); break;
    //                        case Type.Wire3: WorldGen.KillWire3(x, y); break;
    //                        case Type.Wire4: WorldGen.KillWire4(x, y); break;

    //                        // 液体
    //                        case Type.Liquid:
    //                        case Type.Water:
    //                        case Type.Lava:
    //                        case Type.Honey:
    //                        case Type.Shimmer:
    //                            break;
    //                    }

    //                }
    //            }
    //        }).ContinueWith((d) =>
    //        {
    //            TileHelper.GenAfter();
    //            int second = Utils.GetUnixTimestamp - secondLast;
    //            op.SendSuccessMessage($"{opString} 结束（用时 {second}秒）");
    //        });
    //    }


    //    // 清除液体
    //    static void ClearLiquid(int x, int y, Type type)
    //    {
    //        ITile tile = Main.tile[x, y];
    //        if (tile.liquid > 0)
    //        {
    //            bool flag = false;
    //            switch (type)
    //            {
    //                case Type.Liquid: flag = true; break;
    //                case Type.Water: flag = tile.liquidType() == 0; break;
    //                case Type.Lava: flag = tile.liquidType() == 1; break;
    //                case Type.Honey: flag = tile.liquidType() == 2; break;
    //                case Type.Shimmer: flag = tile.liquidType() == 3; break;
    //            }
    //            if (flag)
    //            {
    //                tile.liquid = 0;
    //            }
    //        }
    //    }
    //}
}