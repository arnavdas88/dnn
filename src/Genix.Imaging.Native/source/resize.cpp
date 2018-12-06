#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */
enum _GenixBorderType : int
{
	genixBorderConst = 0,
	genixBorderRepl,
};

enum _GenixInterpolationType : int
{
	genixNearestNeighbor = 0,
	genixLinear = 1,
	genixCubic = 2,
	genixLanczos = 3,
	genixSuper = 4,
};

GENIXAPI(int, resize)(
	const int bitsPerPixel,
	const int widthsrc, const int heightsrc, const unsigned __int64* src, const int stridesrc,
	const int widthdst, const int heightdst, unsigned __int64* dst, const int stridedst,
	const int interpolationType, BOOL antialiasing, float valueB, float valueC, const unsigned numLobes,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	IppiSize srcSize = { widthsrc, heightsrc };
	IppiSize dstSize = { widthdst, heightdst };
	int specSize = 0, initSize = 0, bufSize = 0;
	IppiResizeSpec_32f* pSpec = NULL;
	Ipp8u* pInit = NULL;
	Ipp8u* pBuffer = NULL;

	IppStatus(__stdcall *func)(const Ipp8u*, Ipp32s, Ipp8u*, Ipp32s, IppiPoint, IppiSize, const IppiResizeSpec_32f*, Ipp8u*) = NULL;
	IppStatus(__stdcall *funcWithBorder)(const Ipp8u*, Ipp32s, Ipp8u*, Ipp32s, IppiPoint, IppiSize, IppiBorderType, const Ipp8u*, const IppiResizeSpec_32f*, Ipp8u*) = NULL;
	IppStatus(__stdcall *funcAliasing)(const Ipp8u*, Ipp32s, Ipp8u*, Ipp32s, IppiPoint, IppiSize, IppiBorderType, const Ipp8u*, const IppiResizeSpec_32f*, Ipp8u*) = NULL;

	IppiInterpolationType ippInterpolationType;
	switch (interpolationType)
	{
	default:
	case genixNearestNeighbor:	ippInterpolationType = ippNearest; antialiasing = FALSE; break;
	case genixLinear:			ippInterpolationType = ippLinear; break;
	case genixCubic:			ippInterpolationType = ippCubic; break;
	case genixLanczos:			ippInterpolationType = ippLanczos; break;
	case genixSuper:			ippInterpolationType = ippSuper; antialiasing = FALSE; break;
	}

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	case genixBorderConst:
	default:				ippBorderType = ippBorderConst; break;
	}

	/* Calculation of work buffer size */
	check_sts(status = ippiResizeGetSize_8u(
		srcSize,
		dstSize,
		ippInterpolationType,
		antialiasing ? 1 : 0,
		&specSize,
		&initSize));

	/* Memory allocation */
	pSpec = (IppiResizeSpec_32f*)ippsMalloc_8u(specSize + initSize);
	pInit = (Ipp8u*)pSpec + initSize;

	/* Filter initialization */
	switch (ippInterpolationType)
	{
	case ippNearest:
		check_sts(status = ippiResizeNearestInit_8u(srcSize, dstSize, pSpec));
		break;
	case ippLinear:
		if (antialiasing) { check_sts(status = ippiResizeAntialiasingLinearInit(srcSize, dstSize, pSpec, pInit)); }
		else { check_sts(status = ippiResizeLinearInit_8u(srcSize, dstSize, pSpec)); }
		break;
	case ippCubic:
		if (antialiasing) { check_sts(status = ippiResizeAntialiasingCubicInit(srcSize, dstSize, valueB, valueC, pSpec, pInit)); }
		else { check_sts(status = ippiResizeCubicInit_8u(srcSize, dstSize, valueB, valueC, pSpec, pInit)); }
		break;
	case ippLanczos:
		if (antialiasing) { check_sts(status = ippiResizeAntialiasingLanczosInit(srcSize, dstSize, numLobes, pSpec, pInit)); }
		else { check_sts(status = ippiResizeLanczosInit_8u(srcSize, dstSize, numLobes, pSpec, pInit)); }
		break;
	case ippSuper:
		check_sts(status = ippiResizeSuperInit_8u(srcSize, dstSize, pSpec));
		break;
	default:
		status = ippStsBadArgErr;
		goto exitLine;
	}

	check_sts(status = ippiResizeGetBufferSize_8u(pSpec, dstSize, bitsPerPixel / 8, &bufSize));
	pBuffer = ippsMalloc_8u(bufSize);

	/* Function call */
	if (antialiasing)
	{
		switch (bitsPerPixel)
		{
		case 8:		funcAliasing = &ippiResizeAntialiasing_8u_C1R; break;
		case 24:	funcAliasing = &ippiResizeAntialiasing_8u_C3R; break;
		case 32:	funcAliasing = &ippiResizeAntialiasing_8u_C4R; break;
		}
	}
	else
	{
		switch (ippInterpolationType)
		{
		case ippNearest:
			switch (bitsPerPixel)
			{
			case 8:		func = &ippiResizeNearest_8u_C1R; break;
			case 24:	func = &ippiResizeNearest_8u_C3R; break;
			case 32:	func = &ippiResizeNearest_8u_C4R; break;
			}
			break;

		case ippLinear:
			switch (bitsPerPixel)
			{
			case 8:		funcWithBorder = &ippiResizeLinear_8u_C1R; break;
			case 24:	funcWithBorder = &ippiResizeLinear_8u_C3R; break;
			case 32:	funcWithBorder = &ippiResizeLinear_8u_C4R; break;
			}
			break;

		case ippCubic:
			switch (bitsPerPixel)
			{
			case 8:		funcWithBorder = &ippiResizeCubic_8u_C1R; break;
			case 24:	funcWithBorder = &ippiResizeCubic_8u_C3R; break;
			case 32:	funcWithBorder = &ippiResizeCubic_8u_C4R; break;
			}
			break;

		case ippLanczos:
			switch (bitsPerPixel)
			{
			case 8:		funcWithBorder = &ippiResizeLanczos_8u_C1R; break;
			case 24:	funcWithBorder = &ippiResizeLanczos_8u_C3R; break;
			case 32:	funcWithBorder = &ippiResizeLanczos_8u_C4R; break;
			}
			break;

		case ippSuper:
			switch (bitsPerPixel)
			{
			case 8:		func = &ippiResizeSuper_8u_C1R; break;
			case 24:	func = &ippiResizeSuper_8u_C3R; break;
			case 32:	func = &ippiResizeSuper_8u_C4R; break;
			}
			break;
		}
	}

	if (funcAliasing != NULL)
	{
		check_sts(status = (*funcAliasing)(
			(const Ipp8u*)src,
			stridesrc * sizeof(unsigned __int64),
			(Ipp8u*)dst,
			stridedst * sizeof(unsigned __int64),
			{ 0, 0 },
			dstSize,
			ippBorderType,
			(const Ipp8u*)&borderValue,
			pSpec,
			pBuffer));
	}
	else if (funcWithBorder != NULL)
	{
		check_sts(status = (*funcWithBorder)(
			(const Ipp8u*)src,
			stridesrc * sizeof(unsigned __int64),
			(Ipp8u*)dst,
			stridedst * sizeof(unsigned __int64),
			{ 0, 0 },
			dstSize,
			ippBorderType,
			(const Ipp8u*)&borderValue,
			pSpec,
			pBuffer));
	}
	else if (func != NULL)
	{
		check_sts(status = (*func)(
			(const Ipp8u*)src,
			stridesrc * sizeof(unsigned __int64),
			(Ipp8u*)dst,
			stridedst * sizeof(unsigned __int64),
			{ 0, 0 },
			dstSize,
			pSpec,
			pBuffer));
	}
	else
	{
		status = ippStsBadArgErr;
		goto exitLine;
	}

	EXIT_MAIN
	ippsFree(pSpec);
	ippsFree(pBuffer);
	return (int)status;
}

