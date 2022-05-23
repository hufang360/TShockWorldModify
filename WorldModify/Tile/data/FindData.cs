using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TShockAPI;


namespace WorldModify
{
    public class FindData
    {
        public int id = 0;
        public int w = 1;
        public int h = 1;
        public int style = -1;
        public int frameX = -1;
        public int frameY = -1;

        public FindData(int _id, int _style, int _w, int _h, int _frameX = -1, int _frameY = -1)
        {
            id = _id;
            w = _w;
            h = _h;
            style = _style;
            frameX = _frameX;
            frameY = _frameY;
        }
    }
}