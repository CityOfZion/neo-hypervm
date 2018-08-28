#pragma once

#include "ExecutionContext.h"
#include "Stack.h"

class ExecutionContextStack
{
private:

	Stack<ExecutionContext> _stack;

public:

	inline int32 Count()
	{
		return this->_stack.Count();
	}

	inline ExecutionContext* Top()
	{
		return this->_stack.Top();
	}

	inline ExecutionContext* Peek(int32 index)
	{
		return this->_stack.Peek(index);
	}

	inline void Push(ExecutionContext* i)
	{
		i->Claim();
		this->_stack.Push(i);
	}

	inline void Remove(int32 index)
	{
		ExecutionContext* it = this->_stack.Pop(index);
		ExecutionContext::UnclaimAndFree(it);
	}

	inline void Drop()
	{
		ExecutionContext* it = this->_stack.Pop();
		ExecutionContext::UnclaimAndFree(it);
	}

	inline void Clear()
	{
		for (int32 x = 0, count = this->_stack.Count(); x < count; x++)
		{
			ExecutionContext* ptr = this->_stack.Peek(x);
			ExecutionContext::UnclaimAndFree(ptr);
		}

		this->_stack.Clear();
	}

	inline ~ExecutionContextStack()
	{
		this->Clear();
	}
};