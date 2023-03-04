using System;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace WorldModify.Gen;

/// <summary>
/// 小房间 和 NPC旅馆
/// </summary>
public class Room
{
    /// <summary>
    /// NPC旅馆
    /// </summary>
    public static Task AsyncGenHotel(TSPlayer op, int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            int w = 5;
            int row = 6;
            int roomWidth = 1 + Math.Min(row, total) * w;

            int fixPosX = needCenter ? posX - (roomWidth / 2) : posX;
            for (int i = 0; i < total; i++)
            {
                int startX = fixPosX + i % row * w;
                int startY = posY - (int)Math.Floor((float)(i / row)) * 10;
                GenRoom(startX, startY, isRight);
            }
            int x1 = fixPosX - 1;
            int x2 = fixPosX + roomWidth;
            int y = posY - 1;
            WorldGen.PlaceTile(x1, y, 19, false, true, -1, 14);
            WorldGen.PlaceTile(x2, y, 19, false, true, -1, 14);
            ////Main.tile[x1, y].slope(2);
            ////Main.tile[x2, y].slope(1);
            //WorldGen.SquareTileFrame(x1, y);
            //WorldGen.SquareTileFrame(x2, y);
        }).ContinueWith((d) =>
        {
            TileHelper.GenAfter();
            int second = Utils.GetUnixTimestamp - secondLast;
            op.SendSuccessMessage($"已生成NPC小旅馆，内含{total}个小房间，用时{second}秒。");
        });
    }


    /// <summary>
    /// 小房间
    /// </summary>
    public static Task AsyncGenRoom(TSPlayer op, int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            int w = 5;
            int roomWidth = 1 + total * w;
            int startX = needCenter ? posX - (roomWidth / 2) : posX;
            for (int i = 0; i < total; i++)
            {
                GenRoom(startX, posY, isRight);
                startX += w;
            }
        }).ContinueWith((d) =>
        {
            TileHelper.GenAfter();
            int second = Utils.GetUnixTimestamp - secondLast;
            op.SendSuccessMessage($"已生成{total}个小房间，用时{second}秒。");
        });
    }

    static void GenRoom(int posX, int posY, bool isRight = true)
    {
        RoomTheme th = new();
        th.SetGlass();

        ushort tileID = th.tile;
        ushort wallID = th.wall;
        TileInfo platform = th.platform;
        TileInfo chair = th.chair;
        TileInfo bench = th.bench;
        TileInfo torch = th.torch;

        int Xstart = posX;
        int Ystart = posY;
        int Width = 6;
        int height = 10;

        if (!isRight)
            Xstart += 2;

        for (int cx = Xstart; cx < Xstart + Width; cx++)
        {
            for (int cy = Ystart - height; cy < Ystart; cy++)
            {
                ITile tile = Main.tile[cx, cy];
                tile.ClearEverything();

                // 墙
                if ((cx > Xstart) && (cy < Ystart - 5) && (cx < Xstart + Width - 1) && (cy > Ystart - height))
                {
                    tile.wall = wallID;
                }

                if ((cx == Xstart && cy > Ystart - 5)
                || (cx == Xstart + Width - 1 && cy > Ystart - 5)
                || (cy == Ystart - 1))
                {
                    // 平台
                    WorldGen.PlaceTile(cx, cy, platform.id, false, true, -1, platform.style);
                }

                else if ((cx == Xstart) || (cx == Xstart + Width - 1)
                || (cy == Ystart - height)
                || (cy == Ystart - 5))
                {
                    // 方块
                    tile.type = tileID;
                    tile.active(true);
                    tile.slope(0);
                    tile.halfBrick(false);
                }
            }
        }

        if (isRight)
        {
            WorldGen.PlaceTile(Xstart + 1, Ystart - 6, chair.id, false, true, 0, chair.style);  // 椅子
            Main.tile[Xstart + 1, posY - 6].frameX += 18;
            Main.tile[Xstart + 1, posY - 7].frameX += 18;

            WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.id, false, true, -1, bench.style); // 工作台
            WorldGen.PlaceTile(Xstart + 4, Ystart - 5, torch.id, false, true, -1, torch.style); // 火把
        }
        else
        {
            WorldGen.PlaceTile(Xstart + 4, Ystart - 6, chair.id, false, true, 0, chair.style);
            WorldGen.PlaceTile(Xstart + 2, Ystart - 6, bench.id, false, true, -1, bench.style);
            WorldGen.PlaceTile(Xstart + 1, Ystart - 5, torch.id, false, true, -1, torch.style);
        }
    }
}
