#define BITS_MASK				(BITS_COUNT - 1)

#if BITS_COUNT == 64
#define BITS_SHIFT				6
#define BITS_MAX				_UI64_MAX
#define BITS_MIN				0ul
#define BITS_NAME(name)			name##_64
typedef unsigned __int64		__bits;
#elif BITS_COUNT == 32
#define BITS_SHIFT				5
#define BITS_MAX				_UI32_MAX
#define BITS_MIN				0ui32
#define BITS_NAME(name)			name##_32
typedef unsigned				__bits;
#endif

#define BITSAPI(type, name)		GENIXAPI(type, BITS_NAME(name))

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

#if BITS_COUNT == 64 && defined(_WIN64)
#define _shiftright			__shiftright128
#define _shiftleft			__shiftleft128
#else
__bits __forceinline _shiftright(__bits LowPart, __bits HighPart, unsigned char Shift)
{
	return (LowPart >> Shift) | (HighPart << (BITS_COUNT - Shift));
}
__bits __forceinline _shiftleft(__bits LowPart, __bits HighPart, unsigned char Shift)
{
	return (HighPart << Shift) | (LowPart >> (BITS_COUNT - Shift));
}
#endif

#if BITS_COUNT == 64
#ifdef _WIN64
#define _popcnt				__popcnt64
#else
unsigned __int64 __forceinline _popcnt(unsigned __int64 value)
{
	return (unsigned __int64)(__popcnt(unsigned(value))) + __popcnt((unsigned)(value >> 32));
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
	_BitScanForward64(&index, v);
#else
	_InlineBitScanForward64(&index, v);
#endif
#elif BITS_COUNT == 32
	_BitScanForward(&index, v);
#endif
	return index;
}

// Searches the source operand for the most significant set bit.
// If a most significant set bit is found, the result is its offset from bit 0.
// If the source operand is 0, the result is undefined.
__forceinline int _bsr(__bits v)
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
	return index;
}

// Reverses the order of bytes in an 64-bit integer.
BITSAPI(__bits, byteswap)(__bits bits)
{
	return _byteswap(bits);
}

