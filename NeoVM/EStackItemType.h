#pragma once

enum EStackItemType
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