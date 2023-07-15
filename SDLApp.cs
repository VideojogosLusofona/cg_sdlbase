
using SDL2;
using System;

namespace SDLBase
{
    public class SDLApp
    {
        private int     resX = 512;
        private int     resY = 512;
        private string  title = "Test App";

        private IntPtr  window;
        private IntPtr  windowSurface;
        private IntPtr  renderer;
        private Bitmap  screen;
        private bool    exit = false;

        public SDLApp(int resX, int resY, string title)
        {
            this.resX = resX;
            this.resY = resY;
            this.title = title;
        }

        public bool Initialize()
        {
            // Initialize all modules of SDL
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0)
            {
                Console.WriteLine("Can't initialize SDL!");
                return false;
            }

            // Create a window, centered on the default screen, with the given resolution
            window = SDL.SDL_CreateWindow("SDL Base Application", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, resX, resY, 0);
            if (window == IntPtr.Zero)
            {
                Console.WriteLine("Can't create window or renderer!");
                return false;
            }

            // Get the window surface - We'll need this to copy our data at the end
            windowSurface = SDL.SDL_GetWindowSurface(window);
            // Create a software based renderer. We could create an accelerated one for a series of reasons,
            // but for our test case we want to have full control over the pixels
            renderer = SDL.SDL_CreateSoftwareRenderer(windowSurface);

            // Create buffer for the screen
            screen = new Bitmap(resX, resY);

            return true;
        }

        public void Shutdown()
        {
            // Shutdown SDL
            SDL.SDL_Quit();
        }

        public void Run(Action mainLoopFunction)
        {
            // Set exit to true to exit application
            exit = false;
            while (!exit)
            {
                // Handle all events in the qeueu
                SDL.SDL_Event evt;
                while (SDL.SDL_PollEvent(out evt) != 0)
                {
                    switch (evt.type)
                    {
                        // Quit event => Pressing the Close button on a window or Alt-F4: in this case, just set the exit flag to true
                        case SDL.SDL_EventType.SDL_QUIT:
                            exit = true;
                            break;
                        //  Key presses
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if ((evt.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) && ((evt.key.keysym.mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0))
                            {
                                // If Shift-ESC is pressed, set the exit flag to true to exit the application
                                exit = true;
                            }
                            break;
                    }
                }

                // Run the main loop function (where the actual application is running)
                mainLoopFunction();

                // Copy the contents of the screen buffer to the actual main screen
                CopyToSurface(screen, windowSurface);

                // Update the window (takes the contents of the window surface and transfer it to the screen)
                SDL.SDL_UpdateWindowSurface(window);
            }
        }

        // Copy surface => This is intended to copy the image on the Bitmap (usually the screen buffer) to the 
        // main window.
        // This has to be unsafe since it works with memory copies and such, which is considered unsafe code by C#
        unsafe void CopyToSurface(Bitmap src, IntPtr dest)
        {
            // Get the window surface data. This is information like the size of the surface, and it's pitch (horizontal line size in bytes).
            var surfaceData = (SDL.SDL_Surface)System.Runtime.InteropServices.Marshal.PtrToStructure(dest, typeof(SDL.SDL_Surface));

            // Get destination data position
            var destData = surfaceData.pixels;

            // If the pitch size is the same as the width of the screen bitmap (times 4 bytes, RGBA), then the buffers are a 1 to 1 match and I can 
            // copy everything at the same time. Otherwise I have to do it line by line
            if (surfaceData.pitch == src.width * 4)
            {
                // Fixed will pin a certain area location in place while the block is run: we need to do this to make sure the C# 
                // runtime doesn't move the memory around while we're copying the buffer
                fixed (byte* srcData = &src.data[0])
                {
                    // Copy the buffer on the bitmap screen to the target location
                    Buffer.MemoryCopy(srcData, destData.ToPointer(), src.width * src.height * 4, src.width * src.height * 4);
                }
            }
            else
            {
                // Same as in the previous block, but it has to copy the buffer line by line, since pitch is different than
                // buffer width

                // Fixed will pin a certain area location in place while the block is run: we need to do this to make sure the C# 
                // runtime doesn't move the memory around while we're copying the buffer
                fixed (byte* srcData = &src.data[0])
                {
                    // Start from the first line
                    byte* srcLine = srcData;

                    // For each line in the source data
                    for (int y = 0; y < src.height; y++)
                    {
                        // Copy a whole line
                        Buffer.MemoryCopy(srcLine, destData.ToPointer(), src.width * 4, src.width * 4);

                        // Move down one line on both buffers
                        srcLine = srcLine + src.width;
                        destData = destData + surfaceData.pitch;
                    }
                }
            }
        }

        // This function returns the screen to be used by the main loop
        public Bitmap GetScreen() => screen;
    }
}
