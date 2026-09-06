using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK.src;

public class Program
{
    // configurações
    private const uint SCR_WIDTH = 800;
    private const uint SCR_HEIGHT = 600;

    private const string _vertexShaderSource =
    @"
        layout (location = 0) in vec3 aPos;

        void main()
        {
            gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
        }
    ";

    private const string _fragmentShaderSource =
    @"
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        }
    ";
    
    private unsafe static void Main(string[] args)
    {
        // glfw: inicializar e configurar
        // --------------------------------------------------
        GLFW.Init();
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        if (OperatingSystem.IsMacOS())
        {
            GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
        }

        // criação da janela glfw
        // --------------------------------------------------
        Window* window = GLFW.CreateWindow((int)SCR_WIDTH, (int)SCR_HEIGHT, "Learn OpenTK", null, null);

        if (window == null)
        {
            Console.WriteLine("Falha ao criar a janela OpenTK");
            GLFW.Terminate();
        }

        // Obtém o tamanho da janela passado para glfwCreateWindow
        GLFW.GetWindowSize(window, out int pWidth, out int pHeight);

        // Obtém a resolução do monitor principal
        VideoMode* vidmode = GLFW.GetVideoMode(GLFW.GetPrimaryMonitor());

        // Centralizar a janela
        GLFW.SetWindowPos(
            window,
            (vidmode->Width - pWidth) / 2,
            (vidmode->Height - pHeight) / 2
        );

        GLFW.MakeContextCurrent(window);

        GL.LoadBindings(new GLFWBindingsContext());

        GLFW.SetFramebufferSizeCallback(window, FramebufferSizeCallback);

        // construir e compilar nosso programa de shader
        // --------------------------------------------------

        // vertex shader
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, _vertexShaderSource);
        GL.CompileShader(vertexShader);

        // verificar erros de compilação de shader
        int success;
        string infoLog;

        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            GL.GetShaderInfoLog(vertexShader, out infoLog);
            Console.WriteLine("ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" + infoLog);
        }

        // fragment shader
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, _fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        // verificar erros de compilação de shader
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            GL.GetShaderInfoLog(fragmentShader, out infoLog);
            Console.WriteLine("ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" + infoLog);
        }

        // link shaders
        int shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);

        // verificar erros de vinculação
        GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            GL.GetProgramInfoLog(shaderProgram, out infoLog);
            Console.WriteLine("ERROR::SHADER::PROGRAM::LINKING_FAILED\n" + infoLog);
        }

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
        // --------------------------------------------------
        float[] vertices =
        {
            -0.5f, -0.5f,  0.0f, // inferior esquerdo
             0.5f, -0.5f,  0.0f, // inferior direito
             0.5f,  0.5f,  0.0f, // superior direito
            -0.5f,  0.5f,  0.0f  // superior esquerdo
        };

        uint[] indices = // observe que começamos do 0!
        {
            0, 1, 2, // primeiro triângulo
            0, 2, 3  // segundo triângulo
        };

        uint VAO, VBO, EBO;

        GL.GenVertexArrays(1, out VAO);
        GL.GenBuffers(1, out VBO);
        GL.GenBuffers(1, out EBO);

        // primeiro, vincule o Vertex Array Object; depois, vincule e configure o(s) buffer(s) de vértices; e, em seguida, configure o(s) atributo(s) de vértice.
        GL.BindVertexArray(VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // observe que isso é permitido; a chamada para glVertexAttribPointer registrou o VBO como o objeto de buffer de vértices vinculado ao atributo de vértice, portanto, podemos desvinculá-lo com segurança logo em seguida
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        // lembre-se: NÃO desvincule o EBO enquanto um VAO estiver ativo, pois o objeto de buffer de elementos vinculado ESTÁ armazenado no VAO; mantenha o EBO vinculado.
        // GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        // Você pode desvincular o VAO posteriormente para que outras chamadas relacionadas a VAOs não modifiquem acidentalmente este VAO, mas isso raramente acontece. Modificar outros
        // VAOs exige uma chamada para glBindVertexArray de qualquer forma, então geralmente não desvinculamos VAOs (nem VBOs) quando não é diretamente necessário.
        GL.BindVertexArray(0);

        // descomente esta chamada para desenhar polígonos em wireframe.
        // GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        // loop de renderização
        // --------------------------------------------------
        while (!GLFW.WindowShouldClose(window))
        {
            // input
            // --------------------------------------------------
            ProcessInput(window);

            // render
            // --------------------------------------------------
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // desenhar nosso primeiro triângulo
            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(VAO); // como temos apenas um único VAO, não há necessidade de vinculá-lo todas as vezes, mas faremos isso para manter as coisas um pouco mais organizadas
            // GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            // GL.BindVertexArray(0); // não é necessário desvinculá-lo toda vez

            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // --------------------------------------------------
        GL.DeleteVertexArrays(1, ref VAO);
        GL.DeleteBuffers(1, ref VBO);
        GL.DeleteBuffers(1, ref EBO);
        GL.DeleteProgram(shaderProgram);

        // glfw: encerra, liberando todos os recursos do GLFW alocados anteriormente.
        // --------------------------------------------------
        GLFW.Terminate();
    }

    // processar toda a entrada: consultar a GLFW para saber se teclas relevantes foram pressionadas ou liberadas neste quadro e reagir de acordo
    // --------------------------------------------------
    private static unsafe void ProcessInput(Window* window)
    {
        if (GLFW.GetKey(window, Keys.Escape) == InputAction.Press)
        {
            GLFW.SetWindowShouldClose(window, true);
        }
    }

    // glfw: sempre que o tamanho da janela é alterado (pelo SO ou por redimensionamento do usuário), esta função de callback é executada
    // --------------------------------------------------
    private static unsafe void FramebufferSizeCallback(Window* window, int width, int height)
    {
        // certifique-se de que a viewport corresponda às novas dimensões da janela; observe que a largura e
        // a altura serão significativamente maiores do que as especificadas em telas Retina.
        GL.Viewport(0, 0, width, height);
    }
}
