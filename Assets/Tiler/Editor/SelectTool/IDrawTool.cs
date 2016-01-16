namespace Tiler
{
    public interface IDrawTool
    {
        void DoAction(Point p);

        IBrush GetBrush();
        void SetBrush(IBrush brush);
    }
}
