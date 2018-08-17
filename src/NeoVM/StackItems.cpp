#include "StackItems.h"

void StackItems::Push(IStackItem* it)
{
	it->Claim();
	this->_stack.Push(it);
}

void StackItems::Insert(int32 index, IStackItem* it)
{
	it->Claim();
	this->_stack.Insert(index, it);
}

IStackItem* StackItems::Remove(int32 index)
{
	IStackItem* it = this->_stack.Pop(index);
	if (it != NULL) it->UnClaim();

	return it;
}

void StackItems::Drop()
{
	IStackItem* it = this->_stack.Pop();
	IStackItem::UnclaimAndFree(it);
}

IStackItem* StackItems::Pop()
{
	IStackItem* it = this->_stack.Pop();
	if (it != NULL) it->UnClaim();

	return it;
}

void StackItems::Clear()
{
	for (int32 x = 0, count = this->_stack.Count(); x < count; x++)
	{
		IStackItem* ptr = this->_stack.Peek(x);
		IStackItem::UnclaimAndFree(ptr);
	}

	this->_stack.Clear();
}

void StackItems::SendTo(StackItems* stack, int32 count)
{
	if (stack == NULL || count == 0) return;

	this->_stack.SendTo(&stack->_stack, count);
}

StackItems::~StackItems()
{
	this->Clear();
}