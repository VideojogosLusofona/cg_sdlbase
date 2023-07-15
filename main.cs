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

            Bitmap screen = app.GetScreen();

            app.Run(() =>
            {
                screen.Clear(0, 0, 0, 255);

                screen.Rect(10, 10, 100, 200, 255, 255, 0, 255);

                screen.VerticalLine(200, 10, 200, 0, 255, 0, 255);

                screen.HorizontalLine(10, 100, 220, 0, 255, 255, 255);

                screen.Line(400, 400, 450, 500, 255, 0, 255, 255);
                screen.Line(450, 400, 400, 500, 255, 255, 255, 255);
                screen.Line(350, 425, 500, 475, 255, 0, 0, 255);
            });

            app.Shutdown();
        }
    }
}
