﻿// TODO: Implement Sum algorithm for basic data types that not only uses multi-core, but also uses SIMD instructions, because that's a fun one as it can harness
//       data parallelism and multi-threading parallelism, and is a commonly used function. Can also harness computational unit parallelism (scalar and SIMD) in parallel.
//       and ILP. Average can be easily implemented too, since that depends on sum. Then some of the basic statistics could also be accelerated.using System;
// TODO: Provide a function to sum a field within a user defined type
// TODO: Sum should also provide two types: one for the data types being sumed and the other data type of the sum. For instance, sum up an array of longs, but use a double as the sum to not overflow.
//       Or, summ up an array of int32's, but use int64 for the sum to not overflow.
// TODO: Implement aligned SIMD sum, since memory alignment is critical for SIMD instructions. So, do scalar first until we are SIMD aligned and then do SIMD, followed by more scarlar to finish all
//       left over elements that are not SIMD size divisible.
// TODO: Contribute to Sum C# stackoverflow page, since nobody considered overflow condition and using a larger range values for sum
// TODO: Develop a method to split an array on a cache line (64 byte) boundary. Make it public.
// TODO: Change the partial array interface to (start, length) for consistency with others and standard C#
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        // TODO: use the l to r implementation here to have a single core implementation
        public static long SumSse(this int[] arrayToSum)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            int sseIndexEnd = (arrayToSum.Length / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = 0; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            long overallSum = 0;
            for (; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        public static long SumSse(this int[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        public static ulong SumSse(this uint[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<ulong>();
            var sumVectorUpper = new Vector<ulong>();
            var longLower      = new Vector<ulong>();
            var longUpper      = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<uint>.Count) * Vector<uint>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<uint>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        public static double SumSse(this float[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<double>();
            var sumVectorUpper = new Vector<double>();
            var longLower      = new Vector<double>();
            var longUpper      = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        private static long SumSseAndScalar(this int[] arrayToSum, int l, int r)
        {
            const int numScalarOps = 2;
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            int lengthForVector = (r - l + 1) / (Vector<int>.Count + numScalarOps) * Vector<int>.Count;
            int numFullVectors = lengthForVector / Vector<int>.Count;
            long partialSum0 = 0;
            long partialSum1 = 0;
            int i = l;
            int numScalarAdditions = (arrayToSum.Length - numFullVectors * Vector<int>.Count) / numScalarOps;
            int numIterations = System.Math.Min(numFullVectors, numScalarAdditions);
            int scalarIndex = l + numIterations * Vector<int>.Count;
            int sseIndexEnd = scalarIndex;
            //System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", arrayToSum.Length, lengthForVector, numFullVectors, numScalarAdditions, numIterations, scalarIndex);
            for (; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                partialSum0      += arrayToSum[scalarIndex++];          // interleave SSE and Scalar operations
                sumVectorLower   += longLower;
                partialSum1      += arrayToSum[scalarIndex++];
                sumVectorUpper   += longUpper;
            }
            for (i = scalarIndex; i <= r; i++)
                partialSum0 += arrayToSum[i];
            partialSum0    += partialSum1;
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                partialSum0 += sumVectorLower[i];
            return partialSum0;
        }

        public static int ThresholdParallelSum { get; set; } = 16 * 1024;

        public static long SumSsePar(this int[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSsePar(arrayToSum, l,     m); },
                () => { sumRight = SumSsePar(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            sumLeft += sumRight;
            return sumLeft;
        }

        public static ulong SumSsePar(this uint[] arrayToSum, int l, int r)
        {
            ulong sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r);

            int m = (r + l) / 2;

            ulong sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSsePar(arrayToSum, l,     m); },
                () => { sumRight = SumSsePar(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            sumLeft += sumRight;
            return sumLeft;
        }

        public static double SumSsePar(this float[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSsePar(arrayToSum, l,     m); },
                () => { sumRight = SumSsePar(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            sumLeft += sumRight;
            return sumLeft;
        }

        public static long SumSsePar(this int[] arrayToSum)
        {
            return SumSsePar(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static ulong SumSsePar(this uint[] arrayToSum)
        {
            return SumSsePar(arrayToSum, 0, arrayToSum.Length - 1);
        }

#if false
        public static void FillGenericSse<T>(this T[] arrayToFill, T value, int startIndex, int length) where T : struct
        {
            var fillVector = new Vector<T>(value);
            int numFullVectorsIndex = (length / Vector<T>.Count) * Vector<T>.Count;
            int i;
            for (i = startIndex; i < numFullVectorsIndex; i += Vector<T>.Count)
                fillVector.CopyTo(arrayToFill, i);
            for (; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void FillSse(this byte[] arrayToFill, byte value)
        {
            var fillVector = new Vector<byte>(value);
            int endOfFullVectorsIndex = (arrayToFill.Length / Vector<byte>.Count) * Vector<byte>.Count;
            ulong numBytesUnaligned = 0;
            unsafe
            {
                byte* ptrToArray = (byte *)arrayToFill[0];
                numBytesUnaligned = ((ulong)ptrToArray) & 63;
            }
            //Console.WriteLine("Pointer offset = {0}", numBytesUnaligned);
            int i;
            for (i = 0; i < endOfFullVectorsIndex; i += Vector<byte>.Count)
                fillVector.CopyTo(arrayToFill, i);
            for (; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void FillSse(this byte[] arrayToFill, byte value, int startIndex, int length)
        {
            var fillVector = new Vector<byte>(value);
            int endOfFullVectorsIndex, numBytesUnaligned, i = startIndex;
            unsafe
            {
                fixed (byte* ptrToArray = &arrayToFill[startIndex])
                {
                    numBytesUnaligned = (int)((ulong)ptrToArray & (ulong)(Vector<byte>.Count- 1));
                    int endOfByteUnaligned = (numBytesUnaligned == 0) ? 0 : Vector<byte>.Count;
                    int numBytesFilled = 0;
                    for (int j = numBytesUnaligned; j < endOfByteUnaligned; j++, i++, numBytesFilled++)
                    {
                        if (numBytesFilled < length)
                            arrayToFill[i] = value;
                        else
                            break;
                    }
                    endOfFullVectorsIndex = i + ((length - numBytesFilled) / Vector<byte>.Count) * Vector<byte>.Count;
                    //Console.WriteLine("Pointer offset = {0}  ptr = {1:X}  startIndex = {2}  i = {3} endIndex = {4} length = {5} lengthLeft = {6}",
                    //    numBytesUnaligned, (ulong)ptrToArray, startIndex, i, endOfFullVectorsIndex, length, length - numBytesFilled);
                    for (; i < endOfFullVectorsIndex; i += Vector<byte>.Count)
                        fillVector.CopyTo(arrayToFill, i);
                }
            }
            //Console.WriteLine("After fill using Vector, i = {0}", i);
            for (; i < startIndex + length; i++)
                arrayToFill[i] = value;
        }
#endif
    }
}
