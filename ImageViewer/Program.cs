using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;

namespace ImageViewer;

class Program
{
    private static IWindow? _window;
    private static GL? _gl;

    static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Silk.NET.Maths.Vector2D<int>(800, 600),
            Title = "Image Viewer"
        };

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Run();
    }

    private static void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.CornflowerBlue);
    }

    private static void OnUpdate(double deltaTime)
    {
    }

    private static void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

}
