#include "HyperVM.h"
#include "BoolStackItem.h"
#include "IntegerStackItem.h"
#include "ByteArrayStackItem.h"
#include "InteropStackItem.h"
#include "ArrayStackItem.h"
#include "MapStackItem.h"

// Library

void GetVersion(int32 &major, int32 &minor, int32 &build, int32 &revision)
{
	// TODO: Extract version from file

	major = 2;
	minor = 3;
	build = 0;
	revision = 0;
}

// ExecutionContext

int32 ExecutionContext_GetScriptHash(ExecutionContext* context, byte* output, int32 index)
{
	if (context == nullptr) return 0;

	return context->GetScriptHash(&output[index]);
}

EVMOpCode ExecutionContext_GetNextInstruction(ExecutionContext* context)
{
	if (context == nullptr) return EVMOpCode::RET;

	return context->GetNextInstruction();
}

int32 ExecutionContext_GetInstructionPointer(ExecutionContext* context)
{
	if (context == nullptr) return 0;

	return context->GetInstructionPointer();
}

void ExecutionContext_Claim(ExecutionContext* context, StackItems* &evStack, StackItems* &altStack)
{
	if (context == nullptr) return;

	evStack = &context->EvaluationStack;
	altStack = &context->AltStack;

	context->Claim();
}

void ExecutionContext_Free(ExecutionContext* &context)
{
	if (context == nullptr) return;

	ExecutionContext::UnclaimAndFree(context);
	context = nullptr;
}

// ExecutionEngine

ExecutionEngine* ExecutionEngine_Create
(
	InvokeInteropCallback interopCallback, LoadScriptCallback getScriptCallback, GetMessageCallback getMessageCallback,
	ExecutionContextStack* &invStack, StackItems* &resStack
)
{
	ExecutionEngine* engine = new ExecutionEngine(interopCallback, getScriptCallback, getMessageCallback);

	invStack = &engine->InvocationStack;
	resStack = &engine->ResultStack;

	return engine;
}

void ExecutionEngine_Clean(ExecutionEngine* engine, uint32 iteration)
{
	if (engine == nullptr) return;

	engine->Clean(iteration);
}

void ExecutionEngine_AddLog(ExecutionEngine* engine, OnStepIntoCallback callback)
{
	if (engine == nullptr) return;

	engine->SetLogCallback(callback);
}

void ExecutionEngine_Free(ExecutionEngine* & engine)
{
	if (engine == nullptr) return;

	delete(engine);
	engine = nullptr;
}

int32 ExecutionEngine_LoadScript(ExecutionEngine* engine, byte* script, int32 scriptLength, int32 rvcount)
{
	if (engine == nullptr) return -1;

	return engine->LoadScript(script, scriptLength, rvcount);
}

byte ExecutionEngine_LoadCachedScript(ExecutionEngine* engine, int32 scriptIndex, int32 rvcount)
{
	if (engine == nullptr) return 0x00;

	return engine->LoadScript(scriptIndex, rvcount) ? 0x01 : 0x00;
}

byte ExecutionEngine_IncreaseGas(ExecutionEngine* engine, uint32 gas)
{
	if (engine == nullptr) return 0x00;

	return engine->AddGasCost(gas) ? 0x01 : 0x00;
}

byte ExecutionEngine_Execute(ExecutionEngine* engine, uint32 gas)
{
	if (engine == nullptr) return 0x00;

	return (byte)engine->Execute(gas);
}

void ExecutionEngine_StepInto(ExecutionEngine* engine)
{
	if (engine == nullptr) return;

	engine->StepInto();
}

void ExecutionEngine_StepOver(ExecutionEngine* engine)
{
	if (engine == nullptr) return;

	engine->StepOver();
}

void ExecutionEngine_StepOut(ExecutionEngine* engine)
{
	if (engine == nullptr) return;

	engine->StepOut();
}

byte ExecutionEngine_GetState(ExecutionEngine* engine)
{
	if (engine == nullptr) return 0;

	return engine->GetState();
}

uint32 ExecutionEngine_GetConsumedGas(ExecutionEngine* engine)
{
	if (engine == nullptr) return 0;

	return engine->GetConsumedGas();
}

// StackItems

int32 StackItems_Drop(StackItems* stack, int32 count)
{
	if (stack == nullptr) return 0;

	int ret = stack->Count();
	ret = ret > count ? count : ret;

	for (int x = 0; x < ret; ++x) stack->Drop();
	return ret;
}

IStackItem* StackItems_Pop(StackItems* stack)
{
	if (stack == nullptr) return nullptr;

	return stack->Pop();
}

void StackItems_Push(StackItems* stack, IStackItem* item)
{
	if (stack == nullptr) return;

	stack->Push(item);
}

IStackItem* StackItems_Peek(StackItems* stack, int32 index)
{
	if (stack == nullptr) return nullptr;

	return stack->Peek(index);
}

int32 StackItems_Count(StackItems* stack)
{
	if (stack == nullptr) return 0;

	return stack->Count();
}

// ExecutionContextStack

