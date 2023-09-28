
using System.Drawing;
using System.Numerics;
using System;
using System.Reflection.Emit;

namespace SDLBase
{

    public class Bitmap
    {
        public Color32[] data;
        public int width, height;

        public Bitmap()
        {
            width = height = 0;
        }

        public Bitmap(int width, int height)
        {
            this.width = width;
            this.height = height;
            data = new Color32[width * height];
        }

        public void Clear(Color32 color)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
        }

        public void Rect(Vector2 p1, Vector2 p2, Color32 color)
        {
            int x1 = (int)p1.x;
            int y1 = (int)p1.y;
            int x2 = (int)p2.x;
            int y2 = (int)p2.y;

            for (int y = y1; y < y2; y++)
            {
                int iStart = (x1 + y * width) * 4;
                int iEnd = (x2 + y * width) * 4;
                for (int i = iStart; i < iEnd; i++)
                {
                    data[i] = color;
                }
            }
        }

        public void VerticalLine(int x, int y1, int y2, Color32 color)
        {
            int i = x + y1 * width;
            for (int y = y1; y < y2; y++)
            {
                data[i] = color;

                i += width;
            }
        }

        public void HorizontalLine(int x1, int x2, int y, Color32 color)
        {
            int i = x1 + y * width;
            for (int x = x1; x < x2; x++)
            {
                data[i] = color;
                i++;
            }
        }

        // Default line rendering, uses optimized DDA
        public void Line(Vector2 p1, Vector2 p2, Color32 color) => LineOptimizedDDA(p1, p2, color);

        // Line drawing with DDA
        public void LineDDA(Vector2 p1, Vector2 p2, Color32 color)
        {
            int x1 = (int)p1.x;
            int y1 = (int)p1.y;
            int x2 = (int)p2.x;
            int y2 = (int)p2.y;

            int dx = x2 - x1;
            int dy = y2 - y1;
            int n_steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            float x = p1.x;
            float y = p1.y;
            float incX = (float)dx / n_steps;
            float incY = (float)dy / n_steps;
            int   i;

            for (int j = 0; j <= n_steps; j++)
            {
                i = (int)x + (int)y * width;
                data[i] = color;
                x = x + incX;
                y = y + incY;
            }
        }

        // Optimized DDA line rendering
        public void LineOptimizedDDA(Vector2 p1, Vector2 p2, Color32 color)
        {
            int x1 = (int)p1.x;
            int y1 = (int)p1.y;
            int x2 = (int)p2.x;
            int y2 = (int)p2.y;

            float dx = Math.Abs(x2 - x1);
            float dy = Math.Abs(y2 - y1);

            if (dx > dy)
            {
                // Iterate on horizontal - Swap to always go from left to right
                int tmp;
                if (x2 < x1)
                {
                    tmp = x1; x1 = x2; x2 = tmp;
                    tmp = y1; y1 = y2; y2 = tmp;
                }

                float y = y1;
                float incY = (y2 - y1) / (float)(x2 - x1);
                int i;
                for (int x = x1; x <= x2; x++)
                {
                    i = x + (int)y * width;
                    data[i] = color;
                    y = y + incY;
                }
            }
            else
            {
                // Iterate on vertical - Swap to always go from top to bottom
                int tmp;
                if (y2 < y1)
                {
                    tmp = x1; x1 = x2; x2 = tmp;
                    tmp = y1; y1 = y2; y2 = tmp;
                }

                float x = x1;
                float incX = (x2 - x1) / (float)(y2 - y1);
                int i;
                for (int y = y1; y <= y2; y++)
                {
                    i = (int)x + y * width;
                    data[i] = color;
                    x = x + incX;
                }
            }
        }

