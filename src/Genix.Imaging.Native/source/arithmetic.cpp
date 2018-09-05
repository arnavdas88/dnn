#include "stdafx.h"
#include "ipp.h"

// Adds pixel values of two images.
GENIXAPI(int, _add)(
	int width, int height,
	const unsigned __int64* src1, int stridesrc1,
	const unsigned __int64* src2, int stridesrc2,
	unsigned __int64* dst, int stridedst,
	int bitsPerPixel)
{
	stridesrc1 *= sizeof(unsigned __int64);
	stridesrc2 *= sizeof(unsigned __int64);
	stridedst *= sizeof(unsigned __int64);

	IppiSize roiSize = { width, height };

	switch (bitsPerPixel)
	{
	case 8: return ippiAdd_8u_C1RSfs((const Ipp8u*)src1, stridesrc1, (const Ipp8u*)src2, stridesrc2, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 24: return ippiAdd_8u_C3RSfs((const Ipp8u*)src1, stridesrc1, (const Ipp8u*)src2, stridesrc2, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 32: return ippiAdd_8u_AC4RSfs((const Ipp8u*)src1, stridesrc1, (const Ipp8u*)src2, stridesrc2, (Ipp8u*)dst, stridedst, roiSize, 1);
	default: return ippStsBadArgErr;
	}
}

// Adds a constant to pixel values of an image.
GENIXAPI(int, _addc)(
	int width, int height,
	const unsigned __int64* src, int stridesrc,
	int value,
	unsigned __int64* dst, int stridedst,
	int bitsPerPixel)
{
	stridesrc *= sizeof(unsigned __int64);
	stridedst *= sizeof(unsigned __int64);

	IppiSize roiSize = { width, height };

	switch (bitsPerPixel)
	{
	case 8: return ippiAddC_8u_C1RSfs((const Ipp8u*)src, stridesrc, value, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 24: return ippiAddC_8u_C3RSfs((const Ipp8u*)src, stridesrc, (const Ipp8u*)&value, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 32: return ippiAddC_8u_AC4RSfs((const Ipp8u*)src, stridesrc, (const Ipp8u*)&value, (Ipp8u*)dst, stridedst, roiSize, 1);
	default: return ippStsBadArgErr;
	}
}

// Subtracts pixel values of two images.
GENIXAPI(int, _sub)(
	int width, int height,
	const unsigned __int64* src1, int stridesrc1,
	const unsigned __int64* src2, int stridesrc2,
	unsigned __int64* dst, int stridedst,
	int bitsPerPixel)
{
	stridesrc1 *= sizeof(unsigned __int64);
	stridesrc2 *= sizeof(unsigned __int64);
	stridedst *= sizeof(unsigned __int64);

	IppiSize roiSize = { width, height };

	switch (bitsPerPixel)
	{
	case 8: return ippiSub_8u_C1RSfs((const Ipp8u*)src1, stridesrc1, (const Ipp8u*)src2, stridesrc2, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 24: return ippiSub_8u_C3RSfs((const Ipp8u*)src1, stridesrc1, (const Ipp8u*)src2, stridesrc2, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 32: return ippiSub_8u_AC4RSfs((const Ipp8u*)src1, stridesrc1, (const Ipp8u*)src2, stridesrc2, (Ipp8u*)dst, stridedst, roiSize, 1);
	default: return ippStsBadArgErr;
	}
}

// Subtracts a constant from pixel values of an image.
GENIXAPI(int, _subc)(
	int width, int height,
	const unsigned __int64* src, int stridesrc,
	int value,
	unsigned __int64* dst, int stridedst,
	int bitsPerPixel)
{
	stridesrc *= sizeof(unsigned __int64);
	stridedst *= sizeof(unsigned __int64);

	IppiSize roiSize = { width, height };

	switch (bitsPerPixel)
	{
	case 8: return ippiSubC_8u_C1RSfs((const Ipp8u*)src, stridesrc, value, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 24: return ippiSubC_8u_C3RSfs((const Ipp8u*)src, stridesrc, (const Ipp8u*)&value, (Ipp8u*)dst, stridedst, roiSize, 1);
	case 32: return ippiSubC_8u_AC4RSfs((const Ipp8u*)src, stridesrc, (const Ipp8u*)&value, (Ipp8u*)dst, stridedst, roiSize, 1);
	default: return ippStsBadArgErr;
	}
}