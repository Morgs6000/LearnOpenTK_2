using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace LearnOpenTK.src;

public class Program
{
    // configurações
    private const uint SCR_WIDTH = 800;
    private const uint SCR_HEIGHT = 600;

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
        GLFW.SetCursorPosCallback(window, MouseCallback);
        GLFW.SetScrollCallback(window, ScrollCallback);

        // instruir o GLFW a capturar o mouse
        GLFW.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);

        // instrua a stb_image.h a inverter as texturas carregadas no eixo Y (antes de carregar o modelo).
        StbImage.stbi_set_flip_vertically_on_load(1);

        // configurar estado global do OpenGL
        // --------------------------------------------------
        GL.Enable(EnableCap.DepthTest);

        // construir e compilar nosso programa de shader
        // --------------------------------------------------
        Shader ourShader = new Shader("src/model_loading.vs", "src/model_loading.fs");

        // carregar modelos
        // --------------------------------------------------
        Model ourModel = new Model("res/objects/backpack/backpack.obj");

        // desenhar em wireframe
        // GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

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
            GL.ClearColor(0.05f, 0.05f, 0.05f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // não se esqueça de ativar o shader antes de definir os uniforms
            ourShader.Use();

            // transformações de visualização/projeção
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                fovy:      MathHelper.DegreesToRadians(_camera.Zoom), 
                aspect:    (float)SCR_WIDTH / (float)SCR_HEIGHT, 
                depthNear: 0.1f, 
                depthFar:  100.0f
            );
            Matrix4 view = _camera.GetViewMatrix();

            ourShader.SetMat4("projection", projection);
            ourShader.SetMat4("view", view);

            // renderizar o modelo carregado
            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateScale(new Vector3(1.0f, 1.0f, 1.0f)); // é um pouco grande demais para a nossa cena, então reduza a escala
            model *= Matrix4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f)); // desloque para baixo para posicionar no centro da cena
            ourShader.SetMat4("model", model);

            ourModel.Draw(ourShader);
            
            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

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
