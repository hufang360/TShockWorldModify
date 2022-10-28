using Terraria.ObjectData;


namespace WorldModify
{
    public class FindData
    {
        public int id = 0;
        public int w = 1;
        public int h = 1;
        public int style = 0;
        public int frameX = -1;
        public int frameY = -1;

        public FindData()
        {
        }

        public FindData(int _id, int _style = 0, int _w = 1, int _h = 1, int _frameX = -1, int _frameY = -1)
        {
            id = _id;
            w = _w;
            h = _h;
            style = _style;
            frameX = _frameX;
            frameY = _frameY;
        }

        public override string ToString()
        {
            return $"id={id},style={style},w={w},h={h}";
        }

        /// <summary>
        /// 补齐宽高、及 style信息
        /// </summary>
        public void Fixed()
        {
            TileObjectData tileData = TileObjectData.GetTileData(id, style);
            w = tileData.Width;
            h = tileData.Height;
        }

        public void FixedWH()
        {
            TileObjectData tileData = TileObjectData.GetTileData(id, 0);
            w = tileData.Width;
            h = tileData.Height;
        }
    }
}
