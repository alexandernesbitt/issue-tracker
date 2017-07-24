using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARUP.IssueTracker.Bentley
{
    /// <summary>
    /// Source: https://github.com/cansik/LongLiveTheSquare
    /// </summary>
    public class ConvexHullHelper
    {
        /// <summary>
        /// Calculates the convex hull with the monotone chain approach.
        /// </summary>
        /// <returns>The chain convex hull.</returns>
        /// <param name="points">Points.</param>
        public static Vector2d[] MonotoneChainConvexHull(Vector2d[] points)
        {
            //sort vectors
            Array.Sort<Vector2d>(points);
            var hullPoints = new Vector2d[2 * points.Length];

            //break if only one point as input
            if (points.Length <= 1)
                return points;

            int pointLength = points.Length;
            int counter = 0;

            //iterate for lowerHull
            for (var i = 0; i < pointLength; ++i)
            {
                while (counter >= 2 && Cross(hullPoints[counter - 2],
                           hullPoints[counter - 1],
                           points[i]) <= 0)
                    counter--;
                hullPoints[counter++] = points[i];
            }

            //iterate for upperHull
            for (int i = pointLength - 2, j = counter + 1; i >= 0; i--)
            {
                while (counter >= j && Cross(hullPoints[counter - 2],
                           hullPoints[counter - 1],
                           points[i]) <= 0)
                    counter--;
                hullPoints[counter++] = points[i];
            }

            //remove duplicate start points
            var result = new Vector2d[counter - 1];
            Array.Copy(hullPoints, 0, result, 0, counter - 1);
            return result;
        }

        /// <summary>
        /// Cross the specified o, a and b.
        /// Zero if collinear
        /// Positive if counter-clockwise
        /// Negative if clockwise
        /// </summary>
        /// <param name="o">O.</param>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        public static double Cross(Vector2d o, Vector2d a, Vector2d b)
        {
            return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }
    }
}
