#pragma once

#include <list>
#include "IStackItem.h"

class StackItems
{
	int Size;
	std::list<IStackItem*> Stack;

public:
	int Count();

	IStackItem* Pop();
	void Drop();
	void Push(IStackItem * it);
	IStackItem* Peek(int index);
	IStackItem* Remove(int index);
	void Insert(int index, IStackItem *it);

	~StackItems();
};