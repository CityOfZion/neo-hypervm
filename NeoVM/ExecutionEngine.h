#pragma once

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

	StackItems * AltStack;
	StackItems * EvaluationStack;
	ExecutionContextStack * InvocationStack;

	// Private methods

	byte InvokeInterop(const char* method);

public:

	// Load scripts

	void LoadScript(byte * script, int32 scriptLength);
	void LoadPushOnlyScript(byte * script, int32 scriptLength);

	// Getters

	byte GetState();

	ExecutionContext* GetCurrentContext();
	ExecutionContext* GetCallingContext();
	ExecutionContext* GetEntryContext();

	ExecutionContextStack * GetInvocationStack();
	StackItems * GetEvaluationStack();
	StackItems * GetAltStack();

	// Setters

	void SetLogCallback(OnStepIntoCallback logCallback);
	void Clean(uint32 iteration);

	// Run

	EVMState Execute();
	void StepInto();
	void StepOut();
	void StepOver();

	// Constructor

	ExecutionEngine(InvokeInteropCallback invokeInterop, LoadScriptCallback loadScript, GetMessageCallback getMessage);

	// Destructor

	~ExecutionEngine();
};