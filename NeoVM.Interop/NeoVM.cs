using NeoVM.Interop.Enums;
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
        internal delegate void OnStackChangeCallback(IntPtr item, int index, ELogStackOperation operation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate byte InvokeInteropCallback(string method);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int GetScriptCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 20)]byte[] scriptHash, out IntPtr script);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int GetMessageCallback(uint iteration, out IntPtr script);
        #endregion

        internal delegate IntPtr delExecutionEngine_Create
            (
            InvokeInteropCallback interopCallback, GetScriptCallback scriptCallback, GetMessageCallback getMessageCallback,
            out IntPtr invocationHandle, out IntPtr evaluationHandle, out IntPtr altStack
            );
        internal delegate void delExecutionEngine_LoadScript(IntPtr handle, IntPtr script, int scriptLength);
        internal delegate void delExecutionEngine_LoadPushOnlyScript(IntPtr handle, IntPtr script, int scriptLength);
        internal delegate EVMState delExecutionEngine_Execute(IntPtr handle);
        internal delegate void delExecutionEngine_StepInto(IntPtr handle);
        internal delegate EVMState delExecutionEngine_GetState(IntPtr handle);

        internal delegate int delStackItems_Count(IntPtr stackHandle);
        internal delegate void delStackItems_Push(IntPtr stackHandle, IntPtr item);
        internal delegate IntPtr delStackItems_Pop(IntPtr stackHandle);
        internal delegate IntPtr delStackItems_Peek(IntPtr stackHandle, int index);
        internal delegate int delStackItems_Drop(IntPtr stackHandle, int count);

        internal delegate int delExecutionContextStack_Drop(IntPtr stackHandle, int count);

        internal delegate IntPtr delStackItem_Create(EStackItemType type, IntPtr data, int size);
        internal delegate EStackItemType delStackItem_SerializeDetails(IntPtr item, out int size);
        internal delegate int delStackItem_SerializeData(IntPtr item, IntPtr data, int length);
        internal delegate void delStackItem_Free(ref IntPtr item);

        internal delegate void delStack_AddLog(IntPtr item, OnStackChangeCallback callback);

        internal delegate int delArrayStackItem_Count(IntPtr handle);
        internal delegate void delArrayStackItem_Clear(IntPtr handle);
        internal delegate IntPtr delArrayStackItem_Get(IntPtr handle, int index);
        internal delegate void delArrayStackItem_Set(IntPtr handle, int index, IntPtr item);
        internal delegate void delArrayStackItem_Add(IntPtr handle, IntPtr item);
        internal delegate int delArrayStackItem_IndexOf(IntPtr handle, IntPtr item);
        internal delegate void delArrayStackItem_RemoveAt(IntPtr handle, int index, byte dispose);

        internal delegate int delExecutionContext_GetScriptHash(IntPtr handle, IntPtr data, int index);
        internal delegate EVMOpCode delExecutionContext_GetNextInstruction(IntPtr handle);
        internal delegate int delExecutionContext_GetInstructionPointer(IntPtr handle);

        #endregion

        #region Cache

        internal static delExecutionEngine_Create ExecutionEngine_Create;
        internal static delStackItem_Free ExecutionEngine_Free;
        internal static delExecutionEngine_LoadScript ExecutionEngine_LoadScript;
        internal static delExecutionEngine_LoadPushOnlyScript ExecutionEngine_LoadPushOnlyScript;
        internal static delExecutionEngine_Execute ExecutionEngine_Execute;
        internal static delExecutionEngine_StepInto ExecutionEngine_StepInto;
        internal static delExecutionEngine_StepInto ExecutionEngine_StepOver;
        internal static delExecutionEngine_StepInto ExecutionEngine_StepOut;
        internal static delExecutionEngine_GetState ExecutionEngine_GetState;

        internal static delStackItems_Count StackItems_Count;
        internal static delStackItems_Push StackItems_Push;
        internal static delStackItems_Pop StackItems_Pop;
        internal static delStackItems_Peek StackItems_Peek;
        internal static delStackItems_Drop StackItems_Drop;
        internal static delStack_AddLog StackItems_AddLog;

        internal static delStackItems_Count ExecutionContextStack_Count;
        internal static delExecutionContextStack_Drop ExecutionContextStack_Drop;
        internal static delStackItems_Peek ExecutionContextStack_Peek;
        internal static delStack_AddLog ExecutionContextStack_AddLog;

        internal static delStackItem_Create StackItem_Create;
        internal static delStackItem_SerializeDetails StackItem_SerializeDetails;
        internal static delStackItem_SerializeData StackItem_SerializeData;
        internal static delStackItem_Free StackItem_Free;

        internal static delArrayStackItem_Count ArrayStackItem_Count;
        internal static delArrayStackItem_Clear ArrayStackItem_Clear;
        internal static delArrayStackItem_Get ArrayStackItem_Get;
        internal static delArrayStackItem_Set ArrayStackItem_Set;
        internal static delArrayStackItem_Add ArrayStackItem_Add;
        internal static delArrayStackItem_IndexOf ArrayStackItem_IndexOf;
        internal static delArrayStackItem_RemoveAt ArrayStackItem_RemoveAt;
        internal static delArrayStackItem_Set ArrayStackItem_Insert;

        internal static delExecutionContext_GetScriptHash ExecutionContext_GetScriptHash;
        internal static delExecutionContext_GetNextInstruction ExecutionContext_GetNextInstruction;
        internal static delExecutionContext_GetInstructionPointer ExecutionContext_GetInstructionPointer;

        #endregion

        /// <summary>
        /// Cache calls
        /// </summary>
        static void CacheCalls()
        {
            // Engine

            ExecutionEngine_Create = Core.GetDelegate<delExecutionEngine_Create>("ExecutionEngine_Create");
            ExecutionEngine_Free = Core.GetDelegate<delStackItem_Free>("ExecutionEngine_Free");
            ExecutionEngine_LoadScript = Core.GetDelegate<delExecutionEngine_LoadScript>("ExecutionEngine_LoadScript");
            ExecutionEngine_LoadPushOnlyScript = Core.GetDelegate<delExecutionEngine_LoadPushOnlyScript>("ExecutionEngine_LoadPushOnlyScript");
            ExecutionEngine_GetState = Core.GetDelegate<delExecutionEngine_GetState>("ExecutionEngine_GetState");

            ExecutionEngine_Execute = Core.GetDelegate<delExecutionEngine_Execute>("ExecutionEngine_Execute");
            ExecutionEngine_StepInto = Core.GetDelegate<delExecutionEngine_StepInto>("ExecutionEngine_StepInto");
            ExecutionEngine_StepOver = Core.GetDelegate<delExecutionEngine_StepInto>("ExecutionEngine_StepOver");
            ExecutionEngine_StepOut = Core.GetDelegate<delExecutionEngine_StepInto>("ExecutionEngine_StepOut");

            // ExecutionContext

            ExecutionContext_GetScriptHash = Core.GetDelegate<delExecutionContext_GetScriptHash>("ExecutionContext_GetScriptHash");
            ExecutionContext_GetNextInstruction = Core.GetDelegate<delExecutionContext_GetNextInstruction>("ExecutionContext_GetNextInstruction");
            ExecutionContext_GetInstructionPointer = Core.GetDelegate<delExecutionContext_GetInstructionPointer>("ExecutionContext_GetInstructionPointer");

            // Stacks

            StackItems_Count = Core.GetDelegate<delStackItems_Count>("StackItems_Count");
            StackItems_Push = Core.GetDelegate<delStackItems_Push>("StackItems_Push");
            StackItems_Pop = Core.GetDelegate<delStackItems_Pop>("StackItems_Pop");
            StackItems_Drop = Core.GetDelegate<delStackItems_Drop>("StackItems_Drop");
            StackItems_Peek = Core.GetDelegate<delStackItems_Peek>("StackItems_Peek");
            StackItems_AddLog = Core.GetDelegate<delStack_AddLog>("StackItems_AddLog");

            ExecutionContextStack_Count = Core.GetDelegate<delStackItems_Count>("ExecutionContextStack_Count");
            ExecutionContextStack_Drop = Core.GetDelegate<delExecutionContextStack_Drop>("ExecutionContextStack_Drop");
            ExecutionContextStack_Peek = Core.GetDelegate<delStackItems_Peek>("ExecutionContextStack_Peek");
            ExecutionContextStack_AddLog = Core.GetDelegate<delStack_AddLog>("ExecutionContextStack_AddLog");

            // StackItems

            StackItem_Create = Core.GetDelegate<delStackItem_Create>("StackItem_Create");
            StackItem_SerializeData = Core.GetDelegate<delStackItem_SerializeData>("StackItem_SerializeData");
            StackItem_SerializeDetails = Core.GetDelegate<delStackItem_SerializeDetails>("StackItem_SerializeDetails");
            StackItem_Free = Core.GetDelegate<delStackItem_Free>("StackItem_Free");

            // ArrayStackItem

            ArrayStackItem_Count = Core.GetDelegate<delArrayStackItem_Count>("ArrayStackItem_Count");
            ArrayStackItem_Clear = Core.GetDelegate<delArrayStackItem_Clear>("ArrayStackItem_Clear");
            ArrayStackItem_Get = Core.GetDelegate<delArrayStackItem_Get>("ArrayStackItem_Get");
            ArrayStackItem_Set = Core.GetDelegate<delArrayStackItem_Set>("ArrayStackItem_Set");
            ArrayStackItem_Add = Core.GetDelegate<delArrayStackItem_Add>("ArrayStackItem_Add");
            ArrayStackItem_IndexOf = Core.GetDelegate<delArrayStackItem_IndexOf>("ArrayStackItem_IndexOf");
            ArrayStackItem_Insert = Core.GetDelegate<delArrayStackItem_Set>("ArrayStackItem_Insert");
            ArrayStackItem_RemoveAt = Core.GetDelegate<delArrayStackItem_RemoveAt>("ArrayStackItem_RemoveAt");
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
                file = Environment.GetEnvironmentVariable("NEOVM_PATH");
                if (!string.IsNullOrEmpty(file))
                    file = Path.Combine(file, LibraryName + Core.LibraryExtension);

                if (!File.Exists(file))
                    throw (new FileNotFoundException(file));
            }

            if (!Core.LoadLibrary(file))
            {
                throw (new FileNotFoundException(file));
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