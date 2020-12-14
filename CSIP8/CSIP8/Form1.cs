using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSIP8
{
    public partial class Form1 : Form
    {
        Emulator emu;

        Bitmap screen;

        System.Timers.Timer timer;

        BackgroundWorker backgroundWorker;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            emu = new Emulator();

            screen = new Bitmap(64, 32);
            pictureBox1.Image = screen;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();

            /*timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;

            timer.Enabled = true;
            timer.Interval = 1;
            timer.Start();*/

            //Timer_Elapsed(null, null);
            //Go();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                emu.Cycle();

                if (emu.hasDrawn)
                {
                    for (int r = 0; r < 32; r++)
                    {
                        for (int c = 0; c < 64; c++)
                        {
                            screen.SetPixel(c, r, emu.display[r, c] ? Color.White : Color.Black);
                        }
                    }

                    backgroundWorker.ReportProgress(0);
                }
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Invoke((MethodInvoker)delegate { pictureBox1.Image = screen; pictureBox1.Update(); });
            pictureBox1.Image = screen;
            pictureBox1.Update();
            emu.hasDrawn = false;
        }

        private void Go()
        {
            while (true)
            { 
                emu.Cycle();

                if (emu.hasDrawn)
                {
                    for (int r = 0; r < 32; r++)
                    {
                        for (int c = 0; c < 64; c++)
                        {
                            screen.SetPixel(c, r, emu.display[r, c] ? Color.White : Color.Black);
                        }
                    }

                    Invoke((MethodInvoker)delegate { pictureBox1.Image = screen; pictureBox1.Update(); });
                    emu.hasDrawn = false;
                }
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            emu.Cycle();

            if (emu.hasDrawn)
            { 
                for (int r = 0; r < 32; r++)
                {
                    for (int c = 0; c < 64; c++)
                    {
                        screen.SetPixel(c, r, emu.display[r, c] ? Color.White : Color.Black);
                    }
                }

                Invoke((MethodInvoker)delegate { pictureBox1.Image = screen; });
                emu.hasDrawn = false;
            }

            Console.WriteLine("Timer_Elapsed finished.");
        }
    }
}
