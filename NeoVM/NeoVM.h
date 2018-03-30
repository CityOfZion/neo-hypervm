#pragma once

#include "IStackItem.h"
#include "ArrayStackItem.h"
#include "ExecutionEngine.h"
#include "ExecutionContextStack.h"
#include "Callbacks.h"

#define DllExport __declspec( dllexport )  

extern "C"
{
	// ExecutionContext

	DllExport int ExecutionContext_GetScriptHash(ExecutionContext* context, unsigned char *output, int index);
	DllExport EVMOpCode ExecutionContext_GetNextInstruction(ExecutionContext* context);

	// ExecutionEngine

	DllExport ExecutionEngine* ExecutionEngine_Create
		(
		InvokeInteropCallback interopCallback, GetScriptCallback getScriptCallback, GetMessageCallback getMessageCallback,
		ExecutionContextStack* &invStack, StackItems* &evStack, StackItems* &altStack
		);
	DllExport void ExecutionEngine_Free(ExecutionEngine* &engine);
	DllExport void ExecutionEngine_LoadScript(ExecutionEngine* engine, unsigned char * script, int scriptLength);
	DllExport void ExecutionEngine_LoadPushOnlyScript(ExecutionEngine* engine, unsigned char * script, int scriptLength);
	DllExport __int8 ExecutionEngine_Execute(ExecutionEngine* engine);
	DllExport void ExecutionEngine_StepInto(ExecutionEngine* engine);
	DllExport void ExecutionEngine_StepOver(ExecutionEngine* engine);
	DllExport void ExecutionEngine_StepOut(ExecutionEngine* engine);
	DllExport int ExecutionEngine_GetState(ExecutionEngine* engine);

	// StackItems

	DllExport int StackItems_Count(StackItems* stack);
	DllExport IStackItem* StackItems_Pop(StackItems* stack, int count);
	DllExport IStackItem* StackItems_Peek(StackItems* stack, int index);
	DllExport void StackItems_Push(StackItems* stack, IStackItem *item);
	DllExport int StackItems_Drop(StackItems* stack, int count);

	// ExecutionContextStack

	DllExport int ExecutionContextStack_Count(ExecutionContextStack* stack);
	DllExport int ExecutionContextStack_Drop(ExecutionContextStack* stack, int count);
	DllExport ExecutionContext* ExecutionContextStack_Peek(ExecutionContextStack* stack, int index);

	// StackItem

	DllExport IStackItem* StackItem_Create(EStackItemType type, unsigned char *data, int size);
	DllExport EStackItemType StackItem_SerializeDetails(IStackItem* item, int &size);
	DllExport int StackItem_SerializeData(IStackItem* item, unsigned char * output, int length);
	DllExport void StackItem_Free(IStackItem* &item);

	// ArrayStackItem

	DllExport int ArrayStackItem_Count(ArrayStackItem* array);
	DllExport void ArrayStackItem_Clear(ArrayStackItem* array);
	DllExport IStackItem* ArrayStackItem_Get(ArrayStackItem* array, int index);
	DllExport void ArrayStackItem_Add(ArrayStackItem* array, IStackItem* item);
	DllExport void ArrayStackItem_Set(ArrayStackItem* array, int index, IStackItem* item);
	DllExport int ArrayStackItem_IndexOf(ArrayStackItem* array, IStackItem* item);
	DllExport void ArrayStackItem_Insert(ArrayStackItem* array, int index, IStackItem* item);
	DllExport void ArrayStackItem_RemoveAt(ArrayStackItem* array, int index, unsigned char dispose);
}