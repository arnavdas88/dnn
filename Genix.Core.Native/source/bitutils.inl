#define BITS_MASK			(BITS_COUNT - 1)

#if BITS_COUNT == 64
#define BITS_SHIFT			6
#define BITS_MAX			_UI64_MAX
#define BITS_MIN			0ul
#define BITS_METHOD(x)		x##64
typedef unsigned __int64	__bits;
#elif BITS_COUNT == 32
#define BITS_SHIFT			5
#define BITS_MAX			_UI32_MAX
#define BITS_MIN			0ui32
#define BITS_METHOD(x)		x##32
typedef unsigned			__bits;
#endif

__forceinline void _stdcall _set(int n, __bits a, __bits* y)
{
	for (int i = 0; i < n; i++)
	{
		y[i] = a;
	}
}

#if BITS_COUNT == 64
#define _byteswap			_byteswap_uint64
#else
#define _byteswap			_byteswap_ulong
#endif

#if BITS_COUNT == 64
#ifdef _WIN64
#define _shiftleft			__shiftleft128
#else
constexpr __forceinline __bits _shiftleft(__bits LowPart, __bits HighPart, unsigned char Shift)
{
	return (HighPart << Shift) | (LowPart >> (BITS_COUNT - Shift));
}
#endif
#elif BITS_COUNT == 32
constexpr __forceinline __bits _shiftleft(__bits LowPart, __bits HighPart, unsigned char Shift)
{
	return (HighPart << Shift) | (LowPart >> (BITS_COUNT - Shift));
}
#endif

#if BITS_COUNT == 64
#ifdef _WIN64
#define _popcnt				__popcnt64
#else
__forceinline unsigned __int64 _popcnt(unsigned __int64 value)
{
	return gsl::narrow_cast<unsigned __int64>(__popcnt(gsl::narrow_cast<unsigned>(value))) +
		__popcnt(gsl::narrow_cast<unsigned>(value >> 32));
}
#endif
#elif BITS_COUNT == 32
#define _popcnt				__popcnt
#endif

// Searches the source operand for the least significant set bit.
// If a least significant set bit is found, the result is its offset from bit 0.
// If the source operand is 0, the result is undefined.
__forceinline int _bsf(__bits v)
{
	unsigned long index;
#if BITS_COUNT == 64
#ifdef _WIN64
	_BitScanReverse64(&index, v);
#else
	_InlineBitScanReverse64(&index, v);
#endif
#elif BITS_COUNT == 32
	_BitScanReverse(&index, v);
#endif
	return BITS_COUNT - 1 - index;
}

// Searches the source operand for the most significant set bit.
// If a most significant set bit is found, the result is its offset from bit 0.
// If the source operand is 0, the result is undefined.
__forceinline int _bsr(__bits v)
{
	unsigned long index;
#if BITS_COUNT == 64
#ifdef _WIN64
	_BitScanForward64(&index, v);
#else
	_InlineBitScanForward64(&index, v);
#endif
#elif BITS_COUNT == 32
	_BitScanForward(&index, v);
#endif
	return BITS_COUNT - 1 - index;
}

// Reverses the order of bytes in an 64-bit integer.
extern "C" __declspec(dllexport) __bits WINAPI BITS_METHOD(byteswap_)(__bits bits)
{
	return _byteswap(bits);
}

// Reverses the order of bytes in a range of integers.
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bytesswap_ip_)(
	int n,
	__bits* xy,
	int offxy)
{
	xy += offxy;

	for (int i = 0; i < n; i++)
	{
		xy[i] = _byteswap(xy[i]);
	}
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bytesswap_)(
	int n,
	const __bits* x,
	int offx,
	__bits* y,
	int offy)
{
	x += offx;
	y += offy;

	for (int i = 0; i < n; i++)
	{
		y[i] = _byteswap(x[i]);
	}
}

// Searches the value for a first set bit (1) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bit_scan_forward_be)(__bits bits)
{
	return _bsf(bits);
}

// Searches the value for a last set bit (1) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bit_scan_reverse_be)(__bits bits)
{
	return _bsr(bits);
}

