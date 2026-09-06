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
    private static Vector3 _cameraPos = new Vector3(0.0f, 0.0f, 3.0f);
    private static Vector3 _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    private static Vector3 _cameraUp = new Vector3(0.0f, 1.0f, 0.0f);

    private static bool _firstMouse = true;
    private static float _yaw   = -90.0f; // O yaw é inicializado em -90,0 graus, uma vez que um yaw de 0,0 resulta em um vetor de direção apontando para a direita; portanto, inicialmente rotacionamos um pouco para a esquerda.
    private static float _pitch =  0.0f;
    private static float _lastX =  800.0f / 2.0f;
    private static float _lastY =  600.0f / 2.0f;
    private static float _fov   =  45.0f;

    // timing
    private static float _deltaTime = 0.0f; // tempo entre o quadro atual e o quadro anterior
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

        // configurar estado global do OpenGL
        // --------------------------------------------------
        GL.Enable(EnableCap.DepthTest);

        // construir e compilar nosso programa de shader
        // --------------------------------------------------
        Shader ourShader = new Shader("src/camera.vs", "src/camera.fs");

        // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
        // --------------------------------------------------
        float[] vertices =
        {
            // positions           // texture coords
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,   1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
            
             0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,   0.0f, 1.0f,
            
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,   0.0f, 1.0f,
            
            -0.5f,  0.5f,  0.5f,   0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,   1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,   0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
            
             0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
            
            -0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,   1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,   0.0f, 1.0f
        };

        // posições dos nossos cubos no espaço do mundo
        Vector3[] cubePositions =
        {
            new Vector3( 0.0f,  0.0f,  0.0f),
            new Vector3( 2.0f,  5.0f, -15.0f),
            new Vector3(-1.5f, -2.2f, -2.5f),
            new Vector3(-3.8f, -2.0f, -12.3f),
            new Vector3( 2.4f, -0.4f, -3.5f),
            new Vector3(-1.7f,  3.0f, -7.5f),
            new Vector3( 1.3f, -2.0f, -2.5f),
            new Vector3( 1.5f,  2.0f, -2.5f),
            new Vector3( 1.5f,  0.2f, -1.5f),
            new Vector3(-1.3f,  1.0f, -1.5f)
        };

        uint VAO, VBO;

        GL.GenVertexArrays(1, out VAO);
        GL.GenBuffers(1, out VBO);

        GL.BindVertexArray(VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // atributo de posição
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // texture coord attribute
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        // carregar e criar uma textura
        // --------------------------------------------------
        uint texture1, texture2;

        // texture 1
        // --------------------------------------------------        
        GL.GenTextures(1, out texture1);
        GL.BindTexture(TextureTarget.Texture2D, texture1);

        // define os parâmetros de repetição da textura
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // definir parâmetros de filtragem de textura
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // carregar imagem, criar textura e gerar mipmaps
        int width, height;
        byte[] data;

        StbImage.stbi_set_flip_vertically_on_load(1); // instrui a stb_image.h a inverter as texturas carregadas no eixo Y.

        using (FileStream stream = File.OpenRead("res/textures/container.jpg"))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.Default);

            width = image.Width;
            height = image.Height;
            data = image.Data;
        }

        if (data != null)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura");
        }
        
        // texture 2
        // --------------------------------------------------
        GL.GenTextures(1, out texture2);
        GL.BindTexture(TextureTarget.Texture2D, texture2);

        // define os parâmetros de repetição da textura
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // definir parâmetros de filtragem de textura
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // carregar imagem, criar textura e gerar mipmaps
        using (FileStream stream = File.OpenRead("res/textures/awesomeface.png"))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.Default);

            width = image.Width;
            height = image.Height;
            data = image.Data;
        }

        if (data != null)
        {
            // observe que o awesomeface.png possui transparência e, portanto, um canal alfa; certifique-se de informar ao OpenGL que o tipo de dado é GL_RGBA
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura");
        }

        // informar ao OpenGL, para cada sampler, a qual unidade de textura ele pertence (isso só precisa ser feito uma vez)
        // --------------------------------------------------
        ourShader.Use();
        ourShader.SetInt("texture1", 0);
        ourShader.SetInt("texture2", 1);

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
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // vincular texturas às unidades de textura correspondentes
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, texture2);

            // ativar shader
            ourShader.Use();

            // passa a matriz de projeção para o shader (note que, neste caso, ela pode mudar a cada quadro)
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                fovy:      MathHelper.DegreesToRadians(_fov), 
                aspect:    (float)SCR_WIDTH / (float)SCR_HEIGHT, 
                depthNear: 0.1f, 
                depthFar:  100.0f
            );
            ourShader.SetMat4("projection", projection);

            // transformação de câmera/visualização
            Matrix4 view = Matrix4.LookAt(
                eye:    _cameraPos, 
                target: _cameraPos + _cameraFront, 
                up:     _cameraUp
            );
            ourShader.SetMat4("view", view);

            // renderizar caixas
            GL.BindVertexArray(VAO);

            for (int i = 0; i < 10; i++)
            {
                // calcula a matriz de modelo para cada objeto e a passa para o shader antes de desenhar
                Matrix4 model = Matrix4.Identity;
                float angle = 20.0f * i;
                model *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), MathHelper.DegreesToRadians(angle));
                model *= Matrix4.CreateTranslation(cubePositions[i]);
                ourShader.SetMat4("model", model);
                
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }            

            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // --------------------------------------------------
        GL.DeleteVertexArrays(1, ref VAO);
        GL.DeleteBuffers(1, ref VBO);

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

        float cameraSpeed = 2.5f * _deltaTime;

        if (GLFW.GetKey(window, Keys.W) == InputAction.Press)
        {
            _cameraPos += cameraSpeed * _cameraFront;
        }
        if (GLFW.GetKey(window, Keys.S) == InputAction.Press)
        {
            _cameraPos -= cameraSpeed * _cameraFront;
        }
        if (GLFW.GetKey(window, Keys.A) == InputAction.Press)
        {
            _cameraPos -= cameraSpeed * Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp));
        }
        if (GLFW.GetKey(window, Keys.D) == InputAction.Press)
        {
            _cameraPos += cameraSpeed * Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp));
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

        float sensitivity = 0.1f; // altere este valor conforme sua preferência
        xoffset *= sensitivity;
        yoffset *= sensitivity;

        _yaw += xoffset;
        _pitch += yoffset;

        // certifique-se de que a tela não seja invertida quando o pitch estiver fora dos limites
        if (_pitch > 89.0f)
        {
            _pitch = 89.0f;
        }
        if (_pitch < -89.0f)
        {
            _pitch = -89.0f;
        }

        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Cos(MathHelper.DegreesToRadians(_yaw));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = MathF.Cos(MathHelper.DegreesToRadians(_pitch)) * MathF.Sin(MathHelper.DegreesToRadians(_yaw));
        _cameraFront = Vector3.Normalize(front);
    }

    // glfw: sempre que a roda de rolagem do mouse é girada, este callback é chamado
    // --------------------------------------------------
    private static unsafe void ScrollCallback(Window* window, double xoffset, double yoffset)
    {
        _fov -= (float)yoffset;

        if (_fov < 1.0f)
        {
            _fov = 1.0f;
        }
        if (_fov > 45.0f)
        {
            _fov = 45.0f;
        }
    }
}
