#pragma once

#include "IStackItem.h"
#include "Stack.h"
#include "StackItemHelper.h"

class StackItems
{
	Stack<IStackItem> _stack;

public:

	inline int32 Count() const
	{
		return this->_stack.Count();
	}

	inline IStackItem* Top() const
	{
		return this->_stack.Top();
	}
	
	inline IStackItem* Peek(int32 index) const
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
		auto it = this->_stack.Pop(index);
		
		if (it != NULL) it->UnClaim();

		return it;
	}

	inline void Drop()
	{
		auto it = this->_stack.Pop();
		StackItemHelper::UnclaimAndFree(it);
	}

	inline IStackItem* Pop()
	{
		auto it = this->_stack.Pop();
		if (it != NULL) it->UnClaim();

		return it;
	}

	inline void Clear()
	{
		for (int32 x = 0, count = this->_stack.Count(); x < count; x++)
		{
			auto ptr = this->_stack.Peek(x);
			StackItemHelper::UnclaimAndFree(ptr);
		}

		this->_stack.Clear();
	}

	inline void SendTo(StackItems* stack, int32 count)
	{
		if (stack == NULL) return;

		this->_stack.SendTo(&stack->_stack, count);
	}

	inline ~StackItems()
	{
		this->Clear();
	}
};