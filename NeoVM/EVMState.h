#pragma once

#include "Types.h"

enum EVMState : byte
{
	// Normal state
	NONE = 0,

	// Virtual machine stopped
	HALT = 1,
	// Virtual machine execution with errors
	FAULT = 2,
};