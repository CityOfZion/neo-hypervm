#pragma once

#include "IStackItem.h"
#include "ArrayStackItem.h"
#include "ExecutionEngine.h"
#include "ExecutionContextStack.h"

#if _WINDOWS

#define DllExport __declspec( dllexport ) 

extern "C"
{
	// ExecutionContext

	DllExport int32 ExecutionContext_GetScriptHash(ExecutionContext* context, byte *output, int32 index);
	DllExport EVMOpCode ExecutionContext_GetNextInstruction(ExecutionContext* context);
	DllExport int32 ExecutionContext_GetInstructionPointer(ExecutionContext* context);

	// ExecutionEngine

	DllExport ExecutionEngine* ExecutionEngine_Create
	(
		InvokeInteropCallback interopCallback, GetScriptCallback getScriptCallback, GetMessageCallback getMessageCallback,
		ExecutionContextStack* &invStack, StackItems* &evStack, StackItems* &altStack
	);
	DllExport void ExecutionEngine_Free(ExecutionEngine* &engine);
	DllExport void ExecutionEngine_LoadScript(ExecutionEngine* engine, byte * script, int32 scriptLength);
	DllExport void ExecutionEngine_LoadPushOnlyScript(ExecutionEngine* engine, byte * script, int32 scriptLength);
	DllExport byte ExecutionEngine_Execute(ExecutionEngine* engine);
	DllExport void ExecutionEngine_StepInto(ExecutionEngine* engine);
	DllExport void ExecutionEngine_StepOver(ExecutionEngine* engine);
	DllExport void ExecutionEngine_StepOut(ExecutionEngine* engine);
	DllExport int32 ExecutionEngine_GetState(ExecutionEngine* engine);
	DllExport void ExecutionEngine_AddLog(ExecutionEngine* engine, OnStepIntoCallback callback);

	// StackItems

	DllExport int32 StackItems_Count(StackItems* stack);
	DllExport IStackItem* StackItems_Pop(StackItems* stack);
	DllExport IStackItem* StackItems_Peek(StackItems* stack, int32 index);
	DllExport void StackItems_Push(StackItems* stack, IStackItem *item);
	DllExport int32 StackItems_Drop(StackItems* stack, int32 count);
	DllExport void StackItems_AddLog(StackItems* stack, OnStackChangeCallback callback);

	// ExecutionContextStack

	DllExport int32 ExecutionContextStack_Count(ExecutionContextStack* stack);
	DllExport int32 ExecutionContextStack_Drop(ExecutionContextStack* stack, int32 count);
	DllExport ExecutionContext* ExecutionContextStack_Peek(ExecutionContextStack* stack, int32 index);
	DllExport void ExecutionContextStack_AddLog(ExecutionContextStack* stack, OnStackChangeCallback callback);

	// StackItem

	DllExport IStackItem* StackItem_Create(EStackItemType type, byte *data, int32 size);
	DllExport EStackItemType StackItem_SerializeDetails(IStackItem* item, int32 &size);
	DllExport int32 StackItem_SerializeData(IStackItem* item, byte * output, int32 length);
	DllExport void StackItem_Free(IStackItem* &item);

	// ArrayStackItem

	DllExport int32 ArrayStackItem_Count(ArrayStackItem* array);
	DllExport void ArrayStackItem_Clear(ArrayStackItem* array);
	DllExport IStackItem* ArrayStackItem_Get(ArrayStackItem* array, int32 index);
	DllExport void ArrayStackItem_Add(ArrayStackItem* array, IStackItem* item);
	DllExport void ArrayStackItem_Set(ArrayStackItem* array, int32 index, IStackItem* item);
	DllExport int32 ArrayStackItem_IndexOf(ArrayStackItem* array, IStackItem* item);
	DllExport void ArrayStackItem_Insert(ArrayStackItem* array, int32 index, IStackItem* item);
	DllExport void ArrayStackItem_RemoveAt(ArrayStackItem* array, int32 index, byte dispose);
}

# endif
