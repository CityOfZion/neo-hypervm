#include "ExecutionContextStack.h"
#include "ELogStackOperation.h"

int32 ExecutionContextStack::Count()
{
	return this->Stack.Count();
}

void ExecutionContextStack::Push(ExecutionContext* i)
{
	i->Claim();
	this->Stack.Push(i);
}

ExecutionContext* ExecutionContextStack::Peek(int32 index)
{
	return this->Stack.Peek(index);
}

void ExecutionContextStack::Remove(int32 index)
{
	ExecutionContext* it = this->Stack.Pop(index);
	ExecutionContext::UnclaimAndFree(it);
}

void ExecutionContextStack::Drop()
{
	ExecutionContext* it = this->Stack.Pop();
	ExecutionContext::UnclaimAndFree(it);
}

void ExecutionContextStack::Clear()
{
	for (int32 x = 0, count = this->Stack.Count(); x < count; x++)
	{
		ExecutionContext* ptr = this->Stack.Peek(x);
		ExecutionContext::UnclaimAndFree(ptr);
	}

	this->Stack.Clear();
}

ExecutionContextStack::~ExecutionContextStack()
{
	this->Clear();
}