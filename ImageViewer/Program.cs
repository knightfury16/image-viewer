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
    private static uint _vbo;
    private static uint _vao;


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

    private static unsafe void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.Teal);


        float[] vetices = [
            +0.5f, +0.5f, 0.0f,
            +0.5f, -0.5f, 0.0f,
            -0.5f, +0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f
        ];

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vetices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vetices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);




    }

    private static void OnUpdate(double deltaTime)
    {
    }

    private static void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

    }

}
