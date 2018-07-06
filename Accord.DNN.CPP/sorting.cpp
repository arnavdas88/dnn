#include "stdafx.h"

void __forceinline swap(float* data, int i, int j)
{
	if (i != j)
	{
		const float temp = data[i];
		data[i] = data[j];
		data[j] = temp;
	}
}

void __forceinline swap(int* data, int i, int j)
{
	if (i != j)
	{
		const int temp = data[i];
		data[i] = data[j];
		data[j] = temp;
	}
}

static void qsortf_asc(float* keys, const int lo, const int hi)
{
	const float pivot = keys[(lo + hi) >> 1]; // find pivot item
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
		qsortf_asc(keys, lo, j);
	}

	if (i < hi)
	{
		qsortf_asc(keys, i, hi);
	}
}

static void qsortf_asc(float* keys, int* values, const int lo, const int hi)
{
	const float pivot = keys[(lo + hi) >> 1]; // find pivot item
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
		qsortf_asc(keys, lo, j);
	}

	if (i < hi)
	{
		qsortf_asc(keys, i, hi);
	}
}

static void qsortf_desc(float* keys, const int lo, const int hi)
{
	const float pivot = keys[(lo + hi) >> 1]; // find pivot item
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
		qsortf_desc(keys, lo, j);
	}

	if (i < hi)
	{
		qsortf_desc(keys, i, hi);
	}
}

static void qsortf_desc(float* keys, int* values, const int lo, const int hi)
{
	const float pivot = keys[(lo + hi) >> 1]; // find pivot item
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
		qsortf_desc(keys, values, lo, j);
	}

	if (i < hi)
	{
		qsortf_desc(keys, values, i, hi);
	}
}

extern "C" __declspec(dllexport) void WINAPI qsortf(
	const int n,
	float* x, const int offx,
	BOOL ascending)
{
	x += offx;

	if (ascending)
	{
		::qsortf_asc(x, 0, n - 1);
	}
	else
	{
		::qsortf_desc(x, 0, n - 1);
	}
}

extern "C" __declspec(dllexport) void WINAPI qsortfv(
	const int n,
	float* x, const int offx,
	int* y, const int offy,
	BOOL ascending)
{
	x += offx;
	y += offy;

	if (ascending)
	{
		::qsortf_asc(x, y, 0, n - 1);
	}
	else
	{
		::qsortf_desc(x, y, 0, n - 1);
	}
}