// clears n left (least-significant) bits
#define CLEAR_MASK_LSB_BE(n)				(assert(n >= 0 && n <= BITS_COUNT), (n) == BITS_COUNT ? BITS_MIN : BITS_MAX >> (n))

// clears right (most-significant) bits starting from nth bit
#define CLEAR_MASK_MSB_BE(n)				(assert(n >= 0 && n <= BITS_COUNT), (n) == 0 ? BITS_MIN : BITS_MAX << (BITS_COUNT - (n)))

// clears c (least-significant) bits starting from nth bit
#define CLEAR_MASK_RANGE_BE(n, c)			(CLEAR_MASK_MSB_BE(n) | CLEAR_MASK_LSB_BE(n + c))

// sets n left (least-significant) bits
#define SET_MASK_LSB_BE(n)					CLEAR_MASK_MSB_BE(n)

// sets right (most-significant) bits starting from nth bit
#define SET_MASK_MSB_BE(n)					CLEAR_MASK_LSB_BE(n)

// sets c (least-significant) bits starting from nth bit
#define SET_MASK_RANGE_BE(n, c)				(SET_MASK_MSB_BE(n) & SET_MASK_LSB_BE(n + c))

// Searches the range of values for a first set bit (1) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_one_forward_be)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int endpos = pos + count;

	const int roundpos = pos & ~BITS_MASK;
	if (pos > roundpos)
	{
		const __bits b = *bits & CLEAR_MASK_LSB_BE(pos - roundpos);
		if (b != BITS_MIN)
		{
			pos = roundpos + _bsf(b);
			return pos < endpos ? pos : -1;
		}

		pos = roundpos + BITS_COUNT;
		bits++;
	}

	for (; pos < endpos; pos += BITS_COUNT, bits++)
	{
		if (*bits != BITS_MIN)
		{
			pos += _bsf(*bits);
			return pos < endpos ? pos : -1;
		}
	}

	return -1;
}

// Searches the range of values for a last set bit (1) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_one_reverse_be)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int startpos = pos - count + 1;

	const int roundpos = pos & ~BITS_MASK;
	if (pos - roundpos != BITS_MASK)
	{
		const __bits b = *bits & CLEAR_MASK_MSB_BE(pos - roundpos);
		if (b != BITS_MIN)
		{
			pos = roundpos + _bsr(b);
			return pos >= startpos ? pos : -1;
		}

		pos = roundpos - 1;
		bits--;
	}

	// pos points to last bit in a word
	for (; pos >= startpos; pos -= BITS_COUNT, bits--)
	{
		if (*bits != BITS_MIN)
		{
			pos = (pos & ~BITS_MASK) + _bsr(*bits);
			return pos >= startpos ? pos : -1;
		}
	}

	return -1;
}

// Searches the range of values for a first reset bit (0) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_zero_forward_be)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int endpos = pos + count;

	const int roundpos = pos & ~BITS_MASK;
	if (pos > roundpos)
	{
		const __bits b = *bits | SET_MASK_LSB_BE(pos - roundpos);
		if (b != BITS_MAX)
		{
			pos = roundpos + _bsf(~b);
			return pos < endpos ? pos : -1;
		}

		pos = roundpos + BITS_COUNT;
		bits++;
	}

	for (; pos < endpos; pos += BITS_COUNT, bits++)
	{
		if (*bits != BITS_MAX)
		{
			pos += _bsf(~*bits);
			return pos < endpos ? pos : -1;
		}
	}

	return -1;
}

// Searches the range of values for a last reset bit (0) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_zero_reverse_be)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int startpos = pos - count + 1;

	const int roundpos = pos & ~BITS_MASK;
	if (pos - roundpos != BITS_MASK)
	{
		const __bits b = *bits | SET_MASK_MSB_BE(pos - roundpos);
		if (b != BITS_MAX)
		{
			pos = roundpos + _bsr(~b);
			return pos >= startpos ? pos : -1;
		}

		pos = roundpos - 1;
		bits--;
	}

	// pos points to last bit in a word
	for (; pos >= startpos; pos -= BITS_COUNT, bits--)
	{
		if (*bits != BITS_MAX)
		{
			pos = (pos & ~BITS_MASK) + _bsr(~*bits);
			return pos >= startpos ? pos : -1;
		}
	}

	return -1;
}

