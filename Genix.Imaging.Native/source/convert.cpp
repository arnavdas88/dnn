#include "stdafx.h"
#include <stdlib.h>
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

GENIXAPI(int, _convert1to8)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	const unsigned __int8 value0,
	const unsigned __int8 value1)
{
	/*return ippiBinToGray_1u8u_C1R(
		(const Ipp8u*)src,
		stridesrc * sizeof(unsigned __int64),
		0,
		(Ipp8u*)dst,
		stridedst * sizeof(unsigned __int64),
		{ width, height },
		255,
		0);*/

	unsigned __int64* map = (unsigned __int64*)::malloc(256 * sizeof(unsigned __int64));
	if (map == NULL)
	{
		// insufficient memory available
		return 1;
	}

	const unsigned __int64 values[2] = { value0, value1 };
	for (int i = 0; i < 256; i++)
	{
		map[i] =
			(values[(i >> 7) & 1] << (7 * 8)) |
			(values[(i >> 6) & 1] << (6 * 8)) |
			(values[(i >> 5) & 1] << (5 * 8)) |
			(values[(i >> 4) & 1] << (4 * 8)) |
			(values[(i >> 3) & 1] << (3 * 8)) |
			(values[(i >> 2) & 1] << (2 * 8)) |
			(values[(i >> 1) & 1] << (1 * 8)) |
			(values[(i >> 0) & 1] << (0 * 8));
	}

	const int width64 = width & ~63;
	for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 64 bits at a time
		int x = 0;
		for (; x < width64; x += 64, offxdst += 8, offxsrc++)
		{
			const unsigned __int64 b = src[offxsrc];
			dst[offxdst + 0] = map[(b >> 0) & 0xff];
			dst[offxdst + 1] = map[(b >> 8) & 0xff];
			dst[offxdst + 2] = map[(b >> 16) & 0xff];
			dst[offxdst + 3] = map[(b >> 24) & 0xff];
			dst[offxdst + 4] = map[(b >> 32) & 0xff];
			dst[offxdst + 5] = map[(b >> 40) & 0xff];
			dst[offxdst + 6] = map[(b >> 48) & 0xff];
			dst[offxdst + 7] = map[(b >> 56) & 0xff];
		}

		// convert remaining bits
		if (x < width)
		{
			const unsigned __int64 b = src[offxsrc];
			for (; x < width; x += 8, offxdst++)
			{
				dst[offxdst] = map[(b >> (x & 63)) & 0xff];
			}
		}
	}

	::free(map);

	return 0;
}

unsigned __int64 __forceinline bits8to1(const unsigned __int64 bits, int threshold)
{
	unsigned __int8 result = 0;

	result |= ((int((bits >> 56) & 0xff) - threshold) >> 24) & 0x80;
	result |= ((int((bits >> 48) & 0xff) - threshold) >> 25) & 0x40;
	result |= ((int((bits >> 40) & 0xff) - threshold) >> 26) & 0x20;
	result |= ((int((bits >> 32) & 0xff) - threshold) >> 27) & 0x10;

	result |= ((int((bits >> 24) & 0xff) - threshold) >> 28) & 0x08;
	result |= ((int((bits >> 16) & 0xff) - threshold) >> 29) & 0x04;
	result |= ((int((bits >> 8) & 0xff) - threshold) >> 30) & 0x02;
	result |= ((int((bits >> 0) & 0xff) - threshold) >> 31) & 0x01;

	return result;
}

