using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 全图清除工具
    /// </summary>
    class WMClearTool
    {
        enum OPType
        {
            None,

            Block, Sprite,

            Tomb, Dress, Larva, Bulb,

            RollingCactus, BeeHive, Spikes, WoodenSpikes,
            TNTBarrel, LifeCrystalBoulder, BouncyBoulder,
        };

        static readonly List<string> OPNames = new() {
            "None",

            "指定方块", "精灵",

            "墓碑", "梳妆台", "幼虫", "花苞",

            "仙人球", "马蜂窝", "尖刺", "木尖刺",
            "TNT枪管", "巨型生命水晶", "弹力巨石",
        };

        static string GetOPName(OPType t) { return OPNames[(int)t]; }
        static OPType GetOPType(string s) { return OPNames.Contains(s) ? (OPType)OPNames.IndexOf(s) : OPType.None; }

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            var FT = Mapping.FlagTile;
            void Help()
            {
                List<string> lines = new()
                {
                    "/wm c <id/名称>，全图清除 指定图格（未能区分多样式图格）",
                    $"/wm c tomb，全图清除 {FT("墓碑")}（全样式）",
                    $"/wm c dress，全图清除 {FT("梳妆台")}（全样式）",
                    $"/wm c larva，全图清除 {FT("幼虫")}",

                    $"/wm c bulb，全图清除 {FT("花苞")}",
                    $"/wm c rolling，全图清除 {FT("仙人球")}",
                    $"/wm c hive，全图清除 {FT("马蜂窝")}",
                    $"/wm c tnt，全图清除 {FT("TNT枪管")}",

                    $"/wm c lcb，全图清除 {FT("巨型生命水晶")}",
                    $"/wm c bb，全图清除 {FT("弹力巨石")}",
                    $"/wm c spike，全图清除 {FT("尖刺")}",
                    $"/wm c ws，全图清除 {FT("木尖刺")}",
                };
                Utils.Pagination(args, ref lines, "/wm clear");
            }

            if (args.Parameters.Count == 0 || args.Parameters[0].ToLowerInvariant() == "help")
            {
                Help();
                return;
            }

            var kw = Mapping.AliasSubCMD(args.Parameters[0].ToLowerInvariant());
            OPType type = GetOPType(kw);

            FindInfo fd;
            string name = "";
            switch (type)
            {
                case OPType.Tomb: fd = new FindInfo(TileID.Tombstones, 0, 2, 2); break;
                case OPType.Dress: fd = new FindInfo(TileID.Dressers, 0, 3, 2); break;
                case OPType.Larva: fd = new FindInfo(TileID.Larva, 0, 3, 3); break;
                case OPType.Bulb: fd = new FindInfo(TileID.PlanteraBulb, 0, 2, 2); break;
                case OPType.RollingCactus: fd = new FindInfo(TileID.RollingCactus, 0, 2, 2); break;
                case OPType.BeeHive: fd = new FindInfo(TileID.BeeHive, 0, 2, 2); break;
                case OPType.TNTBarrel: fd = new FindInfo(TileID.TNTBarrel, 0, 2, 2); break;
                case OPType.LifeCrystalBoulder: fd = new FindInfo(TileID.LifeCrystalBoulder, 0, 2, 2); break;
                case OPType.BouncyBoulder: fd = new FindInfo(TileID.BouncyBoulder, 0, 2, 2); break;

                case OPType.Spikes: fd = new FindInfo(TileID.Spikes); break;
                case OPType.WoodenSpikes: fd = new FindInfo(TileID.WoodenSpikes); break;

                default:
                    // 图格id / 图格名称
                    var tileProp = TileHelper.GetTileByIDOrName(args.Parameters[0].ToLowerInvariant());
                    if (tileProp != null)
                    {

                        if (WMFindTool.FindList.ContainsKey(tileProp.name))
                        {
                            type = OPType.Sprite;
                            fd = WMFindTool.FindList[tileProp.name];
                            name = FT(tileProp.name);
                        }
                        else
                        {
                            type = OPType.Block;
                            fd = new FindInfo(tileProp.id);
                            name = tileProp.Desc;
                        }
                        if (TileHelper.IsPylon(op, fd.id)) return;
                    }
                    else
                    {
                        op.SendErrorMessage("输入的图格id无效或名称不匹配！");
                        return;
                    }
                    break;
            }

            if (TileHelper.NeedWaitTask(op)) return;
            Action(op, type, fd, name);
        }

        /// <summary>
        /// 清除墓碑（/cleartomb指令）
        /// </summary>
        /// <param name="args"></param>
        public static void ClearTomb(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (TileHelper.NeedWaitTask(op)) return;
            Action(op, OPType.Tomb, new FindInfo(TileID.Tombstones, 0, 2, 2));
        }

        static async void Action(TSPlayer op, OPType type, FindInfo fd, string name = "")
        {
            WMFindTool.ResetSkip();
            int secondLast = Utils.GetUnixTimestamp;
            Rectangle rect = Utils.GetWorldArea();
            bool flag = false;
            int count = 0;
            await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(name)) name = Mapping.FlagTile(GetOPName(type));
                op.SendSuccessMessage($"全图清除{name}开始……");
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        flag = type switch
                        {
                            OPType.Tomb or
                            OPType.Dress or
                            OPType.Larva or
                            OPType.Bulb or
                            OPType.RollingCactus or
                            OPType.BeeHive or
                            OPType.TNTBarrel or
                            OPType.LifeCrystalBoulder or
                            OPType.BouncyBoulder or
                            OPType.Sprite => ClearSprite(x, y, fd),

                            OPType.Spikes or
                            OPType.WoodenSpikes or
                            OPType.Block => ClearTile(x, y, fd.id),
                            _ => false,
                        };
                        if (flag) count++;
                    }
                }
            }).ContinueWith((d) =>
            {
                TileHelper.FinishGen();
                op.SendSuccessMessage($"全图共清除{count}个{name}，用时 {Utils.GetUnixTimestamp - secondLast}秒。");
            });
        }

        /// <summary>
        /// 清除指定精灵（暂时无法判定style）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tileID"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        static bool ClearSprite(int x, int y, FindInfo fd)
        {
            ITile tile = Main.tile[x, y];
            if (!tile.active() || tile.type != fd.id)
            {
                return false;
            }

            if (WMFindTool.CheckSprite(x, y, fd))
            {
                TileHelper.ClearTile(x, y, fd.w, fd.h);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清除指定图格
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tileID"></param>
        /// <returns></returns>
        static bool ClearTile(int x, int y, int tileID)
        {
            ITile tile = Main.tile[x, y];
            if (tile.type == tileID)
            {
                TileHelper.ClearTile(x, y);
                return true;
            }
            return false;
        }
    }
}