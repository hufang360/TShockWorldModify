using System.Collections.Generic;
using Terraria.ID;

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

            Filling();
        }

        /// <summary>
        /// 补齐frame信息
        /// </summary>
        void Filling()
        {
            if (w == 1 && h == 1)
            {
                var tileProp = TileHelper.GetTileByID((ushort)id);
                if (tileProp != null)
                {
                    w = tileProp.w;
                    h = tileProp.h;
                }
            }

            switch (id)
            {
                case TileID.ShadowOrbs: // 31
                    if (style == 0)
                        frameX = 0; // 暗影珠
                    else if (style == 1)
                        frameX = 36; // 猩红之心
                    break;

                case TileID.DemonAltar: // 26
                    if (style == 0)
                        frameX = 0; // 恶魔祭坛
                    else if (style == 1)
                        frameX = 54; // 猩红祭坛
                    break;

                case TileID.LargePiles2: // 187
                    if (style == 5) //附魔剑
                    {
                        frameX = 918;
                        frameY = 0;
                    }
                    break;
            }
        }

        public override string ToString()
        {
            return $"id={id},style={style},w={w},h={h},frameX={frameX},frameY={frameY}";
        }

        /// <summary>
        /// 是否为多样式图格
        /// </summary>
        /// <returns></returns>
        public bool IsFrame()
        {
            return TileHelper.IsFrame(id);
        }
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
            get
            {
                var itemID = Mapping.GetItemID(name);
                var wallName = Utils.Highlight($"{name}({id})");
                if (itemID != 0)
                    return $"[i:{itemID}]{wallName}";
                else
                    return wallName;
            }
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

        public int frameX = -1;
        public int frameY = -1;

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

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public TileProp Clone()
        {
            TileProp p = new()
            {
                id = id,
                w = w,
                h = h,
                isFrame = isFrame,
                name = name,
                color = color
            };
            return p;
        }

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
            get
            {
                var itemID = Mapping.GetItemID(name);
                var tileName = Utils.Highlight($"{name}({id})");
                if (itemID != 0)
                    return $"[i:{itemID}]{tileName}";
                else
                    return tileName;
            }
        }

        public override string ToString()
        {
            return $"{id},{name},{w},{h},{(isFrame ? "是" : "否")},{FullColor}";
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