// Clears the range of bits for big-endian architecture.
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_reset_be)(
	int count,			// number of bits to clear
	__bits* bits,		// the bits to clear
	int pos 			// the zero-based index of starting bit in bits
	)
{
	bits += (pos >> BITS_SHIFT);
	pos &= BITS_MASK;

	// one word only
	if (pos + count <= BITS_COUNT)
	{
		*bits &= CLEAR_MASK_RANGE_BE(pos, count);
	}
	else
	{
		// clear left side
		if (pos > 0)
		{
			*bits++ &= CLEAR_MASK_MSB_BE(pos);
			count -= BITS_COUNT - pos;
		}

		// clear center
		int wordcount = count >> BITS_SHIFT;
		if (wordcount > 0)
		{
			_set(wordcount, 0, bits);
		}

		// clear right side
		count &= BITS_MASK;
		if (count > 0)
		{
			bits[wordcount] &= CLEAR_MASK_LSB_BE(count);
		}
	}
}

// Sets the range of bits for big-endian architecture.
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_set_be)(
	int count,			// number of bits to set
	__bits* bits,		// the bits to set
	int pos 			// the zero-based index of starting bit in bits
	)
{
	bits += (pos >> BITS_SHIFT);
	pos &= BITS_MASK;

	// one word only
	if (pos + count <= BITS_COUNT)
	{
		*bits |= SET_MASK_RANGE_BE(pos, count);
	}
	else
	{
		// fill left side
		if (pos > 0)
		{
			*bits++ |= SET_MASK_MSB_BE(pos);
			count -= BITS_COUNT - pos;
		}

		// fill center
		int wordcount = count >> BITS_SHIFT;
		if (wordcount > 0)
		{
			_set(wordcount, BITS_MAX, bits);
		}

		// fill right side
		count &= BITS_MASK;
		if (count > 0)
		{
			bits[wordcount] |= SET_MASK_LSB_BE(count);
		}
	}
}

// Copies the DIB bits from one array to another.
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_copy_be)(
	int count,				// number of bits to copy
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	x += (posx >> BITS_SHIFT);
	posx &= BITS_MASK;

	y += (posy >> BITS_SHIFT);
	posy &= BITS_MASK;

	// one word only
	if (posy + count <= BITS_COUNT)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ?
			x[0] >> shift :
			(posx + count <= BITS_COUNT ? x[0] << -shift : _shiftleft(x[1], x[0], -shift));
		const __bits mask = CLEAR_MASK_RANGE_BE(posy, count);
		y[0] = (y[0] & mask) | (x0 & ~mask);

		return;
	}

	// if destination position does not start on word boundary
	// copy right part of the word from the source
	if (posy != 0)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ? x[0] >> shift : _shiftleft(x[1], x[0], -shift);
		const __bits mask = CLEAR_MASK_MSB_BE(posy);
		y[0] = (y[0] & mask) | (x0 & ~mask);

		count -= BITS_COUNT - posy;

		posx += BITS_COUNT - posy;
		x += (posx >> BITS_SHIFT);
		posx &= BITS_MASK;

		y++;
		posy = 0;
	}

	// copy center
	int wordcount = count >> BITS_SHIFT;
	count &= BITS_MASK;

	if (wordcount > 0)
	{
		if (posx == 0)
		{
			::memcpy(y, x, wordcount * sizeof(__bits));
		}
		else
		{
			for (int i = 0; i < wordcount; i++)
			{
				y[i] = _shiftleft(x[i + 1], x[i], posx);
			}
		}
	}

	// copy right side
	if (count > 0)
	{
		x += wordcount;
		y += wordcount;

		const __bits x0 = posx + count <= BITS_COUNT ? x[0] << posx : _shiftleft(x[1], x[0], posx);
		const __bits mask = CLEAR_MASK_LSB_BE(count);
		y[0] = (y[0] & mask) | (x0 & ~mask);
	}
}