        // Bresenham line rendering
        public void LineBresenham(Vector2 p1, Vector2 p2, Color32 color)
        {
            int x1 = (int)p1.x;
            int y1 = (int)p1.y;
            int x2 = (int)p2.x;
            int y2 = (int)p2.y;

            int dx = x2 - x1;
            int dy = y2 - y1;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                // Iterate on horizontal - Swap to always go from left to right
                int tmp;
                if (x2 < x1)
                {
                    tmp = x1; x1 = x2; x2 = tmp;
                    tmp = y1; y1 = y2; y2 = tmp;
                }

                int y = y1;
                int D = 2 * dy - dx;
                int i;
                for (int x = x1; x <= x2; x++)
                {
                    i = x + y * width;
                    data[i] = color;
                    if (D > 0)
                    {
                        D = D - 2 * dx;
                        y++;
                    }
                    D = D + 2 * dy;
                }
            }
            else
            {
                // Iterate on vertical - Swap to always go from top to bottom
                int tmp;
                if (y2 < y1)
                {
                    tmp = x1; x1 = x2; x2 = tmp;
                    tmp = y1; y1 = y2; y2 = tmp;
                }

                int x = x1;
                int D = 2 * dx - dy;
                int i;
                for (int y = y1; y <= y2; y++)
                {
                    i = x + y * width;
                    data[i] = color;
                    if (D > 0)
                    {
                        D = D - 2 * dy;
                        x++;
                    }
                    D = D + 2 * dx;
                }
            }
        }

        // Line rendering, clipped with Liang-Barsky
        public void LineClippedCS(Vector2 p1, Vector2 p2, Color32 color)
        {
            const int Inside = 0;
            const int Left = 1;
            const int Right = 2;
            const int Bottom = 4;
            const int Top = 8;

            uint ComputeOutCode(Vector2 p)
            {
                uint ret = Inside;
                if (p.x < 0.0f) ret |= Left;
                else if (p.x >= width) ret |= Right;
                if (p.y < 0.0f) ret |= Top;
                else if (p.y >= height) ret |= Bottom;

                return ret;
            }

            bool CohenSutherland(ref Vector2 p1, ref Vector2 p2)
            {
                uint outcode1 = ComputeOutCode(p1);
                uint outcode2 = ComputeOutCode(p2);
                int xmin = 0;
                int xmax = width - 1;
                int ymin = 0;
                int ymax = height - 1;

                bool accept = false;

                while (true)
                {
                    if ((outcode1 | outcode2) == Inside)
                    {
                        // Line is completely inside
                        Line(p1, p2, color);
                        accept = true;
                        break;
                    }
                    if ((outcode1 & outcode2) != 0)
                    {
                        // Line is completely outside
                        break;
                    }
                    // Line is partially outside
                    uint outcodeOut = (outcode1 > outcode2) ? (outcode1) : (outcode2);
                    Vector2 p = Vector2.zero;

                    if ((outcodeOut & Bottom) != 0)
                    {
                        p.x = p1.x + (p2.x - p1.x) * (ymax - p1.y) / (p2.y - p1.y);
                        p.y = ymax;
                    }
                    else if ((outcodeOut & Top) != 0)
                    {
                        p.x = p1.x + (p2.x - p1.x) * (ymin - p1.y) / (p2.y - p1.y);
                        p.y = ymin;
                    }
                    else if ((outcodeOut & Right) != 0)
                    {
                        p.y = p1.y + (p2.y - p1.y) * (xmax - p1.x) / (p2.x - p1.y);
                        p.x = xmax;
                    }
                    else if ((outcodeOut & Left) != 0)
                    {
                        p.y = p1.y + (p2.y - p1.y) * (xmin - p1.x) / (p2.x - p1.y);
                        p.x = xmin;
                    }

                    if (outcodeOut == outcode1)
                    {
                        p1 = p;
                        outcode1 = ComputeOutCode(p1);
                    }
                    else
                    {
                        p2 = p;
                        outcode2 = ComputeOutCode(p2);
                    }
                }

                return accept;
            }

            if (CohenSutherland(ref p1, ref p2))
            {
                Line(p1, p2, color);
            }
        }

        // Line rendering, clipped with Liang-Barsky
        public void LineClippedLB(Vector2 p1, Vector2 p2, Color32 color)
        {
            bool is_inside1 = (p1.x > 0) && (p1.x < width) && (p1.y > 0) && (p1.y < height);
            bool is_inside2 = (p2.x > 0) && (p2.x < width) && (p2.y > 0) && (p2.y < height);
            // Check if the line needs clipping
            if (is_inside1 && is_inside2)
            {
                // It's completely inside the viewport
                Line(p1, p2, color);
            }
            else
            {
                var pStart = p1;
                var pEnd = p2;

                // Both points outside
                if (!is_inside1 && !is_inside2)
                {
                    if ((p1.x < 0) && (p2.x < 0)) return;
                    if ((p1.y < 0) && (p2.y < 0)) return;
                    if ((p1.x >= width) && (p2.x >= width)) return;
                    if ((p1.y >= height) && (p2.y >= height)) return;
                }
                if (!is_inside1)
                {
                    Vector2 delta = p1 - p2;

                    // Find intersection of line with the 4 boundaries
                    float t = 1.0f;
                    float test = (0 - p2.x) / delta.x;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);
                    test = ((width - 1) - p2.x) / delta.x;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);
                    test = (0 - p2.y) / delta.y;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);
                    test = ((height - 1) - p2.y) / delta.y;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);

                    pStart = p2 + t * delta;
                }
                if (!is_inside2)
                {
                    Vector2 delta = p2 - p1;

                    // Find intersection of line with the 4 boundaries
                    float t = 1.0f;
                    float test = (0 - p1.x) / delta.x;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);
                    test = ((width - 1) - p1.x) / delta.x;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);
                    test = (0 - p1.y) / delta.y;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);
                    test = ((height - 1) - p1.y) / delta.y;
                    t = (test >= 0) ? (Math.Min(t, test)) : (t);

                    pEnd = p1 + t * delta;
                }

                // Recheck if points are inside
                if ((pStart.x < 0) || (pStart.x >= width) || (pStart.y < 0) || (pStart.y >= height) ||
                    (pEnd.x < 0) || (pEnd.x >= width) || (pEnd.y < 0) || (pEnd.y >= height))
                {
                    // Clipped line is completely outside
                    return;
                }
                else
                {
                    Line(pStart, pEnd, color);
                }
            }
        }

        public void TriangleScanline(Vector2 p0, Vector2 p1, Vector2 p2, Color32 color)
        {
            Vector2[] p = new Vector2[] { p0, p1, p2 };

            // Find smallest Y
            int minIndexY = 0;
            if (p[minIndexY].y > p[1].y) minIndexY = 1;
            if (p[minIndexY].y > p[2].y) minIndexY = 2;

            // Find edge X
            int minIndexX, maxIndexX;
            minIndexX = maxIndexX = -1;

            for (int i = 0; i < 3; i++)
            {
                if (i == minIndexY) continue;
                if ((minIndexX == -1) || (p[minIndexX].x > p[i].x)) minIndexX = i;
                if ((maxIndexX == -1) || (p[maxIndexX].x < p[i].x)) maxIndexX = i;
            }

            // Find Y limits
            int midIndexY, maxIndexY;
            if (p[minIndexX].y < p[maxIndexX].y)
            {
                midIndexY = minIndexX;
                maxIndexY = maxIndexX;
            }
            else
            {
                midIndexY = maxIndexX;
                maxIndexY = minIndexX;
            }

            // We start at minY and go down on both edges minX and maxX
            // Then, at midY, we recompute one of the edges that's not midY and minY
            int y1 = (int)p[minIndexY].y;
            int y2 = (int)p[midIndexY].y;
            int y3 = (int)p[maxIndexY].y;

            float minX, maxX;
            minX = maxX = p[minIndexY].x;

            float incMinX = (p[minIndexX].x - minX) / (p[minIndexX].y - p[minIndexY].y);
            float incMaxX = (p[maxIndexX].x - maxX) / (p[maxIndexX].y - p[minIndexY].y);

            bool earlyOut = false;
            if (y2 > height) { y2 = height - 1; earlyOut = true; }

            // Special case: horizontal edge on top
            if ((int)p[minIndexY].y == (int)p[minIndexX].y)
            {
                earlyOut = true;
                midIndexY = maxIndexX;
                y2 = (int)Math.Min(p[midIndexY].y + 1, height - 1);
                if (p[minIndexY].x < p[minIndexX].x) { minX = p[minIndexY].x; maxX = p[minIndexX].x; }
                else { minX = p[minIndexX].x; maxX = p[minIndexY].x; }

                incMinX = (p[midIndexY].x - minX) / (p[midIndexY].y - p[minIndexY].y);
                incMaxX = (p[midIndexY].x - maxX) / (p[midIndexY].y - p[minIndexY].y);
            }
            else if ((int)p[minIndexY].y == (int)p[maxIndexX].y)
            {
                earlyOut = true;
                midIndexY = minIndexX;
                y2 = (int)Math.Min(p[midIndexY].y + 1, height - 1);
                if (p[minIndexY].x < p[maxIndexX].x) { minX = p[minIndexY].x; maxX = p[maxIndexX].x; }
                else { minX = p[maxIndexX].x; maxX = p[minIndexY].x; }

                incMinX = (p[midIndexY].x - minX) / (p[midIndexY].y - p[minIndexY].y);
                incMaxX = (p[midIndexY].x - maxX) / (p[midIndexY].y - p[minIndexY].y);
            }

            for (int y = y1; y < y2; y++)
            {
                if (y >= 0)
                {
                    // Fill span
                    int m1 = (minX >= 0) ? ((int)minX) : (0);
                    int m2 = (maxX < width) ? ((int)maxX) : (width - 1);

                    int idx = y * width + m1;
                    for (int x = m1; x <= m2; x++)
                    {
                        data[idx] = color;
                        idx++;
                    }
                }

                minX = minX + incMinX;
                maxX = maxX + incMaxX;
            }

            // Out of the bottom of the screen, no point in more calculations
            if (earlyOut) return;

            if (minIndexX == midIndexY)
            {
                incMinX = (p[maxIndexX].x - minX) / (y3 - y2);
            }
            else
            {
                incMaxX = (p[minIndexX].x - maxX) / (y3 - y2);
            }

            if (y3 >= height) y3 = height - 1;

            for (int y = y2; y <= y3; y++)
            {
                if (y >= 0)
                {
                    // Fill span
                    int m1 = (minX >= 0) ? ((int)minX) : (0);
                    int m2 = (maxX < width) ? ((int)maxX) : (width - 1);

                    int idx = y * width + m1;
                    for (int x = m1; x <= m2; x++)
                    {
                        data[idx] = color;
                        idx++;
                    }
                }

                minX = minX + incMinX;
                maxX = maxX + incMaxX;
            }
        }

        // Draw a filled ellipse using a scanline algorithm
        public void EllipseFill(Vector2 center, Vector2 radius, Color32 color)
        {
            // Convert center to integers
            int cx = (int)center.x;
            int cy = (int)center.y;

            // Compute top and bottom
            int yTop = cy - (int)radius.y;
            int yBottom = cy + (int)radius.y;

            // Outside of screen
            if (yTop >= height) return;
            if (yBottom < 0) return;

            // Clip to screen
            if (yTop < 0) yTop = 0;
            if (yBottom >= height) yBottom = height - 1;

            // For all scanlines on the circle
            for (int y = yTop; y <= yBottom; y++)
            {
                // Find out the radius at this height
                // Remember that x^2 + y^2 = 1
                float   tmpY = (y - cy) / radius.y; tmpY *= tmpY;
                int     dx = (int)(radius.x * MathF.Sqrt(1 - tmpY));

                // Define bounds and clip them to screen edges
                int xStart = Math.Max(0, cx - dx);
                int xEnd = Math.Min(width - 1, cx + dx);

                // Draw an horizontal line between start and end
                for (int x = xStart; x <= xEnd; x++)
                {
                    int index = x + y * width;
                    data[index] = color;
                }
            }
        }

        // Draw a filled rotated ellipse with rotation using a fill algorithm
        public void EllipseFill(Vector2 center, Vector2 radius, float rotation, Color32 color)
        {
            // Compute radius squared
            float rx2 = radius.x * radius.x;
            float ry2 = radius.y * radius.y;

            // Figure out the bounds of the rotated ellipse
            // Get corners of the rectangle of the ellipse (unrotated) 
            Vector2[] corners = new Vector2[4] { new Vector2(-radius.x, -radius.y),
                                                 new Vector2(-radius.x, +radius.y),
                                                 new Vector2(+radius.x, -radius.y),
                                                 new Vector2(+radius.x, +radius.y) };
            // Rotate points
            for (int i = 0; i < 4; i++) corners[i].Rotate(rotation);
            // Find the min/max of the coordinates
            int minX = (int)(MathF.Min(corners[0].x, MathF.Min(corners[1].x, MathF.Min(corners[2].x, corners[3].x))) + center.x);
            int maxX = (int)(MathF.Max(corners[0].x, MathF.Max(corners[1].x, MathF.Max(corners[2].x, corners[3].x))) + center.x);
            int minY = (int)(MathF.Min(corners[0].y, MathF.Min(corners[1].y, MathF.Min(corners[2].y, corners[3].y))) + center.y);
            int maxY = (int)(MathF.Max(corners[0].y, MathF.Max(corners[1].y, MathF.Max(corners[2].y, corners[3].y))) + center.y);

            // There is an analytical way to find this, based on the fact that the equation for the 
            // ellipse is (x - cx)^2/rx^2 + (y - cy)^2/ry^2 = 1
            // Solve it for the corners, and you get the code below:
            // float xFactor1 = radius.x * MathF.Cos(rotation); xFactor1 *= xFactor1;
            // float xFactor2 = radius.y * MathF.Sin(rotation); xFactor2 *= xFactor2;
            // float xFactor = MathF.Sqrt(xFactor1 + xFactor2);
            // minX = (int)(center.x - xFactor);
            // maxX = (int)(center.x + xFactor);
            // float yFactor1 = radius.x * MathF.Sin(rotation); yFactor1 *= yFactor1;
            // float yFactor2 = radius.y * MathF.Cos(rotation); yFactor2 *= yFactor2;
            // float yFactor = MathF.Sqrt(yFactor1 + yFactor2);
            // minY = (int)(center.y - yFactor);
            // maxY = (int)(center.y + yFactor);

            // Clip bounds to screen
            if (minY < 0) minY = 0;
            if (maxY < 0) return;
            if (minX < 0) minX = 0;
            if (maxX < 0) return;
            if (minY >= height) return;
            if (maxY >= height) maxY = height - 1;
            if (minX >= width) return;
            if (maxX >= width) maxX = width - 1;

            // Get ellipse axis
            Vector2 axisX = new Vector2(radius.x, 0); axisX.Rotate(rotation);
            Vector2 axisY = new Vector2(0, radius.y); axisY.Rotate(rotation);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // Now check if (x,y) is within the rotated ellipse
                    // Compute vector from center to (x,y)
                    Vector2 d = new Vector2(x, y) - center;
                    // Project d onto both axis
                    float dx = Vector2.Dot(d, axisX) / axisX.magnitude;
                    float dy = Vector2.Dot(d, axisY) / axisY.magnitude;

                    // Solve the ellipse equation
                    float r = (dx * dx / rx2) + (dy * dy / ry2);

                    // Check if inside
                    if (r <= 1)
                    {
                        int index = x + y * width;
                        data[index] = color;
                    }
                }
            }
        }
    }
}