// Reverses the order of bytes in a range of integers.
BITSAPI(void, bytesswap_ip)(
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

BITSAPI(void, bytesswap)(
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

// Searches the value for a first set bit (1) for little-endian architecture.
BITSAPI(int, bit_scan_forward)(__bits bits)
{
	return _bsf(bits);
}

// Searches the value for a last set bit (1) for little-endian architecture.
BITSAPI(int, bit_scan_reverse)(__bits bits)
{
	return _bsr(bits);
}

// clears n least-significant bits
#define CLEAR_MASK_LSB(n)				(assert(n >= 0 && n <= BITS_COUNT), (n) == BITS_COUNT ? BITS_MIN : BITS_MAX << (n))

// clears all most-significant bits starting from nth bit
#define CLEAR_MASK_MSB(n)				(~CLEAR_MASK_LSB(n))

// clears c least-significant bits starting from nth bit
#define CLEAR_MASK_RANGE(n, c)			(CLEAR_MASK_LSB(n) ^ CLEAR_MASK_MSB(n + c))

// sets n least-significant bits
#define SET_MASK_LSB(n)					(~CLEAR_MASK_LSB(n))

// sets all most-significant bits starting from nth bit
#define SET_MASK_MSB(n)					(~CLEAR_MASK_MSB(n))

// sets c least-significant bits starting from nth bit
#define SET_MASK_RANGE(n, c)			(~CLEAR_MASK_RANGE(n, c))

// Searches the range of values for a first set bit (1) for little-endian architecture.
BITSAPI(int, bits_scan_one_forward)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int endpos = pos + count;

	const int roundpos = pos & ~BITS_MASK;
	if (pos > roundpos)
	{
		const __bits b = *bits & CLEAR_MASK_LSB(pos - roundpos);
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

// Searches the range of values for a last set bit (1) for little-endian architecture.
BITSAPI(int, bits_scan_one_reverse)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int startpos = pos - count + 1;

	const int roundpos = pos & ~BITS_MASK;
	if (pos - roundpos != BITS_MASK)
	{
		const __bits b = *bits & CLEAR_MASK_MSB(pos - roundpos);
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

// Searches the range of values for a first reset bit (0) for little-endian architecture.
BITSAPI(int, bits_scan_zero_forward)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int endpos = pos + count;

	const int roundpos = pos & ~BITS_MASK;
	if (pos > roundpos)
	{
		const __bits b = *bits | SET_MASK_LSB(pos - roundpos);
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

// Searches the range of values for a last reset bit (0) for little-endian architecture.
BITSAPI(int, bits_scan_zero_reverse)(
	int count,
	const __bits* bits,
	int pos)
{
	bits += (pos >> BITS_SHIFT);

	const int startpos = pos - count + 1;

	const int roundpos = pos & ~BITS_MASK;
	if (pos - roundpos != BITS_MASK)
	{
		const __bits b = *bits | SET_MASK_MSB(pos - roundpos);
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

// Clears the range of bits for little-endian architecture.
BITSAPI(void, bits_reset)(
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
		*bits &= CLEAR_MASK_RANGE(pos, count);
	}
	else
	{
		// clear left side
		if (pos > 0)
		{
			*bits++ &= CLEAR_MASK_MSB(pos);
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
			bits[wordcount] &= CLEAR_MASK_LSB(count);
		}
	}
}

// Sets the range of bits for little-endian architecture.
BITSAPI(void, bits_set)(
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
		*bits |= SET_MASK_RANGE(pos, count);
	}
	else
	{
		// fill left side
		if (pos > 0)
		{
			*bits++ |= SET_MASK_MSB(pos);
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
			bits[wordcount] |= SET_MASK_LSB(count);
		}
	}
}

// Sets the range of bits using the provided mask for little-endian architecture.
BITSAPI(void, bits_set_mask)(
	int count,			// number of bits to set
	const __bits x,		// the mask used to set the bits
	__bits* bits,		// the bits to set
	int pos 			// the zero-based index of starting bit in bits
	)
{
	bits += (pos >> BITS_SHIFT);
	pos &= BITS_MASK;

	// one word only
	if (pos + count <= BITS_COUNT)
	{
		const __bits mask = CLEAR_MASK_RANGE(pos, count);
		*bits &= mask;
		*bits |= x & ~mask;
	}
	else
	{
		// fill left side
		if (pos > 0)
		{
			const __bits mask = CLEAR_MASK_MSB(pos);
			*bits &= mask;
			*bits |= x & ~mask;

			count -= BITS_COUNT - pos;
			bits++;
		}

		// fill center
		int wordcount = count >> BITS_SHIFT;
		if (wordcount > 0)
		{
			_set(wordcount, x, bits);
		}

		// fill right side
		count &= BITS_MASK;
		if (count > 0)
		{
			const __bits mask = CLEAR_MASK_LSB(count);
			bits[wordcount] &= mask;
			bits[wordcount] |= x & ~mask;
		}
	}
}

// Determines whether the bits in two arrays are the same.
BITSAPI(bool, bits_equals)(
	int count,				// number of bits to test
	const __bits* x, 		// the first source array
	int posx, 				// the zero-based index of starting bit in x
	const __bits* y, 		// the second source array
	int posy 				// the zero-based index of starting bit in y
	)
{
	if (count == 0)
	{
		return true;
	}

	x += (posx >> BITS_SHIFT);
	posx &= BITS_MASK;

	y += (posy >> BITS_SHIFT);
	posy &= BITS_MASK;

	// one word only
	if (posy + count <= BITS_COUNT)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ?
			x[0] << shift :
			(posx + count <= BITS_COUNT ? x[0] >> -shift : _shiftright(x[0], x[1], -shift));
		const __bits mask = SET_MASK_RANGE(posy, count);

		return (y[0] & mask) == (x0 & mask);
	}

	// if destination position does not start on word boundary test right part of the word from the source
	if (posy != 0)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ? x[0] << shift : _shiftright(x[0], x[1], -shift);
		const __bits mask = SET_MASK_MSB(posy);
		if ((y[0] & mask) != (x0 & mask))
		{
			return false;
		}

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
				if (y[i] != x[i])
				{
					return false;
				}
			}
		}
		else
		{
			for (int i = 0; i < wordcount; i++)
			{
				if (y[i] != _shiftright(x[i], x[i + 1], posx))
				{
					return false;
				}
			}
		}

		x += wordcount;
		y += wordcount;
	}

	// copy right side
	if (count > 0)
	{
		const __bits x0 = posx + count <= BITS_COUNT ? x[0] >> posx : _shiftright(x[0], x[1], posx);
		const __bits mask = SET_MASK_LSB(count);
		return (y[0] & mask) == (x0 & mask);
	}

	return true;
}

// Copies the bits from one array to another.
BITSAPI(void, bits_copy)(
	int count,				// number of bits to copy
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	if (count == 0)
	{
		return;
	}

	x += (posx >> BITS_SHIFT);
	posx &= BITS_MASK;

	y += (posy >> BITS_SHIFT);
	posy &= BITS_MASK;

	// one word only
	if (posy + count <= BITS_COUNT)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ?
			x[0] << shift :
			(posx + count <= BITS_COUNT ? x[0] >> -shift : _shiftright(x[0], x[1], -shift));
		const __bits mask = CLEAR_MASK_RANGE(posy, count);
		y[0] = (y[0] & mask) | (x0 & ~mask);

		return;
	}

	// if destination position does not start on word boundary
	// copy right part of the word from the source
	if (posy != 0)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ? x[0] << shift : _shiftright(x[0], x[1], -shift);
		const __bits mask = CLEAR_MASK_MSB(posy);
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
				y[i] = _shiftright(x[i], x[i + 1], posx);
			}
		}
	}

	// copy right side
	if (count > 0)
	{
		x += wordcount;
		y += wordcount;

		const __bits x0 = posx + count <= BITS_COUNT ? x[0] >> posx : _shiftright(x[0], x[1], posx);
		const __bits mask = CLEAR_MASK_LSB(count);
		y[0] = (y[0] & mask) | (x0 & ~mask);
	}
}

