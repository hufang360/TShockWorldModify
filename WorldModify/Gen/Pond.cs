using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify.Gen;

/// <summary>
/// 鱼池
/// </summary>
public class Pond
{
    public static Task AsyncGenPond(TSPlayer op, int posX, int posY, int style)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            GenPond(posX, posY, style);
        }).ContinueWith((d) =>
        {
            TileHelper.GenAfter();
            int second = Utils.GetUnixTimestamp - secondLast;
            string desc = style switch
            {
                LiquidID.Lava => "岩浆",
                LiquidID.Honey => "蜂蜜",
                LiquidID.Shimmer => "微光",
                _ => "普通",
            };
            op.SendSuccessMessage($"已生成{desc}鱼池，用时{second}秒。");
        });
    }

    static void GenPond(int posX, int posY, int style)
    {
        PondTheme th = new();
        switch (style)
        {
            case LiquidID.Lava: th.SetObsidian(); break;
            case LiquidID.Honey: th.SetHoney(); break;
            case LiquidID.Shimmer: th.SetGlass(); break;
            default: th.SetGlass(); break;
        }

        ushort tileID = th.tile;
        TileInfo platform = th.platform;

        int Xstart = posX - 6;
        int Ystart = posY;
        int Width = 11 + 2;
        int height = 30 + 2;

        for (int cx = Xstart; cx < Xstart + Width; cx++)
        {
            for (int cy = Ystart; cy < Ystart + height; cy++)
            {
                ITile tile = Main.tile[cx, cy];
                tile.ClearEverything();

                if ((cx == Xstart) || (cx == Xstart + Width - 1) || (cy == Ystart + height - 1))
                {
                    tile.type = tileID;
                    tile.active(true);
                    tile.slope(0);
                    tile.halfBrick(false);
                }
            }

            WorldGen.PlaceTile(cx, Ystart, platform.id, false, true, -1, platform.style);
        }

        for (int cx = Xstart + 1; cx < Xstart + Width - 1; cx++)
        {
            for (int cy = Ystart + 1; cy < Ystart + height - 1; cy++)
            {
                ITile tile = Main.tile[cx, cy];
                tile.active(active: false);
                tile.liquid = byte.MaxValue;
                switch (style)
                {
                    case LiquidID.Lava: tile.lava(true); break;
                    case LiquidID.Honey: tile.honey(true); break;
                    case LiquidID.Shimmer: tile.shimmer(true); break;
                }
            }
        }
    }

}
