#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, threshold_lt_8bpp)(
	const int bitsPerPixel,
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const unsigned threshold,
	const unsigned value,
	const BOOL convertAlphaChannel)
{
	if (src == dst)
	{
		dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8:
			return ippiThreshold_LTVal_8u_C1IR(
				dst,
				stridedst,
				{ width, height },
				threshold,
				value);

		case 24:
			return ippiThreshold_LTVal_8u_C3IR(
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);

		case 32:
			return (convertAlphaChannel ? ippiThreshold_LTVal_8u_C4IR : ippiThreshold_LTVal_8u_AC4IR)(
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);
		}
	}
	else
	{
		src += (ptrdiff_t(y) * stridesrc) + (x * bitsPerPixel / 8);
		dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8:
			return ippiThreshold_LTVal_8u_C1R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				threshold,
				value);

		case 24:
			return ippiThreshold_LTVal_8u_C3R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);

		case 32:
			return (convertAlphaChannel ? ippiThreshold_LTVal_8u_C4R : ippiThreshold_LTVal_8u_AC4R)(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);
		}
	}

	return ippStsNoOperation;
}

GENIXAPI(int, threshold_gt_8bpp)(
	const int bitsPerPixel,
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const unsigned threshold,
	const unsigned value,
	const BOOL convertAlphaChannel)
{
	if (src == dst)
	{
		dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8:
			return ippiThreshold_GTVal_8u_C1IR(
				dst,
				stridedst,
				{ width, height },
				threshold,
				value);

		case 24:
			return ippiThreshold_GTVal_8u_C3IR(
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);

		case 32:
			return (convertAlphaChannel ? ippiThreshold_GTVal_8u_C4IR : ippiThreshold_GTVal_8u_AC4IR)(
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);
		}
	}
	else
	{
		src += (ptrdiff_t(y) * stridesrc) + (x * bitsPerPixel / 8);
		dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8:
			return ippiThreshold_GTVal_8u_C1R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				threshold,
				value);

		case 24:
			return ippiThreshold_GTVal_8u_C3R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);

		case 32:
			return (convertAlphaChannel ? ippiThreshold_GTVal_8u_C4R : ippiThreshold_GTVal_8u_AC4R)(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&threshold,
				(const Ipp8u*)&value);
		}
	}

	return ippStsNoOperation;
}

GENIXAPI(int, threshold_ltgt_8bpp)(
	const int bitsPerPixel,
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const unsigned thresholdLT,
	const unsigned valueLT,
	const unsigned thresholdGT,
	const unsigned valueGT)
{
	if (src == dst)
	{
		dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8:
			return ippiThreshold_LTValGTVal_8u_C1IR(
				dst,
				stridedst,
				{ width, height },
				thresholdLT,
				valueLT,
				thresholdGT,
				valueGT);

		case 24:
			return ippiThreshold_LTValGTVal_8u_C3IR(
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&thresholdLT,
				(const Ipp8u*)&valueLT,
				(const Ipp8u*)&thresholdGT,
				(const Ipp8u*)&valueGT);

		case 32:
			return ippiThreshold_LTValGTVal_8u_AC4IR(
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&thresholdLT,
				(const Ipp8u*)&valueLT,
				(const Ipp8u*)&thresholdGT,
				(const Ipp8u*)&valueGT);
		}
	}
	else
	{
		src += (ptrdiff_t(y) * stridesrc) + (x * bitsPerPixel / 8);
		dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

		switch (bitsPerPixel)
		{
		case 8:
			return ippiThreshold_LTValGTVal_8u_C1R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				thresholdLT,
				valueLT,
				thresholdGT,
				valueGT);

		case 24:
			return ippiThreshold_LTValGTVal_8u_C3R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&thresholdLT,
				(const Ipp8u*)&valueLT,
				(const Ipp8u*)&thresholdGT,
				(const Ipp8u*)&valueGT);

		case 32:
			return ippiThreshold_LTValGTVal_8u_AC4R(
				src,
				stridesrc,
				dst,
				stridedst,
				{ width, height },
				(const Ipp8u*)&thresholdLT,
				(const Ipp8u*)&valueLT,
				(const Ipp8u*)&thresholdGT,
				(const Ipp8u*)&valueGT);
		}
	}

	return ippStsNoOperation;
}

GENIXAPI(int, threshold_otsu)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* threshold)
{
	*threshold = 0;

	return ippiComputeThreshold_Otsu_8u_C1R(
		src + (ptrdiff_t(y) * stridesrc) + x,
		stridesrc,
		{ width, height },
		threshold);
}

