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

	inline void Push(IStackItem* it)
	{
		it->Claim();
		this->_stack.Push(it);
	}

	inline void Insert(int32 index, IStackItem* it)
	{
		it->Claim();
		this->_stack.Insert(index, it);
	}

	inline IStackItem* Remove(int32 index)
	{
		IStackItem* it = this->_stack.Pop(index);
		if (it != NULL) it->UnClaim();

		return it;
	}

	inline void Drop()
	{
		IStackItem* it = this->_stack.Pop();
		IStackItem::UnclaimAndFree(it);
	}

	inline IStackItem* Pop()
	{
		IStackItem* it = this->_stack.Pop();
		if (it != NULL) it->UnClaim();

		return it;
	}

	inline void Clear()
	{
		for (int32 x = 0, count = this->_stack.Count(); x < count; x++)
		{
			IStackItem* ptr = this->_stack.Peek(x);
			IStackItem::UnclaimAndFree(ptr);
		}

		this->_stack.Clear();
	}

	inline void SendTo(StackItems* stack, int32 count)
	{
		if (stack == NULL || count == 0) return;

		this->_stack.SendTo(&stack->_stack, count);
	}

	inline ~StackItems()
	{
		this->Clear();
	}
};