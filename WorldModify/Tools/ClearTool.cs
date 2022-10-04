using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 橡皮擦工具
    /// </summary>
    class ClearTool
    {
        enum Type
        {
            NULL,
            All,
            Block,
            Water,
            Lava,
            Honey,
            Shimmer,
            Liquid,
        };

        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (args.Parameters.Count == 0)
            {
                op.SendInfoMessage("输入 /igen clear help 可以查看更多选项");
                return;
            }

            Type type = Type.NULL;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    op.SendInfoMessage("/igen c all，清除 全部");
                    //op.SendInfoMessage("/igen c block [图格id]，清除 方块");
                    //op.SendInfoMessage("/igen c wall [图格id]，清除 墙");
                    //op.SendInfoMessage("/igen c wire [red/blue/]，清除 电线");

                    op.SendInfoMessage("/igen c water，清除 水");
                    op.SendInfoMessage("/igen c honey，清除 蜂蜜");
                    op.SendInfoMessage("/igen c lava，清除 岩浆");
                    op.SendInfoMessage("/igen c shimmer，清除 微光");
                    op.SendInfoMessage("/igen c liquid，清除 液体");
                    return;

                case "all": type = Type.All; break;

                case "water": type = Type.Water; break;
                case "lava": type = Type.Lava; break;
                case "honey": type = Type.Honey; break;
                case "shimmer": type = Type.Shimmer; break;
                case "liquid": type = Type.Liquid; break;
            }

            if (type != Type.NULL)
            {
                if (ReGenHelper.NeedWaitTask(op)) return;
                Action(op, type, SelectionTool.GetSelection(op.Index));
            }
        }


        private static async void Action(TSPlayer op, Type type, Rectangle rect)
        {
            if (rect.Width == 0 && rect.Height == 0) rect = utils.GetScreen(op);
            bool needAll = rect.Width == Main.maxTilesX && rect.Height == Main.maxTilesY;


            int secondLast = utils.GetUnixTimestamp;
            string GetOpString()
            {
                switch (type)
                {
                    case Type.Water: return "清除水";
                    case Type.Lava: return "清除岩浆";
                    case Type.Honey: return "清除蜂蜜";
                    case Type.Liquid: return "清除液体";
                    case Type.Shimmer: return "清除微光";
                    default: return "图格修改";
                }
            }
            string opString = GetOpString();


            await Task.Run(() =>
            {
                if (needAll) op.SendSuccessMessage($"全图 {opString} 开始");
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        switch (type)
                        {
                            case Type.All: ReGenHelper.Clear(x, y); break;

                            // 液体
                            case Type.Water:
                            case Type.Lava:
                            case Type.Honey:
                            case Type.Shimmer:
                            case Type.Liquid:
                                ClearLiquid(x, y, type);
                                break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                if (needAll)
                {
                    ReGenHelper.FinishGen(false);
                    op.SendSuccessMessage($"全图 {opString} 结束（用时 {utils.GetUnixTimestamp - secondLast}秒）");
                }
                else
                {
                    ReGenHelper.FinishGen(false);
                    op.SendSuccessMessage($"{opString} 结束");
                }
            });
        }


        // 清除液体
        private static void ClearLiquid(int x, int y, Type type)
        {
            ITile tile = Main.tile[x, y];
            void cl()
            {
                tile.liquid = 0;
                //WorldGen.SquareTileFrame(x, y);
                NetMessage.SendTileSquare(-1, x, y);
            }

            if (tile.liquid > 0)
            {
                switch (type)
                {
                    case Type.Liquid: cl(); break;
                    case Type.Water: if (tile.liquidType() == 0) cl(); break;
                    case Type.Lava: if (tile.liquidType() == 1) cl(); break;
                    case Type.Honey: if (tile.liquidType() == 2) cl(); break;
                    case Type.Shimmer: if (tile.liquidType() == 3) cl(); break;
                    default: break;
                }
            }
        }

    }
}