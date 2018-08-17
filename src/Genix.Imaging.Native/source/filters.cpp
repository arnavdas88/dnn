#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, filterGaussian_8bpp)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	int kernelSize,
	float sigma)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0, specSize = 0;		/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */
	IppFilterGaussianSpec* pSpec = NULL;	/* context structure */

											/* Allocate buffer */
	check_sts(status = ippiFilterGaussianGetBufferSize(
		roiSize,
		kernelSize,
		ipp8u,
		1,
		&specSize,
		&bufferSize));
	pSpec = (IppFilterGaussianSpec *)ippsMalloc_8u(specSize);
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Initialize filter */
	check_sts(status = ippiFilterGaussianInit(
		roiSize,
		kernelSize,
		sigma,
		ippBorderRepl,
		ipp8u,
		1,
		pSpec,
		pBuffer));

	/* Do filtering */
	check_sts(status = ippiFilterGaussianBorder_8u_C1R(
		(const Ipp8u*)src,
		stridesrc * sizeof(unsigned __int64),
		(Ipp8u*)dst,
		stridedst * sizeof(unsigned __int64),
		roiSize,
		0,
		pSpec,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	ippsFree(pSpec);
	return (int)status;
}

GENIXAPI(int, filterGaussian_24bpp)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	int kernelSize,
	float sigma)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0, specSize = 0;		/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */
	IppFilterGaussianSpec* pSpec = NULL;	/* context structure */

	/* Allocate buffer */
	check_sts(status = ippiFilterGaussianGetBufferSize(
		roiSize,
		kernelSize,
		ipp8u,
		3,
		&specSize,
		&bufferSize));
	pSpec = (IppFilterGaussianSpec *)ippsMalloc_8u(specSize);
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Initialize filter */
	check_sts(status = ippiFilterGaussianInit(
		roiSize,
		kernelSize,
		sigma,
		ippBorderRepl,
		ipp8u,
		3,
		pSpec,
		pBuffer));

	/* Do filtering */
	check_sts(status = ippiFilterGaussianBorder_8u_C3R(
		(const Ipp8u*)src,
		stridesrc * sizeof(unsigned __int64),
		(Ipp8u*)dst,
		stridedst * sizeof(unsigned __int64),
		roiSize,
		0,
		pSpec,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	ippsFree(pSpec);
	return (int)status;
}

GENIXAPI(int, filterLowpass_8bpp)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	int maskSize /* 3 or 5 */)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };
	int bufferSize = 0;						/* Common work buffer size */
	Ipp8u *pBuffer = NULL;					/* Pointer to the work buffer */

	/* Allocate buffer */
	check_sts(status = ippiFilterLowpassGetBufferSize_8u_C1R(
		roiSize,
		maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5,
		&bufferSize));
	pBuffer = ippsMalloc_8u(bufferSize);

	/* Do filtering */
	check_sts(status = ippiFilterLowpassBorder_8u_C1R(
		(const Ipp8u*)src,
		stridesrc * sizeof(unsigned __int64),
		(Ipp8u*)dst,
		stridedst * sizeof(unsigned __int64),
		roiSize,
		maskSize == 3 ? ippMskSize3x3 : ippMskSize5x5,
		ippBorderRepl,
		0,
		pBuffer));

	EXIT_MAIN
		ippsFree(pBuffer);
	return (int)status;
}
