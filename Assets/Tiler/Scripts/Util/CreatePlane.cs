// Mesh creation class, modified version of the editor mesh class from wiki

using UnityEngine;

namespace Tiler
{
    public static class CreatePlane
    {
        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public enum AnchorPoint
        {
            TopLeft,
            TopHalf,
            TopRight,
            RightHalf,
            BottomRight,
            BottomHalf,
            BottomLeft,
            LeftHalf,
            Center
        }

        public static Mesh Create(string name, float width, float length, int widthSegments = 1, int lengthSegments = 1,
                                  Orientation orientation = Orientation.Horizontal,
                                  AnchorPoint anchor = AnchorPoint.Center)
        {
            Vector2 anchorOffset;
            switch (anchor)
            {
                case AnchorPoint.TopLeft:
                    anchorOffset = new Vector2(-width/2.0f, length/2.0f);
                    break;
                case AnchorPoint.TopHalf:
                    anchorOffset = new Vector2(0.0f, length/2.0f);
                    break;
                case AnchorPoint.TopRight:
                    anchorOffset = new Vector2(width/2.0f, length/2.0f);
                    break;
                case AnchorPoint.RightHalf:
                    anchorOffset = new Vector2(width/2.0f, 0.0f);
                    break;
                case AnchorPoint.BottomRight:
                    anchorOffset = new Vector2(width/2.0f, -length/2.0f);
                    break;
                case AnchorPoint.BottomHalf:
                    anchorOffset = new Vector2(0.0f, -length/2.0f);
                    break;
                case AnchorPoint.BottomLeft:
                    anchorOffset = new Vector2(-width/2.0f, -length/2.0f);
                    break;
                case AnchorPoint.LeftHalf:
                    anchorOffset = new Vector2(-width/2.0f, 0.0f);
                    break;
                    //case AnchorPoint.Center:
                default:
                    anchorOffset = Vector2.zero;
                    break;
            }


            var m = new Mesh
                        {
                            name = name
                        };

            var hCount2 = widthSegments + 1;
            var vCount2 = lengthSegments + 1;
            var numTriangles = widthSegments*lengthSegments*6;
            var numVertices = hCount2*vCount2;

            var vertices = new Vector3[numVertices];
            var uvs = new Vector2[numVertices];
            var triangles = new int[numTriangles];

            var index = 0;
            var uvFactorX = 1.0f/widthSegments;
            var uvFactorY = 1.0f/lengthSegments;
            var scaleX = width/widthSegments;
            var scaleY = length/lengthSegments;
            for (var y = 0.0f; y < vCount2; y++)
            {
                for (var x = 0.0f; x < hCount2; x++)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        vertices[index] = new Vector3(x*scaleX - width/2f - anchorOffset.x, 0.0f,
                                                      y*scaleY - length/2f - anchorOffset.y);
                    }
                    else
                    {
                        vertices[index] = new Vector3(x*scaleX - width/2f - anchorOffset.x,
                                                      y*scaleY - length/2f - anchorOffset.y, 0.0f);
                    }
                    uvs[index++] = new Vector2(x*uvFactorX, y*uvFactorY);
                }
            }

            index = 0;
            for (var y = 0; y < lengthSegments; y++)
            {
                for (var x = 0; x < widthSegments; x++)
                {
                    triangles[index] = (y*hCount2) + x;
                    triangles[index + 1] = ((y + 1)*hCount2) + x;
                    triangles[index + 2] = (y*hCount2) + x + 1;

                    triangles[index + 3] = ((y + 1)*hCount2) + x;
                    triangles[index + 4] = ((y + 1)*hCount2) + x + 1;
                    triangles[index + 5] = (y*hCount2) + x + 1;
                    index += 6;
                }
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = triangles;
            m.RecalculateNormals();

            m.RecalculateBounds();

            return m;
        }
    }
}