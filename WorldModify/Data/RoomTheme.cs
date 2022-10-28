using Terraria.ID;


namespace WorldModify
{
    class RoomTheme
    {
        public ushort tile = TileID.Dirt;
        public ushort wall = WallID.Dirt;
        public TileInfo platform = new TileInfo(TileID.Platforms, 0);
        public TileInfo chair = new TileInfo(TileID.Chairs, 0);
        public TileInfo bench = new TileInfo(TileID.WorkBenches, 0);
        public TileInfo torch = new TileInfo(TileID.Torches, 0);

        public void SetGlass()
        {
            // 玻璃消耗 边框19 墙16-4 平台12-6 椅子1-4 工作台1-10 火把1-1凝胶1木材
            tile = TileID.Glass;
            wall = WallID.Glass;
            platform.style = 14;
            chair.style = 18;
            bench.style = 25;
            torch.style = TorchID.White;
        }

        public void SetGray()
        {
            // 玻璃消耗 边框19 墙16-4 平台12-6 椅子1-4 工作台1-10 火把1-1凝胶1木材
            tile = TileID.GrayBrick;
            wall = WallID.GrayBrick;
            platform.style = 43;
        }

        public void SetWood()
        {
            tile = TileID.WoodBlock;
            wall = WallID.Wood;
            //th.platform.style = 0;
            //th.chair.style = 0;
            //th.bench.style = 0;
            //th.torch.style = 0;
        }

        public void SetHoney()
        {
            // 蜂蜜主题
            tile = TileID.Hive;
            wall = WallID.Hive;

            platform.style = 24;
        }

        public void SetObsidian()
        {
            tile = TileID.ObsidianBrick;
            wall = WallID.ObsidianBrick;
            platform.style = 13;
        }
    }
}
