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

    private const string _fragmentShader1Source =
    @"
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        }
    ";

    private const string _fragmentShader2Source =
    @"
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);
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

        int pWidth;
        int pHeight;

        // Obtém o tamanho da janela passado para glfwCreateWindow
        GLFW.GetWindowSize(window, out pWidth, out pHeight);

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

        // desta vez, omitimos as verificações do log de compilação para facilitar a leitura (se você encontrar problemas, adicione as verificações de compilação; consulte os exemplos de código anteriores)
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);

        int fragmentShaderOrange = GL.CreateShader(ShaderType.FragmentShader); // o primeiro shader de fragmento que gera a cor laranja
        int fragmentShaderYellow = GL.CreateShader(ShaderType.FragmentShader); // o segundo shader de fragmento que gera a cor amarela

        int shaderProgramOrange = GL.CreateProgram();
        int shaderProgramYellow = GL.CreateProgram(); // o segundo programa de shader

        GL.ShaderSource(vertexShader, _vertexShaderSource);
        GL.CompileShader(vertexShader);

        GL.ShaderSource(fragmentShaderOrange, _fragmentShader1Source);
        GL.CompileShader(fragmentShaderOrange);

        GL.ShaderSource(fragmentShaderYellow, _fragmentShader2Source);
        GL.CompileShader(fragmentShaderYellow);

        // vincular o primeiro objeto de programa
        GL.AttachShader(shaderProgramOrange, vertexShader);
        GL.AttachShader(shaderProgramOrange, fragmentShaderOrange);
        GL.LinkProgram(shaderProgramOrange);

        // em seguida, vincule o segundo objeto de programa usando um shader de fragmento diferente (mas o mesmo shader de vértice)
        // isso é perfeitamente permitido, uma vez que as entradas e saídas de ambos os shaders — de vértice e de fragmento — são compatíveis.
        GL.AttachShader(shaderProgramYellow, vertexShader);
        GL.AttachShader(shaderProgramYellow, fragmentShaderYellow);
        GL.LinkProgram(shaderProgramYellow);

        // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
        // --------------------------------------------------
        float[] firstTriangle =
        {
            -0.9f,  -0.5f,  0.0f,
             0.0f,  -0.5f,  0.0f,
            -0.45f,  0.5f,  0.0f
        };

        float[] secondTriangle =
        {
             0.0f,  -0.5f,  0.0f,
             0.9f,  -0.5f,  0.0f,
             0.45f,  0.5f,  0.0f
        };

        uint[] VAO = new uint[2], VBO = new uint[2];

        GL.GenVertexArrays(2, VAO); // também podemos gerar múltiplos VAOs ou buffers ao mesmo tempo
        GL.GenBuffers(2, VBO);

        // configuração do primeiro triângulo
        // --------------------------------------------------
        GL.BindVertexArray(VAO[0]);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO[0]);
        GL.BufferData(BufferTarget.ArrayBuffer, firstTriangle.Length * sizeof(float), firstTriangle, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); // Os atributos de vértice permanecem os mesmos
        GL.EnableVertexAttribArray(0);

        // GL.BindVertexArray(0); // não é necessário desfazer a vinculação, pois vinculamos diretamente um VAO diferente nas próximas linhas

        // configuração do segundo triângulo
        // --------------------------------------------------
        GL.BindVertexArray(VAO[1]); // observe que agora vinculamos a um VAO diferente

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO[1]); // e um VBO diferente
        GL.BufferData(BufferTarget.ArrayBuffer, secondTriangle.Length * sizeof(float), secondTriangle, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); // como os dados dos vértices estão compactados, também podemos especificar 0 como o stride do atributo de vértice para deixar o OpenGL determiná-lo
        GL.EnableVertexAttribArray(0);

        // GL.BindVertexArray(0); // também não é estritamente necessário, mas cuidado com chamadas que possam afetar VAOs enquanto este estiver vinculado (como vincular *element buffer objects* ou habilitar/desabilitar atributos de vértice)

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

            // agora, ao desenhar o triângulo, usamos primeiro o shader de vértice e o shader de fragmento laranja do primeiro programa
            GL.UseProgram(shaderProgramOrange);

            // desenha o primeiro triângulo usando os dados do primeiro VAO
            GL.BindVertexArray(VAO[0]);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // esta chamada deve gerar um triângulo laranja

            // então, desenhamos o segundo triângulo usando os dados do segundo VAO
            // ao desenhar o segundo triângulo, queremos usar um programa de shader diferente; por isso, alternamos para o programa de shader que utiliza nosso shader de fragmento amarelo.
            GL.UseProgram(shaderProgramYellow);
            GL.BindVertexArray(VAO[1]);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // esta chamada deve gerar um triângulo amarelo

            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // --------------------------------------------------
        GL.DeleteVertexArrays(2, VAO);
        GL.DeleteBuffers(2, VBO);
        GL.DeleteProgram(shaderProgramOrange);
        GL.DeleteProgram(shaderProgramYellow);

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
