#include "stdafx.h"
#include <math.h>
#include <intrin.h>
#include <immintrin.h>
#include "mkl.h"
#include "simddetect.h"

// compare two arrays element-wise
template<typename T> int __forceinline __compare(
	int n,
	const T* x, int offx,
	const T* y, int offy)
{
	return ::memcmp(x + offx, y + offy, n * sizeof(T));
}

GENIXAPI(int, compare_s8)(int n, const __int8* x, int offx, const __int8* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_s16)(int n, const __int16* x, int offx, const __int16* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_s32)(int n, const __int32* x, int offx, const __int32* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_s64)(int n, const __int64* x, int offx, const __int64* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_u8)(int n, const unsigned __int8* x, int offx, const unsigned __int8* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_u16)(int n, const unsigned __int16* x, int offx, const unsigned __int16* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_u32)(int n, const unsigned __int32* x, int offx, const unsigned __int32* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_u64)(int n, const unsigned __int64* x, int offx, const unsigned __int64* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_f32)(int n, const float* x, int offx, const float* y, int offy) { return __compare(n, x, offx, y, offy); }
GENIXAPI(int, compare_f64)(int n, const double* x, int offx, const double* y, int offy) { return __compare(n, x, offx, y, offy); }

// copy arrays
template<typename T> void __forceinline __copy(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	::memcpy(y + offy, x + offx, n * sizeof(T));
}

GENIXAPI(void, copy_s8)(int n, const __int8* x, int offx, __int8* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_s16)(int n, const __int16* x, int offx, __int16* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_u16)(int n, const unsigned __int16* x, int offx, unsigned  __int16* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_f32)(int n, const float* x, int offx, float* y, int offy) { __copy(n, x, offx, y, offy); }
GENIXAPI(void, copy_f64)(int n, const double* x, int offx, double* y, int offy) { __copy(n, x, offx, y, offy); }

extern "C" __declspec(dllexport) void WINAPI scopy_inc(
	int n,
	const float* x, int offx, int incx,
	float* y, int offy, int incy)
{
	x += offx;
	y += offy;

	if (n <= 64)
	{
		if (incx == 1 && incy == 1)
		{
			::memcpy(y, x, n * sizeof(float));
		}
		else
		{
			for (int i = 0, xi = 0, yi = 0; i < n; i++, xi += incx, yi += incy)
			{
				y[yi] = x[xi];
			}
		}
	}
	else
	{
		::cblas_scopy(n, x, incx, y, incy);
	}
}

GENIXAPI(void, copy_strides_s8)(int nstrides, const __int8* x, int stridex, __int8* y, int stridey)
{
	if (stridex >= 0 && stridey >= 0 && stridex == stridey)
	{
		::memcpy(y, x, (size_t)stridey * nstrides);
	}
	else
	{
		for (int i = 0, count = __min(abs(stridex), abs(stridey)); i < nstrides; i++, x += stridex, y += stridey)
		{
			::memcpy(y, x, count);
		}
	}
}

// move arrays
template<typename T> void __forceinline __move(
	int n,
	const T* x, int offx,
	T* y, int offy)
{
	::memmove(y + offy, x + offx, n * sizeof(T));
}

GENIXAPI(void, move_s8)(int n, const __int8* x, int offx, __int8* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_s16)(int n, const __int16* x, int offx, __int16* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_s32)(int n, const __int32* x, int offx, __int32* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_s64)(int n, const __int64* x, int offx, __int64* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_u8)(int n, const unsigned __int8* x, int offx, unsigned __int8* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_u16)(int n, const unsigned __int16* x, int offx, unsigned __int16* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_u32)(int n, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_u64)(int n, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_f32)(int n, const float* x, int offx, float* y, int offy) { __move(n, x, offx, y, offy); }
GENIXAPI(void, move_f64)(int n, const double* x, int offx, double* y, int offy) { __move(n, x, offx, y, offy); }

