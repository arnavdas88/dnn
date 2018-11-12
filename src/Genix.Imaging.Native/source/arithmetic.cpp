#include "stdafx.h"
#include "ipp.h"

void __forceinline copyAlphaChannel(const Ipp8u* src, const int srcstep, Ipp8u* dst, const int dststep, const int width, const int height)
{
	for (int iy = 0; iy < height; iy++, src += srcstep, dst += dststep)
	{
		for (int ix = 0; ix < width; ix++)
		{
			((unsigned __int32*)dst)[ix] |= ((unsigned __int32*)src)[ix] & 0xff000000u;
		}
	}
}

// Adds pixel values of two images.
GENIXAPI(int, _add)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src1, const int src1step, const int src1x, const int src1y,
	const Ipp8u* src2, const int src2step, const int src2x, const int src2y,
	Ipp8u* dst, const int dststep,
	const int scaleFactor)
{
	IppStatus status = ippStsBadArgErr;

	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);
	src1 += (ptrdiff_t(src1y) * src1step) + (src1x * bitsPerPixel / 8);

	if (src2 == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiAdd_8u_C1IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 16: return ippiAdd_16s_C1IRSfs((const Ipp16s*)src1, src1step, (Ipp16s*)dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAdd_8u_C3IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAdd_8u_AC4IRSfs(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		}
	}
	else
	{
		src2 += (ptrdiff_t(src2y) * src2step) + (src2x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiAdd_8u_C1RSfs(src2, src2step, src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 16: return ippiAdd_16s_C1RSfs((const Ipp16s*)src2, src2step, (const Ipp16s*)src1, src1step, (Ipp16s*)dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAdd_8u_C3RSfs(src2, src2step, src1, src1step, dst, dststep, { width, height }, scaleFactor);
		case 32: 
			status = ippiAdd_8u_AC4RSfs(src2, src2step, src1, src1step, dst, dststep, { width, height }, scaleFactor);
			if (status == ippStsNoErr)
			{
				copyAlphaChannel(src1, src1step, dst, dststep, width, height);
			}

			return status;
		}
	}

	return status;
}

// Adds a constant to pixel values of an image.
GENIXAPI(int, _addc)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src, const int srcstep,
	const unsigned value,
	Ipp8u* dst, const int dststep,
	const int scaleFactor)
{
	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);

	if (src == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiAddC_8u_C1IRSfs(value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAddC_8u_C3IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAddC_8u_C4IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		src += (ptrdiff_t(y) * srcstep) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiAddC_8u_C1RSfs(src, srcstep, value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiAddC_8u_C3RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiAddC_8u_C4RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}

// Subtracts pixel values of two images.
GENIXAPI(int, _sub)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src1, int src1step, const int src1x, const int src1y,
	const Ipp8u* src2, int src2step, const int src2x, const int src2y,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	IppStatus status = ippStsBadArgErr;

	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);
	src1 += (ptrdiff_t(src1y) * src1step) + (src1x * bitsPerPixel / 8);

	if (src2 == NULL)
	{
		IppStatus(__stdcall *func)(const Ipp8u*, int, Ipp8u*, int, IppiSize, int) = NULL;
		switch (bitsPerPixel)
		{
		case 8: func = ippiSub_8u_C1IRSfs; break;
		case 24: func = ippiSub_8u_C3IRSfs; break;
		case 32: func = ippiSub_8u_AC4IRSfs; break;
		}

		if (func != NULL)
		{
			status = func(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		}
	}
	else
	{
		src2 += (ptrdiff_t(src2y) * src2step) + (src2x * bitsPerPixel / 8);

		IppStatus(__stdcall *func)(const Ipp8u*, int, const Ipp8u*, int, Ipp8u*, int, IppiSize, int) = NULL;
		switch (bitsPerPixel)
		{
		case 8: func = ippiSub_8u_C1RSfs; break;
		case 24: func = ippiSub_8u_C3RSfs; break;
		case 32: func = ippiSub_8u_AC4RSfs; break;
		}

		if (func != NULL)
		{
			status = func(src2, src2step, src1, src1step, dst, dststep, { width, height }, scaleFactor);
			if (status == ippStsNoErr && bitsPerPixel == 32)
			{
				copyAlphaChannel(src1, src1step, dst, dststep, width, height);
			}
		}
	}

	return status;
}

// Subtracts a constant from pixel values of an image.
GENIXAPI(int, _subc)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src, const int srcstep,
	const unsigned value,
	Ipp8u* dst, const int dststep,
	const int scaleFactor)
{
	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);

	if (src == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiSubC_8u_C1IRSfs(value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiSubC_8u_C3IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiSubC_8u_C4IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		src += (ptrdiff_t(y) * srcstep) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiSubC_8u_C1RSfs(src, srcstep, value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiSubC_8u_C3RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiSubC_8u_C4RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}

// Multiplies pixel values of two images.
GENIXAPI(int, _mul)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src1, int src1step, const int src1x, const int src1y,
	const Ipp8u* src2, int src2step, const int src2x, const int src2y,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	IppStatus status = ippStsBadArgErr;

	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);
	src1 += (ptrdiff_t(src1y) * src1step) + (src1x * bitsPerPixel / 8);

	if (src2 == NULL)
	{
		IppStatus(__stdcall *func)(const Ipp8u*, int, Ipp8u*, int, IppiSize, int) = NULL;
		switch (bitsPerPixel)
		{
		case 8: func = ippiMul_8u_C1IRSfs; break;
		case 24: func = ippiMul_8u_C3IRSfs; break;
		case 32: func = ippiMul_8u_AC4IRSfs; break;
		}

		if (func != NULL)
		{
			status = func(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		}
	}
	else
	{
		src2 += (ptrdiff_t(src2y) * src2step) + (src2x * bitsPerPixel / 8);

		IppStatus(__stdcall *func)(const Ipp8u*, int, const Ipp8u*, int, Ipp8u*, int, IppiSize, int) = NULL;
		switch (bitsPerPixel)
		{
		case 8: func = ippiMul_8u_C1RSfs; break;
		case 24: func = ippiMul_8u_C3RSfs; break;
		case 32: func = ippiMul_8u_AC4RSfs; break;
		}

		if (func != NULL)
		{
			status = func(src2, src2step, src1, src1step, dst, dststep, { width, height }, scaleFactor);
			if (status == ippStsNoErr && bitsPerPixel == 32)
			{
				copyAlphaChannel(src1, src1step, dst, dststep, width, height);
			}
		}
	}

	return status;
}

// Multiplies pixel values of an image by a constant.
GENIXAPI(int, _mulc)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src, const int srcstep,
	const unsigned value,
	Ipp8u* dst, const int dststep,
	const int scaleFactor)
{
	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);

	if (src == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiMulC_8u_C1IRSfs(value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiMulC_8u_C3IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiMulC_8u_C4IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		src += (ptrdiff_t(y) * srcstep) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiMulC_8u_C1RSfs(src, srcstep, value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiMulC_8u_C3RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiMulC_8u_C4RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}

// Divides pixel values of an image by pixel values of another image.
GENIXAPI(int, _div)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src1, int src1step, const int src1x, const int src1y,
	const Ipp8u* src2, int src2step, const int src2x, const int src2y,
	Ipp8u* dst, int dststep,
	int scaleFactor)
{
	IppStatus status = ippStsBadArgErr;

	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);
	src1 += (ptrdiff_t(src1y) * src1step) + (src1x * bitsPerPixel / 8);

	if (src2 == NULL)
	{
		IppStatus(__stdcall *func)(const Ipp8u*, int, Ipp8u*, int, IppiSize, int) = NULL;
		switch (bitsPerPixel)
		{
		case 8: func = ippiDiv_8u_C1IRSfs; break;
		case 24: func = ippiDiv_8u_C3IRSfs; break;
		case 32: func = ippiDiv_8u_AC4IRSfs; break;
		}

		if (func != NULL)
		{
			status = func(src1, src1step, dst, dststep, { width, height }, scaleFactor);
		}
	}
	else
	{
		src2 += (ptrdiff_t(src2y) * src2step) + (src2x * bitsPerPixel / 8);

		IppStatus(__stdcall *func)(const Ipp8u*, int, const Ipp8u*, int, Ipp8u*, int, IppiSize, int) = NULL;
		switch (bitsPerPixel)
		{
		case 8: func = ippiDiv_8u_C1RSfs; break;
		case 24: func = ippiDiv_8u_C3RSfs; break;
		case 32: func = ippiDiv_8u_AC4RSfs; break;
		}

		if (func != NULL)
		{
			status = func(src2, src2step, src1, src1step, dst, dststep, { width, height }, scaleFactor);
			if ((status == ippStsNoErr || status == ippStsDivByZero) && bitsPerPixel == 32)
			{
				copyAlphaChannel(src1, src1step, dst, dststep, width, height);
			}
		}
	}

	return status;
}

// Divides pixel values of an image by pixel values of another image.
GENIXAPI(int, _divc)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	const Ipp8u* src, const int srcstep,
	const unsigned value,
	Ipp8u* dst, const int dststep,
	const int scaleFactor)
{
	dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);

	if (src == NULL)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiDivC_8u_C1IRSfs(value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiDivC_8u_C3IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiDivC_8u_C4IRSfs((const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
	else
	{
		src += (ptrdiff_t(y) * srcstep) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiDivC_8u_C1RSfs(src, srcstep, value, dst, dststep, { width, height }, scaleFactor);
		case 24: return ippiDivC_8u_C3RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		case 32: return ippiDivC_8u_C4RSfs(src, srcstep, (const Ipp8u*)&value, dst, dststep, { width, height }, scaleFactor);
		default: return ippStsBadArgErr;
		}
	}
}
