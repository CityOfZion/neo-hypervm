#pragma once

#include "ExecutionContext.h"
#include "Stack.h"

class ExecutionContextStack
{
private:

	Stack<ExecutionContext> _stack;
	Stack<ExecutionContext> _gc;

public:

	inline int32 Count() const
	{
		return this->_stack.Count();
	}

	inline ExecutionContext* Top() const
	{
		return this->_stack.Top();
	}

	inline ExecutionContext* Peek(int32 index) const
	{
		return this->_stack.Peek(index);
	}

	inline void Push(ExecutionContext* i)
	{
		if (i->Collect()) 
		{
			this->_gc.Push(i);
		}

		this->_stack.Push(i);
	}

	inline ExecutionContext* Pop(int32 index)
	{
		return this->_stack.Pop(index);
	}

	inline ExecutionContext* Pop()
	{
		return this->_stack.Pop();
	}

	inline void Clear()
	{
		this->_stack.Clear();
	}

	inline ~ExecutionContextStack()
	{
		this->Clear();

		for (int32 x = 0, count = this->_gc.Count(); x < count; x++)
		{
			auto ptr = this->_stack.Peek(x);
			delete(ptr);
		}

		this->_gc.Clear();
	}
};