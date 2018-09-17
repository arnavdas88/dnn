#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

// from Genix.Core.Native.dll
extern "C" __declspec(dllimport) unsigned __int64 WINAPI bits_count_64(int count, const unsigned __int64* bits, int pos);
extern "C" __declspec(dllimport) unsigned __int32 WINAPI sum_ip_u8(const int n, const unsigned __int8* x, const int offx);

extern "C" __declspec(dllimport) int WINAPI bits_scan_one_forward_64(int count, const unsigned __int64* bits, int pos);
extern "C" __declspec(dllimport) int WINAPI bits_scan_zero_forward_64(int count, const unsigned __int64* bits, int pos);

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
	const unsigned __int8* bits_u8 = ((const unsigned __int8*)bits) + (ptrdiff_t(y) * stridebytes) + x;

	Ipp64f sum = 0;
	ippiSum_8u_C1R(bits_u8, stridebytes, { width, height }, &sum);

	/*unsigned __int64 sum = 0;
	for (int iy = 0; iy < height; iy++, bits_u8 += stridebytes)
	{
		sum += (unsigned __int64)::sum_ip_u8(width, bits_u8, 0);
	}*/

	return (__int64)sum;
}

GENIXAPI(BOOL, is_all_white)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride)
{
	const int stridebits = stride * 64;	// 64 bits per word
	const int count = width * bitsPerPixel;
	const auto f = bitsPerPixel == 1 ? bits_scan_one_forward_64 : bits_scan_zero_forward_64;

	if (x == 0 && stridebits == count)
	{
		if (f(height * stridebits, bits, y * stridebits) != -1)
		{
			return FALSE;
		}
	}
	else
	{
		for (int iy = 0, pos = (y * stridebits) + (x * bitsPerPixel); iy < height; iy++, pos += stridebits)
		{
			if (f(count, bits, pos) != -1)
			{
				return FALSE;
			}
		}
	}

	return TRUE;
}

GENIXAPI(BOOL, is_all_black)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride)
{
	const int stridebits = stride * 64;	// 64 bits per word
	const int count = width * bitsPerPixel;
	const auto f = bitsPerPixel == 1 ? bits_scan_zero_forward_64 : bits_scan_one_forward_64;

	if (x == 0 && stridebits == count)
	{
		if (f(height * stridebits, bits, y * stridebits) != -1)
		{
			return FALSE;
		}
	}
	else
	{
		for (int iy = 0, pos = (y * stridebits) + (x * bitsPerPixel); iy < height; iy++, pos += stridebits)
		{
			if (f(count, bits, pos) != -1)
			{
				return FALSE;
			}
		}
	}

	return TRUE;
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
	const unsigned __int8* bits_u8 = ((const unsigned __int8*)bits) + (ptrdiff_t(y) * stridebytes) + x;

	for (int iy = 0; iy < height; iy++, bits_u8 += stridebytes)
	{
		hist[iy] = ::sum_ip_u8(width, bits_u8, 0);
	}
}

GENIXAPI(void, minmax_8bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride,
	unsigned __int8* min, unsigned __int8* max)
{
	const int stridebytes = stride * sizeof(unsigned __int64);	// 8 bytes per word
	const Ipp8u* bits_u8 = (const Ipp8u*)bits + (ptrdiff_t(y) * stridebytes) + x;

	ippiMinMax_8u_C1R(
		bits_u8,
		stridebytes,
		{ width, height },
		min,
		max);
}

GENIXAPI(void, minmax_16bpp)(
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride,
	unsigned __int16* min, unsigned __int16* max)
{
	const int stridebytes = stride * sizeof(unsigned __int64);	// 8 bytes per word
	const Ipp16u* bits_u16 = (const Ipp16u*)((const Ipp8u*)bits + (ptrdiff_t(y) * stridebytes)) + x;

	ippiMinMax_16u_C1R(
		bits_u16,
		stridebytes,
		{ width, height },
		min,
		max);
}

