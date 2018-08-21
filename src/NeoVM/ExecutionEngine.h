#pragma once

#include <list>
#include "Types.h"
#include "Limits.h"
#include "StackItems.h"
#include "ExecutionContextStack.h"
#include "EVMState.h"

class ExecutionEngine
{
private:

	// Used for MessageCallback

	uint32 _iteration;
	uint64 _consumedGas;

	// Save the state of the execution

	EVMState _state;

	// Callback Interoperability

	OnStepIntoCallback Log;
	GetMessageCallback OnGetMessage;
	LoadScriptCallback OnLoadScript;
	InvokeInteropCallback OnInvokeInterop;

	// Stacks

	std::list<ExecutionScript*> Scripts;

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

	ExecutionContext* LoadScript(ExecutionScript* script, int32 rvcount);
	int32 LoadScript(byte* script, int32 scriptLength, int32 rvcount);
	bool LoadScript(byte scriptIndex, int32 rvcount);

	// Getters

	inline byte GetState()
	{
		return this->_state;
	}

	inline ExecutionContext* GetCurrentContext()
	{
		return this->InvocationStack.Peek(0);
	}

	inline ExecutionContext* GetCallingContext()
	{
		return this->InvocationStack.Peek(1);
	}

	inline ExecutionContext* GetEntryContext()
	{
		return this->InvocationStack.Peek(-1);
	}

	inline uint64 GetConsumedGas()
	{
		return this->_consumedGas;
	}

	// Setters

	inline void SetLogCallback(OnStepIntoCallback logCallback)
	{
		this->Log = logCallback;
	}

	void Clean(uint32 iteration);

	// Run

	void StepInto();
	void StepOut();
	void StepOver();

	EVMState Execute();
	EVMState ExecuteUntil(uint64 gas);

	// Constructor

	ExecutionEngine(InvokeInteropCallback invokeInterop, LoadScriptCallback loadScript, GetMessageCallback getMessage);

	// Destructor

	~ExecutionEngine();
};