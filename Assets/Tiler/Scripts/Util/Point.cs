using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

/*
 * Copy of System.Drawing.Point
 * Since Unity doesn't include it
 * 
 */

namespace Tiler
{

    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    /// <filterpriority>1</filterpriority>
    [ComVisible(true)]
    [Serializable]

    public struct Point
    {
        /// <summary>
        /// Represents a <see cref="T:System.Drawing.Point"/> that has <see cref="P:System.Drawing.Point.X"/> and <see cref="P:System.Drawing.Point.Y"/> values set to zero.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public static readonly Point Empty = new Point();

        private int x;
        private int y;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:System.Drawing.Point"/> is empty.
        /// </summary>
        /// 
        /// <returns>
        /// true if both <see cref="P:System.Drawing.Point.X"/> and <see cref="P:System.Drawing.Point.Y"/> are 0; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        [Browsable(false)]
        public bool IsEmpty
        {
            get
            {
                if (this.x == 0)
                    return this.y == 0;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets or sets the x-coordinate of this <see cref="T:System.Drawing.Point"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The x-coordinate of this <see cref="T:System.Drawing.Point"/>.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Gets or sets the y-coordinate of this <see cref="T:System.Drawing.Point"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The y-coordinate of this <see cref="T:System.Drawing.Point"/>.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        static Point()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Drawing.Point"/> class with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal position of the point. </param><param name="y">The vertical position of the point. </param>
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Drawing.Point"/> class using coordinates specified by an integer value.
        /// </summary>
        /// <param name="dw">A 32-bit integer that specifies the coordinates for the new <see cref="T:System.Drawing.Point"/>. </param>
        public Point(int dw)
        {
            this.x = (int) (short) Point.LOWORD(dw);
            this.y = (int) (short) Point.HIWORD(dw);
        }


        /// <summary>
        /// Compares two <see cref="T:System.Drawing.Point"/> objects. The result specifies whether the values of the <see cref="P:System.Drawing.Point.X"/> and <see cref="P:System.Drawing.Point.Y"/> properties of the two <see cref="T:System.Drawing.Point"/> objects are equal.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="P:System.Drawing.Point.X"/> and <see cref="P:System.Drawing.Point.Y"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.
        /// </returns>
        /// <param name="left">A <see cref="T:System.Drawing.Point"/> to compare. </param><param name="right">A <see cref="T:System.Drawing.Point"/> to compare. </param><filterpriority>3</filterpriority>
        public static bool operator ==(Point left, Point right)
        {
            if (left.X == right.X)
                return left.Y == right.Y;
            else
                return false;
        }

        /// <summary>
        /// Compares two <see cref="T:System.Drawing.Point"/> objects. The result specifies whether the values of the <see cref="P:System.Drawing.Point.X"/> or <see cref="P:System.Drawing.Point.Y"/> properties of the two <see cref="T:System.Drawing.Point"/> objects are unequal.
        /// </summary>
        /// 
        /// <returns>
        /// true if the values of either the <see cref="P:System.Drawing.Point.X"/> properties or the <see cref="P:System.Drawing.Point.Y"/> properties of <paramref name="left"/> and <paramref name="right"/> differ; otherwise, false.
        /// </returns>
        /// <param name="left">A <see cref="T:System.Drawing.Point"/> to compare. </param><param name="right">A <see cref="T:System.Drawing.Point"/> to compare. </param><filterpriority>3</filterpriority>
        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public static Point operator -(Point left, Point right)
        {
            var p = new Point();
            p.x = left.x - right.x;
            p.y = left.y - right.y;

            return p;
        }

        public static Point operator +(Point left, Point right)
        {
            var p = new Point();
            p.x = left.x + right.x;
            p.y = left.y + right.y;

            return p;
        }

        public static Point operator +(Point left, int right)
        {
            var p = new Point();
            p.x = left.x + right;
            p.y = left.y + right;

            return p;
        }

        public static Point operator -(Point left, int right)
        {
            var p = new Point();
            p.x = left.x - right;
            p.y = left.y - right;

            return p;
        }

        public static Point operator /(Point left, int right)
        {
            var p = new Point();
            p.x = left.x/right;
            p.y = left.y/right;

            return p;
        }

        public static Point operator *(Point left, int right)
        {
            var p = new Point();
            p.x = left.x*right;
            p.y = left.y*right;

            return p;
        }


        /// <summary>
        /// Specifies whether this <see cref="T:System.Drawing.Point"/> contains the same coordinates as the specified <see cref="T:System.Object"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if <paramref name="obj"/> is a <see cref="T:System.Drawing.Point"/> and has the same coordinates as this <see cref="T:System.Drawing.Point"/>.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to test. </param><filterpriority>1</filterpriority>
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;
            Point point = (Point) obj;
            if (point.X == this.X)
                return point.Y == this.Y;
            else
                return false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="T:System.Drawing.Point"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An integer value that specifies a hash value for this <see cref="T:System.Drawing.Point"/>.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int GetHashCode()
        {
            return this.x ^ this.y;
        }

        /// <summary>
        /// Translates this <see cref="T:System.Drawing.Point"/> by the specified amount.
        /// </summary>
        /// <param name="dx">The amount to offset the x-coordinate. </param><param name="dy">The amount to offset the y-coordinate. </param><filterpriority>1</filterpriority>
        public void Offset(int dx, int dy)
        {
            this.X += dx;
            this.Y += dy;
        }

        /// <summary>
        /// Translates this <see cref="T:System.Drawing.Point"/> by the specified <see cref="T:System.Drawing.Point"/>.
        /// </summary>
        /// <param name="p">The <see cref="T:System.Drawing.Point"/> used offset this <see cref="T:System.Drawing.Point"/>.</param>
        public void Offset(Point p)
        {
            this.Offset(p.X, p.Y);
        }

        /// <summary>
        /// Converts this <see cref="T:System.Drawing.Point"/> to a human-readable string.
        /// </summary>
        /// 
        /// <returns>
        /// A string that represents this <see cref="T:System.Drawing.Point"/>.
        /// </returns>
        /// <filterpriority>1</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode"/></PermissionSet>
        public override string ToString()
        {
            return "{X=" + this.X.ToString((IFormatProvider) CultureInfo.CurrentCulture) + ",Y=" +
                   this.Y.ToString((IFormatProvider) CultureInfo.CurrentCulture) + "}";
        }

        private static int HIWORD(int n)
        {
            return n >> 16 & (int) ushort.MaxValue;
        }

        private static int LOWORD(int n)
        {
            return n & (int) ushort.MaxValue;
        }
    }
}