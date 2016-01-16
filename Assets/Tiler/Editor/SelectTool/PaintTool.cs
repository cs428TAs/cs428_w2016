namespace Tiler
{
    public class PaintTool : IDrawTool
    {
        private readonly DrawWindow _parent;
        private IBrush _brush;

        public PaintTool(DrawWindow parent, IBrush brush)
        {
            _parent = parent;
            _brush = brush;

            var map = _parent.TilerMap;

            if (_brush == null)
                _brush = new NormalBrush(map != null ? map.TileResolution : 1, TileTexture.None);
        }

        public void DoAction(Point p)
        {
            _parent.TilerMapEditFunctions.PaintTile(p, _brush);
        }

        public IBrush GetBrush()
        {
            return _brush;
        }

        public void SetBrush(IBrush brush)
        {
            _brush = brush;
        }
    }
}