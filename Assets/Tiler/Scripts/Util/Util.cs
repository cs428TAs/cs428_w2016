using System;
using System.IO;
using UnityEngine;

namespace TileDraw
{
    public static class Util
    {
        public static T[,] ResizeArray<T>(T[,] oldArray, int size)
        {
            if (oldArray == null)
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                System.Reflection.MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                throw new UnityException("Array is null. Called from: " + methodBase.Name); // e.g.
            }

            //Check which method called another method
            var oldSizeX = oldArray.GetLength(1);
            var oldSizeY = oldArray.GetLength(0);

            var newArray = new T[size,size];

            // If old array has a length of zero, just return a new array of max size and default values
            if (oldSizeX == 0 || oldSizeY == 0)
                return newArray;

            var xFactor = oldSizeX/(float) size;
            var yFactor = oldSizeY/(float) size;

            for (var x = 0; x < size; ++x)
                for (var y = 0; y < size; ++y)
                {
                    newArray[y, x] = oldArray[(int) Math.Floor(y*yFactor), (int) Math.Floor(x*xFactor)];
                }

            return newArray;
        }

        public static T[] ResizeArray<T>(T[] oldArray, int size, int oldWidth)
        {
            if (oldArray == null)
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                System.Reflection.MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                throw new UnityException("Array is null. Called from: " + methodBase.Name); // e.g.
            }

            if (oldArray.Length == 0)
                return new T[size*size];

            var oldSizeX = oldWidth;
            var oldSizeY = oldArray.Length/oldWidth;

            // No need to resize
            if (oldSizeX == size && oldSizeY == size)
                return oldArray;

            if (oldArray.Length%oldWidth > 0)
            {
                throw new UnityException("width figure is wrong");
            }

            var newArray = new T[size*size];

            var xFactor = oldSizeX / (float)size;
            var yFactor = oldSizeY / (float)size;

            for (var x = 0; x < size; ++x)
                for (var y = 0; y < size; ++y)
                {
                    newArray[y * size + x] = oldArray[(int)Math.Floor(y * yFactor)*oldSizeX + (int)Math.Floor(x * xFactor)];
                }

            return newArray;
        }
        public static T[] ResizeArray<T>(T[] oldArray, int size)
        {
            if (oldArray == null)
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                System.Reflection.MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                throw new UnityException("Array is null. Called from: " + methodBase.Name); // e.g.
            }

            if (oldArray.Length == 0)
                return new T[size * size];

            var oldWidth = (int)Math.Sqrt(oldArray.Length);
            
            // No need to resize
            if (oldWidth == size)
                return oldArray;

            if (oldArray.Length % oldWidth > 0)
            {
                throw new UnityException("width figure is wrong");
            }

            var newArray = new T[size * size];

            var nFactor = oldWidth / (float)size;

            for (var x = 0; x < size; ++x)
                for (var y = 0; y < size; ++y)
                {
                    newArray[y * size + x] = oldArray[(int)Math.Floor(y * nFactor) * oldWidth + (int)Math.Floor(x * nFactor)];
                }

            return newArray;
        }


        public static T[] InitilizeArray<T>(int size, T value)
        {
            var array = new T[size*size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    array[y*size + x] = value;
                }
            }

            return array;
        }

        public static Vector3 RoundToInt(Vector3 vec3)
        {
            vec3.x = Mathf.RoundToInt(vec3.x);
            vec3.y = Mathf.RoundToInt(vec3.y);
            vec3.z = Mathf.RoundToInt(vec3.z);

            return vec3;
        }
        public static float RoundTo(float f, float i)
        {
            f *= 1/i;
            f = (float)Math.Round(f, MidpointRounding.AwayFromZero);
            f *= i;
            return f;
        }

        public static void SaveTextureToFile(Texture2D texture, string path)
        {
            var bytes = texture.EncodeToPNG();
            using (var file = File.Open(path, FileMode.Create))
            {
                var binary = new BinaryWriter(file);
                binary.Write(bytes);
            }
        }
        public static void SaveTextureToFile(byte[] bytes, string path)
        {
            using (var file = File.Open(path, FileMode.Create))
            {
                var binary = new BinaryWriter(file);
                binary.Write(bytes);
            }
        }

        public static bool[,] MergeArrays(bool[,][] arrays, int lenInner)
        {
            var len = arrays.GetLength(0);

            if (len == 0) return new bool[0,0];

            var totalLen = len*lenInner;

            var output = new bool[totalLen,totalLen];

            for (var yO = 0; yO < len; yO++)
            {
                var yOffset = yO*lenInner;
                for (var xO = 0; xO < len; xO++)
                {
                    var xOffset = xO*lenInner;

                    var iArray = arrays[yO, xO];
                    var rArray = ResizeArray(iArray, lenInner);

                    for (var yI = 0; yI < lenInner; yI++)
                    {
                        for (var xI = 0; xI < lenInner; xI++)
                        {
                            output[yOffset + yI, xOffset + xI] = rArray[yI * lenInner + xI];
                        }

                    }


                }
            }
            return output;
        }
    }
}