using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify.Gen;

/// <summary>
/// 地狱直通车
/// </summary>
public class Hellevator
{
    public static Task AsyncGenHellevator(TSPlayer op, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        int height = 0;
        return Task.Run(() =>
        {
            height = GenHellevator(posX, posY);
        }).ContinueWith((d) =>
        {
            TileHelper.GenAfter();
            int second = Utils.GetUnixTimestamp - secondLast;
            op.SendSuccessMessage($"已生成地狱直通车，高{height}格，用时{second}秒。");
        });
    }

    static int GenHellevator(int posX, int posY)
    {
        int hell = 0;
        int xtile;
        for (hell = Main.UnderworldLayer + 10; hell <= Main.maxTilesY - 100; hell++)
        {
            xtile = posX;
            Parallel.For(posX, posX + 8, (cwidth, state) =>
            {
                if (Main.tile[cwidth, hell].active() && !Main.tile[cwidth, hell].lava())
                {
                    state.Stop();
                    xtile = cwidth;
                    return;
                }
            });

            if (!Main.tile[xtile, hell].active()) break;
        }

        int Width = 5;
        int height = hell;
        int Xstart = posX - 2;
        int Ystart = posY;

        Parallel.For(Xstart, Xstart + Width, (cx) =>
        {
            Parallel.For(Ystart, hell, (cy) =>
            {
                ITile tile = Main.tile[cx, cy];
                tile.ClearEverything();

                if (cx == Xstart + Width / 2)
                {
                    tile.type = TileID.SilkRope;
                    tile.active(true);
                    tile.slope(0);
                    tile.halfBrick(false);
                }
                else if (cx == Xstart || cx == Xstart + Width - 1)
                {
                    tile.type = TileID.ObsidianBrick;
                    tile.active(true);
                    tile.slope(0);
                    tile.halfBrick(false);
                }
            });
        });

        // 平台
        WorldGen.PlaceTile(Xstart + 1, Ystart, 19, false, true, -1, 13);
        WorldGen.PlaceTile(Xstart + 2, Ystart, 19, false, true, -1, 13);
        WorldGen.PlaceTile(Xstart + 3, Ystart, 19, false, true, -1, 13);

        return hell;
    }
}
