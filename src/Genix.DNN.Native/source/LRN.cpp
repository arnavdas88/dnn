#include "stdafx.h"

#include <stdlib.h> 

GENIXAPI(void, LRNKernel)(
	const float* src, float* dst,
	int kernelSize,
	int b, int W, int H, int C,
	int BStride, int WStride, int HStride, int CStride)
{
	const int kernelSize2 = kernelSize / 2;
	const int kernelCStride = kernelSize2 * CStride;
	const int anchor1 = __min(C, kernelSize2 + 1);

	for (int x = 0, ix = b * BStride; x < W; x++, ix += WStride)
	{
		for (int y = 0, iy = ix; y < H; y++, iy += HStride)
		{
			// calculate first
			float sum = 0.0f;
			for (int c = 0, ic = iy; c < anchor1; c++, ic += CStride)
			{
				sum += src[ic];
			}

			dst[iy] = sum;

			// calculate others
			for (int c = 1, ic = iy + CStride; c < C; c++, ic += CStride)
			{
				if (c + kernelSize2 < C)
				{
					sum += src[ic + kernelCStride];
				}

				dst[ic] = sum;

				if (c >= kernelSize2)
				{
					sum -= src[ic - kernelCStride];
				}
			}
		}
	}
}
