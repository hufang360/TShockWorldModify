using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 替换工具
    /// </summary>
    class ReplaceTool
    {
        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0)
            {
                op.SendInfoMessage($"需要提供替换方案，输入 /igen replace help 帮助！");
                return;
            }

            Rectangle selection = SelectionTool.GetSelection(op.Index);
            Type type = Type.NULL;

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    op.SendInfoMessage("/igen r ice，水 → 薄冰");
                    op.SendInfoMessage("/igen r melt，薄冰 → 水");
                    op.SendInfoMessage("/igen r lava，水 → 岩浆");
                    op.SendInfoMessage("/igen r purify，净化");
                    op.SendInfoMessage("/igen r corruption，腐化");
                    op.SendInfoMessage("/igen r crimson，猩红化");
                    op.SendInfoMessage("/igen r hallow，神圣化");
                    op.SendInfoMessage("/igen r mushroom，蘑菇化");
                    op.SendInfoMessage("/igen r conifg，按配置进行替换");
                    break;

                case "config":
                case "con":
                    if (ReGenHelper.NeedWaitTask(op)) return;
                    RetileTool.Init();
                    Action(op, Type.ReplaceConfig, selection);
                    break;


                // 冰河化
                case "ice": type = Type.ReplaceIce; break;

                // 冰融化
                case "melt":
                case "mel": type = Type.ReplaceHelt; break;


                // 岩浆化
                case "lava": type = Type.ReplaceLava; break;


                // 净化
                case "purify":
                case "pur": type = Type.ReplacePurify; break;

                // 腐化
                case "corruption":
                case "cor": type = Type.ReplaceCorruption; break;

                // 猩红化
                case "crimson":
                case "cri": type = Type.ReplaceCrimson; break;

                // 猩红化
                case "hallow":
                case "hal": type = Type.ReplaceHallow; break;

                // 蘑菇化
                case "mushroom":
                case "mus":
                case "truffle":
                case "tru": type = Type.ReplaceTruffle; break;
            }
            if (type != Type.NULL)
            {
                if (ReGenHelper.NeedWaitTask(op)) return;
                Action(op, type, selection);
            }
        }
        enum Type
        {
            NULL,
            ReplaceRandom, // 随机方块
            ReplaceConfig, // 替换方块

            ReplaceBlock,   // 图格替换
            ReplaceIce,    // 冰河化
            ReplaceHelt,   // 冰融化
            ReplaceLava,    // 冰河化
            ReplacePurify, // 净化
            ReplaceCorruption, // 腐化
            ReplaceCrimson,   // 猩红化
            ReplaceHallow,   // 神圣化
            ReplaceTruffle,  // 蘑菇化
        };

        private static async void Action(TSPlayer op, Type type, Rectangle rect)
        {
            if (rect.Width == 0 && rect.Height == 0) rect = utils.GetScreen(op);
            bool needAll = rect.Width == Main.maxTilesX && rect.Height == Main.maxTilesY;


            int secondLast = utils.GetUnixTimestamp;
            string GetOpString()
            {
                switch (type)
                {
                    case Type.ReplaceConfig: return "替换";
                    case Type.ReplaceIce: return "冰河化";
                    case Type.ReplaceHelt: return "冰融化";
                    case Type.ReplaceLava: return "岩浆化";

                    case Type.ReplacePurify: return "净化";
                    case Type.ReplaceCorruption: return "腐化";
                    case Type.ReplaceCrimson: return "猩红化";
                    case Type.ReplaceHallow: return "神圣化";
                    case Type.ReplaceTruffle: return "蘑菇化";
                    default: return "图格修改";
                }
            }
            string opString = GetOpString();


            await Task.Run(() =>
            {
                if (needAll) op.SendSuccessMessage($"{opString} 全图开始");
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        switch (type)
                        {

                            case Type.ReplaceConfig: Replace(x, y); break;
                            case Type.ReplaceIce: IceAgeTile(x, y); break;
                            case Type.ReplaceHelt: IceMeltTile(x, y); break;
                            case Type.ReplaceLava: LavaLiquid(x, y); break;

                            case Type.ReplacePurify: WorldGen.Convert(x, y, 0); break;
                            case Type.ReplaceCorruption: WorldGen.Convert(x, y, 1); break;
                            case Type.ReplaceCrimson: WorldGen.Convert(x, y, 4); break;
                            case Type.ReplaceHallow: WorldGen.Convert(x, y, 2); break;
                            case Type.ReplaceTruffle: WorldGen.Convert(x, y, 3); break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                if (needAll)
                {
                    ReGenHelper.FinishGen(true);
                    op.SendSuccessMessage($"{opString} 全图结束（用时 {utils.GetUnixTimestamp - secondLast}s）");
                }
                else
                {
                    ReGenHelper.FinishGen();
                    op.SendSuccessMessage($"{opString} 结束");
                }
            });
        }

        #region 图格替换
        public static void Replace(int x, int y, List<TileReInfo> replaceInfo = null)
        {
            ITile tile = Main.tile[x, y];
            IEnumerable<TileReInfo> query;

            if (replaceInfo == null) replaceInfo = RetileTool.Con.replace;

            // 方块
            if (tile.active())
            {
                query = replaceInfo.Where(info => info.before.type == 0);
                query = query.Where(info => info.before.id == tile.type);
                foreach (TileReInfo info2 in query)
                {
                    RelaceBlock(x, y, info2);
                }
            }

            // 墙
            query = replaceInfo.Where(info => info.before.type == 1);
            query = query.Where(info => info.before.id == tile.wall);
            foreach (TileReInfo info in query)
            {
                if (info.after.type == 1) tile.wall = (ushort)info.after.id;
            }

            // 液体
            if (tile.liquid > 0)
            {
                int liquidType = tile.liquidType() + 1;
                query = replaceInfo.Where(res => res.before.type == 2);
                query = query.Where(info => info.before.id == liquidType);
                foreach (TileReInfo info in query)
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
                    }
                }
            }

        }
        // 替换方块
        private static void RelaceBlock(int x, int y, TileReInfo info2)
        {
            ITile tile = Main.tile[x, y];
            int atype = info2.after.type;
            int aid = info2.after.id;
            int astyle = info2.after.style;

            if (atype == 0)
            {
                if (aid == -1) // 清空图格
                    tile.ClearTile();

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
            }
        }
        #endregion




        #region 冰河化 / 冰融化
        public static void IceAgeTile(int x, int y)
        {
            // 薄冰 
            ITile tile = Main.tile[x, y];

            if (tile.liquid > 0)
            {
                switch (tile.liquidType())
                {
                    // 水
                    // Tile.Liquid_Water
                    case 0:
                        if (!tile.active())
                        {
                            tile.type = TileID.BreakableIce;
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                        }
                        tile.liquid = 0;
                        break;


                    // 岩浆
                    case 1: break;
                    // 蜂蜜
                    case 2: break;
                }
            }
        }
        public static void LavaLiquid(int x, int y)
        {
            // 薄冰 
            ITile tile = Main.tile[x, y];

            if (tile.liquid > 0)
            {
                switch (tile.liquidType())
                {
                    // 水
                    // Tile.Liquid_Water
                    case 0:
                        if (!tile.active())
                        {
                            tile.lava(lava: true);
                            tile.liquid = byte.MaxValue;
                            WorldGen.SquareTileFrame(x, y);
                            NetMessage.SendTileSquare(-1, x, y);
                        }
                        break;

                    // 岩浆
                    case 1: break;

                    // 蜂蜜
                    case 2: break;
                }
            }
        }




        public static void IceMeltTile(int tileX, int tileY)
        {
            ITile tile = Main.tile[tileX, tileY];
            if (tile.type != TileID.BreakableIce) return;

            tile.active(active: false);
            tile.liquid = byte.MaxValue;
            WorldGen.SquareTileFrame(tileX, tileY);
        }
        #endregion

    }
}