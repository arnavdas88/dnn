#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, colorkey)(
	const int bitsPerPixel,
	const int x, const int y,
	const int width, const int height,
	const unsigned __int8* src1, const int stridesrc1,
	const unsigned __int8* src2, const int stridesrc2,
	unsigned __int8* dst, const int stridedst,
	const unsigned color)
{
	IppStatus status = ippStsNoErr;

	src1 += (ptrdiff_t(y) * stridesrc1) + (x * bitsPerPixel / 8);
	src2 += (ptrdiff_t(y) * stridesrc2) + (x * bitsPerPixel / 8);
	dst += (ptrdiff_t(y) * stridedst) + (x * bitsPerPixel / 8);

	IppiSize roiSize = { width, height };

	switch (bitsPerPixel)
	{
	case 8:
		check_sts(status = ippiCompColorKey_8u_C1R(
			src1,
			stridesrc1,
			src2,
			stridesrc2,
			dst,
			stridedst,
			roiSize,
			color));
		break;

	case 24:
		check_sts(status = ippiCompColorKey_8u_C3R(
			src1,
			stridesrc1,
			src2,
			stridesrc2,
			dst,
			stridedst,
			roiSize,
			(Ipp8u*)&color));
		break;

	case 32:
		check_sts(status = ippiCompColorKey_8u_C4R(
			src1,
			stridesrc1,
			src2,
			stridesrc2,
			dst,
			stridedst,
			roiSize,
			(Ipp8u*)&color));
		break;
	}

	EXIT_MAIN
	return (int)status;
}
