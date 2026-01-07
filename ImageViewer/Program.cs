using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using StbImageSharp;

namespace ImageViewer;

class Program
{
    private static IWindow? _window;
    private static GL? _gl;
    private static uint _vbo;
    private static uint _vao;
    private static uint _ebo;
    private static uint _program;
    private static uint _texture;
    private static string _filePath;


    static void Main(string[] args)
    {
        int width = 800;
        int height = 600;
        _filePath = args[0];


        using (var stream = File.OpenRead(_filePath))
        {
            ImageInfo? info = ImageInfo.FromStream(stream);
            if (info is not null)
            {
                width = info.Value.Width;
                height = info.Value.Height;
            }
        }

        WindowOptions options = WindowOptions.Default with
        {
            Size = new Silk.NET.Maths.Vector2D<int>(width, height),
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
        _gl.ClearColor(Color.Black);


        // Vertex buffer data declaration and data initialization
        float[] vetices = [
            // aPosition         | aTextureCoordinate
            +0.5f,  +0.5f, +0.0f,  +1.0f, +1.0f,
            +0.5f, -0.5f,  +0.0f,  +1.0f, +0.0f,
            -0.5f, -0.5f,  +0.0f,  +0.0f, +0.0f,
            -0.5f,  +0.5f, +0.0f,  +0.0f, +1.0f
        ];

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vetices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vetices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }


        //Genarate vertex array. This will hold the recipie later
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);


        // Element array buffer data declaration and data initialization
        // Vertex are initialized in clockwise fashion with 0 base index
        // 0--------------1
        // -             -
        // -      -      -
        // -    (0,0)    -
        // -             -
        // 3--------------2
        uint[] indices = [
            0u, 1u, 3u,
            1u, 2u, 3u
        ];

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (uint* buf = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        }



        // Image as texture
        //
        _texture = _gl.GenTexture();

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);


        // ImageResult.FromMemory reads the bytes of the .png file and returns all its information!
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(_filePath), ColorComponents.RedGreenBlueAlpha);

        fixed (byte* ptr = result.Data)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                    (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

        }

        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);


        _gl.BindTexture(TextureTarget.Texture2D, 0);



        // Shaders 
        const string vertexCode = @"
            #version 330 core

            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec2 aTexCoord;

            // Add an output variable to pass the texture coordinate to the fragment shader
            // This variable stores the data that we want to be received by the fragment
            out vec2 frag_texCoords;

            void main()
            {
                gl_Position = vec4(aPosition, 1.0);

                // Assigin the texture coordinates without any modification to be recived in the fragment
                frag_texCoords = aTexCoord;
            }";

        const string fragmentCode = @"
            #version 330 core

            in vec2 frag_texCoords;
            uniform sampler2D uTexture;
            out vec4 out_color;

            void main()
            {
                // out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
                out_color = texture(uTexture, frag_texCoords);
            }";

        // Creating and compiling vertex shader
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));

        // Creating and compiling fragment shader
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));


        // creating _program
        _program = _gl.CreateProgram();

        // Attaching shaders to the program
        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        // after attaching we can free up some memory
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);



        // Setting up attribute
        //Tell VAO How to Read Data
        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        // "Hey VAO, each vertex is 3 floats for position, no gaps"
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);


        // For texture
        const uint texCoordLoc = 1;
        _gl.EnableVertexAttribArray(texCoordLoc);
        // The last pointer is important
        _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));



        //More cleaning
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // More texture stuff
        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        //Enabling Transparency
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }


    private static void OnUpdate(double deltaTime)
    {
    }

    private static unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        // _gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

}
