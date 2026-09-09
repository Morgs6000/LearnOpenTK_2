using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace LearnOpenTK.src;

public class Program
{
    // configurações
    private const int SCR_WIDTH = 800;
    private const int SCR_HEIGHT = 600;

    // câmera
    private static Camera _camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));
    private static float _lastX = SCR_WIDTH / 2.0f;
    private static float _lastY = SCR_HEIGHT / 2.0f;
    private static bool _firstMouse = true;

    // timing
    private static float _deltaTime = 0.0f;
    private static float _lastFrame = 0.0f;
    
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
        Window* window = GLFW.CreateWindow(SCR_WIDTH, SCR_HEIGHT, "Learn OpenTK", null, null);

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
        GLFW.SetCursorPosCallback(window, MouseCallback);
        GLFW.SetScrollCallback(window, ScrollCallback);

        // instruir o GLFW a capturar o mouse
        GLFW.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);

        // configurar estado global do OpenGL
        // --------------------------------------------------
        GL.Enable(EnableCap.DepthTest);

        // construir e compilar nosso programa de shader
        // --------------------------------------------------
        Shader shaderRed = new Shader("src/advanced_glsl.vs", "src/red.fs");
        Shader shaderGreen = new Shader("src/advanced_glsl.vs", "src/green.fs");
        Shader shaderBlue = new Shader("src/advanced_glsl.vs", "src/blue.fs");
        Shader shaderYellow = new Shader("src/advanced_glsl.vs", "src/yellow.fs");

        // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
        // --------------------------------------------------
        float[] cubeVertices =
        {
            // positions
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f,
            
             0.5f, -0.5f,  0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f,  0.5f,
            
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
            
            -0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f,  0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            
             0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            
            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f
        };

        // cube VAO
        uint cubeVAO, cubeVBO;

        GL.GenVertexArrays(1, out cubeVAO);
        GL.GenBuffers(1, out cubeVBO);

        GL.BindVertexArray(cubeVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        // configurar um objeto de buffer uniforme
        // --------------------------------------------------

        // primeiro. Obtemos os índices de bloco relevantes
        int uniformBlockIndexRed = GL.GetUniformBlockIndex(shaderRed.ID, "Matrices");
        int uniformBlockIndexGreen = GL.GetUniformBlockIndex(shaderGreen.ID, "Matrices");
        int uniformBlockIndexBlue = GL.GetUniformBlockIndex(shaderBlue.ID, "Matrices");
        int uniformBlockIndexYellow = GL.GetUniformBlockIndex(shaderYellow.ID, "Matrices");

        // então, vinculamos o bloco de uniformes de cada shader a este ponto de vinculação de uniformes
        GL.UniformBlockBinding(shaderRed.ID, uniformBlockIndexRed, 0);
        GL.UniformBlockBinding(shaderGreen.ID, uniformBlockIndexGreen, 0);
        GL.UniformBlockBinding(shaderBlue.ID, uniformBlockIndexBlue, 0);
        GL.UniformBlockBinding(shaderYellow.ID, uniformBlockIndexYellow, 0);

        // Agora, de fato, crie o buffer
        uint uboMatrices;

        GL.GenBuffers(1, out uboMatrices);
        GL.BindBuffer(BufferTarget.UniformBuffer, uboMatrices);
        GL.BufferData(BufferTarget.UniformBuffer, 2 * sizeof(Matrix4), IntPtr.Zero, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        // define o intervalo do buffer que se conecta a um ponto de vinculação de uniform
        GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, uboMatrices, 0, 2 * sizeof(Matrix4));

        // store the projection matrix (we only do this once now) (note: we're not using zoom anymore by changing the FoV)
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
            fovy:      MathHelper.DegreesToRadians(_camera.Zoom), 
            aspect:    (float)SCR_WIDTH / (float)SCR_HEIGHT, 
            depthNear: 0.1f, 
            depthFar:  100.0f
        );
        GL.BindBuffer(BufferTarget.UniformBuffer, uboMatrices);
        GL.BufferSubData(BufferTarget.UniformBuffer, 0, sizeof(Matrix4), ref projection);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        // loop de renderização
        // --------------------------------------------------
        while (!GLFW.WindowShouldClose(window))
        {
            // lógica de tempo por quadro
            // --------------------------------------------------
            float currentFrame = (float)GLFW.GetTime();
            _deltaTime = currentFrame - _lastFrame;
            _lastFrame = currentFrame;

            // input
            // --------------------------------------------------
            ProcessInput(window);

            // render
            // --------------------------------------------------
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // define as matrizes de visualização e projeção no bloco uniform — só precisamos fazer isso uma vez por iteração do loop.
            Matrix4 view = _camera.GetViewMatrix();
            GL.BindBuffer(BufferTarget.UniformBuffer, uboMatrices);
            GL.BufferSubData(BufferTarget.UniformBuffer, sizeof(Matrix4), sizeof(Matrix4), ref view);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            // desenhar 4 cubos

            // VERMELHO
            shaderRed.Use();
            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateTranslation(new Vector3(-0.75f, 0.75f, 0.0f)); // mover para o canto superior esquerdo
            shaderRed.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // VERDE
            shaderGreen.Use();
            model = Matrix4.Identity;
            model *= Matrix4.CreateTranslation(new Vector3(0.75f, 0.75f, 0.0f)); // mover para o canto superior direito
            shaderGreen.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // AMARELO
            shaderYellow.Use();
            model = Matrix4.Identity;
            model *= Matrix4.CreateTranslation(new Vector3(-0.75f, -0.75f, 0.0f)); // mover para baixo à esquerda
            shaderYellow.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // AZUL
            shaderBlue.Use();
            model = Matrix4.Identity;
            model *= Matrix4.CreateTranslation(new Vector3(0.75f, -0.75f, 0.0f)); // mover para baixo e para a direita
            shaderBlue.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            
            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // --------------------------------------------------
        GL.DeleteVertexArrays(1, ref cubeVAO);
        GL.DeleteBuffers(1, ref cubeVBO);

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

        if (GLFW.GetKey(window, Keys.W) == InputAction.Press)
        {
            _camera.ProcessKeyboard(CameraMovement.FORWARD, _deltaTime);
        }
        if (GLFW.GetKey(window, Keys.S) == InputAction.Press)
        {
            _camera.ProcessKeyboard(CameraMovement.BACKWARD, _deltaTime);
        }
        if (GLFW.GetKey(window, Keys.A) == InputAction.Press)
        {
            _camera.ProcessKeyboard(CameraMovement.LEFT, _deltaTime);
        }
        if (GLFW.GetKey(window, Keys.D) == InputAction.Press)
        {
            _camera.ProcessKeyboard(CameraMovement.RIGHT, _deltaTime);
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

    // glfw: sempre que o mouse se move, este callback é chamado
    // --------------------------------------------------
    private static unsafe void MouseCallback(Window* window, double xposIn, double yposIn)
    {
        float xpos = (float)xposIn;
        float ypos = (float)yposIn;

        if (_firstMouse)
        {
            _lastX = xpos;
            _lastY = ypos;

            _firstMouse = false;
        }

        float xoffset = xpos - _lastX;
        float yoffset = _lastY - ypos; // invertido, já que as coordenadas y vão de baixo para cima

        _lastX = xpos;
        _lastY = ypos;

        _camera.ProcessMouseMovement(xoffset, yoffset);
    }

    // glfw: sempre que a roda de rolagem do mouse é girada, este callback é chamado
    // --------------------------------------------------
    private static unsafe void ScrollCallback(Window* window, double xoffset, double yoffset)
    {
        _camera.ProcessMouseScroll((float)yoffset);
    }
}
