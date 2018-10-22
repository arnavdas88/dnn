#include "stdafx.h"
#include <stdio.h>
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, integral32s)(
	const int width, const int height,
	const unsigned __int8* src, const int stridesrc, 
	__int32* dst, const int stridedst,
	const int value)
{
	IppStatus status = ippStsNoErr;
	const IppiSize roiSize = { width, height };

	return ippiIntegral_8u32s_C1R(
		src,
		stridesrc,
		dst,
		stridedst * sizeof(__int32),
		roiSize,
		value);
}