// Shift the bits in the array to the left.
BITSAPI(void, bits_shl)(
	const int count,		// number of bits to copy
	const int shift,		// number of bits to shift
	__bits* bits, 			// the source array
	const int pos 			// the zero-based index of starting bit in bits
	)
{
	if (count == 0 || shift == 0)
	{
		return;
	}

	if (shift >= count)
	{
		BITS_NAME(bits_reset)(count, bits, pos);
		return;
	}

	// copy from left to right (from MSB to LSB)
	// positions points to the MSBs + 1 of x and y (1 - 32/64)
	int remaining = count - shift;
	int posy = pos + count - 1;
	int posx = posy - shift;

	const __bits* x = bits + (posx >> BITS_SHIFT);
	posx &= BITS_MASK;
	posx += 1;

	__bits* y = bits + (posy >> BITS_SHIFT);
	posy &= BITS_MASK;
	posy += 1;

	// first word
	const int shift0 = posy - posx;
	const __bits x0 = shift0 >= 0 ?
		(posx >= remaining ? x[0] << shift0 : _shiftleft(x[-1], x[0], shift0)) :
		x[0] >> -shift0;

	if (posy >= remaining)
	{
		const __bits mask0 = CLEAR_MASK_RANGE(posy - remaining, remaining);
		y[0] = (y[0] & mask0) | (x0 & ~mask0);
	}
	else
	{
		const __bits mask0 = CLEAR_MASK_LSB(posy);
		y[0] = (y[0] & mask0) | (x0 & ~mask0);

		remaining -= posy;

		posx -= posy;
		if (posx <= 0)
		{
			x--;
			posx += BITS_COUNT;
		}

		y--;
		posy = BITS_COUNT;

		// copy center bits
		int wordcount = remaining >> BITS_SHIFT;
		if (wordcount > 0)
		{
			if (posx == BITS_COUNT)
			{
				while (wordcount--)
				{
					*y-- = *x--;
				}
			}
			else
			{
				while (wordcount--)
				{
					*y-- = _shiftleft(x[-1], x[0], BITS_COUNT - posx);
					x--;
				}
			}
		}

		// copy remaining LSBs
		remaining &= BITS_MASK;
		if (remaining > 0)
		{
			const __bits x1 = posx >= remaining ? x[0] << (BITS_COUNT - posx) : _shiftleft(x[-1], x[0], BITS_COUNT - posx);
			const __bits mask1 = CLEAR_MASK_MSB(posy - remaining);
			y[0] = (y[0] & mask1) | (x1 & ~mask1);
		}
	}

	// clear remaining LSBs
	BITS_NAME(bits_reset)(shift, bits, pos);
}

