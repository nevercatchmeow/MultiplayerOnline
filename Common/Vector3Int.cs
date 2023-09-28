using System;

namespace Common
{
    public struct Vector3Int : IEquatable<Vector3Int>, IFormattable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new IndexOutOfRangeException("Invalid Vector3Int index addressed: " + index + "!")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3Int index addressed: " + index + "!");
                }
            }
        }

        public float magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public int sqrMagnitude => X * X + Y * Y + Z * Z;

        public static Vector3Int zero { get; } = new(0, 0, 0);

        public static Vector3Int one { get; } = new(1, 1, 1);
        public static Vector3Int up { get; } = new(0, 1, 0);

        public static Vector3Int down { get; } = new(0, -1, 0);

        public static Vector3Int left { get; } = new(-1, 0, 0);

        public static Vector3Int right { get; } = new(1, 0, 0);

        public static Vector3Int forward { get; } = new(0, 0, 1);

        public static Vector3Int back { get; } = new(0, 0, -1);

        public Vector3Int(int x, int y)
        {
            X = x;
            Y = y;
            Z = 0;
        }

        public Vector3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static float Distance(Vector3Int a, Vector3Int b)
        {
            return (a - b).magnitude;
        }

        public static Vector3Int Min(Vector3Int lhs, Vector3Int rhs)
        {
            return new Vector3Int(Math.Min(lhs.X, rhs.X), Math.Min(lhs.Y, rhs.Y), Math.Min(lhs.Z, rhs.Z));
        }

        public static Vector3Int Max(Vector3Int lhs, Vector3Int rhs)
        {
            return new Vector3Int(Math.Max(lhs.X, rhs.X), Math.Max(lhs.Y, rhs.Y), Math.Max(lhs.Z, rhs.Z));
        }

        public static Vector3Int Scale(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public void Scale(Vector3Int scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
        }

        public void Clamp(Vector3Int min, Vector3Int max)
        {
            X = Math.Max(min.X, X);
            X = Math.Min(max.X, X);
            Y = Math.Max(min.Y, Y);
            Y = Math.Min(max.Y, Y);
            Z = Math.Max(min.Z, Z);
            Z = Math.Min(max.Z, Z);
        }


        public static Vector3Int operator +(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }


        public static Vector3Int operator -(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }


        public static Vector3Int operator *(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }


        public static Vector3Int operator -(Vector3Int a)
        {
            return new Vector3Int(-a.X, -a.Y, -a.Z);
        }


        public static Vector3Int operator *(Vector3Int a, int b)
        {
            return new Vector3Int(a.X * b, a.Y * b, a.Z * b);
        }


        public static Vector3Int operator *(int a, Vector3Int b)
        {
            return new Vector3Int(a * b.X, a * b.Y, a * b.Z);
        }


        public static Vector3Int operator /(Vector3Int a, int b)
        {
            return new Vector3Int(a.X / b, a.Y / b, a.Z / b);
        }


        public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
        }


        public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3Int)) return false;

            return Equals((Vector3Int)other);
        }


        public bool Equals(Vector3Int other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            var hashCode = Y.GetHashCode();
            var hashCode2 = Z.GetHashCode();
            return X.GetHashCode() ^ (hashCode << 4) ^ (hashCode >> 28) ^ (hashCode2 >> 4) ^ (hashCode2 << 28);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "({0}, {1}, {2})", X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
    }
}