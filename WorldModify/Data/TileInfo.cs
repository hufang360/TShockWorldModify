namespace WorldModify
{
    public class TileInfo
    {
        // 0 图格 1 墙 2 液体
        public int type = 0;

        public int id = 0;
        public int style = 0;

        public TileInfo() { }

        public TileInfo(int _id, int _style)
        {
            id = _id;
            style = _style;
        }
    }
}
