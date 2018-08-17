#pragma once

#include "ExecutionContext.h"
#include "Stack.h"

class ExecutionContextStack
{
	Stack<ExecutionContext> Stack;

public:

	int32 Count();

	void Drop();
	void Clear();
	void Remove(int32 index);
	void Push(ExecutionContext* i);
	ExecutionContext* Peek(int32 index);

	~ExecutionContextStack();
};