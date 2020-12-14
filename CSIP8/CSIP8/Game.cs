using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace CSIP8
{
    public class Game : GameWindow
    {
        const byte KEY_COUNT = 16;
        public static bool[] keysPressed = new bool[KEY_COUNT];

        static uint width = 64, height = 32;

        float[] vertices;

        uint[] indices = {
            //0, (width * 31 * 3) + (63 * 3), 63 * 3,
            //0, 63 * 3, (width * 31 * 3) + (63 * 3)
            //63 * 3, 0, (width * 31 * 3) + (63 * 3)
            //63 * 3, (width * 31 * 3) + (63 * 3), 0,
            1 * 3, (width * 1 * 3) + 1 * 3, 0,
            (width * 1 * 3) + 1 * 3, 1 * 3, (width * 1 * 3),

        };

        int VertexBufferObject;

        int ElementBufferObject;

        int VertexArrayObject;

        Shader shader;

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        { 
            
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            vertices = new float[width * height * 3];

            for (int i = 0; i < vertices.Length; i+= 3)
            {
                float x = ((float)(i % ((width + 1) * 3)) / (width * 3)) * 2 - 1.0f,
                    //y = (float)(i % (height * 3)) / (height * 3),
                    y = 1f - ((float)((i) / ((width) * 3)) / height) * 2,// * -2 - 1.0f,
                    z = 0.0f;

                vertices[i] = x;
                vertices[i + 1] = y;
                vertices[i + 2] = z;

                Console.WriteLine($"Setting {i} to {x}, {i + 1} to {y}, {i + 2} to {z}.");
            }

            /*Parallel.For(0, vertices.Length, (int i) =>
            {
                float x = ((float)(i % (width * 3)) / (width * 3)) - 0.5f,
                    //y = (float)(i % (height * 3)) / (height * 3),
                    y = ((float)((i) / (width * 3)) / height) - 0.5f,
                    z = 0.0f;

                vertices[i] = x;
                vertices[i + 1] = y;
                vertices[i + 2] = z;

                //Console.WriteLine($"Setting {i} to {x}, {i + 1} to {y}, {i + 2} to {z}.");
            });*/

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            base.OnLoad(e);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key >= Key.Number0 && e.Key <= Key.Number9)
            {
                keysPressed[e.Key - Key.Number0] = true;
            }
            else if (e.Key >= Key.A && e.Key <= Key.F)
            {
                keysPressed[e.Key - Key.A] = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key >= Key.Number0 && e.Key <= Key.Number9)
            {
                keysPressed[e.Key - Key.Number0] = false;
            }
            else if (e.Key >= Key.A && e.Key <= Key.F)
            {
                keysPressed[e.Key - Key.A] = false;
            }

            base.OnKeyUp(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();

            GL.BindVertexArray(VertexArrayObject);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, ElementBufferObject);

            SwapBuffers();

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState kbstate = Keyboard.GetState();

            if (kbstate.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferObject);
            base.OnUnload(e);
        }
    }
}
