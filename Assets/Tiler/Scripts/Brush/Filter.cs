using System;

namespace Tiler.Editor
{
    [Serializable]
    public class Filter
    {
        public ConnectionMask ConnectionFilter;
        public bool IsFilterWithRotation;
        public bool IsFilterExclusive;

        public void Reset()
        {
            ConnectionFilter = 0;
            IsFilterWithRotation = false;
            IsFilterExclusive = false;
        }
    }
}
