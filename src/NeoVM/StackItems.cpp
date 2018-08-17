#include "StackItems.h"
#include "ELogStackOperation.h"

int32 StackItems::Count()
{
	return this->_stack.Count();
}

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

IStackItem* StackItems::Peek(int32 index)
{
	return this->_stack.Peek(index);
}

IStackItem* StackItems::Remove(int32 index)
{
	IStackItem* it = this->_stack.Pop(index);
	it->UnClaim();

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
	it->UnClaim();

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