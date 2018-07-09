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
constexpr __forceinline __bits __shiftleft128(__bits LowPart, __bits HighPart, unsigned char Shift)
{
	return (LowPart << Shift) | (HighPart >> (BITS_COUNT - Shift));
}
#endif
#elif BITS_COUNT == 32
constexpr __forceinline __bits _shiftleft(__bits LowPart, __bits HighPart, unsigned char Shift)
{
	return (LowPart << Shift) | (HighPart >> (BITS_COUNT - Shift));
}
#endif

#if BITS_COUNT == 64
#ifdef _WIN64
#define _popcnt				__popcnt64
#else
__forceinline unsigned __int64 __popcnt64(unsigned __int64 value)
{
	return __popcnt((unsigned)value) + __popcnt((unsigned)(value >> 32));
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
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bytesswap_ip_)(int n, __bits* xy, int offxy)
{
	xy += offxy;

	for (int i = 0; i < n; i++)
	{
		xy[i] = _byteswap(xy[i]);
	}
}

extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bytesswap_)(int n, const __bits* x, int offx, __bits* y, int offy)
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
#define CLEAR_MASK_LEFT_BE(n)				(assert(n >= 0 && n <= 32), (n) == 32 ? BITS_MIN : BITS_MAX >> (n))

// clears right (most-significant) bits starting from nth bit
#define CLEAR_MASK_RIGHT_BE(n)				(assert(n >= 0 && n <= 32), (n) == 0 ? BITS_MIN : BITS_MAX << (BITS_COUNT - (n)))

// clears c (least-significant) bits starting from nth bit
#define CLEAR_MASK_RANGE_BE(n, c)			(CLEAR_MASK_RIGHT_BE(n) | CLEAR_MASK_LEFT_BE(n + c))

// sets n left (least-significant) bits
#define SET_MASK_LEFT_BE(n)					CLEAR_MASK_RIGHT_BE(n)

// sets right (most-significant) bits starting from nth bit
#define SET_MASK_RIGHT_BE(n)				CLEAR_MASK_LEFT_BE(n)

// sets c (least-significant) bits starting from nth bit
#define SET_MASK_RANGE_BE(n, c)				(SET_MASK_RIGHT_BE(n) & SET_MASK_LEFT_BE(n + c))

// Searches the range of values for a first set bit (1) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_one_forward_be)(int count, const __bits* bits, int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int endpos = pos + count;

	const int roundpos = pos & ~BITS_MASK;
	if (pos > roundpos)
	{
		const __bits b = *bits & CLEAR_MASK_LEFT_BE(pos - roundpos);
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
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_one_reverse_be)(int count, const __bits* bits, int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int startpos = pos - count + 1;

	const int roundpos = pos & ~BITS_MASK;
	if (pos - roundpos != BITS_MASK)
	{
		const __bits b = *bits & CLEAR_MASK_RIGHT_BE(pos - roundpos);
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
			pos -= _bsr(*bits);
			return pos >= startpos ? pos : -1;
		}
	}

	return -1;
}

