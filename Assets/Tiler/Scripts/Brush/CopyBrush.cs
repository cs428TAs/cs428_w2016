using System;
using UnityEngine;

namespace Tiler
{
    [Serializable]
    public sealed class CopyBrush : IBrush
    {
        [SerializeField]private Texture2D _preview;
        [SerializeField]private int _textureSize;
        [SerializeField]private Point _brushSize;
        [SerializeField]private Brush[] _brushes;

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

        public CopyBrush(Brush[] brushes, Point brushSize)
        {
            if (brushes.Length == 0)
                throw new UnityException("No array");

            _textureSize = (int)Math.Sqrt(brushes[0].Colors.GetLength(0));
            _brushSize = brushSize;
            _brushes = brushes;

            CreateTexture();
        }

        public Brush GetBrush(int x = 0, int y = 0)
        {
            var left = (BrushSize.X - 1) / 2;
            var down = (BrushSize.Y - 1) / 2;

            var index = (y + down) * _brushSize.X + (x + left);

            return _brushes[index];
        }

        public Texture2D GetPreview()
        {
            return _preview;
        }

        //  the transformation routines
        public void Rotate()
        {
            for (int index = 0; index < _brushes.Length; index++)
            {
                _brushes[index].Rotate();
            }

            var array = new Brush[_brushes.Length];


            for (int y = 0; y < _brushSize.Y; y++)
            {
                for (int x = 0; x < _brushSize.X; x++)
                {
                    var yy = y;
                    var xx = (_brushSize.X - 1) - x;

                    var index1 = y * _brushSize.X + x;
                    var index2 = xx * _brushSize.Y + yy;

                    array[index2] = _brushes[index1];
                }
            }
            
            _brushes = array;

            // flip brush
            _brushSize = new Point(_brushSize.Y, _brushSize.X);

            CreateTexture();
        }

        private void CreateTexture()
        {
            UnityEngine.Object.DestroyImmediate(_preview);
            
            _preview = new Texture2D(_brushSize.X * _textureSize, _brushSize.Y * TextureSize);

            for (var y = 0; y < _brushSize.Y; y++)
            {
                for (var x = 0; x < _brushSize.X; x++)
                {
                    var index = y * _brushSize.X + x;

                    var pixels = _brushes[index].Colors;
                    _preview.SetPixels(x * _textureSize, y * _textureSize, _textureSize, _textureSize, pixels);
                }
            }
            _preview.Apply();
        }

        public void Destroy()
        {
            if (Application.isEditor)
                UnityEngine.Object.DestroyImmediate(_preview);
            else
                UnityEngine.Object.Destroy(_preview);
        }
    }
}
