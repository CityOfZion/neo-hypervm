#include "ExecutionContextStack.h"

void ExecutionContextStack::Push(ExecutionContext* i)
{
	i->Claim();
	this->_stack.Push(i);
}

void ExecutionContextStack::Remove(int32 index)
{
	ExecutionContext* it = this->_stack.Pop(index);
	ExecutionContext::UnclaimAndFree(it);
}

void ExecutionContextStack::Drop()
{
	ExecutionContext* it = this->_stack.Pop();
	ExecutionContext::UnclaimAndFree(it);
}

void ExecutionContextStack::Clear()
{
	for (int32 x = 0, count = this->_stack.Count(); x < count; x++)
	{
		ExecutionContext* ptr = this->_stack.Peek(x);
		ExecutionContext::UnclaimAndFree(ptr);
	}

	this->_stack.Clear();
}

ExecutionContextStack::~ExecutionContextStack()
{
	this->Clear();
}