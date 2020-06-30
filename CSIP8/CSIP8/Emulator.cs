using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.2

namespace CSIP8
{
    class Emulator
    {
        const string ROM_FILENAME = "Roms/Pong.ch8";

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
        short registerI;

        short registerProgramCounter;

        short registerStackPointer;

        byte registerDelayTimer;

        byte registerSoundTimer;

        const int STACK_LENGTH = 16;
        short[] stack = new short[STACK_LENGTH];

        byte[,] characters = {
            { 0xF0, 0x90, 0x90, 0x90, 0xF0 }, // 0
            { 0x20, 0x60, 0x20, 0x20, 0x70 }, // 1
            { 0xF0, 0x10, 0xF0, 0x80, 0xF0 }, // 2
            { 0xF0, 0x10, 0xF0, 0x10, 0xF0 }, // 3
            { 0x90, 0x90, 0xF0, 0x10, 0x10 }, // 4
            { 0xF0, 0x80, 0xF0, 0x10, 0xF0 }, // 5
            { 0xF0, 0x80, 0xF0, 0x90, 0xF0 }, // 6
            { 0xF0, 0x10, 0x20, 0x40, 0x40 }, // 7
            { 0xF0, 0x90, 0xF0, 0x90, 0xF0 }, // 8
            { 0xF0, 0x90, 0xF0, 0x10, 0xF0 }, // 9
            { 0xF0, 0x90, 0xF0, 0x90, 0x90 }, // A
            { 0xE0, 0x90, 0xE0, 0x90, 0xE0 }, // B
            { 0xF0, 0x80, 0x80, 0x80, 0xF0 }, // C
            { 0xE0, 0x90, 0x90, 0x90, 0xE0 }, // D
            { 0xF0, 0x80, 0xF0, 0x80, 0xF0 }, // E
            { 0xF0, 0x80, 0xF0, 0x80, 0x80 }, // F
        };

        const int MEMORY_LENGTH = 4096;
        byte[] memory = new byte[MEMORY_LENGTH];

        const int DISPLAY_COLUMNS = 64;
        const int DISPLAY_ROWS = 32;

        /// <summary>
        /// Access with ROW, COLUMN.
        /// </summary>
        bool[,] display = new bool[DISPLAY_ROWS, DISPLAY_COLUMNS];

        byte[] rom;

        Random random;
        byte[] randomByteContainer = new byte[1];

        public Emulator()
        {
            rom = File.ReadAllBytes(ROM_FILENAME);
            PrintRom(rom);
            Console.WriteLine("Read rom complete.");
            Console.ReadLine();

            random = new Random();
        }

        public void Cycle()
        {
            
        }

        private void PrintRom(byte[] rom)
        {
            int buffer = 0;

            for (int i = 0; i < rom.Length; i++)
            {
                //buffer += Util.GetLeft4Bits(rom[i]).ToString("X");
                //buffer += Util.GetRight4Bits(rom[i]).ToString("X");

                if (buffer != 0)
                    buffer = buffer << 8;

                buffer |= rom[i];

                if (i % 2 == 1)
                {
                    Console.WriteLine(buffer.ToString("X"));
                    buffer = 0;
                }
            }
        }

        private byte? GetPressedKey()
        {
            //TODO
            return null;
        }

        private bool IsKeyPressed(byte key)
        {
            if (15 < key || key < 0)
                throw new ArgumentException("Key value out of range, ");

            //TODO.
            return false;
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
        private void JumpToMachineCodeRoutine(short addr)
        {

        }

        /// <summary>
        /// 00E - CLS
        /// Clear the display.
        /// </summary>
        private void Clear()
        {

        }

        /// <summary>
        /// 00EE - RET
        /// Return from a subroutine.
        /// The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
        /// </summary>
        private void Return()
        {

        }

        /// <summary>
        /// 1nnn - JP addr
        /// The interpreter sets the program counter to nnn.
        /// </summary>
        private void Jump(short addr)
        {

        }

        /// <summary>
        /// 2nnn - CALL addr
        /// Call subroutine at nnn.
        /// The interpreter increments the stack pointer, then puts the current PC on the top of the stack.The PC is then set to nnn.
        /// </summary>
        private void Call(short addr)
        {

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
        private void CopyRegister(byte from, byte to)
        {
            registers[to] = registers[from];
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
            registers[reg1] = (byte)(registers[reg1] + registers[reg2]);

            if (registers[reg1] > 255)
            {
                registers[REG_F] = 1;
            }
        }

        /// <summary>
        /// 8xy5 - SUB Vx, Vy
        /// Set Vx = Vx - Vy, set VF = NOT borrow.
        /// If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
        /// </summary>
        private void SUB(byte reg1, byte reg2)
        {
            registers[reg1] = (byte)(registers[reg1] - registers[reg2]);

            if (registers[reg1] > registers[reg2])
            {
                registers[REG_F] = 1;
            }
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
            registers[regX] = (byte)(registers[regY] - registers[regX]);

            if (registers[regY] > registers[regX])
            {
                registers[REG_F] = 1;
            }
            else
            {
                registers[REG_F] = 0;
            }
        }

        /// <summary>
        /// 8xyE - SHL Vx {, Vy}
        /// Set Vx = Vx SHL 1.
        /// If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
        /// </summary>
        private void SHL(byte regX)
        {
            if (Util.GetBit(regX, 7))
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
        private void LD(byte nnn)
        {
            registerI = nnn;
        }

        /// <summary>
        /// Bnnn - JP V0, addr
        /// Jump to location nnn + V0.
        /// The program counter is set to nnn plus the value of V0.
        /// </summary>
        private void JP(byte nnn)
        {
            registerProgramCounter = (short)(nnn + registers[REG_0]);
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
        private void DRW(byte regX, byte regY, byte n)
        {
            byte[] spriteBuffer = new byte[n];

            for (int i = 0; i < n; i++)
            {
                spriteBuffer[i] = rom[registerI + i];
            }

            bool pixelWasErased = false;

            for (int spriteIndex = 0; spriteIndex < spriteBuffer.Length; spriteIndex++)
            {
                int row = registers[regX] + spriteIndex;

                if (row > DISPLAY_ROWS)
                {
                    row -= DISPLAY_ROWS;
                }

                for (int spriteBitIndex = 0; spriteBitIndex < 8; spriteBitIndex--)
                {
                    int column = registers[regY] + spriteBitIndex;

                    if (column > DISPLAY_COLUMNS)
                    {
                        column -= DISPLAY_COLUMNS;
                    }

                    bool valueWas = display[row, column];

                    display[row, column] ^= Util.GetBit(spriteBuffer[spriteIndex], 7 - spriteBitIndex);

                    //If pixel was true and is now false
                    if (valueWas && !display[row, column])
                    {
                        pixelWasErased = true;
                    }
                }
            }

            registers[REG_F] = (byte)(pixelWasErased ? 1 : 0);
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
            //TODO: Probably incorrect.
            registerI = registers[regX];
        }

        /// <summary>
        /// Fx33 - LD B, Vx
        /// Store BCD representation of Vx in memory locations I, I+1, and I+2.
        /// The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at 
        /// location in I, the tens digit at location I+1, and the ones digit at location I+2.
        /// </summary>
        private void test(byte regX)
        {
            memory[registerI] = (byte)(registers[regX] / 100);
            memory[registerI + 1] = (byte)(registers[regX] / 10 % 10);
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
        }
    }
}
