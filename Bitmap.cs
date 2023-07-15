
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
}
