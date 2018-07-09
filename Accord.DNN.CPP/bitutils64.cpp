#include "stdafx.h"

#include <intrin.h>
#include <math.h>
#include <limits.h>
#include <assert.h>

#define BITS_COUNT			64

#include "bitutils.inl"
#include "gsl/gsl"

// Copies the 64-bits array into 32-bits array.
extern "C" __declspec(dllexport) void bits_copy_be64to32(
	int count,							// number of 32-bit words to copy
	const unsigned __int64* src, 		// the source array
	int offsrc, 						// the zero-based index of starting element in bitssrc
	unsigned* dst, 						// the destination array
	int offdst 							// the zero-based index of starting element in bitsdst
)
{
	src += offsrc;
	dst += offdst;

	for (int i = 0, ii = count >> 1; i < ii; i++, src++, dst += 2)
	{
		dst[0] = gsl::narrow_cast<unsigned>(src[0] >> 32);
		dst[1] = gsl::narrow_cast<unsigned>(src[0]);
	}

	// copy last uneven 32-bit word
	if (count & 1)
	{
		dst[0] = gsl::narrow_cast<unsigned>(src[0] >> 32);
	}
}

// Copies the 32-bits array into 64-bits array.
extern "C" __declspec(dllexport) void bits_copy_be32to64(
	int count,							// number of 32-bit words to copy
	const unsigned* src, 				// the source array
	int offsrc, 						// the zero-based index of starting element in bitssrc
	unsigned __int64* dst, 				// the destination array
	int offdst 							// the zero-based index of starting element in bitsdst
)
{
	src += offsrc;
	dst += offdst;

	for (int i = 0, ii = count >> 1; i < ii; i++, src += 2, dst++)
	{
		dst[0] = (gsl::narrow_cast<unsigned __int64>(src[0]) << 32) | gsl::narrow_cast<unsigned __int64>(src[0]);
	}

	// copy last uneven 32-bit word
	if (count & 1)
	{
		dst[0] = gsl::narrow_cast<unsigned __int64>(src[0]) << 32;
	}
}

