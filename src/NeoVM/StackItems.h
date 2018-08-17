#pragma once

#include "IStackItem.h"
#include "Stack.h"

class StackItems
{
	Stack<IStackItem> _stack;

public:

	inline int32 Count()
	{
		return this->_stack.Count();
	}

	inline IStackItem* Peek(int32 index)
	{
		return this->_stack.Peek(index);
	}

	IStackItem* Pop();
	void Drop();
	void Clear();
	void Push(IStackItem* it);
	IStackItem* Remove(int32 index);
	void Insert(int32 index, IStackItem* it);
	void SendTo(StackItems* stack, int32 count);

	~StackItems();
};