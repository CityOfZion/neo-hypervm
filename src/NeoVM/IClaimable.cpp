#include "IClaimable.h"

bool IClaimable::UnClaim() 
{
	this->_claims--;
	return this->_claims == 0;
}

bool IClaimable::IsUnClaimed()
{
	return this->_claims == 0;
}

void IClaimable::Claim() { this->_claims++; }

IClaimable::IClaimable() :_claims(0) { }