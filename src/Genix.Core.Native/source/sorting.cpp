#include "stdafx.h"

template<typename T> void __forceinline swap(T* data, int i, int j)
{
	if (i != j)
	{
		const T temp = data[i];
		data[i] = data[j];
		data[j] = temp;
	}
}

template<typename T> static void qsort_asc(T* keys, const int lo, const int hi)
{
	const T pivot = keys[(lo + hi) >> 1]; // find pivot item
	int i = lo;
	int j = hi;

	while (i <= j)
	{
		while (keys[i] < pivot)	i++;
		while (keys[j] > pivot) j--;

		if (i <= j)
		{
			swap(keys, i, j);
			i++;
			j--;
		}
	}

	if (lo < j)
	{
		qsort_asc(keys, lo, j);
	}

	if (i < hi)
	{
		qsort_asc(keys, i, hi);
	}
}

template<typename T> static void qsort_asc(T* keys, int* values, const int lo, const int hi)
{
	const T pivot = keys[(lo + hi) >> 1]; // find pivot item
	int i = lo;
	int j = hi;

	while (i <= j)
	{
		while (keys[i] < pivot)	i++;
		while (keys[j] > pivot) j--;

		if (i <= j)
		{
			swap(keys, i, j);
			swap(values, i, j);
			i++;
			j--;
		}
	}

	if (lo < j)
	{
		qsort_asc(keys, lo, j);
	}

	if (i < hi)
	{
		qsort_asc(keys, i, hi);
	}
}

template<typename T> static void qsort_desc(T* keys, const int lo, const int hi)
{
	const T pivot = keys[(lo + hi) >> 1]; // find pivot item
	int i = lo;
	int j = hi;

	while (i <= j)
	{
		while (keys[i] > pivot)	i++;
		while (keys[j] < pivot) j--;

		if (i <= j)
		{
			swap(keys, i, j);
			i++;
			j--;
		}
	}

	if (lo < j)
	{
		qsort_desc(keys, lo, j);
	}

	if (i < hi)
	{
		qsort_desc(keys, i, hi);
	}
}

template<typename T> static void qsort_desc(T* keys, int* values, const int lo, const int hi)
{
	const T pivot = keys[(lo + hi) >> 1]; // find pivot item
	int i = lo;
	int j = hi;

	while (i <= j)
	{
		while (keys[i] > pivot)	i++;
		while (keys[j] < pivot) j--;

		if (i <= j)
		{
			swap(keys, i, j);
			swap(values, i, j);
			i++;
			j--;
		}
	}

	if (lo < j)
	{
		qsort_desc(keys, values, lo, j);
	}

	if (i < hi)
	{
		qsort_desc(keys, values, i, hi);
	}
}

template<typename T> void __forceinline qsort(const int n, T* keys, BOOL ascending)
{
	if (ascending)
	{
		::qsort_asc(keys, 0, n - 1);
	}
	else
	{
		::qsort_desc(keys, 0, n - 1);
	}
}

template<typename T> void __forceinline qsort(const int n, T* keys, int* values, BOOL ascending)
{
	if (ascending)
	{
		::qsort_asc(keys, values, 0, n - 1);
	}
	else
	{
		::qsort_desc(keys, values, 0, n - 1);
	}
}

GENIXAPI(void, qsort_s8)(const int n, __int8*  x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_s16)(const int n, __int16* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_s32)(const int n, __int32* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_s64)(const int n, __int64* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_u8)(const int n, unsigned __int8* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_u16)(const int n, unsigned __int16* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_u32)(const int n, unsigned __int32* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_u64)(const int n, unsigned __int64* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_f32)(const int n, float* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }
GENIXAPI(void, qsort_f64)(const int n, double* x, const int offx, BOOL ascending) { qsort(n, x + offx, ascending); }

GENIXAPI(void, qsortv_s8)(const int n, __int8*  x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_s16)(const int n, __int16* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_s32)(const int n, __int32* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_s64)(const int n, __int64* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_u8)(const int n, unsigned __int8*  x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_u16)(const int n, unsigned __int16* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_u32)(const int n, unsigned __int32* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_u64)(const int n, unsigned __int64* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_f32)(const int n, float* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
GENIXAPI(void, qsortv_f64)(const int n, double* x, const int offx, int* y, const int offy, BOOL ascending) { qsort(n, x + offx, y + offy, ascending); }
