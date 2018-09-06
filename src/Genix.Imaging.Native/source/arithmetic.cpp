#include "stdafx.h"
#include "ipp.h"

// Adds pixel values of two images.
GENIXAPI(int, _add)(
	int bitsPerPixel, int width, int height,
	const Ipp8u* src1, int src1step,
	const Ipp8u* src2, int src2step,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	if (src2 == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiAdd_8u_C1IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAdd_8u_C3IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAdd_8u_AC4IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiAdd_8u_C1RSfs(src1, src1step, src2, src2step, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAdd_8u_C3RSfs(src1, src1step, src2, src2step, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAdd_8u_AC4RSfs(src1, src1step, src2, src2step, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}

// Adds a constant to pixel values of an image.
GENIXAPI(int, _addc)(
	int bitsPerPixel, int width, int height,
	const Ipp8u* src, int srcstep,
	int value,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	if (src == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiAddC_8u_C1IRSfs(value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAddC_8u_C3IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAddC_8u_AC4IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiAddC_8u_C1RSfs(src, srcstep, value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAddC_8u_C3RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAddC_8u_AC4RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}

// Subtracts pixel values of two images.
GENIXAPI(int, _sub)(
	int bitsPerPixel, int width, int height,
	const Ipp8u* src1, int src1step,
	const Ipp8u* src2, int src2step,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	if (src2 == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiSub_8u_C1IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiSub_8u_C3IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiSub_8u_AC4IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiSub_8u_C1RSfs(src1, src1step, src2, src2step, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiSub_8u_C3RSfs(src1, src1step, src2, src2step, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiSub_8u_AC4RSfs(src1, src1step, src2, src2step, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}

// Subtracts a constant from pixel values of an image.
GENIXAPI(int, _subc)(
	int bitsPerPixel, int width, int height,
	const Ipp8u* src, int srcstep,
	int value,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	if (src == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiSubC_8u_C1IRSfs(value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiSubC_8u_C3IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiSubC_8u_AC4IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiSubC_8u_C1RSfs(src, srcstep, value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiSubC_8u_C3RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiSubC_8u_AC4RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}