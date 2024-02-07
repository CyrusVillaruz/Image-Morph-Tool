using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Image_Morph_Tool.Utils
{
    /**
     * Contains extra methods for the System.Windows Vector class
     */
    public static class VectorUtils
    {
        public static Vector Lerp(this Vector a, Vector b, float interp)
        {
            return new Vector()
            {
                X = a.X + (b.X - a.X) * interp,
                Y = a.Y + (b.Y - a.Y) * interp
            };
        }

        public static double Dot(this Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector Perpendicular(this Vector a)
        {
            return new Vector(a.Y, -a.X);
        }

        public static bool IsInRectangle(this Vector a, Vector rectMin, Vector rectMax)
        {
            return a.X > rectMin.X && a.X < rectMax.X && a.Y > rectMin.Y && a.Y < rectMax.Y;
        }

        public static Vector ClampToImageArea(this Vector v)
        {
            return new Vector(v.X < 0 ? 0 : v.X > 1 ? 1 : v.X,
                              v.Y < 0 ? 0 : v.Y > 1 ? 1 : v.Y);
        }
    }
}
