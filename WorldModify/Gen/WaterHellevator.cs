using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify.Gen;

/// <summary>
/// 水电梯
/// </summary>
public class WaterHellevator
{
    public static Task AsyncGen(TSPlayer op, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        int height = 0;
        return Task.Run(() =>
        {
            height = Gen(posX, posY);
        }).ContinueWith((d) =>
        {
            TileHelper.GenAfter();
            int second = Utils.GetUnixTimestamp - secondLast;
            op.SendSuccessMessage($"已生成水电梯，高{height}格，用时{second}秒。");
        });
    }

    static int Gen(int posX, int posY)
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

                if (cx == Xstart || cx == Xstart + Width - 1 || cy == hell - 1)
                {
                    tile.type = TileID.Bubble;
                    tile.active(true);
                    tile.slope(0);
                    tile.halfBrick(false);
                }
                else
                {
                    tile.liquid = byte.MaxValue;
                    tile.liquidType(LiquidID.Water);
                }

            });
            // 顶部平台
            WorldGen.PlaceTile(Xstart + 1, Ystart, 19, false, true, -1, 46);
            WorldGen.PlaceTile(Xstart + 2, Ystart, 19, false, true, -1, 46);
            WorldGen.PlaceTile(Xstart + 3, Ystart, 19, false, true, -1, 46);
        });


        return hell;
    }
}
