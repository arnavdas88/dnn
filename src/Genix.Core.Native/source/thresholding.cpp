#include "stdafx.h"
#include <cmath>

// Values that are less than the threshold, are set to a specified value.
template<typename T> void __forceinline __threshold_lt_ip(
	const int n,
	const T threshold,
	const T value,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = y[i] < threshold ? value : y[i];
	}
}

GENIXAPI(void, threshold_lt_ip_s8)(int n, __int8 threshold, __int8 value, __int8* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_s16)(int n, __int16 threshold, __int16 value, __int16* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_s32)(int n, __int32 threshold, __int32 value, __int32* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_s64)(int n, __int64 threshold, __int64 value, __int64* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_u8)(int n, unsigned __int8 threshold, unsigned  __int8 value, unsigned __int8* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_u16)(int n, unsigned __int16 threshold, unsigned __int16 value, unsigned __int16* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_u32)(int n, unsigned __int32 threshold, unsigned  __int32 value, unsigned __int32* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_u64)(int n, unsigned __int64 threshold, unsigned __int64 value, unsigned __int64* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_f32)(int n, float threshold, float value, float* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_lt_ip_f64)(int n, double threshold, double value, double* y, int offy)
{
	__threshold_lt_ip(n, threshold, value, y, offy);
}

// Values that are greater than the threshold, are set to a specified value.
template<typename T> void __forceinline __threshold_gt_ip(
	const int n,
	const T threshold,
	const T value,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = y[i] > threshold ? value : y[i];
	}
}

GENIXAPI(void, threshold_gt_ip_s8)(int n, __int8 threshold, __int8 value, __int8* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_s16)(int n, __int16 threshold, __int16 value, __int16* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_s32)(int n, __int32 threshold, __int32 value, __int32* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_s64)(int n, __int64 threshold, __int64 value, __int64* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_u8)(int n, unsigned __int8 threshold, unsigned  __int8 value, unsigned __int8* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_u16)(int n, unsigned __int16 threshold, unsigned __int16 value, unsigned __int16* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_u32)(int n, unsigned __int32 threshold, unsigned  __int32 value, unsigned __int32* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_u64)(int n, unsigned __int64 threshold, unsigned __int64 value, unsigned __int64* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_f32)(int n, float threshold, float value, float* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}
GENIXAPI(void, threshold_gt_ip_f64)(int n, double threshold, double value, double* y, int offy)
{
	__threshold_gt_ip(n, threshold, value, y, offy);
}

// Values that are smaller or greater than the thresholds, are set to a specified value.
template<typename T> void __forceinline __threshold_ltgt_ip(
	const int n,
	const T thresholdLT,
	const T valueLT,
	const T thresholdGT,
	const T valueGT,
	T* y, const int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = y[i] < thresholdLT ? valueLT : y[i];
		y[i] = y[i] > thresholdGT ? valueGT : y[i];
	}
}

GENIXAPI(void, threshold_ltgt_ip_s8)(int n, __int8 thresholdLT, __int8 valueLT, __int8 thresholdGT, __int8 valueGT, __int8* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_s16)(int n, __int16 thresholdLT, __int16 valueLT, __int16 thresholdGT, __int16 valueGT, __int16* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_s32)(int n, __int32 thresholdLT, __int32 valueLT, __int32 thresholdGT, __int32 valueGT, __int32* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_s64)(int n, __int64 thresholdLT, __int64 valueLT, __int64 thresholdGT, __int64 valueGT, __int64* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_u8)(int n, unsigned __int8 thresholdLT, unsigned __int8 valueLT, unsigned __int8 thresholdGT, unsigned  __int8 valueGT, unsigned __int8* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_u16)(int n, unsigned __int16 thresholdLT, unsigned __int16 valueLT, unsigned __int16 thresholdGT, unsigned __int16 valueGT, unsigned __int16* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_u32)(int n, unsigned __int32 thresholdLT, unsigned __int32 valueLT, unsigned __int32 thresholdGT, unsigned  __int32 valueGT, unsigned __int32* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_u64)(int n, unsigned __int64 thresholdLT, unsigned __int64 valueLT, unsigned __int64 thresholdGT, unsigned __int64 valueGT, unsigned __int64* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_f32)(int n, float thresholdLT, float valueLT, float thresholdGT, float valueGT, float* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
GENIXAPI(void, threshold_ltgt_ip_f64)(int n, double thresholdLT, double valueLT, double thresholdGT, double valueGT, double* y, int offy)
{
	__threshold_ltgt_ip(n, thresholdLT, valueLT, thresholdGT, valueGT, y, offy);
}
