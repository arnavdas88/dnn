class SIMDDetect
{
private:
	SIMDDetect();

public:
	// Returns true if SSE4.1 is available on this system.
	static __forceinline bool IsSSEAvailable() { return instance.sse_available; }
	// Returns true if AVX is available on this system.
	static __forceinline bool IsAVXAvailable() { return instance.avx_available; }
	// Returns true if AVX2 (integer support) is available on this system.
	static __forceinline bool IsAVX2Available() { return instance.avx2_available; }

private:
	static SIMDDetect instance;
	// If true, then SSe4.1 has been detected.
	static bool sse_available;
	// If true, then AVX has been detected.
	static bool avx_available;
	static bool avx2_available;
};
