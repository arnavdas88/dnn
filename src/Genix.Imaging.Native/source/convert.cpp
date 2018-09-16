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

		// convert 8 words at a time
		int x = 0;
		for (; x < width64; x += 64)
		{
			const unsigned __int64 b = src[offxsrc++];
			dst[offxdst++] = map[(b >> 0) & 0xff];
			dst[offxdst++] = map[(b >> 8) & 0xff];
			dst[offxdst++] = map[(b >> 16) & 0xff];
			dst[offxdst++] = map[(b >> 24) & 0xff];
			dst[offxdst++] = map[(b >> 32) & 0xff];
			dst[offxdst++] = map[(b >> 40) & 0xff];
			dst[offxdst++] = map[(b >> 48) & 0xff];
			dst[offxdst++] = map[(b >> 56) & 0xff];
		}

		// convert remaining bits
		if (x < width)
		{
			const unsigned __int64 b = src[offxsrc];
			for (; x < width; x += 8)
			{
				dst[offxdst++] = map[(b >> (x & 63)) & 0xff];
			}
		}
	}

	::free(map);

	return 0;
}

GENIXAPI(int, _convert1to16)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	const unsigned __int16 value0,
	const unsigned __int16 value1)
{
	unsigned __int64 map[16];
	const unsigned __int64 values[2] = { value0, value1 };
	for (int i = 0; i < 16; i++)
	{
		map[i] =
			(values[(i >> 3) & 1] << (3 * 16)) |
			(values[(i >> 2) & 1] << (2 * 16)) |
			(values[(i >> 1) & 1] << (1 * 16)) |
			(values[(i >> 0) & 1] << (0 * 16));
	}

	const int width64 = width & ~63;
	for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 64 bits at a time (16 pixels)
		int x = 0;
		for (; x < width64; x += 64)
		{
			unsigned __int64 b = src[offxsrc++];
			for (int i = 0; i < 64; i += 4, b >>= 4)
			{
				dst[offxdst++] = map[b & 15];
			}
		}

		// convert remaining bits
		if (x < width)
		{
			unsigned __int64 b = src[offxsrc];
			for (; x < width; x += 4, b >>= 4)
			{
				dst[offxdst++] = map[b & 15];
			}
		}
	}

	return 0;
}

GENIXAPI(int, _convert1to32f)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	float* dst, const int stridedst,
	const float value0,
	const float value1)
{
	const int width64 = width & ~63;
	for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 64 bits at a time
		int x = 0;
		for (; x < width64; x += 64)
		{
			unsigned __int64 b = src[offxsrc++];
			for (int i = 0; i < 64; i++, b >>= 1)
			{
				dst[offxdst++] = (b & 1) ? value1 : value0;
			}
		}

		// convert remaining bits
		if (x < width)
		{
			unsigned __int64 b = src[offxsrc];
			for (; x < width; x++, b >>= 1)
			{
				dst[offxdst++] = (b & 1) ? value1 : value0;
			}
		}
	}

	return 0;
}

GENIXAPI(int, _convert2to8)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	const unsigned __int8 value0,
	const unsigned __int8 value1,
	const unsigned __int8 value2,
	const unsigned __int8 value3)
{
	unsigned __int32* map = (unsigned __int32*)::malloc(256 * sizeof(unsigned __int32));
	if (map == NULL)
	{
		// insufficient memory available
		return 1;
	}

	const unsigned __int32 values[4] = { value0, value1, value2, value3 };
	for (int i = 0; i < 256; i++)
	{
		map[i] =
			(values[(i >> 6) & 3] << (3 * 8)) |
			(values[(i >> 4) & 3] << (2 * 8)) |
			(values[(i >> 2) & 3] << (1 * 8)) |
			(values[(i >> 0) & 3] << (0 * 8));
	}

	const unsigned __int32* src_u32 = (const unsigned __int32*)src;
	const int stridesrc_u32 = stridesrc * 2;
	const int width32 = width & ~31;

	for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc_u32, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 4 words at a time
		int x = 0;
		for (; x < width32; x += 32)
		{
			const unsigned __int32 b = src_u32[offxsrc++];
			dst[offxdst++] = map[(b >> 0) & 0xff];
			dst[offxdst++] = map[(b >> 8) & 0xff];
			dst[offxdst++] = map[(b >> 16) & 0xff];
			dst[offxdst++] = map[(b >> 24) & 0xff];
		}

		// convert remaining bits
		if (x < width)
		{
			const unsigned __int32 b = src_u32[offxsrc];
			for (; x < width; x += 4)
			{
				dst[offxdst++] = map[(b >> (x & 31)) & 0xff];
			}
		}
	}

	::free(map);

	return 0;
}

