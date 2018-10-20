#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, deconv_FFT)(
	const int x, const int y,
	const int width, const int height,
	const float* src, const int stridesrc,
	float* dst, const int stridedst,
	const int channels,
	const int kernelSize,
	const float* pKernel,
	const int FFTorder)
{
	IppStatus status = ippStsNoErr;

	src += (ptrdiff_t(y) * stridesrc) + x;
	dst += (ptrdiff_t(y) * stridedst) + x;

	IppiSize roiSize = { width, height };
	void* pState = NULL;
	int stateSize = 0;

	/* Computes the temporary work buffer size */
	check_sts(status = ippiDeconvFFTGetSize_32f(
		channels,
		kernelSize,
		FFTorder,
		&stateSize));
	pState = ippsMalloc_8u(stateSize);

	switch (channels)
	{
	case 1:
		check_sts(status = ippiDeconvFFTInit_32f_C1R(
			(IppiDeconvFFTState_32f_C1R*)pState,
			pKernel,
			kernelSize,
			FFTorder,
			0.0001f));

		check_sts(status = ippiDeconvFFT_32f_C1R(
			src,
			stridesrc * sizeof(float),
			dst,
			stridedst * sizeof(float),
			roiSize,
			(IppiDeconvFFTState_32f_C1R*)pState));

		break;

	case 3:
		check_sts(status = ippiDeconvFFTInit_32f_C3R(
			(IppiDeconvFFTState_32f_C3R*)pState,
			pKernel,
			kernelSize,
			FFTorder,
			0.0001f));

		check_sts(status = ippiDeconvFFT_32f_C3R(
			src,
			stridesrc * sizeof(float),
			dst,
			stridedst * sizeof(float),
			roiSize,
			(IppiDeconvFFTState_32f_C3R*)pState));

		break;
	}

	EXIT_MAIN
	ippiFree(pState);
	return (int)status;
}

GENIXAPI(int, deconv_LR)(
	const int x, const int y,
	const int width, const int height,
	const float* src, const int stridesrc,
	float* dst, const int stridedst,
	const int channels,
	const int kernelSize,
	const float* pKernel,
	const int numIter)
{
	IppStatus status = ippStsNoErr;

	src += (ptrdiff_t(y) * stridesrc) + x;
	dst += (ptrdiff_t(y) * stridedst) + x;

	IppiSize roiSize = { width, height };
	IppiSize maxRoi = { width + kernelSize - 1, height + kernelSize - 1 };
	void* pState = NULL;
	int stateSize = 0;

	/* Computes the temporary work buffer size */
	check_sts(status = ippiDeconvLRGetSize_32f(
		channels,
		kernelSize,
		maxRoi,
		&stateSize));
	pState = ippsMalloc_8u(stateSize);

	switch (channels)
	{
	case 1:
		check_sts(status = ippiDeconvLRInit_32f_C1R(
			(IppiDeconvLR_32f_C1R*)pState,
			pKernel,
			kernelSize,
			maxRoi,
			0.0001f));

		check_sts(status = ippiDeconvLR_32f_C1R(
			src,
			stridesrc * sizeof(float),
			dst,
			stridedst * sizeof(float),
			roiSize,
			numIter,
			(IppiDeconvLR_32f_C1R*)pState));

		break;

	case 3:
		check_sts(status = ippiDeconvLRInit_32f_C3R(
			(IppiDeconvLR_32f_C3R*)pState,
			pKernel,
			kernelSize,
			maxRoi,
			0.0001f));

		check_sts(status = ippiDeconvLR_32f_C3R(
			src,
			stridesrc * sizeof(float),
			dst,
			stridedst * sizeof(float),
			roiSize,
			numIter,
			(IppiDeconvLR_32f_C3R*)pState));

		break;
	}

	EXIT_MAIN
	ippiFree(pState);
	return (int)status;
}
