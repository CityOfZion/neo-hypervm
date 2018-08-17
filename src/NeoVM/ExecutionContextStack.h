#pragma once

#include "ExecutionContext.h"
#include "Stack.h"

class ExecutionContextStack
{
	Stack<ExecutionContext> _stack;

public:

	inline int32 Count()
	{
		return this->_stack.Count();
	}

	inline ExecutionContext* Peek(int32 index)
	{
		return this->_stack.Peek(index);
	}

	void Drop();
	void Clear();
	void Remove(int32 index);
	void Push(ExecutionContext* i);
	
	~ExecutionContextStack();
};