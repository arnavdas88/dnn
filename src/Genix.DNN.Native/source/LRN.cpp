#include "stdafx.h"

#include <stdlib.h> 

GENIXAPI(void, LRNKernel)(
	const float* src, float* dst,
	const int kernelSize,
	const int b,
	const int* axes,
	const int* strides)
{
	const int W = axes[1];
	const int H = axes[2];
	const int C = axes[3];
	const int BStride = strides[0];
	const int WStride = strides[1];
	const int HStride = strides[2];
	const int CStride = strides[3];

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
