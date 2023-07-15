using System;
using SDL2;


namespace SDLBase
{
    public class SDLProgram
    {
        public static void Main()
        {
            SDLApp app = new SDLApp(512, 512, "SDL Base Application");

            app.Initialize();

            RunLines(app);

            app.Shutdown();
        }

        static float RangeRandom(Random rnd, float min, float max)
        {
            return min + (float)rnd.NextDouble() * (max - min);
        }

        static void RunLines(SDLApp app)
        {
            Bitmap screen = app.GetScreen();

            var rnd = new Random();

            Vector2 p0 = new Vector2(rnd.Next() % 512, rnd.Next() % 512);
            Vector2 p1 = new Vector2(rnd.Next() % 512, rnd.Next() % 512);
            Vector2 i0 = new Vector2(RangeRandom(rnd, -1.0f, 1.0f), RangeRandom(rnd, -1.0f, 1.0f));
            Vector2 i1 = new Vector2(RangeRandom(rnd, -1.0f, 1.0f), RangeRandom(rnd, -1.0f, 1.0f));

            Color[] colors = new Color[]
            {
                Color.red,
                Color.green,
                Color.blue,
                Color.cyan,
                Color.yellow,
                Color.magenta
            };

            int     colorIndex = 0;
            int     frameCount = 0;
            int     framesPerColor = 100;

            app.Run(() =>
            {
                frameCount++;
                if (frameCount > framesPerColor)
                {
                    frameCount = 0;
                    colorIndex = (colorIndex + 1) % colors.Length;
                }
                var c1 = colors[colorIndex];
                var c2 = colors[(colorIndex + 1) % colors.Length];
                float t = (float)frameCount / (float)framesPerColor;

                screen.Line(p0, p1, (Color32)Color.Lerp(c1, c2, t));

                p0 += i0;
                if (p0.x < 0) { p0.x = 0; i0.x = -i0.x; }
                else if (p0.x > 511) { p0.x = 511; i0.x = -i0.x; }
                if (p0.y < 0) { p0.y = 0; i0.y = -i0.y; }
                else if (p0.y > 511) { p0.y = 511; i0.y = -i0.y; }

                p1 += i1;
                if (p1.x < 0) { p1.x = 0; i1.x = -i1.x; }
                else if (p1.x > 511) { p1.x = 511; i1.x = -i1.x; }
                if (p1.y < 0) { p1.y = 0; i1.y = -i1.y; }
                else if (p1.y > 511) { p1.y = 511; i1.y = -i1.y; }

            });

        }
    }
}
