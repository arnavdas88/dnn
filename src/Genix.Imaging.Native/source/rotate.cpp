#include "stdafx.h"
#include "ipp.h"

/* Next two defines are created to simplify code reading and understanding */
#define EXIT_MAIN exitLine:                                  /* Label for Exit */
#define check_sts(st) if((st) != ippStsNoErr) goto exitLine; /* Go to Exit if Intel(R) IPP function returned status different from ippStsNoErr */

/* Results of ippMalloc() are not validated because Intel(R) IPP functions perform bad arguments check and will return an appropriate status  */

// rotates the image 90 degrees counter clockwise
GENIXAPI(int, rotate90)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int srcstep,
	unsigned __int8* dst, const int dststep)
{
	// move to beginning of last destination row
	const int dstsize = width * dststep;
	if (bitsPerPixel == 1)
	{
		::memset(dst, 0, dstsize);
	}

	dst += ptrdiff_t(dstsize) - dststep;

	switch (bitsPerPixel)
	{
	case 1:
		for (int iy = 0, width8 = width & ~7; iy < height; iy++, src += srcstep)
		{
			const unsigned __int8* tsrc = src;
			unsigned __int8* tdst = dst;
			const int dstshift = iy & 7;
			int ix = 0;

			for (; ix < width8; ix += 8)
			{
				unsigned __int8 b = *tsrc++;

				tdst[0] |= ((b >> 0) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 1) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 2) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 3) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 4) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 5) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 6) & 1) << dstshift; tdst -= dststep;
				tdst[0] |= ((b >> 7) & 1) << dstshift; tdst -= dststep;
			}

			if (ix < width)
			{
				unsigned __int8 b = *tsrc;
				for (; ix < width; ix++)
				{
					tdst[0] |= ((b >> (ix & 7)) & 1) << dstshift; tdst -= dststep;
				}
			}

			// move to next column in destination
			if (dstshift == 7)
			{
				dst++;
			}
		}
		break;

	case 8:
		for (int iy = 0; iy < height; iy++, src += srcstep, dst++)
		{
			unsigned __int8* tdst = dst;
			for (int ix = 0; ix < width; ix++, tdst -= dststep)
			{
				tdst[0] = src[ix];
			}
		}
		break;

	case 24:
		for (int iy = 0; iy < height; iy++, src += srcstep, dst += 3)
		{
			unsigned __int8* tdst = dst;
			for (int ix = 0, iix = 0; ix < width; ix++, iix += 3, tdst -= dststep)
			{
				tdst[0] = src[iix + 0];
				tdst[1] = src[iix + 1];
				tdst[2] = src[iix + 2];
			}
		}
		break;

	case 32:
		for (int iy = 0; iy < height; iy++, src += srcstep, dst += 4)
		{
			unsigned __int8* tdst = dst;
			for (int ix = 0; ix < width; ix++, tdst -= dststep)
			{
				*(unsigned __int32*)tdst = ((const unsigned __int32*)src)[ix];
			}
		}
		break;
	}

	return ippStsNoOperation;
}

// rotates the image 90 degrees clockwise
GENIXAPI(int, rotate270)(
	const int bitsPerPixel,
	const int width, const int height,
	const unsigned __int8* src, const int srcstep,
	unsigned __int8* dst, const int dststep)
{
	// move to beginning of last source row
	src += (ptrdiff_t(height) - 1) * srcstep;

	switch (bitsPerPixel)
	{
	case 1:
		::memset(dst, 0, ptrdiff_t(width) * dststep);
		for (int iy = height - 1, width8 = width & ~7; iy >= 0; iy--, src -= srcstep)
		{
			const unsigned __int8* tsrc = src;
			unsigned __int8* tdst = dst;
			const int dstshift = (height - 1 - iy) & 7;
			int ix = 0;

			for (; ix < width8; ix += 8)
			{
				unsigned __int8 b = *tsrc++;

				tdst[0] |= ((b >> 0) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 1) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 2) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 3) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 4) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 5) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 6) & 1) << dstshift; tdst += dststep;
				tdst[0] |= ((b >> 7) & 1) << dstshift; tdst += dststep;
			}

			if (ix < width)
			{
				unsigned __int8 b = *tsrc;
				for (; ix < width; ix++)
				{
					tdst[0] |= ((b >> (ix & 7)) & 1) << dstshift; tdst += dststep;
				}
			}

			// move to next column in destination
			if (dstshift == 7)
			{
				dst++;
			}
		}
		break;

	case 8:
		for (int iy = height - 1; iy >= 0; iy--, src -= srcstep, dst++)
		{
			unsigned __int8* tdst = dst;
			for (int ix = 0; ix < width; ix++, tdst += dststep)
			{
				tdst[0] = src[ix];
			}
		}
		break;

	case 24:
		for (int iy = height - 1; iy >= 0; iy--, src -= srcstep, dst += 3)
		{
			unsigned __int8* tdst = dst;
			for (int ix = 0, iix = 0; ix < width; ix++, iix += 3, tdst += dststep)
			{
				tdst[0] = src[iix + 0];
				tdst[1] = src[iix + 1];
				tdst[2] = src[iix + 2];
			}
		}
		break;

	case 32:
		for (int iy = height - 1; iy >= 0; iy--, src -= srcstep, dst += 4)
		{
			unsigned __int8* tdst = dst;
			for (int ix = 0; ix < width; ix++, tdst += dststep)
			{
				*(unsigned __int32*)tdst = ((const unsigned __int32*)src)[ix];
			}
		}
		break;
	}

	return ippStsNoOperation;
}
