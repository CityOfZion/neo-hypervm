#include "NeoVM.h"
#include "BoolStackItem.h"
#include "IntegerStackItem.h"
#include "ByteArrayStackItem.h"
#include "InteropStackItem.h"
#include "ArrayStackItem.h"
#include "MapStackItem.h"

// ExecutionContext

int32 ExecutionContext_GetScriptHash(ExecutionContext* context, byte* output, int32 index)
{
	return context->GetScriptHash(&output[index]);
}

EVMOpCode ExecutionContext_GetNextInstruction(ExecutionContext* context)
{
	return context->GetNextInstruction();
}

int32 ExecutionContext_GetInstructionPointer(ExecutionContext* context)
{
	return context->InstructionPointer;
}

void ExecutionContext_Claim(ExecutionContext* context)
{
	if (context == NULL) return;
	context->Claim();
}

void ExecutionContext_Free(ExecutionContext* &context)
{
	if (context == NULL) return;
	ExecutionContext::UnclaimAndFree(context);
}

// ExecutionEngine

ExecutionEngine * ExecutionEngine_Create
(
	InvokeInteropCallback interopCallback, LoadScriptCallback getScriptCallback, GetMessageCallback getMessageCallback,
	ExecutionContextStack* &invStack, StackItems* &evStack, StackItems* &altStack
)
{
	ExecutionEngine* engine = new ExecutionEngine(interopCallback, getScriptCallback, getMessageCallback);

	invStack = engine->GetInvocationStack();
	evStack = engine->GetEvaluationStack();
	altStack = engine->GetAltStack();

	return engine;
}

void ExecutionEngine_Clean(ExecutionEngine* engine, uint32 iteration)
{
	engine->Clean(iteration);
}

void ExecutionEngine_AddLog(ExecutionEngine* engine, OnStepIntoCallback callback)
{
	engine->SetLogCallback(callback);
}

void ExecutionEngine_Free(ExecutionEngine * & engine)
{
	if (engine == NULL)
		return;

	delete(engine);
	engine = NULL;
}

void ExecutionEngine_LoadScript(ExecutionEngine* engine, byte * script, int32 scriptLength)
{
	engine->LoadScript(script, scriptLength);
}

void ExecutionEngine_LoadPushOnlyScript(ExecutionEngine* engine, byte * script, int32 scriptLength)
{
	engine->LoadPushOnlyScript(script, scriptLength);
}

byte ExecutionEngine_Execute(ExecutionEngine* engine)
{
	return (byte)engine->Execute();
}

void ExecutionEngine_StepInto(ExecutionEngine* engine)
{
	engine->StepInto();
}

void ExecutionEngine_StepOver(ExecutionEngine* engine)
{
	engine->StepOver();
}

void ExecutionEngine_StepOut(ExecutionEngine* engine)
{
	engine->StepOut();
}

int32 ExecutionEngine_GetState(ExecutionEngine* engine)
{
	return engine->GetState();
}

int32 StackItems_Drop(StackItems* stack, int32 count)
{
	int ret = stack->Count();
	ret = ret > count ? count : ret;

	for (int x = 0; x < ret; x++) stack->Drop();
	return ret;
}

// StackItems

IStackItem* StackItems_Pop(StackItems* stack)
{
	if (stack->Count() <= 0) return NULL;

	return stack->Pop();
}

void StackItems_Push(StackItems* stack, IStackItem * item)
{
	stack->Push(item);
}

IStackItem* StackItems_Peek(StackItems* stack, int32 index)
{
	return stack->Count() <= index ? NULL : stack->Peek(index);
}

int32 StackItems_Count(StackItems* stack)
{
	return stack->Count();
}

void StackItems_AddLog(StackItems* stack, OnStackChangeCallback callback)
{
	stack->Log = callback;
}

// ExecutionContextStack

int32 ExecutionContextStack_Drop(ExecutionContextStack* stack, int32 count)
{
	int ret = stack->Count();
	ret = ret > count ? count : ret;

	for (int x = 0; x < ret; x++) stack->Drop();
	return ret;
}

ExecutionContext* ExecutionContextStack_Peek(ExecutionContextStack* stack, int32 index)
{
	return stack->Count() <= index ? NULL : stack->Peek(index);
}

int32 ExecutionContextStack_Count(ExecutionContextStack* stack)
{
	return stack->Count();
}

void ExecutionContextStack_AddLog(ExecutionContextStack* stack, OnStackChangeCallback callback)
{
	stack->Log = callback;
}

// StackItem

void StackItem_Free(IStackItem*& item)
{
	IStackItem::UnclaimAndFree(item);
}

IStackItem* StackItem_Create(EStackItemType type, byte *data, int32 size)
{
	IStackItem * it = NULL;
	switch (type)
	{
	default:
	case EStackItemType::None: return NULL;
	case EStackItemType::Bool:
	{
		// https://github.com/neo-project/neo-vm/blob/master/src/neo-vm/StackItem.cs#L37
		// return GetByteArray().Any(p => p != 0);

		if (size == 1)
		{
			it = new BoolStackItem(data[0] != 0x00);
			break;
		}
		else
		{
			if (size <= 0)
			{
				return NULL;
			}

			bool ret = false;

			for (int x = 0; x < size; x++)
				if (data[x] != 0x00)
				{
					ret = true;
					break;
				}

			it = new BoolStackItem(ret);
			break;
		}
	}
	case EStackItemType::Integer: { it = new IntegerStackItem(data, size); break; }
	case EStackItemType::ByteArray: { it = new ByteArrayStackItem(data, size, false); break; }
	case EStackItemType::Interop: { it = new InteropStackItem(data, size); break; }
	case EStackItemType::Array:
	case EStackItemType::Struct: { it = new ArrayStackItem(type == EStackItemType::Struct); break; }
	case EStackItemType::Map: { it = new MapStackItem(); break; }
	}

	if (it != NULL) it->Claim();
	return it;
}

int32 StackItem_SerializeData(IStackItem* item, byte * output, int32 length)
{
	if (item == NULL) return -1;

	return item->Serialize(output, length);
}

EStackItemType StackItem_SerializeDetails(IStackItem* item, int32 &size)
{
	if (item == NULL)
	{
		size = 0;
		return EStackItemType::None;
	}

	item->Claim();
	size = item->GetSerializedSize();
	return item->Type;
}

// ArrayStackItem

int32 ArrayStackItem_Count(ArrayStackItem* array)
{
	return array->Count();
}

void ArrayStackItem_Clear(ArrayStackItem* array)
{
	array->Clear();
}

IStackItem* ArrayStackItem_Get(ArrayStackItem* array, int32 index)
{
	return array->Get(index);
}

void ArrayStackItem_Add(ArrayStackItem* array, IStackItem* item)
{
	array->Add(item);
}

void ArrayStackItem_Set(ArrayStackItem* array, IStackItem* item, int32 index)
{
	array->Set(index, item);
}

int32 ArrayStackItem_IndexOf(ArrayStackItem* array, IStackItem* item)
{
	return array->IndexOf(item);
}

void ArrayStackItem_Insert(ArrayStackItem* array, IStackItem* item, int32 index)
{
	array->Insert(index, item);
}

void ArrayStackItem_RemoveAt(ArrayStackItem* array, int32 index)
{
	array->RemoveAt(index);
}