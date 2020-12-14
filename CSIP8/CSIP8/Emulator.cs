using OpenTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Key = System.Windows.Input.Key;
using Keyboard = System.Windows.Input.Keyboard;
using SFML.Audio;

//http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.2

namespace CSIP8
{
    class Emulator
    {
        const string ROM_FILENAME = "Roms/Breakout.ch8";

        const byte REG_0 = 0x0,
                    REG_1 = 0x1,
                    REG_2 = 0x2,
                    REG_3 = 0x3,
                    REG_4 = 0x4,
                    REG_5 = 0x5,
                    REG_6 = 0x6,
                    REG_7 = 0x7,
                    REG_8 = 0x8,
                    REG_9 = 0x9,
                    REG_A = 0xA,
                    REG_B = 0xB,
                    REG_C = 0xC,
                    REG_D = 0xD,
                    REG_E = 0xE,
                    REG_F = 0xF;

        const int REGISTER_LENGTH = 16;
        byte[] registers = new byte[REGISTER_LENGTH];

        /// <summary>
        /// Address Register
        /// </summary>
        ushort registerI;

        ushort registerProgramCounter = MEMORY_PROGRAM_START;

        ushort registerStackPointer;

        byte registerDelayTimer;

        byte registerSoundTimer;

        const int STACK_LENGTH = 16;
        Stack<ushort> stack = new Stack<ushort>(STACK_LENGTH);

        byte[] characters = {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80 // F
        };

        const int MEMORY_LENGTH = 4096;
        byte[] memory = new byte[MEMORY_LENGTH];

        const int DISPLAY_COLUMNS = 64;
        const int DISPLAY_ROWS = 32;

        /// <summary>
        /// The location most chip8 programs start at in memory (512).
        /// Space before this point is reserved for the CHIP8 interpreter.
        /// </summary>
        const int MEMORY_PROGRAM_START = 0x200;

        /// <summary>
        /// Access with ROW, COLUMN.
        /// </summary>
        public bool[,] display { get; private set; } = new bool[DISPLAY_ROWS, DISPLAY_COLUMNS];

        Random random;

        /// <summary>
        /// Container used by "random" to generate a random byte.
        /// </summary>
        byte[] randomByteContainer = new byte[1];

        ushort lastInstruction;

        const bool ONLY_DRAW_IF_SCREEN_UPDATED = true;

        /// <summary>
        /// A flag for checking if the screen should be drawn, as there is no point if the screen hasnt been drawn to.
        /// </summary>
        public bool hasDrawn = false;

        public Emulator()
        {
            byte[] rom = File.ReadAllBytes(ROM_FILENAME);
            PrintRom(rom);

            //Load characters into memory.
            for (int i = 0; i < characters.Length; i++)
            {
                memory[i] = characters[i];
            }

            //Load program into memory.
            for (int i = 0; i < rom.Length; i++)
            {
                memory[MEMORY_PROGRAM_START + i] = rom[i];
            }

            Console.WriteLine("Read rom complete.");
            //Console.ReadLine();

            random = new Random();

            Console.Clear();

            /*while (true)
            {
                Cycle();

                if (!ONLY_DRAW_IF_SCREEN_UPDATED || (ONLY_DRAW_IF_SCREEN_UPDATED && hasDrawn))
                { 
                    DrawScreenToConsole(true);
                    hasDrawn = false;
                }
                
                //Thread.Sleep(1);
            }*/
        }

        private void PrintRom(byte[] rom)
        {
            ushort buffer = 0;

            for (int i = 0; i < rom.Length; i++)
            {
                //buffer += Util.GetLeft4Bits(rom[i]).ToString("X");
                //buffer += Util.GetRight4Bits(rom[i]).ToString("X");

                if (buffer != 0)
                    buffer <<= 8;

                buffer |= rom[i];

                if (i % 2 == 1)
                {
                    Console.WriteLine($"{MEMORY_PROGRAM_START + i - 1}: {buffer.ToString("X")}");
                    buffer = 0;
                }
            }
        }

