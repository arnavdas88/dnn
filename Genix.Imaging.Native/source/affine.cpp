#include "stdafx.h"
#include <stdio.h>
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, affine_rotate_coeff)(
	const int widthsrc, const int heightsrc,
	const int widthdst, const int heightdst,
	const double angle,
	double* c00, double* c01, double* c02,
	double* c10, double* c11, double* c12)
{
	IppStatus status = ippStsNoErr;

	IppiSize srcSize = { widthsrc, heightsrc };
	IppiSize dstSize = { widthdst, heightdst };
	double xCenterSrc = widthsrc / 2;
	double yCenterSrc = heightsrc / 2;
	double xCenterDst = widthdst / 2;
	double yCenterDst = heightdst / 2;
	double coeffs[2][3];

	/* Get transform */
	double xShift;
	double yShift;
	check_sts(status = ippiGetRotateShift(xCenterSrc, yCenterSrc, angle, &xShift, &yShift));

	double xShiftTmp;
	double yShiftTmp;
	check_sts(status = ippiGetRotateShift(1.0, 1.0, angle, &xShiftTmp, &yShiftTmp));

	xShift += xShiftTmp; yShift += yShiftTmp;
	xShift += xCenterDst - xCenterSrc;
	yShift += yCenterDst - yCenterSrc;
	check_sts(status = ippiGetRotateTransform(angle, xShift, yShift, coeffs));

	*c00 = coeffs[0][0];
	*c01 = coeffs[0][1];
	*c02 = coeffs[0][2];
	*c10 = coeffs[1][0];
	*c11 = coeffs[1][1];
	*c12 = coeffs[1][2];

	EXIT_MAIN
	return (int)status;
}

GENIXAPI(int, affine)(
	const int bitsPerPixel,
	const int widthsrc, const int heightsrc, const int stridesrc, const unsigned __int64* src,
	const int widthdst, const int heightdst, const int stridedst, unsigned __int64* dst,
	const double c00, const double c01, const double c02,
	const double c10, const double c11, const double c12)
{
	IppStatus status = ippStsNoErr;
	const IppiSize srcSize = { widthsrc, heightsrc };
	const IppiSize dstSize = { widthdst, heightdst };
	int specSize = 0, initSize = 0, bufSize = 0;
	IppiWarpSpec* pSpec = NULL;
	Ipp8u* pBuffer = NULL;
	const Ipp64f borderValue = 255;

	/* Set transform */
	double coeffs[2][3];
	coeffs[0][0] = c00;
	coeffs[0][1] = c01;
	coeffs[0][2] = c02;
	coeffs[1][0] = c10;
	coeffs[1][1] = c11;
	coeffs[1][2] = c12;

	/* Spec and init buffer sizes */
	check_sts(status = ippiWarpAffineGetSize(
		srcSize,
		dstSize,
		ipp8u,
		coeffs,
		ippNearest,
		ippWarpForward,
		ippBorderConst,
		&specSize,
		&initSize));

	/* Memory allocation */
	pSpec = (IppiWarpSpec*)ippsMalloc_8u(specSize);

	/* Filter initialization */
	check_sts(status = ippiWarpAffineNearestInit(
		srcSize,
		dstSize,
		ipp8u,
		coeffs,
		ippWarpForward,
		1,
		ippBorderConst,
		&borderValue,
		0,
		pSpec));

	/* Work buffer size */
	check_sts(status = ippiWarpGetBufferSize(pSpec, dstSize, &bufSize));
	pBuffer = ippsMalloc_8u(bufSize);

	/* Function call */
	if (bitsPerPixel == 8)
	{
		check_sts(status = ippiWarpAffineNearest_8u_C1R(
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
		check_sts(status = ippiWarpAffineNearest_8u_C4R(
			(const Ipp8u*)src,
			stridesrc * sizeof(unsigned __int64),
			(Ipp8u*)dst,
			stridedst * sizeof(unsigned __int64),
			{ 0, 0 },
			dstSize,
			pSpec,
			pBuffer));
	}

	EXIT_MAIN
	ippsFree(pSpec);
	ippsFree(pBuffer);
	return (int)status;
}