// Counts the number of one bits(population count) in a range of integers.
extern "C" __declspec(dllexport) __bits WINAPI BITS_METHOD(bits_popcount_)(
	const __bits bits		// the bits to count
	)
{
	return _popcnt(bits);
}

// Counts the number of one bits(population count) in a range of integers.
extern "C" __declspec(dllexport) __bits WINAPI BITS_METHOD(bits_count_)(
	int count,				// number of bits to count
	const __bits* bits,		// the bits to count
	int pos					// the zero-based index of starting bit in bits
	)
{
	bits += (pos >> BITS_SHIFT);
	pos &= BITS_MASK;

	__bits sum = 0;

	// one word only
	if (pos + count <= BITS_COUNT)
	{
		sum += _popcnt(*bits++ & SET_MASK_RANGE_BE(pos, count));
	}
	else
	{
		// count left side
		if (pos > 0)
		{
			sum += _popcnt(*bits++ & CLEAR_MASK_LSB_BE(pos));
			count -= BITS_COUNT - pos;
		}

		// count center
		const int wordcount = count >> BITS_SHIFT;
		if (wordcount > 0)
		{
			for (int i = 0; i < wordcount; i++)
			{
				sum += _popcnt(bits[i]);
			}
		}

		// count right side
		count &= BITS_MASK;
		if (count > 0)
		{
			sum += _popcnt(bits[wordcount] & CLEAR_MASK_MSB_BE(count));
		}
	}

	return sum;
}

// Logical operations
template<void T2(__bits&, __bits), void T3(__bits&, __bits, __bits)> void __forceinline __bits_logical_be(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
)
{
	x += (posx >> BITS_SHIFT);
	posx &= BITS_MASK;

	y += (posy >> BITS_SHIFT);
	posy &= BITS_MASK;

	// one word only
	if (posy + count <= BITS_COUNT)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ?
			x[0] >> shift :
			(posx + count <= BITS_COUNT ? x[0] << -shift : _shiftleft(x[1], x[0], -shift));
		const __bits mask = CLEAR_MASK_RANGE_BE(posy, count);
		T3(y[0], x0, mask);

		return;
	}

	// if destination position does not start on word boundary
	// copy right part of the word from the source
	if (posy != 0)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ? x[0] >> shift : _shiftleft(x[1], x[0], -shift);
		const __bits mask = CLEAR_MASK_MSB_BE(posy);
		T3(y[0], x0, mask);

		count -= BITS_COUNT - posy;

		posx += BITS_COUNT - posy;
		x += (posx >> BITS_SHIFT);
		posx &= BITS_MASK;

		y++;
		posy = 0;
	}

	// copy center
	int wordcount = count >> BITS_SHIFT;
	count &= BITS_MASK;

	if (wordcount > 0)
	{
		if (posx == 0)
		{
			for (int i = 0; i < wordcount; i++)
			{
				T2(y[i], x[i]);
			}
		}
		else
		{
			for (int i = 0; i < wordcount; i++)
			{
				T2(y[i], _shiftleft(x[i + 1], x[i], posx));
			}
		}
	}

	// copy right side
	if (count > 0)
	{
		x += wordcount;
		y += wordcount;

		const __bits x0 = posx + count <= BITS_COUNT ? x[0] << posx : _shiftleft(x[1], x[0], posx);
		const __bits mask = CLEAR_MASK_LSB_BE(count);
		T3(y[0], x0, mask);
	}
}

void __forceinline logical_or2(__bits &result, __bits value)
{
	result |= value;
}

void __forceinline logical_or3(__bits &result, __bits value, __bits mask)
{
	result |= value & ~mask;
}

void __forceinline logical_and2(__bits &result, __bits value)
{
	result &= value;
}

void __forceinline logical_and3(__bits &result, __bits value, __bits mask)
{
	result &= value | mask;
}

void __forceinline logical_xor2(__bits &result, __bits value)
{
	result ^= value;
}

void __forceinline logical_xor3(__bits &result, __bits value, __bits mask)
{
	result ^= value & ~mask;
}

