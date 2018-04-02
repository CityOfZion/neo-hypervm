#include "StackItems.h"

int32 StackItems::Count()
{
	return this->Size;
}

void StackItems::Push(IStackItem * it)
{
	if (it != NULL)
		it->Claims++;

	this->Size++;
	this->Stack.push_front(it);
}

void StackItems::Insert(int32 index, IStackItem *it)
{
	if (it != NULL)
		it->Claims++;

	if (index == this->Size)
	{
		this->Size++;
		this->Stack.push_back(it);
		return;
	}

	std::list<IStackItem*>::iterator iter = this->Stack.begin();
	if (index > 0)
	{
		std::advance(iter, index);
	}

	this->Size++;
	this->Stack.insert(iter, it);
}

IStackItem* StackItems::Peek(int32 index)
{
	if (index == 0) return this->Stack.front();
	if (index < 0) return NULL;

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	std::advance(it, index);

	return (IStackItem*)*it;
}

IStackItem* StackItems::Remove(int32 index)
{
	if (index < 0) return NULL;

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	if (index > 0) std::advance(it, index);

	IStackItem *itr = (IStackItem*)*it;

	this->Size--;
	this->Stack.erase(it);
	return itr;
}

void StackItems::Drop()
{
	IStackItem *it = this->Stack.front();
	IStackItem::Free(it);

	this->Size--;
	this->Stack.pop_front();
}

IStackItem* StackItems::Pop()
{
	IStackItem *it = this->Stack.front();
	this->Size--;
	this->Stack.pop_front();
	return it;
}

StackItems::~StackItems()
{
	for (std::list<IStackItem*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
	{
		IStackItem* ptr = (IStackItem*)*it;
		IStackItem::Free(ptr);
	}

	this->Size = 0;
	this->Stack.clear();
}