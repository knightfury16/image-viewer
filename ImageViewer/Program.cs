using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer;

class Program
{
    private static IWindow? _window;
    private static GL? _gl;
    private static uint _vao;
    private static uint _vbo;

    private static Vector3[] colors = new[]
    {
        new Vector3(0.2f, 0.3f, 0.3f), // Teal
        new Vector3(0.8f, 0.2f, 0.2f), // Reddish
        new Vector3(0.2f, 0.7f, 0.3f), // Greenish
        new Vector3(0.2f, 0.2f, 0.7f)  // Bluish
    };

    static int currentColorIndex = 0;
    static double timer = 0.0;

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
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);
        float[] vertices =
        {
     0.5f,  0.5f, 0.0f,
     0.5f, -0.5f, 0.0f,
    -0.5f, -0.5f, 0.0f,
    -0.5f,  0.5f, 0.0f
};
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
    }

    private static void OnUpdate(double deltaTime)
    {
        // This is where all the logic go
        //
        timer += deltaTime;

        if (timer > 2.0f)
        {
            currentColorIndex = (currentColorIndex + 1) % colors.Length;
            timer = 0;
            Console.WriteLine($"Switched to color index {currentColorIndex}");
        }
    }

    private static void OnRender(double deltaTime)
    {

        var color = colors[currentColorIndex];
        _gl?.ClearColor(color.X, color.Y, color.Z, 1.0f);
        _gl?.Clear(ClearBufferMask.ColorBufferBit);
    }

}
