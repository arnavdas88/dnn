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

GENIXAPI(int, canny)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	unsigned __int8* dst, const int stridedst,
	const float thresholdLow, const float thresholdHigh,
	const int borderType, const unsigned borderValue)
{
	IppStatus status = ippStsNoErr;

	src += (ptrdiff_t(y) * stridesrc) + x;
	dst += (ptrdiff_t(y) * stridedst) + x;

	IppiSize roiSize = { width, height };
	int bufSizeSobV = 0, bufSizeSobH = 0, bufSize = 0;
	Ipp8u* pBuffer = NULL;
	Ipp16s* dx = NULL;
	Ipp16s* dy = NULL; /* Pointers to the buffer for first derivatives with respect to X / Y */
	int stridedx = 0, stridedy = 0;

	IppiBorderType ippBorderType;
	switch (borderType)
	{
	case genixBorderRepl:	ippBorderType = ippBorderRepl; break;
	default:				ippBorderType = ippBorderConst; break;
	}

	/* Computes the temporary work buffer sizes */
	check_sts(status = ippiFilterSobelVertBorderGetBufferSize(roiSize, ippMskSize3x3, ipp8u, ipp16s, 1, &bufSizeSobV));
	check_sts(status = ippiFilterSobelHorizBorderGetBufferSize(roiSize, ippMskSize3x3, ipp8u, ipp16s, 1, &bufSizeSobH));
	check_sts(status = ippiCannyGetSize(roiSize, &bufSize));

	/* Find maximum buffer size and allocate buffer */
	if (bufSizeSobV > bufSize) bufSize = bufSizeSobV;
	if (bufSizeSobH > bufSize) bufSize = bufSizeSobH;
	pBuffer = ippsMalloc_8u(bufSize);

	/* Allocate buffers for derivatives */
	dx = ippiMalloc_16s_C1(width, height, &stridedx);
	dy = ippiMalloc_16s_C1(width, height, &stridedy);

	/* Compute derivatives */
	check_sts(status = ippiFilterSobelNegVertBorder_8u16s_C1R(
		src,
		stridesrc,
		dx,
		stridedx,
		roiSize,
		ippMskSize3x3,
		ippBorderType,
		borderValue,
		pBuffer));

	check_sts(status = ippiFilterSobelHorizBorder_8u16s_C1R(
		src,
		stridesrc,
		dy,
		stridedy,
		roiSize,
		ippMskSize3x3,
		ippBorderType,
		borderValue,
		pBuffer));

	/* Apply edge detector */
	check_sts(status = ippiCanny_16s8u_C1R(
		dx,
		stridedx,
		dy,
		stridedy,
		dst,
		stridedst,
		roiSize,
		thresholdLow,
		thresholdHigh,
		pBuffer));

	EXIT_MAIN
	ippiFree(dy);
	ippiFree(dx);
	ippiFree(pBuffer);
	return (int)status;
}
