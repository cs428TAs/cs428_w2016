using System;
using TileDraw;
using UnityEngine;

namespace Tiler
{
    [Serializable]
    public sealed class NormalBrush : IBrush
    {
        [SerializeField]private readonly Texture2D _preview;
        [SerializeField]private Brush _brush;
        [SerializeField]private static Point _brushSize = new Point(1,1);
        [SerializeField]private int _textureSize;

        public int TextureSize
        {
            get { return _textureSize; }
            set { _textureSize = value; }
        }

        public Point BrushSize
        {
            get { return _brushSize; }
            set { _brushSize = value; }
        }

        public NormalBrush(int textureSize, TileTexture tt)
        {
            TextureSize = textureSize;

            if (tt == null)
            {
                return;
            }

            var pixels = tt.Texture.GetPixels();
            var oldSize = (int)Mathf.Sqrt(pixels.Length);

            var colors = Util.ResizeArray(pixels, _textureSize, oldSize);

            var c = tt.Collision;
            var collision = new bool[c.Length];
            Array.Copy(c, collision, c.Length);

            _brush = new Brush(colors, new TileProperties(tt.ID), collision);

            _preview = new Texture2D(_textureSize, _textureSize, TextureFormat.ARGB32, false);
            _preview.SetPixels(_brush.Colors);
            _preview.Apply();
        }

        public void Destroy()
        {
            if (Application.isEditor)
                UnityEngine.Object.DestroyImmediate(_preview);
            else
                UnityEngine.Object.Destroy(_preview);
        }

        public Brush GetBrush(int x = 0, int y = 0)
        {
            return _brush;
        }
        public Texture2D GetPreview()
        {
            return _preview;
        }

        //  the transformation routines
        public void Rotate()
        {
            _brush.Rotate();

            _preview.SetPixels(_brush.Colors);
            _preview.Apply();
        }
    }
}
