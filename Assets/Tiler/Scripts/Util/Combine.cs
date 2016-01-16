// Largest area is a modified version of http://tech-queries.blogspot.com.au/2011/03/maximum-area-rectangle-in-histogram.html

using System;
using System.Collections.Generic;

namespace Tiler
{
    public class Combine
    {
        public int[,] Find(bool[,] mat)
        {
            var nrows = mat.GetLength(1);
            var ncols = mat.GetLength(0);

            var counts = new int[ncols, nrows];

            for (var i = nrows - 1; i >= 0; i--)
            {
                for (var j = ncols - 1; j >= 0; j--)
                {
                    if (mat[j, i])
                    {
                        if (i < (nrows - 1) && j < (ncols - 1))
                            counts[j, i] = (1 +
                                            Math.Min(Math.Min(counts[j + 1, i], counts[j, i + 1]), counts[j + 1, i + 1]));
                        else
                            counts[j, i] = 1;
                    }
                }
            }
            return counts;
        }

        public Point[,] FindRect(bool[,] mat)
        {
            var rows = mat.GetLength(1);
            var cols = mat.GetLength(0);

            var result = new Point[cols, rows];

            while (true)
            {
                var aux = new int[cols, rows];

                for (var y = 0; y < cols; y++)
                {
                    for (var x = 0; x < rows; x++)
                    {
                        if (mat[y, x])
                        {
                            if (y == 0)
                                aux[y, x] = 1;
                            else
                                aux[y, x] = aux[y - 1, x] + 1;
                        }
                    }
                }

                Data[,] data = LargestArea(aux);

                var pos = new Point();
                var max = 0;
                for (var y = 0; y < cols; y++)
                {
                    for (var x = 0; x < rows; x++)
                    {
                        if (data[y, x].LargestStack > max)
                        {
                            max = data[y, x].LargestStack;
                            pos = new Point(x, y);
                        }

                    }
                }

                if (max < 2)
                {
                    for (var y = 0; y < cols; y++)
                    {
                        for (var x = 0; x < rows; x++)
                        {
                            if (mat[y, x])
                            {
                                //result[y, x] = ++found;
                                result[y, x] = new Point(1, 1);
                            }
                        }
                    }

                    return result;
                }

                var d = data[pos.Y, pos.X];

                var width = d.Right + d.Left + 1;
                var height = max / width;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var yy = pos.Y - y;
                        var xx = pos.X + x - d.Left;

                        if (x == 0 && y == 0)
                        {
                            result[yy, xx] = new Point(width, height);
                        }

                        mat[yy, xx] = false;
                    }
                }
            }
        }

        private Data[,] LargestArea(int[,] arr)
        {
            var cols = arr.GetLength(0);
            var rows = arr.GetLength(1);

            var area = new Data[cols, rows];

            for (var j = 0; j < cols; j++)
            {
                int i, t;
                var st = new Stack<int>(); //include stack for using this #include<stack>  

                for (i = 0; i < rows; i++)
                {
                    while (st.Count != 0)
                    {
                        if (arr[j, i] <= arr[j, st.Peek()])
                        {
                            st.Pop();
                        }
                        else
                            break;
                    }
                    if (st.Count == 0)
                        t = -1;
                    else
                        t = st.Peek();
                    //Calculating Li  
                    area[j, i].Left = i - t - 1;
                    st.Push(i);
                }

                st.Clear();

                for (i = rows - 1; i >= 0; i--)
                {
                    while (st.Count != 0)
                    {
                        if (arr[j, i] <= arr[j, st.Peek()])
                        {
                            st.Pop();
                        }
                        else
                            break;
                    }
                    t = st.Count == 0 ? rows : st.Peek();
                    //calculating Ri, after this step area[i] = Li + Ri  
                    area[j, i].Right = t - i - 1;
                    st.Push(i);
                }

                for (i = 0; i < rows; i++)
                {
                    area[j, i].LargestStack = arr[j, i] * ((area[j, i].Right + area[j, i].Left) + 1);
                }
            }

            return area;
        }


        private struct Data
        {
            public int LargestStack;
            public int Left;
            public int Right;
        }
    }
}