int32 ExecutionContextStack_Drop(ExecutionContextStack* stack, int32 count)
{
	if (stack == nullptr) return 0;

	int ret = stack->Count();
	ret = ret > count ? count : ret;

	for (int x = 0; x < ret; ++x) stack->Drop();
	return ret;
}

ExecutionContext* ExecutionContextStack_Peek(ExecutionContextStack* stack, int32 index)
{
	if (stack == nullptr) return nullptr;

	return stack->Peek(index);
}

int32 ExecutionContextStack_Count(ExecutionContextStack* stack)
{
	if (stack == nullptr) return 0;

	return stack->Count();
}

// StackItem

void StackItem_Free(IStackItem*& item)
{
	StackItemHelper::UnclaimAndFree(item);
	item = nullptr;
}

IStackItem* StackItem_Create(ExecutionEngine* engine, EStackItemType type, byte* data, int32 size)
{
	if (engine == nullptr)
	{
		return nullptr;
	}

	IStackItem* it;

	switch (type)
	{
	default:
	case EStackItemType::None: return nullptr;
	case EStackItemType::Bool:
	{
		// https://github.com/neo-project/neo-vm/blob/master/src/neo-vm/StackItem.cs#L37
		// return GetByteArray().Any(p => p != 0);

		if (size == 1)
		{
			it = engine->CreateBool(data[0] != 0x00);
		}
		else
		{
			if (size <= 0)
			{
				return nullptr;
			}

			bool ret = false;

			for (int x = 0; x < size; ++x)
				if (data[x] != 0x00)
				{
					ret = true;
					break;
				}

			it = engine->CreateBool(ret);
		}
		break;
	}
	case EStackItemType::Integer: { it = engine->CreateInteger(data, size); break; }
	case EStackItemType::ByteArray: { it = engine->CreateByteArray(data, size, false); break; }
	case EStackItemType::Interop: { it = engine->CreateInterop(data, size); break; }
	case EStackItemType::Array: { it = engine->CreateArray(); break; }
	case EStackItemType::Struct: { it = engine->CreateStruct(); break; }
	case EStackItemType::Map: { it = engine->CreateMap(); break; }
	}

	it->Claim();
	return it;
}

int32 StackItem_Serialize(IStackItem* item, byte* output, int32 length)
{
	return item == nullptr ? -1 : item->Serialize(output, length);
}

void StackItem_Claim(IStackItem* item)
{
	if (item != nullptr)
	{
		item->Claim();
	}
}

EStackItemType StackItem_SerializeInfo(IStackItem* item, int32 &size)
{
	if (item == nullptr)
	{
		size = 0;
		return EStackItemType::None;
	}

	size = item->GetSerializedSize();
	item->Claim();

	return item->Type;
}

// MapStackItem

int32 MapStackItem_Count(MapStackItem* map)
{
	if (map == nullptr) return 0;

	return map->Count();
}

void MapStackItem_Clear(MapStackItem* map)
{
	if (map == nullptr) return;

	return map->Clear();
}

byte MapStackItem_Remove(MapStackItem* map, IStackItem* key)
{
	if (map == nullptr) return 0x00;

	return map->Remove(key) ? 0x01 : 0x00;
}

void MapStackItem_Set(MapStackItem* map, IStackItem* key, IStackItem* value)
{
	if (map == nullptr) return;

	map->Set(key, value);
}

IStackItem* MapStackItem_Get(MapStackItem* map, IStackItem* key)
{
	if (map == nullptr) return nullptr;

	return map->Get(key);
}

IStackItem* MapStackItem_GetKey(MapStackItem* map, int index)
{
	if (map == nullptr) return nullptr;

	return map->GetKey(index);
}

IStackItem* MapStackItem_GetValue(MapStackItem* map, int index)
{
	if (map == nullptr) return nullptr;

	return map->GetValue(index);
}

// ArrayStackItem

int32 ArrayStackItem_Count(ArrayStackItem* array)
{
	if (array == nullptr) return 0;

	return array->Count();
}

void ArrayStackItem_Clear(ArrayStackItem* array)
{
	if (array == nullptr) return;

	array->Clear();
}

IStackItem* ArrayStackItem_Get(ArrayStackItem* array, int32 index)
{
	if (array == nullptr) return nullptr;

	return array->Get(index);
}

void ArrayStackItem_Add(ArrayStackItem* array, IStackItem* item)
{
	if (array == nullptr) return;

	array->Add(item);
}

void ArrayStackItem_Set(ArrayStackItem* array, IStackItem* item, int32 index)
{
	if (array == nullptr) return;

	array->Set(index, item);
}

int32 ArrayStackItem_IndexOf(ArrayStackItem* array, IStackItem* item)
{
	if (array == nullptr) return 0;

	return array->IndexOf(item);
}

void ArrayStackItem_Insert(ArrayStackItem* array, IStackItem* item, int32 index)
{
	if (array == nullptr) return;

	array->Insert(index, item);
}

void ArrayStackItem_RemoveAt(ArrayStackItem* array, int32 index)
{
	if (array == nullptr) return;

	array->RemoveAt(index);
}