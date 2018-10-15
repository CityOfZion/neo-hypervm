#pragma once

#include "Types.h"

class IClaimable
{
private:

	int32 _claims;

public:

	// Claims

	inline bool UnClaim()
	{
		this->_claims--;
		return this->_claims == 0;
	}

	inline bool IsUnClaimed() const
	{
		return this->_claims == 0;
	}

	inline void Claim()
	{
		this->_claims++;
	}

	// Constructor

	inline IClaimable() :_claims(0) { }
};