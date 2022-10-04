using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// ÃÓ≥‰π§æﬂ
    /// </summary>
    class FillTool
    {
        enum Type
        {
            NULL,
            Block,  // ÃÓ≥‰Õº∏Ò
            Dirt,   // ÃÓ≥‰Õ¡øÈ
            Mud,    // ÃÓ≥‰ƒ‡øÈ
            Water,  // ÃÓÀÆ
            Honey,  // ÃÓ≥‰∑‰√€
            Lava,   // ÃÓ≥‰—“Ω¨
            Shimmer,   // ÃÓ≥‰Œ¢π‚
        };

        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            Rectangle selection = SelectionTool.GetSelection(op.Index);
            if (args.Parameters.Count == 0)
            {
                op.SendInfoMessage($"–Ë“™Ã·π©ÃÓ≥‰∑Ω∞∏£¨ ‰»Î /igen fill help ∞Ô÷˙£°");
                return;
            }

            Type type = Type.NULL;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    op.SendInfoMessage("/igen f dirt£¨ÃÓ≥‰Õ¡øÈ");
                    op.SendInfoMessage("/igen f mud£¨ÃÓ≥‰ƒ‡øÈ");
                    op.SendInfoMessage("/igen f water£¨ÃÓ≥‰ÀÆ");
                    op.SendInfoMessage("/igen f honey£¨ÃÓ≥‰∑‰√€");
                    op.SendInfoMessage("/igen f lava£¨ÃÓ≥‰—“Ω¨");
                    op.SendInfoMessage("/igen f shimmer£¨ÃÓ≥‰Œ¢π‚");
                    op.SendInfoMessage("/igen f <Õº∏Òid>£¨ÃÓ≥‰÷∏∂®Õº∏Ò");
                    return;

                case "dirt": type = Type.Dirt; break;
                case "mud": type = Type.Mud; break;
                case "water": type = Type.Water; break;
                case "honey": type = Type.Honey; break;
                case "lava": type = Type.Lava; break;
                case "shimmer": type = Type.Shimmer; break;
            }
            if (type != Type.NULL)
            {
                if (ReGenHelper.NeedWaitTask(op)) return;
                Action(op, type, selection);
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
                    case Type.Dirt: return "ÃÓ≥‰Õ¡øÈ";
                    case Type.Mud: return "ÃÓ≥‰ƒ‡øÈ";
                    case Type.Water: return "◊¢ÀÆ";
                    case Type.Honey: return "◊¢»Î∑‰√€";
                    case Type.Lava: return "◊¢»Î—“Ω¨";
                    case Type.Shimmer: return "◊¢»ÎŒ¢π‚";
                    default: return "Õº∏Ò–ﬁ∏ƒ";
                }
            }
            string opString = GetOpString();


            await Task.Run(() =>
            {
                if (needAll) op.SendSuccessMessage($"{opString} »´Õºø™ º");
                for (int x = rect.X; x < rect.Right; x++)
                {
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        switch (type)
                        {
                            case Type.Dirt:
                            case Type.Mud:
                            case Type.Water:
                            case Type.Honey:
                            case Type.Lava:
                            case Type.Shimmer:
                                Fill(x, y, type);
                                break;
                        }

                    }
                }
            }).ContinueWith((d) =>
            {
                if (needAll)
                {
                    ReGenHelper.FinishGen(true);
                    op.SendSuccessMessage($"{opString} »´ÕºΩ· ¯£®”√ ± {utils.GetUnixTimestamp - secondLast}s£©");
                }
                else
                {
                    ReGenHelper.FinishGen();
                    op.SendSuccessMessage($"{opString} Ω· ¯");
                }
            });
        }

        private static void Fill(int x, int y, Type type)
        {

            ITile tile = Main.tile[x, y];
            if (tile.active())
                return;

            switch (type)
            {
                case Type.Dirt:
                    tile.type = TileID.Dirt;
                    tile.active(active: true);
                    NetMessage.SendTileSquare(-1, x, y);
                    break;

                case Type.Mud:
                    tile.type = TileID.Mud;
                    tile.active(active: true);
                    NetMessage.SendTileSquare(-1, x, y);
                    break;

                case Type.Water:
                    tile.honey(honey: false);
                    tile.lava(lava: false);
                    tile.liquid = byte.MaxValue;
                    WorldGen.SquareTileFrame(x, y);
                    NetMessage.SendTileSquare(-1, x, y);
                    break;

                case Type.Honey:
                    tile.honey(honey: true);
                    tile.liquid = byte.MaxValue;
                    WorldGen.SquareTileFrame(x, y);
                    NetMessage.SendTileSquare(-1, x, y);
                    break;

                case Type.Lava:
                    tile.lava(lava: true);
                    tile.liquid = byte.MaxValue;
                    WorldGen.SquareTileFrame(x, y);
                    NetMessage.SendTileSquare(-1, x, y);
                    break;

                case Type.Shimmer:
                    tile.shimmer(shimmer: true);
                    tile.liquid = byte.MaxValue;
                    WorldGen.SquareTileFrame(x, y);
                    NetMessage.SendTileSquare(-1, x, y);
                    break;
            }
        }
    }
}