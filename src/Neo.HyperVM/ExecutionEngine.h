#pragma once

#include <list>
#include <memory>
#include "Types.h"
#include "Limits.h"
#include "StackItems.h"
#include "ExecutionContextStack.h"
#include "EVMState.h"
#include "IStackItemCounter.h"
#include "MapStackItem.h"
#include "ArrayStackItem.h"
#include "BoolStackItem.h"
#include "IntegerStackItem.h"
#include "ByteArrayStackItem.h"
#include "InteropStackItem.h"

class ExecutionEngine
{
private:

	// Used for MessageCallback

	uint32 _iteration;
	uint64 _consumedGas;
	uint64 _maxGas;
	IStackItemCounter *_counter;

	// Save the state of the execution

	EVMState _state;

	// Callback Interoperability

	OnStepIntoCallback Log;
	GetMessageCallback OnGetMessage;
	LoadScriptCallback OnLoadScript;
	InvokeInteropCallback OnInvokeInterop;

	// Stacks

	std::list<std::shared_ptr<ExecutionScript>> Scripts;

	void InternalStepInto();

	inline void SetHalt()
	{
		this->_state = EVMState::HALT;
	}

	inline void SetFault()
	{
		this->_state = EVMState::FAULT;
	}

public:

	// Stacks

	StackItems ResultStack;
	ExecutionContextStack InvocationStack;

	// Load scripts

	ExecutionContext* LoadScript(std::shared_ptr<ExecutionScript> script, int32 rvcount);
	int32 LoadScript(byte* script, int32 scriptLength, int32 rvcount);
	bool LoadScript(byte scriptIndex, int32 rvcount);

	inline bool AddGasCost()
	{
		if ((this->_consumedGas += 1) > this->_maxGas)
		{
			this->_state = EVMState::FAULT_BY_GAS;
			return false;
		}

		return true;
	}

	inline bool AddGasCost(uint64 cost)
	{
		if ((this->_consumedGas += cost) > this->_maxGas)
		{
			this->_state = EVMState::FAULT_BY_GAS;
			return false;
		}

		return true;
	}

	// Getters

	inline byte GetState() const
	{
		return this->_state;
	}

	inline ExecutionContext* GetCurrentContext() const
	{
		return this->InvocationStack.Top();
	}

	inline ExecutionContext* GetCallingContext() const
	{
		return this->InvocationStack.Peek(1);
	}

	inline ExecutionContext* GetEntryContext() const
	{
		return this->InvocationStack.Peek(-1);
	}

	inline uint64 GetConsumedGas() const
	{
		return this->_consumedGas;
	}

	// Setters

	inline void SetLogCallback(OnStepIntoCallback &logCallback)
	{
		this->Log = logCallback;
	}

	void Clean(uint32 iteration);

	// Run

	void StepInto();
	void StepOut();
	void StepOver();

	EVMState Execute(uint32 gas);

	// Creators

	inline MapStackItem* CreateMap()
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new MapStackItem(_counter);
	}

	inline IntegerStackItem* CreateInteger(int32 value)
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new IntegerStackItem(_counter, value);
	}

	inline IntegerStackItem* CreateInteger(BigInteger *value)
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new IntegerStackItem(_counter, value);
	}

	inline IntegerStackItem* CreateInteger(byte* data, int32 length)
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new IntegerStackItem(_counter, data, length);
	}

	inline InteropStackItem* CreateInterop(byte* data, int32 length)
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new InteropStackItem(_counter, data, length);
	}

	inline ByteArrayStackItem* CreateByteArray(byte* data, int32 length, bool copyPointer)
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new ByteArrayStackItem(_counter, data, length, copyPointer);
	}

	inline BoolStackItem* CreateBool(bool value)
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new BoolStackItem(_counter, value);
	}

	inline ArrayStackItem* CreateArray()
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new ArrayStackItem(_counter);
	}

	inline ArrayStackItem* CreateStruct()
	{
		if (!this->_counter->ItemCounterInc())
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		return new ArrayStackItem(_counter, true);
	}

	inline ArrayStackItem* CreateArray(int32 count)
	{
		if (!this->_counter->ItemCounterInc(count + 1))
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		auto ret = new ArrayStackItem(_counter, false);

		for (int32 i = 0; i < count; ++i)
		{
			ret->Add(new BoolStackItem(_counter, false));
		}

		return ret;
	}

	inline ArrayStackItem* CreateStruct(int32 count)
	{
		if (!this->_counter->ItemCounterInc(count + 1))
		{
			this->_state = EVMState::FAULT;
			return nullptr;
		}

		auto ret = new ArrayStackItem(_counter, true);

		for (int32 i = 0; i < count; ++i)
		{
			ret->Add(new BoolStackItem(_counter, false));
		}

		return ret;
	}

	// Constructor

	ExecutionEngine(InvokeInteropCallback &invokeInterop, LoadScriptCallback &loadScript, GetMessageCallback &getMessage);

	// Destructor

	~ExecutionEngine();
};