// Shift the bits in the array to the right.
BITSAPI(void, bits_shr)(
	const int count,		// number of bits to copy
	const int shift,		// number of bits to shift
	__bits* bits, 			// the source array
	const int pos 			// the zero-based index of starting bit in bits
	)
{
	if (count == 0 || shift == 0)
	{
		return;
	}

	if (shift >= count)
	{
		BITS_NAME(bits_reset)(count, bits, pos);
		return;
	}

	int remaining = count - shift;
	int posx = pos + shift;
	int posy = pos;

	const __bits* x = bits + (posx >> BITS_SHIFT);
	posx &= BITS_MASK;

	__bits* y = bits + (posy >> BITS_SHIFT);
	posy &= BITS_MASK;

	// first word
	const int shift0 = posy - posx;
	const __bits x0 = shift0 >= 0 ?
		x[0] << shift0 :
		(posx + remaining <= BITS_COUNT ? x[0] >> -shift0 : _shiftright(x[0], x[1], -shift0));

	if (posy + remaining <= BITS_COUNT)
	{
		const __bits mask0 = CLEAR_MASK_RANGE(posy, remaining);
		y[0] = (y[0] & mask0) | (x0 & ~mask0);
	}
	else
	{
		const __bits mask0 = CLEAR_MASK_MSB(posy);
		y[0] = (y[0] & mask0) | (x0 & ~mask0);

		remaining -= BITS_COUNT - posy;

		posx += BITS_COUNT - posy;
		x += (posx >> BITS_SHIFT);
		posx &= BITS_MASK;

		y++;
		posy = 0;

		// copy center bits
		int wordcount = remaining >> BITS_SHIFT;
		if (wordcount > 0)
		{
			if (posx == 0)
			{
				for (int i = 0; i < wordcount; i++)
				{
					y[i] = x[i];
				}
			}
			else
			{
				for (int i = 0; i < wordcount; i++)
				{
					y[i] = _shiftright(x[i], x[i + 1], posx);
				}
			}

			x += wordcount;
			y += wordcount;
		}

		// copy last word
		remaining &= BITS_MASK;
		if (remaining > 0)
		{
			const __bits x1 = posx + remaining <= BITS_COUNT ? x[0] >> posx : _shiftright(x[0], x[1], posx);
			const __bits mask1 = CLEAR_MASK_LSB(remaining);
			y[0] = (y[0] & mask1) | (x1 & ~mask1);
		}
	}

	// clear remaining MSBs
	BITS_NAME(bits_reset)(shift, bits, pos + count - shift);
}

// Counts the number of one bits(population count) in a range of integers.
BITSAPI(__bits, bits_popcount)(
	const __bits bits		// the bits to count
	)
{
	return _popcnt(bits);
}

// Counts the number of one bits(population count) in a range of integers.
BITSAPI(__bits, bits_count)(
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
		sum += _popcnt(*bits++ & SET_MASK_RANGE(pos, count));
	}
	else
	{
		// count left side
		if (pos > 0)
		{
			sum += _popcnt(*bits++ & CLEAR_MASK_LSB(pos));
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
			sum += _popcnt(bits[wordcount] & CLEAR_MASK_MSB(count));
		}
	}

	return sum;
}

// Logical operations
template<void T2(__bits&, __bits), void T3(__bits&, __bits, __bits)> void __forceinline __bits_logical(
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
			x[0] << shift :
			(posx + count <= BITS_COUNT ? x[0] >> -shift : _shiftright(x[0], x[1], -shift));
		const __bits mask = CLEAR_MASK_RANGE(posy, count);
		T3(y[0], x0, mask);

		return;
	}

	// if destination position does not start on word boundary
	// copy right part of the word from the source
	if (posy != 0)
	{
		const int shift = posy - posx;
		const __bits x0 = shift >= 0 ? x[0] << shift : _shiftright(x[0], x[1], -shift);
		const __bits mask = CLEAR_MASK_MSB(posy);
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
				T2(y[i], _shiftright(x[i], x[i + 1], posx));
			}
		}
	}

	// copy right side
	if (count > 0)
	{
		x += wordcount;
		y += wordcount;

		const __bits x0 = posx + count <= BITS_COUNT ? x[0] >> posx : _shiftright(x[0], x[1], posx);
		const __bits mask = CLEAR_MASK_LSB(count);
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

void __forceinline logical_xand2(__bits &result, __bits value)
{
	result &= ~value;
}

void __forceinline logical_xand3(__bits &result, __bits value, __bits mask)
{
	result &= ~value | mask;
}

void __forceinline logical_xor2(__bits &result, __bits value)
{
	result ^= value;
}

void __forceinline logical_xor3(__bits &result, __bits value, __bits mask)
{
	result ^= value & ~mask;
}

// Logical OR
BITSAPI(void, bits_or)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical<logical_or2, logical_or3>(count, x, posx, y, posy);
}

// Logical AND
BITSAPI(void, bits_and)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical<logical_and2, logical_and3>(count, x, posx, y, posy);
}

// Logical XAND (A AND NOT B)
BITSAPI(void, bits_xand)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical<logical_xand2, logical_xand3>(count, x, posx, y, posy);
}

// Logical XOR
BITSAPI(void, bits_xor)(
	int count,				// number of bits to process
	const __bits* x, 		// the source array
	int posx, 				// the zero-based index of starting bit in x
	__bits* y, 				// the destination array
	int posy 				// the zero-based index of starting bit in y
	)
{
	__bits_logical<logical_xor2, logical_xor3>(count, x, posx, y, posy);
}
