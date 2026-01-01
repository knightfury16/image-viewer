using Silk.NET.Windowing;

namespace ImageViewer;

class Program
{
    private static IWindow? _window;

    static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Silk.NET.Maths.Vector2D<int>(800, 600),
            Title = "Image Viewer"
        };

        _window = Window.Create(options);

        _window.Run();
    }
}
