using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Native;
using NeoVM.Interop.Types;
using System;
using System.IO;
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

        #region Core cache

        #region Delegates

        // https://www.codeproject.com/Tips/318140/How-to-make-a-callback-to-Csharp-from-C-Cplusplus

        #region Callbacks

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void OnStepIntoCallback(IntPtr item);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void OnStackChangeCallback(IntPtr item, int index, byte operation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate byte InvokeInteropCallback(string method);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate byte LoadScriptCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 20)]byte[] scriptHash);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int GetMessageCallback(uint iteration, out IntPtr script);
        #endregion

        // Shared

        internal delegate int delInt_Handle(IntPtr pointer);
        internal delegate void delVoid_Handle(IntPtr pointer);
        internal delegate byte delByte_Handle(IntPtr pointer);
        internal delegate IntPtr delHandle_Handle(IntPtr pointer);
        internal delegate void delVoid_RefHandle(ref IntPtr pointer);
        internal delegate int delInt_HandleInt(IntPtr pointer, int value);
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

        internal static delCreateExecutionEngine ExecutionEngine_Create;
        internal static delVoid_RefHandle ExecutionEngine_Free;
        internal static delVoid_HandleHandleInt ExecutionEngine_LoadScript;
        internal static delVoid_HandleHandleInt ExecutionEngine_LoadPushOnlyScript;
        internal static delByte_Handle ExecutionEngine_Execute;
        internal static delVoid_Handle ExecutionEngine_StepInto;
        internal static delVoid_Handle ExecutionEngine_StepOver;
        internal static delVoid_Handle ExecutionEngine_StepOut;
        internal static delByte_Handle ExecutionEngine_GetState;
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
        internal static delByte_HandleRefInt StackItem_SerializeDetails;
        internal static delInt_HandleHandleInt StackItem_SerializeData;
        internal static delVoid_RefHandle StackItem_Free;

        internal static delInt_Handle ArrayStackItem_Count;
        internal static delVoid_Handle ArrayStackItem_Clear;
        internal static delHandle_HandleInt ArrayStackItem_Get;
        internal static delVoid_HandleHandleInt ArrayStackItem_Set;
        internal static delVoid_HandleHandle ArrayStackItem_Add;
        internal static delInt_HandleHandle ArrayStackItem_IndexOf;
        internal static delVoid_HandleIntByte ArrayStackItem_RemoveAt;
        internal static delVoid_HandleHandleInt ArrayStackItem_Insert;

        internal static delInt_HandleHandleInt ExecutionContext_GetScriptHash;
        internal static delByte_Handle ExecutionContext_GetNextInstruction;
        internal static delInt_Handle ExecutionContext_GetInstructionPointer;
        internal static delVoid_RefHandle ExecutionContext_Free;
        internal static delVoid_Handle ExecutionContext_Claim;

        #endregion

        /// <summary>
        /// Cache calls
        /// </summary>
        static void CacheCalls()
        {
            // Engine

            ExecutionEngine_Create = Core.GetDelegate<delCreateExecutionEngine>("ExecutionEngine_Create");
            ExecutionEngine_Free = Core.GetDelegate<delVoid_RefHandle>("ExecutionEngine_Free");
            ExecutionEngine_LoadScript = Core.GetDelegate<delVoid_HandleHandleInt>("ExecutionEngine_LoadScript");
            ExecutionEngine_LoadPushOnlyScript = Core.GetDelegate<delVoid_HandleHandleInt>("ExecutionEngine_LoadPushOnlyScript");
            ExecutionEngine_GetState = Core.GetDelegate<delByte_Handle>("ExecutionEngine_GetState");
            ExecutionEngine_AddLog = Core.GetDelegate<delVoid_HandleOnStepIntoCallback>("ExecutionEngine_AddLog");

            ExecutionEngine_Execute = Core.GetDelegate<delByte_Handle>("ExecutionEngine_Execute");
            ExecutionEngine_StepInto = Core.GetDelegate<delVoid_Handle>("ExecutionEngine_StepInto");
            ExecutionEngine_StepOver = Core.GetDelegate<delVoid_Handle>("ExecutionEngine_StepOver");
            ExecutionEngine_StepOut = Core.GetDelegate<delVoid_Handle>("ExecutionEngine_StepOut");

            // ExecutionContext

            ExecutionContext_GetScriptHash = Core.GetDelegate<delInt_HandleHandleInt>("ExecutionContext_GetScriptHash");
            ExecutionContext_GetNextInstruction = Core.GetDelegate<delByte_Handle>("ExecutionContext_GetNextInstruction");
            ExecutionContext_GetInstructionPointer = Core.GetDelegate<delInt_Handle>("ExecutionContext_GetInstructionPointer");
            ExecutionContext_Free = Core.GetDelegate<delVoid_RefHandle>("ExecutionContext_Free");
            ExecutionContext_Claim = Core.GetDelegate<delVoid_Handle>("ExecutionContext_Claim");

            // Stacks

            StackItems_Count = Core.GetDelegate<delInt_Handle>("StackItems_Count");
            StackItems_Push = Core.GetDelegate<delVoid_HandleHandle>("StackItems_Push");
            StackItems_Pop = Core.GetDelegate<delHandle_Handle>("StackItems_Pop");
            StackItems_Drop = Core.GetDelegate<delInt_HandleInt>("StackItems_Drop");
            StackItems_Peek = Core.GetDelegate<delHandle_HandleInt>("StackItems_Peek");
            StackItems_AddLog = Core.GetDelegate<delVoid_HandleOnStackChangeCallback>("StackItems_AddLog");

            ExecutionContextStack_Count = Core.GetDelegate<delInt_Handle>("ExecutionContextStack_Count");
            ExecutionContextStack_Drop = Core.GetDelegate<delInt_HandleInt>("ExecutionContextStack_Drop");
            ExecutionContextStack_Peek = Core.GetDelegate<delHandle_HandleInt>("ExecutionContextStack_Peek");
            ExecutionContextStack_AddLog = Core.GetDelegate<delVoid_HandleOnStackChangeCallback>("ExecutionContextStack_AddLog");

            // StackItems

            StackItem_Create = Core.GetDelegate<delHandle_ByteHandleInt>("StackItem_Create");
            StackItem_SerializeData = Core.GetDelegate<delInt_HandleHandleInt>("StackItem_SerializeData");
            StackItem_SerializeDetails = Core.GetDelegate<delByte_HandleRefInt>("StackItem_SerializeDetails");
            StackItem_Free = Core.GetDelegate<delVoid_RefHandle>("StackItem_Free");

            // ArrayStackItem

            ArrayStackItem_Count = Core.GetDelegate<delInt_Handle>("ArrayStackItem_Count");
            ArrayStackItem_Clear = Core.GetDelegate<delVoid_Handle>("ArrayStackItem_Clear");
            ArrayStackItem_Get = Core.GetDelegate<delHandle_HandleInt>("ArrayStackItem_Get");
            ArrayStackItem_Set = Core.GetDelegate<delVoid_HandleHandleInt>("ArrayStackItem_Set");
            ArrayStackItem_Add = Core.GetDelegate<delVoid_HandleHandle>("ArrayStackItem_Add");
            ArrayStackItem_IndexOf = Core.GetDelegate<delInt_HandleHandle>("ArrayStackItem_IndexOf");
            ArrayStackItem_Insert = Core.GetDelegate<delVoid_HandleHandleInt>("ArrayStackItem_Insert");
            ArrayStackItem_RemoveAt = Core.GetDelegate<delVoid_HandleIntByte>("ArrayStackItem_RemoveAt");
        }

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
                case PlatformID.WinCE: Core = new WindowsCore(); break;
                case PlatformID.Unix: Core = new UnixCore(); break;
                case PlatformID.MacOSX: Core = new MacCore(); break;
            }

            // Check core
            if (Core == null)
                throw (new NotSupportedException("Native library not found"));

            // Load library
            string file = Path.Combine(AppContext.BaseDirectory, Core.Platform.ToString(),
                Core.Architecture.ToString(), LibraryName + Core.LibraryExtension);

            // Check Environment path
            if (!File.Exists(file))
            {
                string nfile = Environment.GetEnvironmentVariable("NEO_HYPERVM_PATH");

                if (string.IsNullOrEmpty(nfile))
                    throw (new FileNotFoundException(file));

                file = nfile;
                if (!File.Exists(file))
                    throw (new FileNotFoundException(file));
            }

            if (!Core.LoadLibrary(file))
            {
                throw (new ArgumentException("Wrong library file: " + file));
            }

            // Static destructor
            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                if (Core != null)
                {
                    Core.Dispose();
                }
            };

            CacheCalls();
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