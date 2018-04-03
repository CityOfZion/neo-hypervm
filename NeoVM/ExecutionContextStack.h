#pragma once

#include <list>
#include "ExecutionContext.h"

class ExecutionContextStack
{
	int32 Size;
	std::list<ExecutionContext*> Stack;

public:
	int32 Count();

	void Drop();
	ExecutionContext* Pop();
	void Push(ExecutionContext * i);
	ExecutionContext* Peek(int32 index);
	ExecutionContext* TryPeek(int32 index);

	~ExecutionContextStack();
};