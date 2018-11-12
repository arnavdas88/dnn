#include "stdafx.h"
#include <stdio.h>
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, houghline)(
	const int x, const int y, const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	const int maxLineCount,
	const int threshold,
	const float deltaRho,
	const float deltaTheta,
	int* lineCount,
	float* lines)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	const IppPointPolar delta = { deltaRho, deltaTheta };
	Ipp8u* pBuffer = NULL;                   /* Pointer to the work buffer */
	int bufSize = 0;

	src += (ptrdiff_t(y) * stridesrc) + x;

	// hough detectors work on black-on-white images
	// so we need to invert it
	ippiNot_8u_C1IR(const_cast<Ipp8u*>(src), stridesrc, roiSize);

	check_sts(status = ippiHoughLineGetSize_8u_C1R(
		roiSize,
		delta,
		maxLineCount,
		&bufSize));
	pBuffer = ippsMalloc_8u(bufSize);

	check_sts(status = ippiHoughLine_8u32f_C1R(
		src + (ptrdiff_t(y) * stridesrc) + x,
		stridesrc,
		roiSize,
		delta,
		threshold,
		(IppPointPolar*)lines,
		maxLineCount,
		lineCount,
		pBuffer));

	EXIT_MAIN
		// invert image back
		ippiNot_8u_C1IR(const_cast<Ipp8u*>(src), stridesrc, roiSize);

	ippiFree(pBuffer);
	return (int)status;
}

GENIXAPI(int, houghprobline)(
	const int x, const int y, const int width, const int height,
	const unsigned __int8* src, const int stridesrc,
	const int maxLineCount,
	const int threshold,
	const int minLineLength,
	const int maxLineGap,
	const float deltaRho,
	const float deltaTheta,
	int* lineCount,
	int* lines)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	const IppPointPolar delta = { deltaRho, deltaTheta };
	Ipp8u* pBuffer = NULL;                   /* Pointer to the work buffer */
	IppiHoughProbSpec* pSpec = NULL;
	int bufSize = 0, specSize = 0;

	src += (ptrdiff_t(y) * stridesrc) + x;

	// hough detectors work on black-on-white images
	// so we need to invert it
	ippiNot_8u_C1IR(const_cast<Ipp8u*>(src), stridesrc, roiSize);

	check_sts(status = ippiHoughProbLineGetSize_8u_C1R(
		roiSize,
		delta,
		&specSize,
		&bufSize));
	pBuffer = ippsMalloc_8u(bufSize);
	pSpec = (IppiHoughProbSpec*)ippsMalloc_8u(specSize);

	check_sts(status = ippiHoughProbLineInit_8u32f_C1R(
		roiSize,
		delta,
		ippAlgHintNone,
		pSpec));

	check_sts(status = ippiHoughProbLine_8u32f_C1R(
		src,
		stridesrc,
		roiSize,
		threshold,
		minLineLength,
		maxLineGap,
		(IppiPoint*)lines,
		maxLineCount,
		lineCount,
		pBuffer,
		pSpec));

	EXIT_MAIN
		// invert image back
		ippiNot_8u_C1IR(const_cast<Ipp8u*>(src), stridesrc, roiSize);

	ippsFree(pSpec);
	ippiFree(pBuffer);
	return (int)status;
}
