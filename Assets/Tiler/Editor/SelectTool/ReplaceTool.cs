namespace Tiler
{
    class ReplaceTool : IDrawTool
    {
        private readonly DrawWindow _parent;
        private NormalBrush _brush;

        public ReplaceTool(DrawWindow parent, NormalBrush brush)
        {
            _parent = parent;

            var map = _parent.TilerMap;
            _brush = brush;

            if (_brush == null)
                _brush = new NormalBrush(map != null ? map.TileResolution : 1, TileTexture.None);

            _brush.BrushSize = new Point(1, 1);
        }

        public void DoAction(Point p)
        {
            _parent.TilerMapEditFunctions.ReplaceTiles(p, _brush);
        }

        public IBrush GetBrush()
        {
            return _brush;
        }

        public void SetBrush(IBrush brush)
        {
            // Only allow normal brushes
            var b = brush as NormalBrush;
            if (b == null)
            {
                var map = _parent.TilerMap;
                b = new NormalBrush(map != null ? map.TileResolution : 1, TileTexture.None);
            }

            _brush = b;
        }
    }
}
