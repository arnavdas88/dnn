#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, mirror_vert)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	unsigned __int8* src, const int srcstep,
	unsigned __int8* dst, const int dststep)
{
	const IppiSize roiSize = { width, height };

	src += (ptrdiff_t(y) * srcstep) + (x * bitsPerPixel / 8);

	if (dst == NULL || dst == src)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiMirror_8u_C1IR(src, srcstep, roiSize, ippAxsVertical);
		case 24: return ippiMirror_8u_C3IR(src, srcstep, roiSize, ippAxsVertical);
		case 32: return ippiMirror_8u_C4IR(src, srcstep, roiSize, ippAxsVertical);
		}
	}
	else
	{
		dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiMirror_8u_C1R(src, srcstep, dst, dststep, roiSize, ippAxsVertical);
		case 24: return ippiMirror_8u_C3R(src, srcstep, dst, dststep, roiSize, ippAxsVertical);
		case 32: return ippiMirror_8u_C4R(src, srcstep, dst, dststep, roiSize, ippAxsVertical);
		}
	}

	return ippStsNoOperation;
}

GENIXAPI(int, mirror_horz)(
	const int bitsPerPixel,
	const int x, const int y, const int width, const int height,
	unsigned __int8* src, const int srcstep,
	unsigned __int8* dst, const int dststep)
{
	const IppiSize roiSize = { width, height };

	src += (ptrdiff_t(y) * srcstep) + (x * bitsPerPixel / 8);

	if (dst == NULL || dst == src)
	{
		switch (bitsPerPixel)
		{
		case 8: return ippiMirror_8u_C1IR(src, srcstep, roiSize, ippAxsVertical);
		case 24: return ippiMirror_8u_C3IR(src, srcstep, roiSize, ippAxsVertical);
		case 32: return ippiMirror_8u_C4IR(src, srcstep, roiSize, ippAxsVertical);
		}
	}
	else
	{
		dst += (ptrdiff_t(y) * dststep) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8: return ippiMirror_8u_C1R(src, srcstep, dst, dststep, roiSize, ippAxsVertical);
		case 24: return ippiMirror_8u_C3R(src, srcstep, dst, dststep, roiSize, ippAxsVertical);
		case 32: return ippiMirror_8u_C4R(src, srcstep, dst, dststep, roiSize, ippAxsVertical);
		}
	}

	return ippStsNoOperation;
}

