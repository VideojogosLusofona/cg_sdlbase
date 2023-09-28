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

            DrawSkewedCircles(app);

            app.Shutdown();
        }

        static void DrawCircles(SDLApp app)
        {
            Bitmap screen = app.GetScreen();

            app.Run(() =>
            {
                screen.EllipseFill(new Vector2(200, 200), new Vector2(100, 100), new Color32(255, 255, 0, 255));
                screen.EllipseFill(new Vector2(350, 200), new Vector2(50, 100), new Color32(255, 0, 0, 255));
                screen.EllipseFill(new Vector2(250, 400), new Vector2(100, 50), new Color32(0, 255, 0, 255));
            });

        }
        static void DrawSkewedCircles(SDLApp app)
        {
            Bitmap screen = app.GetScreen();

            float angle = 0;

            app.Run(() =>
            {
                screen.Clear(new Color32(0, 0, 0, 255));

                screen.EllipseFill(new Vector2(200, 200), new Vector2(50, 100), angle, new Color32(255, 255, 0, 255));
                screen.EllipseFill(new Vector2(200, 200), new Vector2(40, 90), angle, new Color32(255, 0, 0, 255));
                angle += 0.01f;
            });

        }
    }
}
