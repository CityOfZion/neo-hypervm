using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Memory = GetTotalMemory();
        }

        private long GetTotalMemory()
        {
            //var current = Process.GetCurrentProcess();
            //return  current.WorkingSet64;

            return GC.GetTotalMemory(true);
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var after = GetTotalMemory();

            Assert.AreEqual(0L, after - Memory);
        }
    }
}