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

        // configurar estado global do OpenGL
        // --------------------------------------------------
        GL.Enable(EnableCap.DepthTest);

        // construir e compilar nosso programa de shader
        // --------------------------------------------------
        Shader shader = new Shader("src/instancing.vs", "src/instancing.fs");

        // gerar uma lista de 100 localizações de quad/vetores de translação
        // --------------------------------------------------
        Vector2[] translations = new Vector2[100];
        int index = 0;
        float offset = 0.1f;

        for (int y = -10; y < 10; y += 2)
        {
            for (int x = -10; x < 10; x += 2)
            {
                Vector2 translation;
                translation.X = (float)x / 10.0f + offset;
                translation.Y = (float)y / 10.0f + offset;

                translations[index++] = translation;
            }
        }

        // armazena dados da instância em um buffer de array
        // --------------------------------------------------
        uint instanceVBO;

        GL.GenBuffers(1, out instanceVBO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(Vector2) * 100, ref translations[0], BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
        // --------------------------------------------------
        float[] quadVertices =
        {
            // positions      // colors
            -0.05f, -0.05f,   1.0f, 0.0f, 0.0f,
             0.05f, -0.05f,   0.0f, 1.0f, 0.0f,
             0.05f,  0.05f,   0.0f, 0.0f, 1.0f,
            -0.05f, -0.05f,   1.0f, 0.0f, 0.0f,
             0.05f,  0.05f,   0.0f, 0.0f, 1.0f,
            -0.05f,  0.05f,   1.0f, 1.0f, 0.0f
        };

        uint quadVAO, quadVBO;

        GL.GenVertexArrays(1, out quadVAO);
        GL.GenBuffers(1, out quadVBO);

        GL.BindVertexArray(quadVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));

        // define também os dados da instância
        GL.EnableVertexAttribArray(2);
        GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO); // este atributo vem de um buffer de vértices diferente
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.VertexAttribDivisor(2, 1); // informe ao OpenGL que este é um atributo de vértice instanciado.

        // loop de renderização
        // --------------------------------------------------
        while (!GLFW.WindowShouldClose(window))
        {
            // render
            // --------------------------------------------------
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // desenha 100 quads instanciados
            shader.Use();
            
            GL.BindVertexArray(quadVAO);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 100); // 100 triângulos de 6 vértices cada
            GL.BindVertexArray(0);
            
            // glfw: troca os buffers e processa eventos de E/S (teclas pressionadas/liberadas, movimento do mouse, etc.)
            // --------------------------------------------------
            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }

        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // --------------------------------------------------
        GL.DeleteVertexArrays(1, ref quadVAO);
        GL.DeleteBuffers(1, ref quadVBO);

        // glfw: encerra, liberando todos os recursos do GLFW alocados anteriormente.
        // --------------------------------------------------
        GLFW.Terminate();
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
