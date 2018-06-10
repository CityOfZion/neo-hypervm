#pragma once

#include <list>
#include "IStackItem.h"

class StackItems
{
	int32 Size = 0;
	std::list<IStackItem*> Stack;

public:

	OnStackChangeCallback Log;
	int32 Count();

	IStackItem* Pop();
	void Drop();
	void Clear();
	void Push(IStackItem* it);
	IStackItem* Peek(int32 index);
	IStackItem* TryPeek(int32 index);
	IStackItem* Remove(int32 index);
	void Insert(int32 index, IStackItem* it);
	void CopyTo(StackItems* stack, int32 count);

	~StackItems();
};