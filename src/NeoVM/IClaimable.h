#pragma once

#include "Types.h"

class IClaimable
{
private:

	int32 _claims;

public:

	// Claims

	void Claim();
	bool UnClaim();
	bool IsUnClaimed();

	// Constructor

	IClaimable();
};