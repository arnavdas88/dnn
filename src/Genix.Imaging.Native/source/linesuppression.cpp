#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, supresslines)(
	const int width, const int height, const int stride,
	const unsigned __int64* src,
	unsigned __int64* dst)
{
	IppStatus status = ippStsNoErr;

	IppiSize roiSize = { width, height };
	IppiMaskSize filterMask = ippMskSize3x3;	/* Linear size of derivative filter aperture */
	Ipp32u avgWndSize = 1;						/* Neighborhood block size for smoothing */
	Ipp32u numChannels = 1;
	Ipp32f threshold = 5.0f;
	const Ipp8u borderValue = 0;
	Ipp8u* pBuffer = NULL;                   /* Pointer to the work buffer */
	int bufSize = 0;
	IppiDifferentialKernel filterType = ippFilterSobel; /* Type of derivative operator */

	int featureStep = 0;		/* Steps, in bytes, through the feature image */
	Ipp8u* pFeature = ippiMalloc_8u_C1(roiSize.width, roiSize.height, &featureStep);

	/* Computes the temporary work buffer size */
	check_sts(status = ippiLineSuppressionGetBufferSize(
		roiSize,
		filterMask,
		avgWndSize,
		ipp8u,
		numChannels,
		&bufSize));
	pBuffer = ippsMalloc_8u(bufSize);

	/* Line Suppression processing */
	check_sts(status = ippiLineSuppression_8u_C1R(
		(const Ipp8u *)src,
		stride * sizeof(unsigned __int64),
		pFeature,
		featureStep,
		(Ipp8u *)dst,
		stride * sizeof(unsigned __int64),
		roiSize,
		filterType,
		filterMask,
		avgWndSize,
		threshold,
		ippBorderRepl,
		borderValue,
		pBuffer));

	EXIT_MAIN
		ippiFree(pFeature);
	return (int)status;
}
