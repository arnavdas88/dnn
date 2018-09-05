#include "stdafx.h"
#include <intrin.h>
#include "simddetect.h"

SIMDDetect SIMDDetect::instance;
// If true, then SSe4.1 has been detected.
bool SIMDDetect::sse_available = false;
// If true, then AVX has been detected.
bool SIMDDetect::avx_available = false;
bool SIMDDetect::avx2_available = false;

SIMDDetect::SIMDDetect()
{
	int cpui[4];
	__cpuid(cpui, 0);
	int nIds = cpui[0];
	if (nIds >= 1)
	{
		__cpuid(cpui, 1);
		SIMDDetect::sse_available = (cpui[2] & 0x00080000) != 0;
		SIMDDetect::avx_available = (cpui[2] & 0x10000000) != 0;
	}
	
	if (SIMDDetect::avx_available)
	{
		if (nIds >= 7)
		{
			__cpuidex(cpui, 7, 0);
			SIMDDetect::avx2_available = (cpui[1] & 0x00000020) != 0;
		}
	}
}
