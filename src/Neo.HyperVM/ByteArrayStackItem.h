#pragma once
#include "IStackItem.h"

class ByteArrayStackItem : public IStackItem
{
private:

	int32 _payloadLength;
	byte* _payload;

public:

	// Converters

	inline bool GetBoolean()
	{
		for (int32 x = 0; x < this->_payloadLength; ++x)
			if (this->_payload[x] != 0x00)
				return true;

		return false;
	}

	inline BigInteger* GetBigInteger()
	{
		if (this->_payloadLength == 0)
		{
			return new BigInteger(BigInteger::Zero);
		}

		return new BigInteger(this->_payload, this->_payloadLength);
	}

	inline bool GetInt32(int32 &ret)
	{
		if (this->_payloadLength == 0)
		{
			ret = 0;
			return true;
		}

		BigInteger* bi = new BigInteger(this->_payload, this->_payloadLength);
		if (bi == NULL) return false;

		bool bret = bi->ToInt32(ret);
		delete(bi);

		return bret;
	}

	int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count);

	inline int32 ReadByteArraySize()
	{
		return this->_payloadLength;
	}

	bool Equals(IStackItem* it);

	// Constructor

	ByteArrayStackItem(byte* data, int32 length, bool copyPointer);

	// Destructor

	inline ~ByteArrayStackItem()
	{
		if (this->_payload == NULL)
			return;

		delete[](this->_payload);
		this->_payload = NULL;
	}

	// Serialize

	int32 Serialize(byte* data, int32 length);

	inline int32 GetSerializedSize()
	{
		return this->_payloadLength;
	}
};