// set all elements of an array to constant value
template<typename T> void __forceinline __set(
	int n,
	T a,
	T* y, int offy)
{
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = a;
	}
}

GENIXAPI(void, set_s8)(int n, __int8 a, __int8* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_s16)(int n, __int16 a, __int16* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_s32)(int n, __int32 a, __int32* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_s64)(int n, __int64 a, __int64* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_u8)(int n, unsigned __int8 a, unsigned __int8* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_u16)(int n, unsigned __int16 a, unsigned __int16* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_u32)(int n, unsigned __int32 a, unsigned __int32* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_u64)(int n, unsigned __int64 a, unsigned __int64* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_f32)(int n, float a, float* y, int offy) { __set(n, a, y, offy); }
GENIXAPI(void, set_f64)(int n, double a, double* y, int offy) { __set(n, a, y, offy); }

extern "C" __declspec(dllexport) void WINAPI sset_inc(
	int n,
	float a,
	float* y, int offy, int incy)
{
	y += offy;

	if (incy == 1)
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = a;
		}
	}
	else
	{
		for (int i = 0; i < n; i++, y += incy)
		{
			*y = a;
		}
	}
}

extern "C" __declspec(dllexport) void WINAPI sreplace(
	int n,
	const float* x, int offx,
	float oldValue,
	float newValue,
	float* y, int offy)
{
	x += offx;
	y += offy;

	if (_isnanf(oldValue))
	{
		for (int i = 0; i < n; i++)
		{
			y[i] = _isnanf(x[i]) ? newValue : x[i];
		}
	}
	else
	{
		// two different versions for debug and release
		// in debug mode, we need to explicitly compare with nan
		// in release mode, it is done by vectorization functions
#ifdef _DEBUG
		for (int i = 0; i < n; i++)
		{
			y[i] = x[i] == oldValue && !_isnanf(x[i]) ? newValue : x[i];
		}
#else
		for (int i = 0; i < n; i++)
		{
			y[i] = x[i] == oldValue ? newValue : x[i];
		}
#endif
}
}

extern "C" __declspec(dllexport) void WINAPI pack(
	int n,
	const float* a, int offa, int inca,
	float* y, int offy)
{
	::vsPackI(n, a + offa, inca, y + offy);
}

extern "C" __declspec(dllexport) void WINAPI unpack(
	int n,
	const float* a, int offa,
	float* y, int offy, int incy)
{
	::vsUnpackI(n, a + offa, y + offy, incy);
}

// swaps elements of two arrays
template<typename T> void __forceinline __swap(
	const int n,
	T* x, const int offx,
	T* y, const int offy)
{
	x += offx;
	y += offy;

	int i = 0;

	if (SIMDDetect::IsAVX2Available())
	{
		const int TStep = 256 / 8 / sizeof(T);
		for (int ii = (n / TStep) * TStep; i < ii; i += TStep)
		{
			__m256i xtemp = _mm256_loadu_si256(reinterpret_cast<__m256i*>(&x[i]));
			__m256i ytemp = _mm256_loadu_si256(reinterpret_cast<__m256i*>(&y[i]));
			_mm256_storeu_si256(reinterpret_cast<__m256i*>(&y[i]), xtemp);
			_mm256_storeu_si256(reinterpret_cast<__m256i*>(&x[i]), ytemp);
		}
	}

	for (; i < n; i++)
	{
		T temp = x[i];
		x[i] = y[i];
		y[i] = temp;
	}
}