        public void Cycle()
        {
            ushort instruction = memory[registerProgramCounter];
            instruction <<= 8;
            instruction |= memory[registerProgramCounter + 1];
            registerProgramCounter += 2;

            if (instruction == 0x00E0)
            {
                CLS();
            }
            else if (instruction == 0x00EE)
            {
                RET();
            }
            else if ((instruction >> 12) == 0)
            {
                JumpToMachineCodeRoutine((ushort)(instruction * 0xFFF));
            }
            else if ((instruction >> 12) == 1)
            {
                Jump((ushort)(instruction & 0x0FFF));
            }
            else if ((instruction >> 12) == 2)
            {
                Call((ushort)(instruction & 0x0FFF));
            }
            else if ((instruction >> 12) == 3)
            {
                SkipIf((byte)((instruction & 0x0F00) >> 8), (byte)(instruction & 0x00FF));
            }
            else if ((instruction >> 12) == 4)
            {
                SkipIfNot((byte)((instruction & 0x0F00) >> 8), (byte)(instruction & 0x00FF));
            }
            else if ((instruction >> 12) == 5)
            {
                SkipIfRegister(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
            }
            else if ((instruction >> 12) == 6)
            {
                Set(Util.GetBits12To8(instruction), Util.GetBits8To0(instruction));
            }
            else if ((instruction >> 12) == 7)
            {
                AddToRegister(Util.GetBits12To8(instruction), Util.GetBits8To0(instruction));
            }
            else if ((instruction >> 12) == 8)
            {
                // One of 9 instructions that begin with 8.
                if ((instruction & 0x000F) == 0)
                    CopyRegister(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 1)
                    BitwiseOrRegister(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 2)
                    BitwiseAndRegister(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 3)
                    BitwiseXorRegister(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 4)
                    EIGHTXY4(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 5)
                    SUB(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 6)
                    SHR(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x000F) == 7)
                    SUBN(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else if ((instruction & 0x00F) == 0xE)
                    SHL(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
                else
                    throw new Exception("Instruction entered 8 branch but not executed, operation was not found.");
            }
            else if ((instruction >> 12) == 9)
            {
                SNE(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction));
            }
            else if ((instruction >> 12) == 0xA)
            {
                LD(Util.GetBits12To0(instruction));
            }
            else if ((instruction >> 12) == 0xB)
            {
                JP(Util.GetBits12To0(instruction));
            }
            else if ((instruction >> 12) == 0xC)
            {
                RND(Util.GetBits12To8(instruction), Util.GetBits8To0(instruction));
            }
            else if ((instruction >> 12) == 0xD)
            {
                DRW(Util.GetBits12To8(instruction), Util.GetBits8To4(instruction), Util.GetBits4To0(instruction));
            }
            else if ((instruction & 0xE09E) == 0xE09E)
            {
                SKP(Util.GetBits12To8(instruction));
            }
            else if ((instruction & 0xE0A1) == 0xE0A1)
            {
                SKNP(Util.GetBits12To8(instruction));
            }
            else if ((instruction >> 12) == 0xF)
            {
                // One of 9 instructions that start with F.

                if ((instruction & 0x00FF) == 0x07)
                    LDDT(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x0A)
                    LDK(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x15)
                    SetDelayTimer(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x18)
                    SetSoundTimer(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x1E)
                    ADDI(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x29)
                    FX29(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x33)
                    FX33(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x55)
                    RegistersToMemory(Util.GetBits12To8(instruction));
                else if ((instruction & 0x00FF) == 0x65)
                    MemoryToRegisters(Util.GetBits12To8(instruction));
                else
                    throw new Exception("Instruction entered F branch but not executed, operation was not found.");
            }
            else
            {
                throw new Exception("Instruction was not executed, operation was not found.");
            }

            if (registerSoundTimer > 0)
            {
                //new Thread(() => { Console.Beep(800, 16); }).Start();
                Console.Beep(800, 16);
                registerSoundTimer -= 1;
            }

            if (registerDelayTimer > 0)
            { 
                registerDelayTimer -= 1;
            }

            lastInstruction = instruction;
        }

        private void DrawScreenToConsole(bool drawDebugInfo)
        {
            string screenBuffer = "";
            string buffer = "";

            Console.CursorVisible = false;

            /*for (int r = 0; r < DISPLAY_ROWS; r++)
            {
                for (int c = 0; c < DISPLAY_COLUMNS; c++)
                {
                    buffer += display[r, c] ? " O " : "   ";
                }

                //Console.WriteLine(buffer);
                screenBuffer += buffer + Environment.NewLine;
                buffer = "";
            }*/
            
            for (int y = 0; y < DISPLAY_ROWS; y += 2)
            {
                for (int x = 0; x < DISPLAY_COLUMNS; ++x)
                {
                    bool upperPixel = display[y, x];
                    bool lowerPixel = display[y + 1, x];

                    String s = "░";

                    if (upperPixel && lowerPixel)
                    {
                        s = "█";
                    }
                    else if (upperPixel)
                    {
                        s = "▀";
                    }
                    else if (lowerPixel)
                    {
                        s = "▄";
                    }
                    else
                    {
                        s = " ";
                    }

                    screenBuffer += s;
                    //tg.putString(x, y / 2, s);
                }

                screenBuffer += Environment.NewLine;
            }

            if (drawDebugInfo)
            { 
                for (int i = 0; i < REGISTER_LENGTH; i++)
                {
                    //Console.WriteLine($"Register {i.ToString("X")}: {registers[i]} (Hex {registers[i].ToString("X")})");
                    screenBuffer += ($"Register {i.ToString("X")}: {registers[i]} (Hex {registers[i].ToString("X")}){Environment.NewLine}");
                }

                /*Console.WriteLine($"Program Counter: {registerProgramCounter} (Hex {registerProgramCounter.ToString("X")})");
                Console.WriteLine($"Register I: {registerI} (Hex {registerI.ToString("X")})");
                Console.WriteLine($"Register Delay Timer: {registerDelayTimer} (Hex {registerDelayTimer.ToString("X")})");
                Console.WriteLine($"Register Sound Timer: {registerSoundTimer} (Hex {registerSoundTimer.ToString("X")})");*/

                screenBuffer += ($"Program Counter: {registerProgramCounter} (Hex {registerProgramCounter.ToString("X")}){Environment.NewLine}");
                screenBuffer += ($"Register I: {registerI} (Hex {registerI.ToString("X")}){Environment.NewLine}");
                screenBuffer += ($"Register Delay Timer: {registerDelayTimer} (Hex {registerDelayTimer.ToString("X")}){Environment.NewLine}");
                screenBuffer += ($"Register Sound Timer: {registerSoundTimer} (Hex {registerSoundTimer.ToString("X")}){Environment.NewLine}");
                screenBuffer += ($"Last Instruction: {lastInstruction} (Hex {lastInstruction.ToString("X")}){Environment.NewLine}");
                //screenBuffer += ($"Console.KeyAvailable: {Console.KeyAvailable}{Environment.NewLine}");
                //screenBuffer += ($"Console.ReadKeyss: {(Console.KeyAvailable ? Console.ReadKey(true).Key.ToString() : "None")}{Environment.NewLine}");
            }

            //Console.Clear();
            int top = 0;
            int left = 0;
            for (int i = 0; i < screenBuffer.Length; i++)
            {
                if (screenBuffer.Substring(i, 2) == Environment.NewLine)
                {
                    top += 1;
                    left = 0;
                    i += 1;
                }
                else
                {
                    Console.SetCursorPosition(left, top);
                    Console.Write(screenBuffer[i]);
                    left += 1;
                }
            }
            //Console.WriteLine(screenBuffer);
        }

        private byte? GetPressedKey()
        {
            for (int i = 0; i < 16; i++)
            {
                if (Program.input[i])
                    return (byte?)i;
            }

            return null;
            /*if (Console.KeyAvailable)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);

                if (cki.Key >= ConsoleKey.D0 && cki.Key <= ConsoleKey.D9)
                {
                    return (byte)(cki.Key - ConsoleKey.D0);
                }
                else if (cki.Key >= ConsoleKey.A && cki.Key <= ConsoleKey.F)
                {
                    return (byte)(cki.Key - ConsoleKey.A);
                }
                else
                    return null;
            }
            else
                return null;*/
        }

        private bool IsKeyPressed(byte key)
        {
            if (15 < key || key < 0)
                throw new ArgumentException("Key value out of range, ");

            return Program.input[key];
        }

        /* Variables:
         * nnn or addr - A 12-bit value, the lowest 12 bits of the instruction
         * n or nibble - A 4-bit value, the lowest 4 bits of the instruction
         * x - A 4-bit value, the lower 4 bits of the high byte of the instruction
         * y - A 4-bit value, the upper 4 bits of the low byte of the instruction
         * kk or byte - An 8-bit value, the lowest 8 bits of the instruction
         */

        /// <summary>
        /// 0nnn - SYS addr
        /// Jump to a machine code routine at nnn.
        /// </summary>
        private void JumpToMachineCodeRoutine(ushort addr)
        {
            //TODO.
        }

        /// <summary>
        /// 00E0 - CLS
        /// Clear the display.
        /// </summary>
        private void CLS()
        {
            for (int r = 0; r < DISPLAY_ROWS; r++)
            {
                for (int c = 0; c < DISPLAY_COLUMNS; c++)
                {
                    display[r, c] = false;
                }
            }
        }

        /// <summary>
        /// 00EE - RET
        /// Return from a subroutine.
        /// The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
        /// </summary>
        private void RET()
        {
            //The correct return position is stored in the stack so this will work just fine.
            registerProgramCounter = stack.Pop();
        }

        /// <summary>
        /// 1nnn - JP addr
        /// The interpreter sets the program counter to nnn.
        /// </summary>
        private void Jump(ushort nnn)
        {
            registerProgramCounter = nnn;
        }

        /// <summary>
        /// 2nnn - CALL addr
        /// Call subroutine at nnn.
        /// The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
        /// </summary>
        private void Call(ushort nnn)
        {
            //registerProgramCounter will be incremented by 2 before this method is called, so we are storing the return position.
            stack.Push(registerProgramCounter);
            registerProgramCounter = nnn;
        }

        /// <summary>
        /// 3xkk - SE Vx, byte
        /// Skip next instruction if Vx = kk.
        /// The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.
        /// </summary>
        private void SkipIf(byte register, byte value)
        {
            if (registers[register] == value)
            {
                registerProgramCounter += 2;
            }
        }

        /// <summary>
        /// 4xkk - SNE Vx, byte
        /// Skip next instruction if Vx != kk.
        /// The interpreter compares register Vx to kk, and if they are not equal, increments the program counter by 2.
        /// </summary>
        private void SkipIfNot(byte register, byte value)
        {
            if (registers[register] != value)
            {
                registerProgramCounter += 2;
            }
        }

        /// <summary>
        /// 5xy0 - SE Vx, Vy
        /// Skip next instruction if Vx = Vy.
        /// The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2.
        /// </summary>
        private void SkipIfRegister(byte register1, byte register2)
        {
            if (registers[register1] == registers[register2])
            {
                registerProgramCounter += 2;
            }
        }

        /// <summary>
        /// 6xkk - LD Vx, byte
        /// Set Vx = kk.
        /// The interpreter puts the value kk into register Vx.
        /// </summary>
        private void Set(byte register, byte value)
        {
            registers[register] = value;
        }

        /// <summary>
        /// 7xkk - ADD Vx, byte
        /// Set Vx = Vx + kk.
        /// Adds the value kk to the value of register Vx, then stores the result in Vx.
        /// </summary>
        private void AddToRegister(byte register, byte amount)
        {
            registers[register] += amount;
        }

        /// <summary>
        /// 8xy0 - LD Vx, Vy
        /// Set Vx = Vy.
        /// Stores the value of register Vy in register Vx.
        /// </summary>
        private void CopyRegister(byte regX, byte regY)
        {
            registers[regX] = registers[regY];
        }

        /// <summary>
        /// 8xy1 - OR Vx, Vy
        /// Set Vx = Vx OR Vy.
        /// Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
        /// </summary>
        private void BitwiseOrRegister(byte reg1, byte reg2)
        {
            registers[reg1] = (byte)(registers[reg1] | registers[reg2]);
        }

        /// <summary>
        /// 8xy2 - AND Vx, Vy
        /// Set Vx = Vx AND Vy.
        /// Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
        /// </summary>
        private void BitwiseAndRegister(byte reg1, byte reg2)
        {
            registers[reg1] = (byte)(registers[reg1] & registers[reg2]);
        }

        /// <summary>
        /// 8xy3 - XOR Vx, Vy
        /// Set Vx = Vx XOR Vy.
        /// Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx.
        /// </summary>
        private void BitwiseXorRegister(byte reg1, byte reg2)
        {
            registers[reg1] = (byte)(registers[reg1] ^ registers[reg2]);
        }

        /// <summary>
        /// 8xy4 - ADD Vx, Vy
        /// Set Vx = Vx + Vy, set VF = carry.
        /// The values of Vx and Vy are added together.
        /// If the result is greater than 8 bits (> 255) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.
        /// </summary>
        // TODO: New name.
        private void EIGHTXY4(byte reg1, byte reg2)
        {
            ushort temp = (ushort)(registers[reg1] + registers[reg2]);
            registers[reg1] = Util.GetBits8To0(temp);

            if (temp > 255)
            {
                registers[REG_F] = 1;
            }
            else
            {
                registers[REG_F] = 0;
            }
        }

        /// <summary>
        /// 8xy5 - SUB Vx, Vy
        /// Set Vx = Vx - Vy, set VF = NOT borrow.
        /// If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
        /// </summary>
        private void SUB(byte regX, byte regY)
        {
            if (registers[regX] > registers[regY])
            {
                registers[REG_F] = 1;
            }
            else
            {
                registers[REG_F] = 0;
            }

            registers[regX] = (byte)(registers[regX] - registers[regY]);
        }

        /// <summary>
        /// 8xy6 - SHR Vx {, Vy}
        /// Set Vx = Vx SHR 1.
        /// If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
        /// </summary>
        private void SHR(byte reg1, byte reg2)
        {
            //TODO: Can be optimised, turn IsBitTrue result into an integer and store into register F.
            if (Util.GetBit(registers[reg1], 0))
            {
                registers[REG_F] = 1;
            }
            else
            {
                registers[REG_F] = 0;
            }
        }

        /// <summary>
        /// 8xy7 - SUBN Vx, Vy
        /// Set Vx = Vy - Vx, set VF = NOT borrow.
        /// If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
        /// </summary>
        private void SUBN(byte regX, byte regY)
        {
            if (registers[regY] > registers[regX])
            {
                registers[REG_F] = 1;
            }
            else
            {
                registers[REG_F] = 0;
            }

            registers[regX] = (byte)(registers[regY] - registers[regX]);
        }

        /// <summary>
        /// 8xyE - SHL Vx {, Vy}
        /// Set Vx = Vx SHL 1.
        /// If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
        /// </summary>
        private void SHL(byte regX, byte regY)
        {
            if (Util.GetBit(registers[regX], 7))
            {
                registers[REG_F] = 1;
            }
            else
            {
                registers[REG_F] = 0;
            }

            registers[regX] = (byte)(registers[regX] << 1);
        }

        /// <summary>
        /// 9xy0 - SNE Vx, Vy
        /// Skip next instruction if Vx != Vy.
        /// The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.
        /// </summary>
        private void SNE(byte regX, byte regY)
        {
            if (registers[regX] != registers[regY])
            {
                registerProgramCounter += 2;
            }
        }

        /// <summary>
        /// Annn - LD I, addr
        /// Set I = nnn.
        /// The value of register I is set to nnn.
        /// </summary>
        private void LD(ushort nnn)
        {
            registerI = nnn;
        }

        /// <summary>
        /// Bnnn - JP V0, addr
        /// Jump to location nnn + V0.
        /// The program counter is set to nnn plus the value of V0.
        /// </summary>
        private void JP(ushort nnn)
        {
            registerProgramCounter = (ushort)(nnn + registers[REG_0]);
        }

        /// <summary>
        /// Cxkk - RND Vx, byte
        /// Set Vx = random byte AND kk.
        /// The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk.
        /// The results are stored in Vx.
        /// </summary>
        private void RND(byte regX, byte kk)
        {
            random.NextBytes(randomByteContainer);
            registers[regX] = (byte)(randomByteContainer[0] & kk);
        }

        /// <summary>
        /// Dxyn - DRW Vx, Vy, nibble
        /// Display n-byte sprite starting at memory location I at(Vx, Vy), set VF = collision.
        /// The interpreter reads n bytes from memory, starting at the address stored in I.
        /// These bytes are then displayed as sprites on screen at coordinates(Vx, Vy).
        /// Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0.
        /// If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen.
        /// </summary>
        private void DRW(byte regX, byte regY, byte height)
        {
            //Load coords.
            byte x = registers[regX];
            byte y = registers[regY];

            //Load sprites in.
            byte[] spriteBuffer = new byte[height];

            for (int i = 0; i < height; i++)
            {
                spriteBuffer[i] = memory[registerI + i];
            }

            bool pixelWasErased = false;

            for (byte r = 0; r < spriteBuffer.Length; r++)
            {
                byte tr = Util.Wrap((byte)(r + y), DISPLAY_ROWS);

                for (byte c = 0; c < 8; c++)
                {
                    byte tc = Util.Wrap((byte)(c + x), DISPLAY_COLUMNS);

                    bool valueWas = display[tr, tc];

                    display[tr, tc] ^= Util.GetBit(spriteBuffer[r], 7 - c);

                    //If pixel was true and is now false
                    if (valueWas && !display[tr, tc])
                    {
                        pixelWasErased = true;
                    }
                }
            }

            registers[REG_F] = (byte)(pixelWasErased ? 1 : 0);

            hasDrawn = true;
        }

        /// <summary>
        /// Ex9E - SKP Vx
        /// Skip next instruction if key with the value of Vx is pressed.
        /// Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.
        /// </summary>
        private void SKP(byte regX)
        {
            if (IsKeyPressed(registers[regX]))
                registerProgramCounter += 2;
        }

        /// <summary>
        /// ExA1 - SKNP Vx
        /// Skip next instruction if key with the value of Vx is not pressed.
        /// Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.
        /// </summary>
        private void SKNP(byte regX)
        {
            if (!IsKeyPressed(registers[regX]))
                registerProgramCounter += 2;
        }

        /// <summary>
        /// Fx07 - LD Vx, DT
        /// Set Vx = delay timer value.
        /// The value of DT is placed into Vx.
        /// </summary>
        private void LDDT(byte regX)
        {
            registers[regX] = registerDelayTimer;
        }

        /// <summary>
        /// Fx0A - LD Vx, K
        /// Wait for a key press, store the value of the key in Vx.
        /// All execution stops until a key is pressed, then the value of that key is stored in Vx.
        /// </summary>
        private void LDK(byte regX)
        {
            byte? key = null;

            while ((key = GetPressedKey()) == null)
            {
                // Do nothing, just wait for a key to be pressed.
            }

            registers[regX] = key.Value;
        }

        /// <summary>
        /// Fx15 - LD DT, Vx
        /// Set delay timer = Vx.
        /// DT is set equal to the value of Vx.
        /// </summary>
        private void SetDelayTimer(byte regX)
        {
            registerDelayTimer = registers[regX];
        }

        /// <summary>
        /// Fx18 - LD ST, Vx
        /// Set sound timer = Vx.
        /// ST is set equal to the value of Vx.
        /// </summary>
        private void SetSoundTimer(byte regX)
        {
            registerSoundTimer = registers[regX];
        }

        /// <summary>
        /// Fx1E - ADD I, Vx
        /// Set I = I + Vx.
        /// The values of I and Vx are added, and the results are stored in I.
        /// </summary>
        private void ADDI(byte regX)
        {
            registerI += registers[regX];
        }

        /// <summary>
        /// Fx29 - LD F, Vx
        /// Set I = location of sprite for digit Vx.
        /// The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx.
        /// </summary>
        private void FX29(byte regX)
        {
            registerI = (ushort)(registers[regX] * 5);
        }

        /// <summary>
        /// Fx33 - LD B, Vx
        /// Store BCD representation of Vx in memory locations I, I+1, and I+2.
        /// The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at 
        /// location in I, the tens digit at location I+1, and the ones digit at location I+2.
        /// </summary>
        private void FX33(byte regX)
        {
            memory[registerI] = (byte)(registers[regX] / 100);
            memory[registerI + 1] = (byte)((registers[regX] / 10) % 10);
            memory[registerI + 2] = (byte)(registers[regX] % 10);
        }

        /// <summary>
        /// Fx55 - LD [I], Vx
        /// Store registers V0 through Vx in memory starting at location I.
        /// The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
        /// </summary>
        private void RegistersToMemory(byte regX)
        {
            //TODO: < regX or <= regX?
            for (int i = 0; i <= regX; i++)
            {
                memory[registerI + i] = registers[i];
            }

            registerI = (ushort)(registerI + regX + 1);
        }

        /// <summary>
        /// Fx65 - LD Vx, [I]
        /// Read registers V0 through Vx from memory starting at location I.
        /// The interpreter reads values from memory starting at location I into registers V0 through Vx.
        /// </summary>
        private void MemoryToRegisters(byte regX)
        {
            //TODO: < regX or <= regX?
            for (int i = 0; i <= regX; i++)
            {
                registers[i] = memory[registerI + i];
            }

            registerI = (ushort)(registerI + regX + 1);
        }
    }
}
