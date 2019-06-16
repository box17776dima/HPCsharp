﻿// TODO: Implement not just Zero detection, but detection of any constant value, specified by the user.
// TODO: Implement constant detection - detect whether the array is a constant or not.
// TODO: Unroll the SIMD/SSE loop to check intermediate values to stop early if not zero.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ZeroDetect
    {
#if false
        // https://stackoverflow.com/questions/33294580/sse-instruction-to-check-if-byte-array-is-zeroes-c-sharp
        static unsafe bool BySimdUnrolled(byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (16 * 16);
                Vector16b* b = (Vector16b*)bytes;
                Vector16b* e = (Vector16b*)(bytes + len - rem);
                Vector16b zero = Vector16b.Zero;

                while (b < e)
                {
                    if ((*(b) | *(b + 1) | *(b + 2) | *(b + 3) | *(b + 4) |
                        *(b + 5) | *(b + 6) | *(b + 7) | *(b + 8) |
                        *(b + 9) | *(b + 10) | *(b + 11) | *(b + 12) |
                        *(b + 13) | *(b + 14) | *(b + 15)) != zero)
                        return false;
                    b += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data[len - 1 - i] != 0)
                        return false;

                return true;
            }
        }
#endif
        private static bool ZeroDetectSseInner(this byte[] arrayToOr, int l, int r)
        {
            var orVector   = new Vector<byte>(0);
            var ZeroVector = new Vector<byte>(0);
            int sseIndexEnd = l + ((r - l + 1) / Vector<byte>.Count) * Vector<byte>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<byte>.Count)
            {
                var inVector = new Vector<byte>(arrayToOr, i);
                orVector |= inVector;
                if (orVector != ZeroVector)
                    return false;
            }
            byte overallOr = 0;
            for (; i <= r; i++)
                overallOr |= arrayToOr[i];
            for (i = 0; i < Vector<byte>.Count; i++)
                overallOr |= orVector[i];
            return overallOr == 0;
        }

        public static bool ZeroValueDetectSse(this byte[] arrayToDetect)
        {
            return arrayToDetect.ZeroDetectSseInner(0, arrayToDetect.Length - 1);
        }

        private static bool ZeroDetectSseInner2(this byte[] arrayToOr, int l, int r)
        {
            var orVector1 = new Vector<byte>(0);
            var orVector2 = new Vector<byte>(0);
            var orVector3 = new Vector<byte>(0);
            int concurrentAmount = 3;
            int sseIndexEnd = l + ((r - l + 1) / (Vector<byte>.Count * concurrentAmount)) * (Vector<byte>.Count * concurrentAmount);
            int i, j, k;
            int increment = Vector<byte>.Count * concurrentAmount;
            for (i = l, j = l + Vector<byte>.Count, k = l + Vector<byte>.Count + Vector<byte>.Count; i < sseIndexEnd; i += increment, j += increment, k += increment)
            {
                var inVector1 = new Vector<byte>(arrayToOr, i);
                var inVector2 = new Vector<byte>(arrayToOr, j);
                var inVector3 = new Vector<byte>(arrayToOr, k);
                orVector1 |= inVector1;
                orVector2 |= inVector2;
                orVector3 |= inVector3;
            }
            byte overallOr = 0;
            for (; i <= r; i++)
                overallOr |= arrayToOr[i];
            orVector1 |= orVector2;
            orVector1 |= orVector3;
            for (i = 0; i < Vector<byte>.Count; i++)
                overallOr |= orVector1[i];
            return overallOr == 0;
        }

        public static bool ZeroValueDetectSse2(this byte[] arrayToDetect)
        {
            return arrayToDetect.ZeroDetectSseInner2(0, arrayToDetect.Length - 1);
        }

        public static bool ByFor(byte[] data)
        {
            // Local variable is accessed faster than a property, and loop multiplies the difference.
            int len = data.Length;
            for (int i = 0; i < len; i++)
                if (data[i] != 0)
                    return false;
            return true;
        }

        public static bool ByForUnrolled(byte[] data)
        {
            // Bitwise-or instead of addition does not gain performance.
            int len = (data.Length / 16) * 16;
            int rem = len % (sizeof(long) * 16);

            for (int i = 0; i < len; i += 16)
            {
                if ((data[i] | data[i + 1] | data[i + 2] | data[i + 3] |
                    data[i + 4] | data[i + 5] | data[i + 6] | data[i + 7] |
                    data[i + 8] | data[i + 9] | data[i + 10] | data[i + 11] |
                    data[i + 12] | data[i + 13] | data[i + 14] | data[i + 15]) != 0)
                    return false;
            }
            for (int i = 0; i < rem; i++)
                if (data[len - 1 - i] != 0)
                    return false;
            return true;
        }
        // Stackoverflow post claims this version is faster than the SIMD implementation above
        // Opinion: One unfairness in comparison is that with pointers long (8-byte type) can be used instead of a single byte, which is 8X acceleration.
        //          Byte pointer versus Byte[] is a bit over 2X speedup, when both are unrolled.
        // Thoughts: SIMD/SSE should help, since SIMD/SSE is 256-bit (32 bytes) and possibly 512-bit (64 bytes). SIMD that was used in the stackoverflow write up was 16 byte (128-bit).
        //           Thus, we should be able to beat scalar results, even pointers by using 32 byte and 64 byte SIMD/SSE
        //           Unrolling helps by about 2X
        public static unsafe bool ByFixedLongUnrolled(byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (sizeof(long) * 16);
                long* b = (long*)bytes;
                long* e = (long*)(bytes + len - rem);

                while (b < e)
                {
                    if ((*(b)     | *(b +  1) | *(b +  2) | *(b +  3) | *(b + 4) |
                        *(b +  5) | *(b +  6) | *(b +  7) | *(b +  8) |
                        *(b +  9) | *(b + 10) | *(b + 11) | *(b + 12) |
                        *(b + 13) | *(b + 14) | *(b + 15)) != 0)
                        return false;
                    b += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data[len - 1 - i] != 0)
                        return false;

                return true;
            }
        }
    }
}
