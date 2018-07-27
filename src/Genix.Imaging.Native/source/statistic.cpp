#include "stdafx.h"

// from Genix.Core.Native.dll
extern "C" __declspec(dllimport) unsigned __int64 WINAPI bits_count_64(int count, const unsigned __int64* bits, int pos);
extern "C" __declspec(dllimport) __int32 WINAPI sum_u8(const int n, const unsigned __int8* x, const int offx);

GENIXAPI(__int64, power_1bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride)
{
	const int stridebits = stride * 64;	// 64 bits per word

	unsigned __int64 sum = 0;
	for (int iy = 0, pos = (y * stridebits) + x; iy < height; iy++, pos += stridebits)
	{
		sum += ::bits_count_64(width, bits, pos);
	}

	return (__int64)sum;
}

GENIXAPI(__int64, power_8bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride)
{
	const int stridebytes = stride * 8;	// 8 bytes per word

	unsigned __int64 sum = 0;
	for (int iy = 0, pos = (y * stridebytes) + x; iy < height; iy++, pos += stridebytes)
	{
		sum += (unsigned __int64)::sum_u8(width, (const unsigned __int8*)bits, pos);
	}

	return (__int64)sum;
}

GENIXAPI(void, grayhist_8bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride,
	__int32* hist)
{
	const int stridebytes = stride * 8;	// 8 bytes per word
	const unsigned __int8* bits_u8 = ((const unsigned __int8*)bits) + (ptrdiff_t(y) * stridebytes) + x;

	for (int iy = 0; iy < height; iy++, bits_u8 += stridebytes)
	{
		for (int ix = 0; ix < width; ix++)
		{
			hist[bits_u8[ix]] ++;
		}
	}
}

GENIXAPI(void, vhist_1bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride,
	__int32* hist)
{
	const int stridebits = stride * 64;	// 64 bits per word

	for (int iy = 0, pos = (y * stridebits) + x; iy < height; iy++, pos += stridebits)
	{
		hist[iy] = (int)::bits_count_64(width, bits, pos);
	}
}

GENIXAPI(void, vhist_8bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride,
	__int32* hist)
{
	const int stridebytes = stride * 8;	// 8 bytes per word

	for (int iy = 0, pos = (y * stridebytes) + x; iy < height; iy++, pos += stridebytes)
	{
		hist[iy] = ::sum_u8(width, (const unsigned __int8*)bits, pos);
	}
}
