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

            DrawTriangle(app);

            app.Shutdown();
        }

        static void DrawTriangle(SDLApp app)
        {
            Bitmap screen = app.GetScreen();

            Color32 color = new Color32(255, 255, 0, 255);

            app.Run(() =>
            {
                screen.TriangleScanline(new Vector2(100, 100), new Vector2(200, 400), new Vector2(300, 200), color);
            });

        }
    }
}
