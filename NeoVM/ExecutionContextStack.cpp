#include "ExecutionContextStack.h"

int ExecutionContextStack::Count()
{
	return this->Size;
}

void ExecutionContextStack::Push(ExecutionContext * i)
{
	this->Size++;
	this->Stack.push_front(i);
}

ExecutionContext* ExecutionContextStack::Peek(int index)
{
	if (index == 0) return this->Stack.front();
	if (index < 0) return NULL;

	std::list<ExecutionContext*>::iterator it = this->Stack.begin();
	std::advance(it, index);

	return (ExecutionContext*)*it;
}

void ExecutionContextStack::Drop()
{
	ExecutionContext *it = this->Stack.front();
	delete(it);
	
	this->Size--;
	this->Stack.pop_front();
}

ExecutionContext* ExecutionContextStack::Pop()
{
	ExecutionContext *it = this->Stack.front();
	this->Size--;
	this->Stack.pop_front();
	return it;
}

ExecutionContextStack::~ExecutionContextStack()
{
	for (std::list<ExecutionContext*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
	{
		ExecutionContext* ptr = (ExecutionContext*)*it;
		if (ptr == NULL) continue;

		delete(ptr);
		ptr = NULL;
	}

	this->Size = 0;
	this->Stack.clear();
}