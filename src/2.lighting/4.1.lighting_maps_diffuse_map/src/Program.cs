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

    // iluminação
    private static Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);
    
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
        Shader lightingShader = new Shader("src/lighting_maps.vs", "src/lighting_maps.fs");
        Shader lightCubeShader = new Shader("src/light_cube.vs", "src/light_cube.fs");

        // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
        // --------------------------------------------------
        float[] vertices =
        {
            // positions           // normals             // texture coords
            -0.5f, -0.5f, -0.5f,   -1.0f,  0.0f,  0.0f,   0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,   -1.0f,  0.0f,  0.0f,   1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,   -1.0f,  0.0f,  0.0f,   1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,   -1.0f,  0.0f,  0.0f,   0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,   -1.0f,  0.0f,  0.0f,   1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,   -1.0f,  0.0f,  0.0f,   0.0f, 1.0f,
            
             0.5f, -0.5f,  0.5f,    1.0f,  0.0f,  0.0f,   0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,    1.0f,  0.0f,  0.0f,   1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,    1.0f,  0.0f,  0.0f,   1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,    1.0f,  0.0f,  0.0f,   0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,    1.0f,  0.0f,  0.0f,   1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,    1.0f,  0.0f,  0.0f,   0.0f, 1.0f,
            
            -0.5f, -0.5f, -0.5f,    0.0f, -1.0f,  0.0f,   0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,    0.0f, -1.0f,  0.0f,   1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,    0.0f, -1.0f,  0.0f,   1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,    0.0f, -1.0f,  0.0f,   0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,    0.0f, -1.0f,  0.0f,   1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,    0.0f, -1.0f,  0.0f,   0.0f, 1.0f,
            
            -0.5f,  0.5f,  0.5f,    0.0f,  1.0f,  0.0f,   0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,    0.0f,  1.0f,  0.0f,   1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,    0.0f,  1.0f,  0.0f,   1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,    0.0f,  1.0f,  0.0f,   0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,    0.0f,  1.0f,  0.0f,   1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,    0.0f,  1.0f,  0.0f,   0.0f, 1.0f,
            
             0.5f, -0.5f, -0.5f,    0.0f,  0.0f, -1.0f,   0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,    0.0f,  0.0f, -1.0f,   1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,    0.0f,  0.0f, -1.0f,   1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,    0.0f,  0.0f, -1.0f,   0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,    0.0f,  0.0f, -1.0f,   1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,    0.0f,  0.0f, -1.0f,   0.0f, 1.0f,
            
            -0.5f, -0.5f,  0.5f,    0.0f,  0.0f,  1.0f,   0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,    0.0f,  0.0f,  1.0f,   1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,    0.0f,  0.0f,  1.0f,   1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,    0.0f,  0.0f,  1.0f,   0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,    0.0f,  0.0f,  1.0f,   1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,    0.0f,  0.0f,  1.0f,   0.0f, 1.0f
        };

        // primeiro, configure o VAO (e o VBO) do cubo
        uint cubeVAO, VBO;

        GL.GenVertexArrays(1, out cubeVAO);
        GL.GenBuffers(1, out VBO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(cubeVAO);

        // atributo de posição
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // normal attribute
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // texture attribute
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        // segundo, configure o VAO da luz (o VBO permanece o mesmo; os vértices são os mesmos para o objeto de luz, que também é um cubo 3D)
        uint lightCubeVAO;

        GL.GenVertexArrays(1, out lightCubeVAO);
        GL.BindVertexArray(lightCubeVAO);

        // precisamos apenas vincular o VBO (para associá-lo ao glVertexAttribPointer), sem necessidade de preenchê-lo; os dados do VBO já contêm tudo o que precisamos (ele já está vinculado, mas fazemos isso novamente para fins didáticos)
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

        // observe que atualizamos o stride do atributo de posição da lâmpada para refletir os dados atualizados do buffer
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // carregar texturas (agora usamos uma função utilitária para manter o código mais organizado)
        // --------------------------------------------------
        uint diffuseMap = LoadTexture("res/textures/container2.png");

        // configuração do shader
        lightingShader.Use();
        lightingShader.SetInt("material.diffuse", 0);

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

            // certifique-se de ativar o shader ao definir uniforms ou desenhar objetos
            lightingShader.Use();
            lightingShader.SetVec3("light.position", _lightPos);
            lightingShader.SetVec3("viewPos", _camera.Position);

            // propriedades da luz
            lightingShader.SetVec3("light.ambient", 0.2f, 0.2f, 0.2f); 
            lightingShader.SetVec3("light.diffuse", 0.5f, 0.5f, 0.5f);
            lightingShader.SetVec3("light.specular", 1.0f, 1.0f, 1.0f);

            // propriedades do material
            lightingShader.SetVec3("material.specular", 0.5f, 0.5f, 0.5f);
            lightingShader.SetFloat("material.shininess", 64.0f);

            // transformações de visualização/projeção
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                fovy:      MathHelper.DegreesToRadians(_camera.Zoom), 
                aspect:    (float)SCR_WIDTH / (float)SCR_HEIGHT, 
                depthNear: 0.1f, 
                depthFar:  100.0f
            );
            Matrix4 view = _camera.GetViewMatrix();

            lightingShader.SetMat4("projection", projection);
            lightingShader.SetMat4("view", view);

            // transformação do mundo
            Matrix4 model = Matrix4.Identity;
            lightingShader.SetMat4("model", model);

            // vincular mapa de difusão
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, diffuseMap);

            // renderiza o cubo
            GL.BindVertexArray(cubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // desenhe também o objeto da lâmpada
            lightCubeShader.Use();
            lightCubeShader.SetMat4("projection", projection);
            lightCubeShader.SetMat4("view", view);

            model = Matrix4.Identity;
            model *= Matrix4.CreateScale(new Vector3(0.2f)); // um cubo menor
            model *= Matrix4.CreateTranslation(_lightPos);
            lightCubeShader.SetMat4("model", model);

            GL.BindVertexArray(lightCubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);            

            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // --------------------------------------------------
        GL.DeleteVertexArrays(1, ref cubeVAO);
        GL.DeleteVertexArrays(1, ref lightCubeVAO);
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

    // função utilitária para carregar uma textura 2D a partir de um arquivo
    // --------------------------------------------------
    private static uint LoadTexture(string path)
    {
        uint textureID;
        GL.GenTextures(1, out textureID);

        int width, height;
        byte[] data;

        ImageResult image;

        using (FileStream stream = File.OpenRead(path))
        {
            image = ImageResult.FromStream(stream, ColorComponents.Default);

            width = image.Width;
            height = image.Height;
            data = image.Data;
        }

        if (data != null)
        {
            PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgb;
            PixelFormat pixelFormat = PixelFormat.Rgb;

            if (image.Comp == ColorComponents.Grey)
            {
                pixelInternalFormat = PixelInternalFormat.CompressedRed;
                pixelFormat = PixelFormat.Red;
            }
            else if (image.Comp == ColorComponents.RedGreenBlue)
            {
                pixelInternalFormat = PixelInternalFormat.Rgb;
                pixelFormat = PixelFormat.Rgb;
            }
            else if (image.Comp == ColorComponents.RedGreenBlueAlpha)
            {
                pixelInternalFormat = PixelInternalFormat.Rgba;
                pixelFormat = PixelFormat.Rgba;
            }

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, width, height, 0, pixelFormat, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura no caminho: " + path);
        }

        return textureID;
    }
}
