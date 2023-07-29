﻿
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
            int i;

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

        public void TriangleHalfspace(Vector2 p0, Vector2 p1, Vector2 p2, 
                                      Color32 c0, Color32 c1, Color32 c2)
        {
            // Get bounding rectangle
            int xMin = (int)Math.Min(p0.x, Math.Min(p1.x, p2.x));
            int xMax = (int)Math.Max(p0.x, Math.Max(p1.x, p2.x));
            int yMin = (int)Math.Min(p0.y, Math.Min(p1.y, p2.y));
            int yMax = (int)Math.Max(p0.y, Math.Max(p1.y, p2.y));

            // Clip to screen
            xMin = Math.Max(0, Math.Min(xMin, width - 1));
            xMax = Math.Max(0, Math.Min(xMax, width - 1));
            yMin = Math.Max(0, Math.Min(yMin, height - 1));
            yMax = Math.Max(0, Math.Min(yMax, height - 1));

            // Get edge vectors
            Vector2 e0 = p1 - p0;
            Vector2 e1 = p2 - p0;
            float dot00 = Vector2.Dot(e0, e0);
            float dot01 = Vector2.Dot(e0, e1);
            float dot11 = Vector2.Dot(e1, e1);
            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);

            Vector2 currentPoint = new Vector2();

            // Convert color to floating point - We can't do this directly on
            // the Color32 because of the limited precision of bytes
            Color color0 = (Color)c0;
            Color color1 = (Color)c1;
            Color color2 = (Color)c2;

            // Other variables we need in iteration
            Vector2 p;
            int     idx;
            float   dot02, dot12;
            float   u, v;
            Color32 color;
            // For all points in rectangle
            for (int y = yMin; y <= yMax; y++)
            {
                currentPoint.y = y;
                for (int x = xMin; x <= xMax; x++)
                {
                    currentPoint.x = x;

                    p = currentPoint - p0;
                    dot02 = Vector2.Dot(e0, p);
                    dot12 = Vector2.Dot(e1, p);
                    u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                    v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                    if ((u >= 0) && (v >= 0) && (u + v < 1))
                    {
                        idx = x + y * width;
                        color = (Color32)(color0 + u * (color1 - color0) + v * (color2 - color0));
                        data[idx] = color;
                    }
                }
            }
        }
    }
}
