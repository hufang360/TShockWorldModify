using Terraria.ID;

namespace WorldModify
{
    public class RoomTheme
    {
        public ushort tile = TileID.Dirt;
        public ushort wall = WallID.Dirt;
        public TileInfo platform = new TileInfo(TileID.Platforms, 0);
        public TileInfo chair = new TileInfo(TileID.Chairs, 0);
        public TileInfo bench = new TileInfo(TileID.WorkBenches, 0);
        public TileInfo torch = new TileInfo(TileID.Torches, 0);

        public static RoomTheme GetGlass()
        {
            // 玻璃消耗 边框19 墙16-4 平台12-6 椅子1-4 工作台1-10 火把1-1凝胶1木材
            RoomTheme th = new RoomTheme
            {
                tile = TileID.Glass,
                wall = WallID.Glass
            };
            th.platform.style = 14;
            th.chair.style = 18;
            th.bench.style = 25;
            th.torch.style = TorchID.White;

            return th;
        }

        public static RoomTheme GetWood()
        {
            RoomTheme th = new RoomTheme
            {
                tile = TileID.WoodBlock,
                wall = WallID.Wood,
            };
            //th.platform.style = 0;
            //th.chair.style = 0;
            //th.bench.style = 0;
            //th.torch.style = 0;

            return th;
        }
    }
}