#pragma once

#include "IStackItem.h"
#include <string.h>

class InteropStackItem : public IStackItem
{
private:

	int32 _payloadLength;
	byte* _payload;

public:

	// Converters

	inline bool GetBoolean()
	{
		return this->_payloadLength > 0;
	}

	inline BigInteger* GetBigInteger() { return NULL; }

	inline bool GetInt32(int32 &ret) { return false; }

	inline int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count) { return -1; }

	inline int32 ReadByteArraySize() { return -1; }

	bool Equals(IStackItem* it);

	// Constructor

	inline InteropStackItem(byte* data, int32 length)
		:IStackItem(EStackItemType::Interop), _payloadLength(length)
	{
		this->_payload = new byte[length];

		memcpy(this->_payload, data, length);
	}

	// Destructor

	inline ~InteropStackItem()
	{
		if (this->_payload == NULL) return;

		delete[](this->_payload);
		this->_payload = NULL;
	}

	// Serialize

	inline int32 Serialize(byte* data, int32 length)
	{
		if (this->_payloadLength <= 0 || length <= 0)
		{
			return 0;
		}

		length = this->_payloadLength > length ? length : this->_payloadLength;
		memcpy(data, this->_payload, length);

		return length;
	}

	inline int32 GetSerializedSize()
	{
		return this->_payloadLength;
	}
};