GENIXAPI(int, _convert8to1)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	const unsigned __int8 threshold)
{
	/*return ippiGrayToBin_8u1u_C1R(
		(const Ipp8u*)src,
		stridesrc * sizeof(unsigned __int64),
		(Ipp8u*)dst,
		stridedst * sizeof(unsigned __int64),
		0,
		{ width, height },
		threshold);

	return 0;*/

	const int width64 = width & ~63;
	for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 64 bits at a time
		int x = 0;
		for (; x < width64; x += 64, offxdst++, offxsrc += 8)
		{
			dst[offxdst] =
				(bits8to1(src[offxsrc + 0], threshold) << 0) |
				(bits8to1(src[offxsrc + 1], threshold) << 8) |
				(bits8to1(src[offxsrc + 2], threshold) << 16) |
				(bits8to1(src[offxsrc + 3], threshold) << 24) |
				(bits8to1(src[offxsrc + 4], threshold) << 32) |
				(bits8to1(src[offxsrc + 5], threshold) << 40) |
				(bits8to1(src[offxsrc + 6], threshold) << 48) |
				(bits8to1(src[offxsrc + 7], threshold) << 56);
		}

		// convert remaining bits
		if (x < width)
		{
			dst[offxdst] = 0;
			for (; x < width; x += 8, offxsrc++)
			{
				dst[offxdst] |= bits8to1(src[offxsrc], threshold) << (x & 63);
			}
		}
	}

	return 0;
}

GENIXAPI(int, otsu)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	const int sx, const int sy,
	const int smoothx, const int smoothy)
{
	IppStatus status = ippStsNoErr;
	Ipp8u* pThresholds = NULL;
	Ipp8u* pFilterBoxBorderBuffer = NULL;

	// Calculate tile size
	////sx = __max(sx, 16);
	////sy = __max(sy, 16);
	int nx = __max(1, width / sx);
	int ny = __max(1, height / sy);

	// Allocate thresholds
	pThresholds = (Ipp8u*)ippsMalloc_8u(nx * ny);
	if (pThresholds == NULL)
	{
		// insufficient memory available
		status = ippStsNoMemErr;
		goto exitLine;
	}

	// Compute the threshold array for the tiles
	for (int iy = 0, offy = 0, ithresh = 0; iy < ny; iy++, offy += sy)
	{
		const int th = iy + 1 == ny ? height - offy : sy;

		for (int ix = 0, offx = 0; ix < nx; ix++, offx += sx)
		{
			const int tw = ix + 1 == nx ? width - offx : sx;

			check_sts(status = ippiComputeThreshold_Otsu_8u_C1R(
				(const Ipp8u*)(src + (offy * stridesrc)),
				stridesrc,
				{ tw, th },
				&pThresholds[ithresh++]));
		}
	}

	// Optionally smooth the threshold array
	if (smoothx > 0 || smoothy > 0)
	{
		// kernel too large; reducing!
		if (nx < (2 * smoothx) + 1 || ny < (2 * smoothy) + 1)
		{
			//smoothx = __min(smoothx, (nx - 1) / 2);
			//smoothy = __min(smoothy, (ny - 1) / 2);
		}

		if (smoothx > 0 || smoothy > 0)
		{
			IppiSize roiSize = { nx, ny };
			IppiSize maskSize = { smoothx, smoothy };
			int iBufSize = 0;

			check_sts(status = ippiFilterBoxBorderGetBufferSize(
				roiSize,
				maskSize,
				ipp8u,
				1,
				&iBufSize));

			pFilterBoxBorderBuffer = ippsMalloc_8u(iBufSize);

			check_sts(status = ippiFilterBoxBorder_8u_C1R(
				pThresholds,
				nx,
				pThresholds,
				nx,
				roiSize,
				maskSize,
				ippBorderRepl,
				NULL,
				pFilterBoxBorderBuffer));
		}
	}

	// Apply the threshold
	for (int iy = 0, offy = 0, ithresh = 0; iy < ny; iy++, offy += sy)
	{
		const int th = iy + 1 == ny ? height - offy : sy;

		for (int ix = 0, offx = 0; ix < nx; ix++, offx += sx)
		{
			const int tw = ix + 1 == nx ? width - offx : sx;
		}
	}

	EXIT_MAIN
	ippsFree(pFilterBoxBorderBuffer);
	ippsFree(pThresholds);
	return (int)status;
}