// Searches the range of values for a first reset bit (0) for big-endian architecture.
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_zero_forward_be)(int count, const __bits* bits, int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int endpos = pos + count;

	const int roundpos = pos & ~BITS_MASK;
	if (pos > roundpos)
	{
		const __bits b = *bits | SET_MASK_LEFT_BE(pos - roundpos);
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
extern "C" __declspec(dllexport) int WINAPI BITS_METHOD(bits_scan_zero_reverse_be)(int count, const __bits* bits, int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int startpos = pos - count + 1;

	const int roundpos = pos & ~BITS_MASK;
	if (pos - roundpos != BITS_MASK)
	{
		const __bits b = *bits | SET_MASK_RIGHT_BE(pos - roundpos);
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
			pos -= _bsr(~*bits);
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
			*bits++ &= CLEAR_MASK_RIGHT_BE(pos);
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
			bits[wordcount] &= CLEAR_MASK_LEFT_BE(count);
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
			*bits++ |= SET_MASK_RIGHT_BE(pos);
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
			bits[wordcount] |= SET_MASK_LEFT_BE(count);
		}
	}
}

// Copies the DIB bits from one array to another.
extern "C" __declspec(dllexport) void WINAPI BITS_METHOD(bits_copy_be)(
	int count,				// number of bits to copy
	const __bits* bitssrc, 	// the source array
	int possrc, 			// the zero-based index of starting bit in bitssrc
	__bits* bitsdst, 		// the destination array
	int posdst 				// the zero-based index of starting bit in bitsdst
	)
{
	bitssrc += (possrc >> BITS_SHIFT);
	possrc &= BITS_MASK;

	bitsdst += (posdst >> BITS_SHIFT);
	posdst &= BITS_MASK;

	// one word only
	if (posdst + count <= BITS_COUNT)
	{
		const int shift = posdst - possrc;
		const __bits x0 = shift >= 0 ?
			bitssrc[0] >> shift :
			(possrc + count <= BITS_COUNT ? bitssrc[0] << -shift : _shiftleft(bitssrc[0], bitssrc[1], -shift));
		const __bits mask = CLEAR_MASK_RANGE_BE(posdst, count);
		bitsdst[0] = (bitsdst[0] & mask) | (x0 & ~mask);

		return;
	}

	// if destination position does not start on word boundary
	// copy right part of the word from the source
	if (posdst != 0)
	{
		const int shift = posdst - possrc;
		const __bits x0 = shift >= 0 ? bitssrc[0] >> shift : _shiftleft(bitssrc[0], bitssrc[1], -shift);
		const __bits mask = CLEAR_MASK_RIGHT_BE(posdst);
		bitsdst[0] = (bitsdst[0] & mask) | (x0 & ~mask);

		count -= BITS_COUNT - posdst;

		possrc += BITS_COUNT - posdst;
		bitssrc += (possrc >> BITS_SHIFT);
		possrc &= BITS_MASK;

		bitsdst++;
		posdst = 0;
	}

	// copy center
	int wordcount = count >> BITS_SHIFT;
	count &= BITS_MASK;

	if (wordcount > 0)
	{
		if (possrc == 0)
		{
			::memcpy(bitsdst, bitssrc, wordcount * sizeof(__bits));
		}
		else
		{
			for (int i = 0; i < wordcount; i++)
			{
				bitsdst[i] = _shiftleft(bitssrc[i], bitssrc[i + 1], possrc);
			}
		}
	}

	// copy right side
	if (count > 0)
	{
		bitssrc += wordcount;
		bitsdst += wordcount;

		const __bits x0 = possrc + count <= BITS_COUNT ? bitssrc[0] << possrc : _shiftleft(bitssrc[0], bitssrc[1], possrc);
		const __bits mask = CLEAR_MASK_LEFT_BE(count);
		bitsdst[0] = (bitsdst[0] & mask) | (x0 & ~mask);
	}
}

// Counts the number of one bits(population count) in a range of integers.
extern "C" __declspec(dllexport) __bits WINAPI BITS_METHOD(bits_onecount_)(
	int count,				// number of bits to count
	const __bits* bits,		// the bits to set
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
			sum += _popcnt(*bits++ & CLEAR_MASK_LEFT_BE(pos));
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
			sum += _popcnt(bits[wordcount] & CLEAR_MASK_RIGHT_BE(count));
		}
	}

	return sum;
}

// Counts the number of zero bits in a range of integers.
extern "C" __declspec(dllexport) __bits WINAPI BITS_METHOD(bits_zerocount_)(
	int count,				// number of bits to count
	const __bits* bits,		// the bits to set
	int pos					// the zero-based index of starting bit in bits
	)
{
	bits += (pos >> BITS_SHIFT);
	pos &= BITS_MASK;

	__bits sum = 0;

	// one word only
	if (pos + count <= BITS_COUNT)
	{
		sum += _popcnt(~*bits++ & SET_MASK_RANGE_BE(pos, count));
	}
	else
	{
		// count left side
		if (pos > 0)
		{
			sum += _popcnt(~*bits++ & CLEAR_MASK_LEFT_BE(pos));
			count -= BITS_COUNT - pos;
		}

		// count center
		const int wordcount = count >> BITS_SHIFT;
		if (wordcount > 0)
		{
			for (int i = 0; i < wordcount; i++)
			{
				sum += _popcnt(~bits[i]);
			}
		}

		// count right side
		count &= BITS_MASK;
		if (count > 0)
		{
			sum += _popcnt(~bits[wordcount] & CLEAR_MASK_RIGHT_BE(count));
		}
	}

	return sum;
}
