#pragma once

#include <list>
#include "ExecutionContext.h"

class ExecutionContextStack
{
	int Size;
	std::list<ExecutionContext*> Stack;

public:
	int Count();

	void Drop();
	ExecutionContext* Pop();
	void Push(ExecutionContext * i);
	ExecutionContext* Peek(int index);

	~ExecutionContextStack();
};