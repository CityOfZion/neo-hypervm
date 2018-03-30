#pragma once

enum EVMState :__int8
{
	// Normal state
	NONE = 0,

	// Virtual machine stopped
	HALT = 1,
	// Virtual machine execution with errors
	FAULT = 2,
};