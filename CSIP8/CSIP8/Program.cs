using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace CSIP8
{
    class Program
    {
        public static bool[] input = new bool[16];

        public static void Main(String[] args)
        {
            /*new Thread(() => {
                Emulator emu = new Emul ator();
            }).Start();*/

            /*Form1 form = new Form1();
            Application.Run(form);*/

            Emulator emu = new Emulator();

            RenderWindow window = new RenderWindow(new VideoMode(900, 600), "Chip8.net");
            window.Closed += Window_Closed;
            window.SetFramerateLimit(60);

            float TILESIZEX = (float)window.Size.X / 64;
            float TILESIZEY = (float)window.Size.Y / 32;
            RectangleShape shape = new RectangleShape(new Vector2f(TILESIZEX, TILESIZEY));

            //Action beep = Console.Beep;
            //beep.BeginInvoke(null, null);

            while (window.IsOpen)
            {
                emu.Cycle();

                if (emu.hasDrawn)
                {
                    window.DispatchEvents();

                    for (int x = 0; x < 64; x++)
                    {
                        for (int y = 0; y < 32; y++)
                        {
                            shape.Position = new Vector2f(x * TILESIZEX, y * TILESIZEY);

                            if (emu.display[y, x])
                            {
                                shape.FillColor = new Color(255, 255, 255);
                            }
                            else
                            {
                                shape.FillColor = new Color(100, 100, 100);
                            }

                            window.Draw(shape);
                        }
                    }

                    window.Display();
                    emu.hasDrawn = false;
                }

                input[0] = Keyboard.IsKeyPressed(Keyboard.Key.Num0);
                input[1] = Keyboard.IsKeyPressed(Keyboard.Key.Num2);
                input[2] = Keyboard.IsKeyPressed(Keyboard.Key.Num2);
                input[3] = Keyboard.IsKeyPressed(Keyboard.Key.Num3);
                input[4] = Keyboard.IsKeyPressed(Keyboard.Key.Num4);
                input[5] = Keyboard.IsKeyPressed(Keyboard.Key.Num5);
                input[6] = Keyboard.IsKeyPressed(Keyboard.Key.Num6);
                input[7] = Keyboard.IsKeyPressed(Keyboard.Key.Num7);
                input[8] = Keyboard.IsKeyPressed(Keyboard.Key.Num8);
                input[9] = Keyboard.IsKeyPressed(Keyboard.Key.Num9);
                input[10] = Keyboard.IsKeyPressed(Keyboard.Key.A);
                input[11] = Keyboard.IsKeyPressed(Keyboard.Key.B);
                input[12] = Keyboard.IsKeyPressed(Keyboard.Key.C);
                input[13] = Keyboard.IsKeyPressed(Keyboard.Key.D);
                input[14] = Keyboard.IsKeyPressed(Keyboard.Key.E);
                input[15] = Keyboard.IsKeyPressed(Keyboard.Key.F);

                if (Keyboard.IsKeyPressed(Keyboard.Key.R))
                {
                    emu = new Emulator();
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                {
                    window.Close();   
                }
            }

            /*int heightPixels = 32;
            int widthPixels = 64;

            int sizePerPixel = 16;

            using (Game game = new Game(widthPixels * sizePerPixel, heightPixels * sizePerPixel, "CSIP8"))
            {
                const double goalFPS = 60.0;
                game.Run(goalFPS);
            }*/
        }



        private static void Window_Closed(object sender, EventArgs e)
        {
            ((RenderWindow)sender).Close();
        }
    }
}
