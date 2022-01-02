using System.Threading;

namespace YWB.Helpers
{
    public static class StaticRandom
    {
        private static readonly ThreadLocal<CryptoRandom> ThreadLocal =
            new ThreadLocal<CryptoRandom>(() => new CryptoRandom());

        public static CryptoRandom Instance => ThreadLocal.Value;
    }
}