#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, gradientVectorPrewitt_f32)(
	const int width, const int height, const float* src, const int stride,
	float* magnitude, const int magnitudeStride,
	float* angle, const int angleStride)
{
	IppStatus status = ippStsNoErr;

	const IppiSize roiSize = { width, height };
	const IppiMaskSize maskSize = ippMskSize3x3;
	int bufferSize = 0;
	Ipp8u* pBuffer = NULL;

	/* Allocate buffer */
	check_sts(status = ippiGradientVectorGetBufferSize(roiSize, maskSize, ipp32f, 1, &bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Compute gradient using 3x3 Sobel operator (source ROI at point x=1, y=1) */
	check_sts(status = ippiGradientVectorPrewitt_32f_C1R(
		src,
		stride * sizeof(float),
		NULL,
		0,
		NULL,
		0,
		magnitude,
		magnitudeStride * sizeof(float),
		angle,
		angleStride * sizeof(float),
		roiSize,
		maskSize,
		ippNormL2,
		ippBorderConst,
		255,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}

