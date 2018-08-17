#include "StackItems.h"
#include "ELogStackOperation.h"

int32 StackItems::Count()
{
	return this->Stack.Count();
}

void StackItems::Push(IStackItem* it)
{
	it->Claim();
	this->Stack.Push(it);
}

void StackItems::Insert(int32 index, IStackItem* it)
{
	it->Claim();
	this->Stack.Insert(index, it);
}

IStackItem* StackItems::Peek(int32 index)
{
	return this->Stack.Peek(index);
}

IStackItem* StackItems::Remove(int32 index)
{
	IStackItem* it = this->Stack.Pop(index);
	it->UnClaim();

	return it;
}

void StackItems::Drop()
{
	IStackItem* it = this->Stack.Pop();
	IStackItem::UnclaimAndFree(it);
}

IStackItem* StackItems::Pop()
{
	IStackItem* it = this->Stack.Pop();
	it->UnClaim();

	return it;
}

void StackItems::Clear()
{
	for (int32 x = 0, count = this->Stack.Count(); x < count; x++)
	{
		IStackItem* ptr = this->Stack.Peek(x);
		IStackItem::UnclaimAndFree(ptr);
	}

	this->Stack.Clear();
}

void StackItems::SendTo(StackItems* stack, int32 count)
{
	if (stack == NULL || count == 0) return;

	Stack.SendTo(&stack->Stack, count);
}

StackItems::~StackItems()
{
	this->Clear();
}