GENIXAPI(void, swap_s8)(const int n, __int8* x, const int offx, __int8* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_s16)(const int n, __int16* x, const int offx, __int16* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_s32)(const int n, __int32* x, const int offx, __int32* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_s64)(const int n, __int64* x, const int offx, __int64* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_u8)(const int n, unsigned __int8* x, const int offx, unsigned __int8* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_u16)(const int n, unsigned __int16* x, const int offx, unsigned __int16* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_u32)(const int n, unsigned __int32* x, const int offx, unsigned __int32* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_u64)(const int n, unsigned __int64* x, const int offx, unsigned __int64* y, const int offy) { __swap(n, x, offx, y, offy); }
GENIXAPI(void, swap_f32)(const int n, float* x, const int offx, float* y, const int offy) {

	////__swap(n, x, offx, y, offy);
	const int incxy = 1;
	::sswap(&n, x, &incxy, y, &incxy);
}
GENIXAPI(void, swap_f64)(const int n, double* x, const int offx, double* y, const int offy) {

	////__swap(n, x, offx, y, offy);
	const int incxy = 1;
	::dswap(&n, x, &incxy, y, &incxy);
}

// logical operations
template<typename T> T __forceinline logical_and(T a, T b)
{
	return a & b;
}
template<typename T> T __forceinline logical_and(T a, T b, T c)
{
	return a & b & c;
}
template<typename T> T __forceinline logical_and(T a, T b, T c, T d)
{
	return a & b & c & d;
}

template<typename T> T __forceinline logical_xand(T a, T b)
{
	return b & ~a;
}

template<typename T> T __forceinline logical_or(T a, T b)
{
	return a | b;
}
template<typename T> T __forceinline logical_or(T a, T b, T c)
{
	return a | b | c;
}
template<typename T> T __forceinline logical_or(T a, T b, T c, T d)
{
	return a | b | c | d;
}

template<typename T> T __forceinline logical_xor(T a, T b)
{
	return a ^ b;
}
template<typename T> T __forceinline logical_xor(T a, T b, T c)
{
	return a ^ b ^ c;
}
template<typename T> T __forceinline logical_xor(T a, T b, T c, T d)
{
	return a ^ b ^ c ^ d;
}

template<typename T, T OP(T, T)> void __forceinline __logical(
	int length,				// number of elements to process
	T mask,					// the mask
	T* y, 					// the destination array
	int offy 				// the zero-based index of starting element in y
)
{
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = OP(y[i], mask);
	}
}

template<typename T, T OP(T, T)> void __forceinline __logical(
	int length,				// number of elements to process
	const T* x, 			// the source array
	int offx, 				// the zero-based index of starting element in x
	T* y, 					// the destination array
	int offy 				// the zero-based index of starting element in y
)
{
	x += offx;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = OP(x[i], y[i]);
	}
}

template<typename T, T OP(T, T)> void __forceinline __logical(
	int length,				// number of elements to process
	const T* x, 			// the source array
	int offx, 				// the zero-based index of starting element in x
	T mask,					// the mask
	T* y, 					// the destination array
	int offy 				// the zero-based index of starting element in y
)
{
	x += offx;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = OP(x[i], mask);
	}
}

template<typename T, T OP(T, T)> void __forceinline __logical(
	int length,				// number of elements to process
	const T* a, 			// the first source array
	int offa, 				// the zero-based index of starting element in a
	const T* b, 			// the second source array
	int offb, 				// the zero-based index of starting element in b
	T* y, 					// the destination array
	int offy 				// the zero-based index of starting element in y
)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = OP(a[i], b[i]);
	}
}

template<typename T, T OP(T, T, T)> void __forceinline __logical(
	int length,				// number of elements to process
	const T* a, 			// the first source array
	int offa, 				// the zero-based index of starting element in a
	const T* b, 			// the second source array
	int offb, 				// the zero-based index of starting element in b
	const T* c, 			// the third source array
	int offc, 				// the zero-based index of starting element in c
	T* y, 					// the destination array
	int offy 				// the zero-based index of starting element in y
)
{
	a += offa;
	b += offb;
	c += offc;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = OP(a[i], b[i], c[i]);
	}
}

template<typename T, T OP(T, T, T, T)> void __forceinline __logical(
	int length,				// number of elements to process
	const T* a, 			// the first source array
	int offa, 				// the zero-based index of starting element in a
	const T* b, 			// the second source array
	int offb, 				// the zero-based index of starting element in b
	const T* c, 			// the third source array
	int offc, 				// the zero-based index of starting element in c
	const T* d, 			// the fourh source array
	int offd, 				// the zero-based index of starting element in d
	T* y, 					// the destination array
	int offy 				// the zero-based index of starting element in y
)
{
	a += offa;
	b += offb;
	c += offc;
	d += offd;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = OP(a[i], b[i], c[i], d[i]);
	}
}

