#pragma once

#include "IStackItem.h"
#include "Stack.h"

class StackItems
{
	Stack<IStackItem> Stack;

public:

	int32 Count();

	IStackItem* Pop();
	void Drop();
	void Clear();
	void Push(IStackItem* it);
	IStackItem* Peek(int32 index);
	IStackItem* Remove(int32 index);
	void Insert(int32 index, IStackItem* it);
	void SendTo(StackItems* stack, int32 count);

	~StackItems();
};