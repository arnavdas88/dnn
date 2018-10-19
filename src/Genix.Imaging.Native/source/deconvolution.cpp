#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, deconvolution)(
	const int channels,
	const int kernelSize,
	const float* pKernel,
	const int FFTorder,
	const float threshold,
	const int x, const int y,
	const int width, const int height,
	const float* src, const int stridesrc,
	float* dst, const int stridedst)
{
	IppStatus status = ippStsNoErr;

	src += (ptrdiff_t(y) * stridesrc) + x;
	dst += (ptrdiff_t(y) * stridedst) + x;

	IppiSize roiSize = { width, height };
	IppiDeconvFFTState_32f_C1R* pState = NULL;
	int stateSize = 0;

	/* Computes the temporary work buffer size */
	check_sts(status = ippiDeconvFFTGetSize_32f(
		channels,
		kernelSize,
		FFTorder,
		&stateSize));
	pState = (IppiDeconvFFTState_32f_C1R*)ippsMalloc_8u(stateSize);

	check_sts(status = ippiDeconvFFTInit_32f_C1R(
		pState,
		pKernel,
		kernelSize,
		FFTorder,
		threshold));

	/* Line Suppression processing */
	check_sts(status = ippiDeconvFFT_32f_C1R(
		src,
		stridesrc * sizeof(float),
		dst,
		stridedst * sizeof(float),
		roiSize,
		pState));

	EXIT_MAIN
	ippiFree(pState);
	return (int)status;
}
