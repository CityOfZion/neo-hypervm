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

	uint32 Iteration;

	// Save the state of the execution

	EVMState State;

	// Callback Interoperability

	OnStepIntoCallback Log;
	GetMessageCallback OnGetMessage;
	LoadScriptCallback OnLoadScript;
	InvokeInteropCallback OnInvokeInterop;

	// Stacks

	StackItems* ResultStack;
	ExecutionContextStack* InvocationStack;

	std::list<ExecutionScript*> Scripts;

	void SetFault();

public:

	// Load scripts

	ExecutionContext* LoadScript(ExecutionScript* script, int32 rvcount);
	int32 LoadScript(byte* script, int32 scriptLength, int32 rvcount);
	bool LoadScript(byte scriptIndex, int32 rvcount);

	// Getters

	byte GetState();

	ExecutionContext* GetCurrentContext();
	ExecutionContext* GetCallingContext();
	ExecutionContext* GetEntryContext();

	ExecutionContextStack* GetInvocationStack();
	StackItems* GetResultStack();

	// Setters

	void SetLogCallback(OnStepIntoCallback logCallback);
	void Clean(uint32 iteration);

	// Run

	void StepInto();
	void StepOut();
	void StepOver();
	EVMState Execute();

	// Constructor

	ExecutionEngine(InvokeInteropCallback invokeInterop, LoadScriptCallback loadScript, GetMessageCallback getMessage);

	// Destructor

	~ExecutionEngine();
};