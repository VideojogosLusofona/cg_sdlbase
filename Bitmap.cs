
using System.Drawing;
using System.Numerics;
using System;

public class Bitmap
{
    public byte[]   data;
    public int      width, height;

    public Bitmap()
    {
        width = height = 0;
    }

    public Bitmap(int width, int height)
    {
        this.width = width;
        this.height = height;
        data = new byte[width * height * 4];
    }

    public void Clear(byte r, byte g, byte b, byte a)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = b;
            data[i + 1] = g;
            data[i + 2] = r;
            data[i + 3] = a;
        }
    }

    public void Rect(int x1, int y1, int x2, int y2, byte r, byte g, byte b, byte a)
    {
        for (int y = y1; y < y2; y ++)
        {
            int iStart = (x1 + y * width) * 4;
            int iEnd = (x2 + y * width) * 4;
            for (int i = iStart; i < iEnd; i += 4)
            {
                data[i] = b;
                data[i + 1] = g;
                data[i + 2] = r;
                data[i + 3] = a;
            }
        }
    }

    public void VerticalLine(int x, int y1, int y2, byte r, byte g, byte b, byte a)
    {
        int i= (x + y1 * width) * 4;
        for (int y = y1; y < y2; y++)
        {
            data[i] = b;
            data[i + 1] = g;
            data[i + 2] = r;
            data[i + 3] = a;

            i += (width * 4);
        }
    }

    public void HorizontalLine(int x1, int x2, int y, byte r, byte g, byte b, byte a)
    {
        int i = (x1 + y * width) * 4;
        for (int x = x1; x < x2; x++)
        {
            data[i] = b;
            data[i + 1] = g;
            data[i + 2] = r;
            data[i + 3] = a;
            i += 4;
        }
    }

    public void Line(int x1, int y1, int x2, int y2, byte r, byte g, byte b, byte a)
    {
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
            int   i;
            for (int x = x1; x <= x2; x++)
            {
                i = (x + (int)y * width) * 4;
                data[i] = b;
                data[i + 1] = g;
                data[i + 2] = r;
                data[i + 3] = a;
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
                i = ((int)x + y * width) * 4;
                data[i] = b;
                data[i + 1] = g;
                data[i + 2] = r;
                data[i + 3] = a;
                x = x + incX;
            }
        }
    }
}
