#pragma once

#include "IStackItem.h"
#include "MapStackItem.h"
#include "ArrayStackItem.h"
#include "ExecutionEngine.h"
#include "ExecutionContextStack.h"

#if defined(_WINDOWS)
// Windows
#define DllExport __declspec( dllexport )
#elif defined (__GNUC__)
// GCC
#define DllExport __attribute__((visibility("default"))) 
#endif

extern "C"
{
	// Get version

	DllExport void __stdcall GetVersion(int32 &major, int32 &minor, int32 &build, int32 &revision);

	// ExecutionContext

	DllExport int32 __stdcall ExecutionContext_GetScriptHash(ExecutionContext* context, byte *output, int32 index);
	DllExport EVMOpCode __stdcall ExecutionContext_GetNextInstruction(ExecutionContext* context);
	DllExport int32 __stdcall ExecutionContext_GetInstructionPointer(ExecutionContext* context);
	DllExport void __stdcall ExecutionContext_Claim(ExecutionContext* context, StackItems* &evStack, StackItems* &altStack);
	DllExport void __stdcall ExecutionContext_Free(ExecutionContext* &context);

	// ExecutionEngine

	DllExport ExecutionEngine* __stdcall ExecutionEngine_Create
	(
		InvokeInteropCallback interopCallback, LoadScriptCallback getScriptCallback, GetMessageCallback getMessageCallback,
		ExecutionContextStack* &invStack, StackItems* &resStack
	);
	DllExport void __stdcall ExecutionEngine_Free(ExecutionEngine* &engine);
	DllExport void __stdcall ExecutionEngine_Clean(ExecutionEngine* engine, uint32 iteration);
	DllExport int32 __stdcall ExecutionEngine_LoadScript(ExecutionEngine* engine, byte * script, int32 scriptLength, int32 rvcount);
	DllExport byte __stdcall ExecutionEngine_LoadCachedScript(ExecutionEngine* engine, int32 scriptIndex, int32 rvcount);
	DllExport byte __stdcall ExecutionEngine_Execute(ExecutionEngine* engine);
	DllExport void __stdcall ExecutionEngine_StepInto(ExecutionEngine* engine);
	DllExport void __stdcall ExecutionEngine_StepOver(ExecutionEngine* engine);
	DllExport void __stdcall ExecutionEngine_StepOut(ExecutionEngine* engine);
	DllExport int32 __stdcall ExecutionEngine_GetState(ExecutionEngine* engine);
	DllExport void __stdcall ExecutionEngine_AddLog(ExecutionEngine* engine, OnStepIntoCallback callback);

	// StackItems

	DllExport int32 __stdcall StackItems_Count(StackItems* stack);
	DllExport IStackItem* __stdcall StackItems_Pop(StackItems* stack);
	DllExport IStackItem* __stdcall StackItems_Peek(StackItems* stack, int32 index);
	DllExport void __stdcall StackItems_Push(StackItems* stack, IStackItem *item);
	DllExport int32 __stdcall StackItems_Drop(StackItems* stack, int32 count);
	DllExport void __stdcall StackItems_AddLog(StackItems* stack, OnStackChangeCallback callback);

	// ExecutionContextStack

	DllExport int32 __stdcall ExecutionContextStack_Count(ExecutionContextStack* stack);
	DllExport int32 __stdcall ExecutionContextStack_Drop(ExecutionContextStack* stack, int32 count);
	DllExport ExecutionContext* __stdcall ExecutionContextStack_Peek(ExecutionContextStack* stack, int32 index);
	DllExport void __stdcall ExecutionContextStack_AddLog(ExecutionContextStack* stack, OnStackChangeCallback callback);

	// StackItem

	DllExport IStackItem* __stdcall StackItem_Create(EStackItemType type, byte *data, int32 size);
	DllExport EStackItemType __stdcall StackItem_SerializeInfo(IStackItem* item, int32 &size);
	DllExport int32 __stdcall StackItem_Serialize(IStackItem* item, byte * output, int32 length);
	DllExport void __stdcall StackItem_Free(IStackItem* &item);

	// MapStackItem

	DllExport int32 __stdcall MapStackItem_Count(MapStackItem* map);
	DllExport void __stdcall MapStackItem_Clear(MapStackItem* map);
	DllExport byte __stdcall MapStackItem_Remove(MapStackItem* map, IStackItem* key);
	DllExport void __stdcall MapStackItem_Set(MapStackItem* map, IStackItem* key, IStackItem* value);
	DllExport IStackItem* __stdcall MapStackItem_Get(MapStackItem* map, IStackItem* key);
	DllExport IStackItem* __stdcall MapStackItem_GetKey(MapStackItem* map, int index);
	DllExport IStackItem* __stdcall MapStackItem_GetValue(MapStackItem* map, int index);

	// ArrayStackItem

	DllExport int32 __stdcall ArrayStackItem_Count(ArrayStackItem* array);
	DllExport void __stdcall ArrayStackItem_Clear(ArrayStackItem* array);
	DllExport IStackItem* __stdcall ArrayStackItem_Get(ArrayStackItem* array, int32 index);
	DllExport void __stdcall ArrayStackItem_Add(ArrayStackItem* array, IStackItem* item);
	DllExport void __stdcall ArrayStackItem_Set(ArrayStackItem* array, IStackItem* item, int32 index);
	DllExport int32 __stdcall ArrayStackItem_IndexOf(ArrayStackItem* array, IStackItem* item);
	DllExport void __stdcall ArrayStackItem_Insert(ArrayStackItem* array, IStackItem* item, int32 index);
	DllExport void __stdcall ArrayStackItem_RemoveAt(ArrayStackItem* array, int32 index);
}