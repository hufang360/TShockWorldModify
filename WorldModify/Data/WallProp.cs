namespace WorldModify
{
    public class WallProp
    {
        public int id = 0;
        public string name;
        public string color;


        public string FullColor
        {
            get { return $"#FF{color}"; }
        }

        public override string ToString()
        {
            return $"{id},{name},{FullColor}";
        }
    }
}
