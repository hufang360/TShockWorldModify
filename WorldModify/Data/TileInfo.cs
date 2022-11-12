using System.Collections.Generic;

namespace WorldModify
{
    /// <summary>
    /// 图格信息
    /// </summary>
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

    /// <summary>
    /// 图格替换信息
    /// </summary>
    class ReTileInfo
    {
        public TileInfo before = new();
        public TileInfo after = new();
        public string comment = "";

        public ReTileInfo(int beforeID, int afterID, int bType = 0, int aType = 0, string _comment = "")
        {
            before.id = beforeID;
            after.id = afterID;

            before.type = bType;
            after.type = aType;

            if (!string.IsNullOrEmpty(_comment)) comment = _comment;
        }
    }

    /// <summary>
    /// 查找信息
    /// </summary>
    public class FindInfo
    {
        public int id = 0;
        public int w = 1;
        public int h = 1;
        public int style = 0;
        public int frameX = -1;
        public int frameY = -1;

        public FindInfo()
        {
        }

        public FindInfo(int _id, int _style = 0, int _w = 1, int _h = 1, int _frameX = -1, int _frameY = -1)
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
        //public void Fixed()
        //{
        //    TileObjectData tileData = TileObjectData.GetTileData(id, style);
        //    w = tileData.Width;
        //    h = tileData.Height;
        //}

        //public void FixedWH()
        //{
        //    TileObjectData tileData = TileObjectData.GetTileData(id, 0);
        //    w = tileData.Width;
        //    h = tileData.Height;
        //}
    }


    /// <summary>
    /// 墙属性
    /// </summary>
    public class WallProp
    {
        public int id = 0;
        public string name;
        public string color;

        /// <summary>
        /// 完整的色值
        /// </summary>
        public string FullColor
        {
            get { return $"#FF{color}"; }
        }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Desc
        {
            get { return $"{name}（{id}）"; }
        }

        public override string ToString()
        {
            return $"{id},{name},{FullColor}";
        }
    }


    /// <summary>
    /// 图格属性
    /// </summary>
    public class TileProp
    {
        public int id = 0;
        public int w = 1;
        public int h = 1;
        public bool isFrame = false;
        public string name = "";
        public string color = "";
        public List<FrameProp> frames = new();

        public void Add(int frameX, int frameY, string name = "", string variety = "")
        {
            if (!string.IsNullOrEmpty(variety))
                variety = $"({variety})";

            frames.Add(new FrameProp
            {
                style = frames.Count,
                frameX = frameX,
                frameY = frameY,
                name = $"{name}{variety}",
            });
        }
    }

    /// <summary>
    /// 子图格
    /// </summary>
    public class FrameProp
    {
        public int style = 0;
        public int frameX = 0;
        public int frameY = 0;
        public string name = "";
    }

}
