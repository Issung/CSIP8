using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSIP8
{
    public static class Util
    {
        /// <summary>
        /// Check if a bit in a byte. Returns true for 1, returns false for 0.
        /// </summary>
        /// <param name="b">Byte to check.</param>
        /// <param name="pos">The bit to retrieve. 0 is least significant bit.</param>
        public static bool GetBit(byte b, int pos)
        {
            if (7 < pos || pos < 0)
                throw new ArgumentException("GetBit pos out of range, should be between 0 and 7");

            return (b & (1 << pos)) != 0;
        }

        /// <summary>
        /// Get the 4 most significant bits of a byte.
        /// Example: GetLeft4Bits(0b1001 0101) returns 0b1001.
        /// Decimal values:           149                9
        /// </summary>
        public static byte GetLeft4Bits(byte b)
        {
            return (byte)((b & 0b1111_0000) >> 4);
        }

        /// <summary>
        /// Get the 4 least significant bits of a byte.
        /// Example: GetLeft4Bits(0b1001 0101) returns 0b0101.
        /// Decimal values:           149                5
        /// </summary>
        public static byte GetRight4Bits(byte b)
        {
            return (byte)(b & 0b0000_1111);
        }
    }
}