// Logical NOT
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_not1_)(
	int length,				// number of elements to process
	__bits* xy, 			// the array
	int offxy 				// the zero-based index of starting element in xy
	)
{
	xy += offxy;

	for (int i = 0; i < length; i++)
	{
		xy[i] = ~xy[i];
	}
}

// Logical NOT
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_not2_)(
	int length,				// number of elements to process
	const __bits* x, 		// the source array
	int offx, 				// the zero-based index of starting element in x
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	x += offx;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = ~x[i];
	}
}

// Logical OR
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_or2_)(
	int length,				// number of elements to process
	const __bits* x, 		// the source array
	int offx, 				// the zero-based index of starting element in x
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	x += offx;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] |= x[i];
	}
}

// Logical OR
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_or2_be)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical_be<logical_or2, logical_or3>(count, x, posx, y, posy);
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_or3_)(
	int length,				// number of elements to process
	const __bits* a, 		// the first source array
	int offa, 				// the zero-based index of starting element in a
	const __bits* b, 		// the second source array
	int offb, 				// the zero-based index of starting element in b
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = a[i] | b[i];
	}
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_or4_)(
	int length,				// number of elements to process
	const __bits* a, 		// the first source array
	int offa, 				// the zero-based index of starting element in a
	const __bits* b, 		// the second source array
	int offb, 				// the zero-based index of starting element in b
	const __bits* c, 		// the third source array
	int offc, 				// the zero-based index of starting element in c
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	a += offa;
	b += offb;
	c += offc;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = a[i] | b[i] | c[i];
	}
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_or5_)(
	int length,				// number of elements to process
	const __bits* a, 		// the first source array
	int offa, 				// the zero-based index of starting element in a
	const __bits* b, 		// the second source array
	int offb, 				// the zero-based index of starting element in b
	const __bits* c, 		// the third source array
	int offc, 				// the zero-based index of starting element in c
	const __bits* d, 		// the fourth source array
	int offd, 				// the zero-based index of starting element in d
	__bits* y, 				// the destination array
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
		y[i] = a[i] | b[i] | c[i] | d[i];
	}
}

// Logical AND
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_and_mask_)(
	int length,				// number of elements to process
	__bits mask,			// the mask to apply
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] &= mask;
	}
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_and_mask_inc_)(
	int length,				// number of elements to process
	__bits mask,			// the mask to apply
	__bits* y, 				// the destination array
	int offy, 				// the zero-based index of starting element in y
	int incy				// the increment for the elements of y
	)
{
	y += offy;

	if (incy == 1)
	{
		for (int i = 0; i < length; i++)
		{
			y[i] &= mask;
		}

	}
	else
	{
		for (int i = 0, yi = 0; i < length; i++, yi += incy)
		{
			y[yi] &= mask;
		}
	}
}

// Logical AND
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_and2_)(
	int length,				// number of elements to process
	const __bits* x, 		// the source array
	int offx, 				// the zero-based index of starting element in x
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	x += offx;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] &= x[i];
	}
}

// Logical AND
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_and2_be)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical_be<logical_and2, logical_and3>(count, x, posx, y, posy);
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_and3_)(
	int length,				// number of elements to process
	const __bits* a, 		// the first source array
	int offa, 				// the zero-based index of starting element in a
	const __bits* b, 		// the second source array
	int offb, 				// the zero-based index of starting element in b
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = a[i] & b[i];
	}
}

// Logical XOR
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_xor2_)(
	int length,				// number of elements to process
	const __bits* x, 		// the source array
	int offx, 				// the zero-based index of starting element in x
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	x += offx;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] ^= x[i];
	}
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_xor2_be)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical_be<logical_xor2, logical_xor3>(count, x, posx, y, posy);
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_xor3_)(
	int length,				// number of elements to process
	const __bits* a, 		// the first source array
	int offa, 				// the zero-based index of starting element in a
	const __bits* b, 		// the second source array
	int offb, 				// the zero-based index of starting element in b
	__bits* y, 				// the destination array
	int offy 				// the zero-based index of starting element in y
	)
{
	a += offa;
	b += offb;
	y += offy;

	for (int i = 0; i < length; i++)
	{
		y[i] = a[i] ^ b[i];
	}
}
