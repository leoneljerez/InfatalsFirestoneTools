using System.Runtime.CompilerServices;

namespace InfatalsFirestoneTools.Services.Optimizer
{
    public struct XorShiftState(uint seed)
    {
        // Seed must not be 0
        public uint State = seed == 0 ? 1 : seed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble()
        {
            State ^= State << 13;
            State ^= State >> 17;
            State ^= State << 5;
            // Convert to double in [0, 1) range
            return (State & 0xFFFFFF) / (double)16777216;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next(int minValue, int maxValue)
        {
            State ^= State << 13;
            State ^= State >> 17;
            State ^= State << 5;

            // Fast integer mapping
            uint range = (uint)(maxValue - minValue);
            return minValue + (int)(State % range);
        }
    }
}
