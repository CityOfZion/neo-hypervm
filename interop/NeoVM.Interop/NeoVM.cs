using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Native;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.Arguments;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NeoVM.Interop
{
    public static class NeoVM
    {
        const string LibraryName = "NeoVM";

        /// <summary>
        /// Library core
        /// </summary>
        readonly static CrossPlatformLibrary Core;
        /// <summary>
        /// Library path
        /// </summary>
        public readonly static string LibraryPath;
        /// <summary>
        /// Version
        /// </summary>
        public readonly static Version LibraryVersion;

        #region Core cache

        #region Delegates

        // https://www.codeproject.com/Tips/318140/How-to-make-a-callback-to-Csharp-from-C-Cplusplus

        #region Callbacks

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void OnStepIntoCallback(IntPtr item);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void OnStackChangeCallback(IntPtr item, int index, byte operation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate byte InvokeInteropCallback(IntPtr ptr, byte size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate byte LoadScriptCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 20)]byte[] scriptHash, byte isDynamicInvoke);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int GetMessageCallback(uint iteration, out IntPtr script);
        #endregion

        // Shared

        internal delegate int delInt_Handle(IntPtr pointer);
        internal delegate void delVoid_Handle(IntPtr pointer);
        internal delegate byte delByte_Handle(IntPtr pointer);
        internal delegate IntPtr delHandle_Handle(IntPtr pointer);
        internal delegate void delVoid_RefHandle(ref IntPtr pointer);

        internal delegate void delVoid_OutIntOutIntOutIntOutInt(out int i1, out int i2, out int i3, out int i4);

        internal delegate int delInt_HandleInt(IntPtr pointer, int value);
        internal delegate void delVoid_HandleUInt(IntPtr pointer, uint value);
        internal delegate void delVoid_HandleInt(IntPtr pointer, int value);
        internal delegate int delInt_HandleHandle(IntPtr handle, IntPtr item);
        internal delegate byte delByte_HandleRefInt(IntPtr item, out int size);
        internal delegate IntPtr delHandle_HandleInt(IntPtr pointer, int value);
        internal delegate void delVoid_HandleHandle(IntPtr pointer1, IntPtr pointer2);
        internal delegate IntPtr delHandle_ByteHandleInt(byte type, IntPtr data, int size);
        internal delegate void delVoid_HandleIntByte(IntPtr handle, int index, byte dispose);
        internal delegate int delInt_HandleHandleInt(IntPtr pointer1, IntPtr pointer2, int value);
        internal delegate void delVoid_HandleHandleInt(IntPtr pointer1, IntPtr pointer2, int value);

        // Specific

        internal delegate void delVoid_HandleOnStepIntoCallback(IntPtr handle, OnStepIntoCallback callback);
        internal delegate void delVoid_HandleOnStackChangeCallback(IntPtr item, OnStackChangeCallback callback);
        internal delegate IntPtr delCreateExecutionEngine
            (
            InvokeInteropCallback interopCallback, LoadScriptCallback scriptCallback, GetMessageCallback getMessageCallback,
            out IntPtr invocationHandle, out IntPtr evaluationHandle, out IntPtr altStack
            );

        #endregion

        #region Cache

#pragma warning disable CS0649
        internal static delVoid_OutIntOutIntOutIntOutInt GetVersion;

        internal static delCreateExecutionEngine ExecutionEngine_Create;
        internal static delVoid_RefHandle ExecutionEngine_Free;
        internal static delVoid_HandleHandleInt ExecutionEngine_LoadScript;
        internal static delVoid_HandleHandleInt ExecutionEngine_LoadPushOnlyScript;
        internal static delByte_Handle ExecutionEngine_Execute;
        internal static delVoid_Handle ExecutionEngine_StepInto;
        internal static delVoid_Handle ExecutionEngine_StepOver;
        internal static delVoid_Handle ExecutionEngine_StepOut;
        internal static delByte_Handle ExecutionEngine_GetState;
        internal static delVoid_HandleUInt ExecutionEngine_Clean;
        internal static delVoid_HandleOnStepIntoCallback ExecutionEngine_AddLog;

        internal static delInt_Handle StackItems_Count;
        internal static delVoid_HandleHandle StackItems_Push;
        internal static delHandle_Handle StackItems_Pop;
        internal static delHandle_HandleInt StackItems_Peek;
        internal static delInt_HandleInt StackItems_Drop;
        internal static delVoid_HandleOnStackChangeCallback StackItems_AddLog;

        internal static delInt_Handle ExecutionContextStack_Count;
        internal static delInt_HandleInt ExecutionContextStack_Drop;
        internal static delHandle_HandleInt ExecutionContextStack_Peek;
        internal static delVoid_HandleOnStackChangeCallback ExecutionContextStack_AddLog;

        internal static delHandle_ByteHandleInt StackItem_Create;
        internal static delByte_HandleRefInt StackItem_SerializeInfo;
        internal static delInt_HandleHandleInt StackItem_Serialize;
        internal static delVoid_RefHandle StackItem_Free;

        internal static delInt_Handle ArrayStackItem_Count;
        internal static delVoid_Handle ArrayStackItem_Clear;
        internal static delHandle_HandleInt ArrayStackItem_Get;
        internal static delVoid_HandleHandleInt ArrayStackItem_Set;
        internal static delVoid_HandleHandle ArrayStackItem_Add;
        internal static delInt_HandleHandle ArrayStackItem_IndexOf;
        internal static delVoid_HandleInt ArrayStackItem_RemoveAt;
        internal static delVoid_HandleHandleInt ArrayStackItem_Insert;

        internal static delInt_HandleHandleInt ExecutionContext_GetScriptHash;
        internal static delByte_Handle ExecutionContext_GetNextInstruction;
        internal static delInt_Handle ExecutionContext_GetInstructionPointer;
        internal static delVoid_RefHandle ExecutionContext_Free;
        internal static delVoid_Handle ExecutionContext_Claim;
#pragma warning restore CS0649

        #endregion

        #endregion

        /// <summary>
        /// Static constructor for load NativeCoreType
        /// </summary>
        static NeoVM()
        {
            // Detect OS
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    {
                        Core = new WindowsCore();
                        break;
                    }
                case PlatformID.Unix:
                case (PlatformID)128:
                    {
                        Core = new UnixCore();
                        break;
                    }
                case PlatformID.MacOSX:
                    {
                        Core = new MacCore();
                        break;
                    }
            }

            // Check core
            if (Core == null)
                throw (new NotSupportedException("Native library not found"));

            // Load library
            LibraryPath = Path.Combine(AppContext.BaseDirectory, Core.Platform.ToString(),
                Core.Architecture.ToString(), LibraryName + Core.LibraryExtension);

            // Check Environment path
            if (!File.Exists(LibraryPath))
            {
                string nfile = Environment.GetEnvironmentVariable("NEO_HYPERVM_PATH");

                if (string.IsNullOrEmpty(nfile))
                    throw (new FileNotFoundException(LibraryPath));

                LibraryPath = nfile;
                if (!File.Exists(LibraryPath))
                    throw (new FileNotFoundException(LibraryPath));
            }

            if (!Core.LoadLibrary(LibraryPath))
            {
                throw (new ArgumentException("Wrong library file: " + LibraryPath));
            }

            // Static destructor
            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                if (Core != null)
                {
                    Core.Dispose();
                }
            };

            // Cache delegates using reflection

            Type delegateType = typeof(MulticastDelegate);

            foreach (FieldInfo fi in typeof(NeoVM).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(fi => fi.FieldType.BaseType == delegateType))
            {
                Delegate del = Core.GetDelegate(fi.Name, fi.FieldType);

                if (del == null)
                    throw (new NotImplementedException(fi.Name));

                fi.SetValue(null, del);
            }

            // Get version

            GetVersion(out int major, out int minor, out int build, out int revision);
            LibraryVersion = new Version(major, minor, build, revision);
        }

        /// <summary>
        /// Create new Execution Engine
        /// </summary>
        /// <param name="e">Arguments</param>
        public static ExecutionEngine CreateEngine(ExecutionEngineArgs e)
        {
            return new ExecutionEngine(e);
        }
    }
}