using System;
using UnityEngine;

namespace Tiler
{
    [Serializable]
    [Flags]
    public enum ConnectionMask
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
    }

    [Serializable]
    public class TileTexture
    {
        // Because unity won't serialize longs we split it but keep it hidden.
        [SerializeField] private int _setID;
        [SerializeField] private int _tileID;

        public long ID
        {
            get { return ((long) _setID << 32) | (uint) _tileID; }
            private set
            {
                _tileID = (int) value;
                _setID = (int) (value >> 32);
            }
        }

        public Texture2D Texture;
        public ConnectionMask Connections;
        public bool[] Collision;

        public static TileTexture None
        {
            get
            {
                var t = new Texture2D(1, 1);
                var c = t.GetPixels();
                c[0].a = 0;
                t.SetPixels(c);
                return new TileTexture(0, t, 1);
            }
        }

        public TileTexture(long id, Texture2D texture, int collisionSize)
        {
            ID = id;
            Texture = texture;

            Collision = new bool[collisionSize*collisionSize];
        }
    }
}