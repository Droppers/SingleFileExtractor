using System.IO.MemoryMappedFiles;

namespace SingleFileExtractor.Core.Helpers
{
    internal static class BinaryKmpSearch
    {
        public static unsafe long SearchInFile(MemoryMappedViewAccessor accessor, byte[] searchPattern)
        {
            var safeBuffer = accessor.SafeMemoryMappedViewHandle;
            return KmpSearch(searchPattern, (byte*)safeBuffer.DangerousGetHandle(), (long)safeBuffer.ByteLength);
        }

        // See: https://en.wikipedia.org/wiki/Knuth%E2%80%93Morris%E2%80%93Pratt_algorithm
        private static long[] ComputeKmpFailureFunction(byte[] pattern)
        {
            var table = new long[pattern.Length];
            if (pattern.Length >= 1)
            {
                table[0] = -1;
            }

            if (pattern.Length >= 2)
            {
                table[1] = 0;
            }

            var pos = 2L;
            var cnd = 0L;
            while (pos < pattern.Length)
            {
                if (pattern[pos - 1] == pattern[cnd])
                {
                    table[pos] = cnd + 1;
                    cnd++;
                    pos++;
                }
                else if (cnd > 0)
                {
                    cnd = table[cnd];
                }
                else
                {
                    table[pos] = 0;
                    pos++;
                }
            }

            return table;
        }

        // See: https://en.wikipedia.org/wiki/Knuth%E2%80%93Morris%E2%80%93Pratt_algorithm
        private static unsafe long KmpSearch(byte[] pattern, byte* bytes, long bytesLength)
        {
            var m = 0L;
            var i = 0L;
            var table = ComputeKmpFailureFunction(pattern);

            while (m + i < bytesLength)
            {
                if (pattern[i] == bytes[m + i])
                {
                    if (i == pattern.Length - 1)
                    {
                        return m;
                    }

                    i++;
                }
                else
                {
                    if (table[i] > -1)
                    {
                        m = m + i - table[i];
                        i = table[i];
                    }
                    else
                    {
                        m++;
                        i = 0;
                    }
                }
            }

            return -1;
        }
    }
}