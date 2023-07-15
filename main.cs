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
                screen.Clear(Color32.black);

                screen.LineDDA(new Vector2(50, 100), new Vector2(412, 200), Color32.yellow);
            });

            app.Shutdown();
        }
    }
}
