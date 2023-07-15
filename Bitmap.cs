
using System.Drawing;
using System.Numerics;
using System;

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
    }
}
