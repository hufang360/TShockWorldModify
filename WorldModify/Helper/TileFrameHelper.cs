using System.Collections.Generic;
using Terraria.DataStructures;

namespace WorldModify
{

    #region TileHelper
    public class TileFrameHelper
    {
        private static List<TileProperty> tiles = new List<TileProperty>();
        private static bool isLoad = false;

        public static void Load()
        {
            if (isLoad) return;
            else isLoad = true;

            foreach (string line in utils.FromEmbeddedPath("WorldModify.res.TileFrame.csv").Split('\n'))
            {
                var arr = line.Split(';');

                var arrSize = arr[1].Split(',');
                var arrFrame = arr[2].Split(':');

                TileProperty tile = new TileProperty
                {
                    id = int.Parse(arr[0])
                };
                if (arrSize.Length == 2)
                {
                    tile.w = int.Parse(arrSize[0]);
                    tile.h = int.Parse(arrSize[1]);
                }
                foreach (string s in arrFrame)
                {
                    var arrUV = s.Split(',');
                    if (arrUV.Length == 2)
                    {
                        tile.frames.Add(new Point16(int.Parse(arrUV[0]), int.Parse(arrUV[1])));
                    }
                }
                tiles.Add(tile);
            }
        }

        public static FindData GetFindData(int id, int style = -1)
        {
            FindData fd = new FindData(id, style);
            fd.style = style;

            TileProperty tprop = GetTileProp(id);
            fd.w = tprop.w;
            fd.h = tprop.h;
            if (style != -1 && style < tprop.frames.Count)
            {
                fd.frameX = tprop.frames[style].X;
                fd.frameY = tprop.frames[style].Y;
            }
            return fd;
        }

        private static TileProperty GetTileProp(int id)
        {
            foreach (var tile in tiles)
            {
                if (tile.id == id) return tile;
            }
            return new TileProperty();
        }
    }
    #endregion


    #region TileProperty
    public class TileProperty
    {
        public int id = 0;
        public int w = 1;
        public int h = 1;
        public List<Point16> frames = new List<Point16>();
    }
    #endregion







}
