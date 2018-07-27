#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, scale8)(
	const int widthsrc, const int heightsrc, const int stridesrc, const unsigned __int64* src,
	const int widthdst, const int heightdst, const int stridedst, unsigned __int64* dst)
{
	IppStatus status = ippStsNoErr;
	IppiSize srcSize = { widthsrc, heightsrc };
	IppiSize dstSize = { widthdst, heightdst };
	int specSize = 0, initSize = 0, bufSize = 0;
	IppiResizeSpec_32f* pSpec = NULL;
	Ipp8u* pInit = NULL;
	Ipp8u* pBuffer = NULL;

	/* Calculation of work buffer size */
	check_sts(status = ippiResizeGetSize_8u(
		srcSize,
		dstSize,
		ippLinear,
		1,
		&specSize,
		&initSize));

	/* Memory allocation */
	pSpec = (IppiResizeSpec_32f*)ippsMalloc_8u(specSize + initSize);
	pInit = (Ipp8u*)pSpec + initSize;

	/* Filter initialization */
	check_sts(status = ippiResizeAntialiasingLinearInit(srcSize, dstSize, pSpec, pInit));
	check_sts(status = ippiResizeGetBufferSize_8u(pSpec, dstSize, 3, &bufSize));
	pBuffer = ippsMalloc_8u(bufSize);

	/* Function call */
	check_sts(status = ippiResizeAntialiasing_8u_C1R(
		(const Ipp8u*)src,
		stridesrc * sizeof(unsigned __int64),
		(Ipp8u*)dst,
		stridedst * sizeof(unsigned __int64),
		{ 0, 0 },
		dstSize,
		ippBorderRepl,
		NULL,
		pSpec,
		pBuffer));

	EXIT_MAIN
	ippsFree(pSpec);
	ippsFree(pBuffer);
	return (int)status;
}

