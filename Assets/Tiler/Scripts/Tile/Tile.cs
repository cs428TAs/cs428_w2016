using System;

namespace Tiler
{
    [Serializable]
    public class Tile
    {
        public TileProperties Properties;
        public bool[] Collision;

        public Tile()
        {
            Properties = new TileProperties(0);

            Collision = new bool[1];
        }
    }
}