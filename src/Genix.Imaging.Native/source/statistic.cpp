#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

// from Genix.Core.Native.dll
extern "C" __declspec(dllimport) unsigned __int64 WINAPI bits_count_64(int count, const unsigned __int64* bits, int pos);
extern "C" __declspec(dllimport) unsigned __int32 WINAPI sum_ip_u8u32(const int n, const unsigned __int8* x, const int offx);

extern "C" __declspec(dllimport) int WINAPI bits_scan_one_forward_64(int count, const unsigned __int64* bits, int pos);
extern "C" __declspec(dllimport) int WINAPI bits_scan_zero_forward_64(int count, const unsigned __int64* bits, int pos);

GENIXAPI(unsigned __int64, power)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const unsigned __int64* bits, const int stride)
{
	switch (bitsPerPixel)
	{
	case 1:
	{
		unsigned __int64 sum = 0;
		const int stridebits = stride * 64;	// 64 bits per word
		for (int iy = 0, pos = (y * stridebits) + x; iy < height; iy++, pos += stridebits)
		{
			sum += ::bits_count_64(width, bits, pos);
		}

		return sum;
	}

	case 8:
	{
#if true
		// IPP version is approximately 3 times faster ???
		Ipp64f sum = 0;
		const int stridebytes = stride * 8;	// 8 bytes per word
		const unsigned __int8* bits_u8 = ((const unsigned __int8*)bits) + (ptrdiff_t(y) * stridebytes) + x;
		ippiSum_8u_C1R(bits_u8, stridebytes, { width, height }, &sum);
		return (unsigned __int64)sum;
#else
		unsigned __int64 sum = 0;
		const int stridebytes = stride * 8;	// 8 bytes per word
		for (int iy = 0, off = (y * stridebytes) + x; iy < height; iy++, off += stridebytes)
		{
			sum += (unsigned __int64)::sum_ip_u8u32(width, (const unsigned __int8*)bits, off);
		}

		return sum;
#endif
	}
	}

	return (unsigned __int64)-1;
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
		hist[iy] = ::sum_ip_u8u32(width, bits_u8, 0);
	}
}

GENIXAPI(int, minmax)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const unsigned __int8* bits, const int stride,
	unsigned* min, unsigned* max)
{
	IppStatus status = ippStsNoErr;

	switch (bitsPerPixel)
	{
	case 8:
	{
		Ipp8u _min, _max;
		status = ippiMinMax_8u_C1R(bits + (ptrdiff_t(y) * stride) + x, stride, { width, height }, &_min, &_max);
		*min = _min;
		*max = _max;
	}
	break;

	case 16:
	{
		Ipp16u _min, _max;
		status = ippiMinMax_16u_C1R((const Ipp16u*)(bits + (ptrdiff_t(y) * stride)) + x, stride, { width, height }, &_min, &_max);
		*min = _min;
		*max = _max;
	}
	break;
	}

	return status;
}