// Generic logical operation
#define LOGICAL(op) \
GENIXAPI(void, op##c_ip_u32)(int length, const unsigned __int32 mask, unsigned __int32* y, int offy) \
{ \
	__logical<unsigned __int32, logical_##op>(length, mask, y, offy); \
} \
GENIXAPI(void, op##c_ip_u64)(int length, const unsigned __int64 mask, unsigned __int64* y, int offy) \
{ \
	__logical<unsigned __int64, logical_##op>(length, mask, y, offy); \
} \
GENIXAPI(void, op##_ip_u32)(int length, const unsigned __int32* x, int offx, unsigned __int32* y, int offy) \
{ \
	__logical<unsigned __int32, logical_##op>(length, x, offx, y, offy); \
} \
GENIXAPI(void, op##_ip_u64)(int length, const unsigned __int64* x, int offx, unsigned __int64* y, int offy) \
{ \
	__logical<unsigned __int64, logical_##op>(length, x, offx, y, offy); \
} \
GENIXAPI(void, op##c_u32)(int length, const unsigned __int32* x, int offx, const unsigned __int32 mask, unsigned __int32* y, int offy) \
{ \
	__logical<unsigned __int32, logical_##op>(length, x, offx, mask, y, offy); \
} \
GENIXAPI(void, op##c_u64)(int length, const unsigned __int64* x, int offx, const unsigned __int64 mask, unsigned __int64* y, int offy) \
{ \
	__logical<unsigned __int64, logical_##op>(length, x, offx, mask, y, offy); \
} \
GENIXAPI(void, op##_u32)(int length, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, unsigned __int32* y, int offy) \
{ \
	__logical<unsigned __int32, logical_##op>(length, a, offa, b, offb, y, offy); \
} \
GENIXAPI(void, op##_u64)(int length, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, unsigned __int64* y, int offy) \
{ \
	__logical<unsigned __int64, logical_##op>(length, a, offa, b, offb, y, offy); \
}

#define LOGICAL3(op) \
GENIXAPI(void, op##3_u32)(int length, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, const unsigned __int32* c, int offc, unsigned __int32* y, int offy) \
{ \
	__logical<unsigned __int32, logical_##op>(length, a, offa, b, offb, c, offc, y, offy); \
} \
GENIXAPI(void, op##3_u64)(int length, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, const unsigned __int64* c, int offc, unsigned __int64* y, int offy) \
{ \
	__logical<unsigned __int64, logical_##op>(length, a, offa, b, offb, c, offc, y, offy); \
}

#define LOGICAL4(op) \
GENIXAPI(void, op##4_u32)(int length, const unsigned __int32* a, int offa, const unsigned __int32* b, int offb, const unsigned __int32* c, int offc, const unsigned __int32* d, int offd, unsigned __int32* y, int offy) \
{ \
	__logical<unsigned __int32, logical_##op>(length, a, offa, b, offb, c, offc, d, offd, y, offy); \
} \
GENIXAPI(void, op##4_u64)(int length, const unsigned __int64* a, int offa, const unsigned __int64* b, int offb, const unsigned __int64* c, int offc, const unsigned __int64* d, int offd, unsigned __int64* y, int offy) \
{ \
	__logical<unsigned __int64, logical_##op>(length, a, offa, b, offb, c, offc, d, offd, y, offy); \
}


// Logical AND
LOGICAL(and);
LOGICAL3(and);
LOGICAL4(and);

// Logical XAND (A AND NOT B)
LOGICAL(xand);

// Logical OR
LOGICAL(or);
LOGICAL3(or);
LOGICAL4(or);

// Logical XOR
LOGICAL(xor);