GENIXAPI(int, _convert4to8)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst)
{
	unsigned __int16* map = (unsigned __int16*)::malloc(256 * sizeof(unsigned __int16));
	if (map == NULL)
	{
		// insufficient memory available
		return 1;
	}

	for (int i = 0; i < 256; i++)
	{
		map[i] =
			(((i & 0xf0) >> 4) | (i & 0xf0)) << 8 |
			(((i & 0x0f) << 4) | (i & 0x0f)) ;
	}

	const unsigned __int16* src_u16 = (const unsigned __int16*)src;
	const int stridesrc_u16 = stridesrc * 4;
	const int width16 = width & ~15;

	for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc_u16, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 2 words at a time
		int x = 0;
		for (; x < width16; x += 16)
		{
			const unsigned __int16 b = src_u16[offxsrc++];
			dst[offxdst++] = map[(b >> 0) & 0xff];
			dst[offxdst++] = map[(b >> 8) & 0xff];
		}

		// convert remaining bits
		if (x < width)
		{
			const unsigned __int16 b = src_u16[offxsrc];
			for (; x < width; x += 2)
			{
				dst[offxdst++] = map[(b >> (x & 31)) & 0xff];
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
	const int x, const int y,
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

	src += (ptrdiff_t(y) * stridesrc) + (x / 8);
	dst += (ptrdiff_t(y) * stridedst) + (x / 64);

	const int width64 = width & ~63;
	for (int iy = 0, offysrc = 0, offydst = 0; iy < height; iy++, offysrc += stridesrc, offydst += stridedst)
	{
		int offxsrc = offysrc;
		int offxdst = offydst;

		// convert 64 bits at a time
		int ix = 0;
		for (; ix < width64; ix += 64, offxdst++, offxsrc += 8)
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
		if (ix < width)
		{
			dst[offxdst] = 0;
			for (; ix < width; ix += 8, offxsrc++)
			{
				dst[offxdst] |= bits8to1(src[offxsrc], threshold) << (ix & 63);
			}
		}
	}

	return 0;
}

GENIXAPI(int, _convert8to24)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst)
{
	const Ipp8u* src_u8 = (const Ipp8u*)(src + (ptrdiff_t(y) * stridesrc)) + x;
	Ipp8u* dst_u8 = (Ipp8u*)(dst + (ptrdiff_t(y) * stridedst)) + (ptrdiff_t(x) * 3);

	return ippiGrayToRGB_8u_C1C3R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_u8,
		stridedst * sizeof(unsigned __int64),
		{ width, height });
}

GENIXAPI(int, _convert8to32)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	const unsigned __int8 alpha)
{
	const Ipp8u* src_u8 = (const Ipp8u*)(src + (ptrdiff_t(y) * stridesrc)) + x;
	Ipp8u* dst_u8 = (Ipp8u*)(dst + (ptrdiff_t(y) * stridedst)) + (ptrdiff_t(x) * 4);

	return ippiGrayToRGB_8u_C1C4R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_u8,
		stridedst * sizeof(unsigned __int64),
		{ width, height },
		alpha);
}

GENIXAPI(int, _convert8to32f)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	float* dst, const int stridedst)
{
	const Ipp8u* src_u8 = (const Ipp8u*)(src + (ptrdiff_t(y) * stridesrc)) + x;
	Ipp32f* dst_f32 = (Ipp32f*)(dst + (ptrdiff_t(y) * stridedst)) + x;

	return ippiConvert_8u32f_C1R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_f32,
		stridedst * sizeof(float),
		{ width, height });
}

GENIXAPI(int, _convert24to8)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst)
{
	const Ipp8u* src_u8 = (const Ipp8u*)(src + (ptrdiff_t(y) * stridesrc)) + (ptrdiff_t(x) * 3);
	Ipp8u* dst_u8 = (Ipp8u*)(dst + (ptrdiff_t(y) * stridedst)) + x;

	return ippiRGBToGray_8u_C3C1R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_u8,
		stridedst * sizeof(unsigned __int64),
		{ width, height });
}

GENIXAPI(int, _convert24to32)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst)
{
	const unsigned __int8* src_u8 = (const unsigned __int8*)(src + (ptrdiff_t(y) * stridesrc)) + (ptrdiff_t(x) * 3);
	unsigned __int8* dst_u8 = (unsigned __int8*)(dst + (ptrdiff_t(y) * stridedst)) + (ptrdiff_t(x) * 4);

	return ippiCopy_8u_C3AC4R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_u8,
		stridedst * sizeof(unsigned __int64),
		{ width, height });
}

