#include <ppl.h>
using namespace concurrency;

template <typename _Index_type, typename _Function>
void parallel(_Index_type _Length, _Index_type _Partition, const _Function& _Func)
{
	if (_Length < _Partition)
	{
		_Func(0, _Length);
	}
	else
	{
		parallel_for(0, (_Length + _Partition - 1) / _Partition, [&](int idx) {

			const int start = idx * _Partition;
			const int end = __min(start + _Partition, _Length);
			_Func(start, end);
		});
	}
}
