using System;
using UnityEngine;

namespace Tiler
{
    public enum Rotation
    {
        None,
        Clockwise90,
        Clockwise180,
        Clockwise270
    }

    [Serializable]
    public class TileProperties
    {
        // Because unity won't serialize longs we split it but keep it hidden.
        [SerializeField]
        private int _setID;
        [SerializeField]
        private int _tileID;

        public long ID
        {
            get { return ((long)_setID << 32) | (uint)_tileID; }
            private set
            {
                _tileID = (int)value;
                _setID = (int)(value >> 32);
            }
        }

        public Rotation Rot = Rotation.None;

        public TileProperties(long id)
        {
            ID = id;
        }

        public TileProperties(TileProperties t)
        {
            ID = t.ID;
            Rot = t.Rot;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _setID;
                hashCode = (hashCode * 397) ^ _tileID;
                hashCode = (hashCode * 397) ^ (int)Rot;
                return hashCode;
            }
        }

        protected bool Equals(TileProperties other)
        {
            return _setID == other._setID && _tileID == other._tileID && Rot == other.Rot;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TileProperties) obj);
        }

        public static bool operator ==(TileProperties left, TileProperties right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TileProperties left, TileProperties right)
        {
            return !Equals(left, right);
        }
    }
}