GENIXAPI(int, _convert32to8)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst)
{
	const unsigned __int8* src_u8 = (const unsigned __int8*)(src + (ptrdiff_t(y) * stridesrc)) + (ptrdiff_t(x) * 4);
	unsigned __int8* dst_u8 = (unsigned __int8*)(dst + (ptrdiff_t(y) * stridedst)) + x;

	return ippiRGBToGray_8u_AC4C1R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_u8,
		stridedst * sizeof(unsigned __int64),
		{ width, height });
}

GENIXAPI(int, _convert32to24)(
	const int x, const int y,
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst)
{
	const unsigned __int8* src_u8 = (const unsigned __int8*)(src + (ptrdiff_t(y) * stridesrc)) + (ptrdiff_t(x) * 4);
	unsigned __int8* dst_u8 = (unsigned __int8*)(dst + (ptrdiff_t(y) * stridedst)) + (ptrdiff_t(x) * 3);

	return ippiCopy_8u_AC4C3R(
		src_u8,
		stridesrc * sizeof(unsigned __int64),
		dst_u8,
		stridedst * sizeof(unsigned __int64),
		{ width, height });
}

GENIXAPI(int, otsu)(
	const int width, const int height,
	const unsigned __int64* src, const int stridesrc,
	unsigned __int64* dst, const int stridedst,
	int sx, int sy,
	int smoothx, int smoothy)
{
	IppStatus status = ippStsNoErr;
	Ipp8u* pThresholds = NULL;
	Ipp8u* pFilterBoxBorderBuffer = NULL;

	// Calculate tile size
	sx = __max(((sx + 7) / 8) * 8, 16);	// horizontal tile size must be rounded to 8 pixels
	sy = __max(sy, 16);
	int nx = __max(1, width / sx);
	int ny = __max(1, height / sy);

	if (nx == 1 && ny == 1)
	{
		Ipp8u threshold = 0;

		// Compute the threshold
		check_sts(status = ippiComputeThreshold_Otsu_8u_C1R(
			(const Ipp8u*)src,
			stridesrc * sizeof(unsigned __int64),
			{ width, height },
			&threshold));

		// Apply the threshold
		check_sts(status = _convert8to1(
			0,
			0,
			width,
			height,
			src,
			stridesrc,
			dst,
			stridedst,
			threshold));
	}
	else
	{
		// Allocate thresholds
		pThresholds = (Ipp8u*)ippsMalloc_8u(nx * ny);
		if (pThresholds == NULL)
		{
			// insufficient memory available
			status = ippStsNoMemErr;
			goto exitLine;
		}

		// Compute the threshold array for the tiles
		for (int iy = 0, ty = 0, ithresh = 0; iy < ny; iy++, ty += sy)
		{
			const int th = iy + 1 == ny ? height - ty : sy;

			for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
			{
				const int tw = ix + 1 == nx ? width - tx : sx;

				check_sts(status = ippiComputeThreshold_Otsu_8u_C1R(
					(const Ipp8u*)(src + (ptrdiff_t(ty) * stridesrc)) + (tx >> 3),
					stridesrc * sizeof(unsigned __int64),
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
				smoothx = __min(smoothx, (nx - 1) / 2);
				smoothy = __min(smoothy, (ny - 1) / 2);
			}

			if (smoothx > 0 || smoothy > 0)
			{
				IppiSize roiSize = { nx, ny };
				IppiSize maskSize = { (2 * smoothx) + 1, (2 * smoothy) + 1 };
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
		for (int iy = 0, ty = 0, ithresh = 0; iy < ny; iy++, ty += sy)
		{
			const int th = iy + 1 == ny ? height - ty : sy;

			for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
			{
				const int tw = ix + 1 == nx ? width - tx : sx;

				check_sts(status = _convert8to1(
					tx,
					ty,
					tw,
					th,
					src,
					stridesrc,
					dst,
					stridedst,
					pThresholds[ithresh++]));
			}
		}
	}

	EXIT_MAIN
		ippsFree(pFilterBoxBorderBuffer);
	ippsFree(pThresholds);
	return (int)status;
}

GENIXAPI(int, cart2polar)(
	const int n,
	const float* re,
	const float* im,
	float* magnitude,
	float* phase)
{
	return ippsCartToPolar_32f(re, im, magnitude, phase, n);
}
