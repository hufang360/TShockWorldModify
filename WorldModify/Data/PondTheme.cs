using Terraria.ID;


namespace WorldModify
{
    class PondTheme
    {
        public ushort tile = TileID.Dirt;
        public ushort wall = WallID.Dirt;
        public TileInfo platform = new TileInfo(TileID.Platforms, 0);
        public TileInfo chair = new TileInfo(TileID.Chairs, 0);
        public TileInfo bench = new TileInfo(TileID.WorkBenches, 0);
        public TileInfo torch = new TileInfo(TileID.Torches, 0);

        public void SetGlass()
        {
            tile = TileID.Glass;
            wall = WallID.Glass;
            platform.style = 14;
            chair.style = 18;
            bench.style = 25;
            torch.style = TorchID.White;
        }

        public void SetGray()
        {
            tile = TileID.GrayBrick;
            wall = WallID.GrayBrick;
            platform.style = 43;
        }

        public void SetWood()
        {
            tile = TileID.WoodBlock;
            wall = WallID.Wood;
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
            // 蜂蜜主题
            tile = TileID.ObsidianBrick;
            wall = WallID.ObsidianBrick;
            platform.style = 13;
        }
    }
}
