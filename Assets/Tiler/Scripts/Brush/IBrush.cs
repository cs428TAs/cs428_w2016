using System;
using UnityEngine;

namespace Tiler
{
    public interface IBrush
    {
        Brush GetBrush(int x = 0, int y = 0);
        Texture2D GetPreview();

        int TextureSize { get; set; }
        Point BrushSize { get; set; }

        //  the transformation routines
        void Rotate();
        void Destroy();
    }

    [Serializable]
    public struct Brush
    {
        public Color[] Colors;
        public TileProperties Properties;
        public bool[] Collision;

        private readonly int _textureSize;
        private readonly int _collisionSize;

        public Brush(Color[] colors, TileProperties properties, bool[] collision)
        {
            Colors = colors;
            Properties = properties;
            Collision = collision;

            _textureSize = (int)Math.Sqrt(Colors.Length);
            _collisionSize = (int)Math.Sqrt(Collision.Length);
        }

        public void Rotate()
        {
            Properties.Rot = (Rotation)(((int)Properties.Rot + 1) & 3);

            var array = new Color[_textureSize * _textureSize];

            for (int y = 0; y < _textureSize; y++)
            {
                for (int x = 0; x < _textureSize; x++)
                {
                    var yy = y;
                    var xx = (_textureSize - 1) - x;

                    var index1 = y * _textureSize + x;
                    var index2 = xx * _textureSize + yy;

                    array[index2] = Colors[index1];
                }
            }
            Colors = array;

            var array1 = new bool[_collisionSize * _collisionSize];

            for (int y = 0; y < _collisionSize; y++)
            {
                for (int x = 0; x < _collisionSize; x++)
                {
                    var yy = y;
                    var xx = (_collisionSize - 1) - x;

                    var index1 = y * _collisionSize + x;
                    var index2 = xx * _collisionSize + yy;

                    array1[index2] = Collision[index1];
                }
            }
            Collision = array1;

        }
    }
}
