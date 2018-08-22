#pragma once

#include "Types.h"

enum EStackItemType : byte
{
	None = 0,

	Bool = 1,
	Integer = 2,
	ByteArray = 3,
	Interop = 4,

	Array = 5,
	Struct = 6,
	Map = 7,
};