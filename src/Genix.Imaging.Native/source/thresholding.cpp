#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(void, threshold_lt_8bpp)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, int stridesrc,
	unsigned __int64* dst, int stridedst,
	const unsigned __int8 threshold,
	const unsigned __int8 value)
{
	stridesrc *= sizeof(unsigned __int64);	// 8 bytes per word
	const Ipp8u* src_u8 = ((const Ipp8u*)src) + (ptrdiff_t(y) * stridesrc) + x;

	stridedst *= sizeof(unsigned __int64);	// 8 bytes per word
	Ipp8u* dst_u8 = ((Ipp8u*)dst) + (ptrdiff_t(y) * stridedst) + x;

	ippiThreshold_LTVal_8u_C1R(
		src_u8,
		stridesrc,
		dst_u8,
		stridedst,
		{ width, height },
		threshold,
		value);
}

GENIXAPI(void, threshold_gt_8bpp)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, int stridesrc,
	unsigned __int64* dst, int stridedst,
	const unsigned __int8 threshold,
	const unsigned __int8 value)
{
	stridesrc *= sizeof(unsigned __int64);	// 8 bytes per word
	const Ipp8u* src_u8 = ((const Ipp8u*)src) + (ptrdiff_t(y) * stridesrc) + x;

	stridedst *= sizeof(unsigned __int64);	// 8 bytes per word
	Ipp8u* dst_u8 = ((Ipp8u*)dst) + (ptrdiff_t(y) * stridedst) + x;

	ippiThreshold_GTVal_8u_C1R(
		src_u8,
		stridesrc,
		dst_u8,
		stridedst,
		{ width, height },
		threshold,
		value);
}

GENIXAPI(void, threshold_ltgt_8bpp)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, int stridesrc,
	unsigned __int64* dst, int stridedst,
	const unsigned __int8 thresholdLT,
	const unsigned __int8 valueLT,
	const unsigned __int8 thresholdGT,
	const unsigned __int8 valueGT)
{
	stridesrc *= sizeof(unsigned __int64);	// 8 bytes per word
	const Ipp8u* src_u8 = ((const Ipp8u*)src) + (ptrdiff_t(y) * stridesrc) + x;

	stridedst *= sizeof(unsigned __int64);	// 8 bytes per word
	Ipp8u* dst_u8 = ((Ipp8u*)dst) + (ptrdiff_t(y) * stridedst) + x;

	ippiThreshold_LTValGTVal_8u_C1R(
		src_u8,
		stridesrc,
		dst_u8,
		stridedst,
		{ width, height },
		thresholdLT,
		valueLT,
		thresholdGT,
		valueGT);
}
