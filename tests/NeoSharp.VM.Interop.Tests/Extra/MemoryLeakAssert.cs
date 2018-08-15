using System;

namespace NeoSharp.VM.Interop.Tests.Extra
{
    public class MemoryLeakAssert : IDisposable
    {
        /// <summary>
        /// Memory
        /// </summary>
        private readonly long Memory = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryLeakAssert()
        {
            Memory = GetTotalMemory();
        }

        private long GetTotalMemory()
        {
            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return GC.GetTotalMemory(true);
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            var after = GetTotalMemory();
            var result = after - Memory;
            var before = Console.ForegroundColor;

            if (result != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result);
                Console.ForegroundColor = before;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK!");
                Console.ForegroundColor = before;
            }
        }
    }
}