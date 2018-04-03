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
	
	uint32 Iteration = 0;

	// Save the state of the execution
	
	EVMState State = EVMState::NONE;

	// Callback Interoperability
	
	GetMessageCallback MessageCallback;
	GetScriptCallback ScriptCallback;
	InvokeInteropCallback InteropCallback;

	// Stacks

	ExecutionContextStack * InvocationStack;
	StackItems * EvaluationStack;
	StackItems * AltStack;

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

	// Run

	EVMState Execute();
	void StepInto();
	void StepOut();
	void StepOver();

	// Constructor

	ExecutionEngine(InvokeInteropCallback invokeInteropCallback, GetScriptCallback getScriptCallback, GetMessageCallback getMessageCallback);

	// Destructor

	~ExecutionEngine();
};