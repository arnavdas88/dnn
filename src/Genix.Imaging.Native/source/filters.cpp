#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */
enum _GenixNormalizationType : int
{
	genixInfinity = 0,
	genixL1,
	genixL2,
};
enum _GenixBorderType : int
{
	genixBorderConst = 0,
	genixBorderRepl,
};

GENIXAPI(int, filterRectangular)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int kernelWidth, const int kernelHeight, const float* kernel,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	const IppiSize kernelSize = { kernelWidth, kernelHeight };
	int bufferSize = 0, specSize = 0;		/* Common work buffer size */
	IppiFilterBorderSpec* pSpec = NULL;		/* context structure */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	/* Allocate buffer */
	check_sts(status = ippiFilterBorderGetSize(
		kernelSize,
		roiSize,
		ipp8u,
		ipp32f,
		bitsPerPixel / 8,
		&specSize,
		&bufferSize));
	pSpec = (IppiFilterBorderSpec *)ippsMalloc_8u(specSize);
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Initialize filter */
	check_sts(status = ippiFilterBorderInit_32f(
		kernel,
		kernelSize,
		ipp8u,
		bitsPerPixel / 8,
		ippRndFinancial,
		pSpec));

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterBorder_8u_C1R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pSpec,
			pBuffer));
		break;

	case 24:
		check_sts(status = ippiFilterBorder_8u_C3R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pSpec,
			pBuffer));
		break;

	case 32:
		check_sts(status = ippiFilterBorder_8u_C4R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pSpec,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	ippsFree(pSpec);
	return (int)status;
}

GENIXAPI(int, filterBox)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int maskWidth, const int maskHeight,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	const IppiSize maskSize = { maskWidth, maskHeight };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	/* Allocate buffer */
	check_sts(status = ippiFilterBoxBorderGetBufferSize(
		roiSize,
		maskSize,
		ipp8u,
		bitsPerPixel / 8,
		&bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterBoxBorder_8u_C1R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			maskSize,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;

	case 24:
		check_sts(status = ippiFilterBoxBorder_8u_C3R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			maskSize,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;

	case 32:
		check_sts(status = ippiFilterBoxBorder_8u_AC4R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			maskSize,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterGaussian)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int kernelSize,
	const float sigma,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0, specSize = 0;		/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */
	IppFilterGaussianSpec* pSpec = NULL;	/* context structure */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	/* Allocate buffer */
	check_sts(status = ippiFilterGaussianGetBufferSize(
		roiSize,
		kernelSize,
		ipp8u,
		bitsPerPixel / 8,
		&specSize,
		&bufferSize));
	pSpec = (IppFilterGaussianSpec *)ippsMalloc_8u(specSize);
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Initialize filter */
	check_sts(status = ippiFilterGaussianInit(
		roiSize,
		kernelSize,
		sigma,
		ippBorderType,
		ipp8u,
		bitsPerPixel / 8,
		pSpec,
		pBuffer));

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterGaussianBorder_8u_C1R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			(Ipp8u)borderValue,
			pSpec,
			pBuffer));
		break;

	case 24:
		check_sts(status = ippiFilterGaussianBorder_8u_C3R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			(Ipp8u*)&borderValue,
			pSpec,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	ippsFree(pSpec);
	return (int)status;
}

GENIXAPI(int, filterLaplace)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterLaplaceBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp8u,
		bitsPerPixel / 8,
		&bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterLaplaceBorder_8u_C1R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u)borderValue,
			pBuffer));
		break;

	case 24:
		check_sts(status = ippiFilterLaplaceBorder_8u_C3R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;

	case 32:
		check_sts(status = ippiFilterLaplaceBorder_8u_AC4R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobel)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int normType,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppNormType ippNormType;
	switch (normType)
	{
	case genixInfinity:	ippNormType = ippNormInf; break;
	case genixL1:		ippNormType = ippNormL1; break;
	default:			ippNormType = ippNormL2; break;
	}

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelGetBufferSize(
		roiSize,
		mask,
		ippNormType,
		ipp8u,
		ipp16s,
		1,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobel_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippNormType,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobelHoriz)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelHorizBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp16s,
		1,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobelHorizBorder_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobelHorizSecond)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelHorizSecondBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp16s,
		1,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobelHorizSecondBorder_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobelVert)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelVertBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp16s,
		1,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobelVertBorder_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobelNegVert)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelNegVertBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp16s,
		1,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobelNegVertBorder_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobelVertSecond)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelVertSecondBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp16s,
		1,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobelVertSecondBorder_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterSobelCross)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	__int16* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterSobelCrossGetBufferSize_8u16s_C1R(
		roiSize,
		mask,
		&bufferSize));

	/* Do filtering */
	check_sts(status = ippiFilterSobelCrossBorder_8u16s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int16),
		roiSize,
		mask,
		ippBorderType,
		(Ipp8u)borderValue,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterWiener)(
	const int bitsPerPixel,
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int maskWidth, const int maskHeight,
	const int anchorx, const int anchory,
	float* noise)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	const IppiSize maskSize = { maskWidth, maskHeight };
	const IppiPoint anchor = { anchorx, anchory };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	/* Allocate buffer */
	check_sts(status = ippiFilterWienerGetBufferSize(
		roiSize,
		maskSize,
		bitsPerPixel / 8,
		&bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterWiener_8u_C1R(
			src + (ptrdiff_t(y) * stridesrc) + x,
			stridesrc,
			dst + (ptrdiff_t(y) * stridedst) + x,
			stridedst,
			roiSize,
			maskSize,
			anchor,
			noise,
			pBuffer));
		break;

	case 24:
		check_sts(status = ippiFilterWiener_8u_C3R(
			src + (ptrdiff_t(y) * stridesrc) + (ptrdiff_t(x) * 3),
			stridesrc,
			dst + (ptrdiff_t(y) * stridedst) + (ptrdiff_t(x) * 3),
			stridedst,
			roiSize,
			maskSize,
			anchor,
			noise,
			pBuffer));
		break;

	case 32:
		check_sts(status = ippiFilterWiener_8u_AC4R(
			src + (ptrdiff_t(y) * stridesrc) + (ptrdiff_t(x) * 4),
			stridesrc,
			dst + (ptrdiff_t(y) * stridedst) + (ptrdiff_t(x) * 4),
			stridedst,
			roiSize,
			maskSize,
			anchor,
			noise,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterHipass)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterHipassBorderGetBufferSize(
		roiSize,
		mask,
		ipp8u,
		ipp8u,
		bitsPerPixel / 8,
		&bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterHipassBorder_8u_C1R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u)borderValue,
			pBuffer));
		break;

	case 24:
		check_sts(status = ippiFilterHipassBorder_8u_C3R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;

	case 32:
		check_sts(status = ippiFilterHipassBorder_8u_AC4R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u*)&borderValue,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, filterLowpass)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const int maskSize /* 3 or 5 */,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	IppiMaskSize mask = maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5;

	/* Allocate buffer */
	check_sts(status = ippiFilterLowpassGetBufferSize_8u_C1R(
		roiSize,
		mask,
		&bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Do filtering */
	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiFilterLowpassBorder_8u_C1R(
			src,
			stridesrc,
			dst,
			stridedst,
			roiSize,
			mask,
			ippBorderType,
			(Ipp8u)borderValue,
			pBuffer));
		break;
	}

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}
