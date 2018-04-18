using System;

namespace NeoVM.Interop.Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Native Library Info");
            Console.WriteLine("  Path: " + NeoVM.LibraryPath);
            Console.WriteLine("  Version: " + NeoVM.LibraryVersion);
            Console.WriteLine("");
        }
    }
}