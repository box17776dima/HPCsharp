# High Performance Computing in C# (HPC#)

High performance C#/.NET generic algorithms. Community driven to move C# toward high performance computing.
Includes linear and stable Radix Sort algorithm, as well as Merge Sort and Merge of arrays and lists.
Provides a trade-off between performance and level of abstraction: Array, List, IList, IEnumerable.
Lowering the level of abstraction results in higher performance. Some algorithms are parallel for additional performance.
Open source.

Version 1.0 includes the following Linq style algorithms:

*Algorithm*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array, List|1.5-3X faster
Max|Array, List|1.5X faster

Sorting and Merging Algorithms:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*
--- | --- | --- | --- | --- | ---
Radix Sort|Array, List|Random|4X-7X|14X-35X|4X-9X
Radix Sort|Array, List|Presorted|0.3X-0.5X|3X-5X|1X-3X
Radix Sort|Array, List|Constant|1X-1.5X|5X-8X|2X-3X

Radix Sort is linear time O(N) and stable. Radix Sort runs on a single core, whereas Linq.AsParallel ran on all the cores.
Only slower when sorting presorted Array or List, but faster in all other cases, even than parallel Linq.OrderBy.AsParallel, using a single core.

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*
--- | --- | --- | --- | --- | ---
Merge Sort|Array|Random|0.6X-0.7X|2X-4X|1X-3X
Merge Sort|Array|Presorted|0.3X-0.3X|4X-5X|2X-3X|
Merge Sort|Array|Constant|0.5X-0.6X|3X-4X|2X-3X|

Merge Sort is O(NlgN), never O(N<sup>2</sup>) and generic. Faster than Linq.OrderBy and Linq.OrderBy.AsParallel.

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*
--- | --- | --- | --- | --- | ---
Insertion Sort|Array, List||||
Merge|Array, List||||

Insertion Sort is O(N<sup>2</sup>), and only useful for fast in-place sorting of very small collections.

Merge algorithm merges two presorted collections.

See HPCsharpExample folder in this repo for usage examples - a complete VisualStudio 2017 solution provided.

For details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

# More High Performance Algorithms
Soon to be available at https://foostate.com/

Radix Sort has been extended to sort user defined classes based on a UInt32 or UInt64 key in the class. Radix Sort is still using only a single core.

Merge Sort has been extended to run in parallel using multiple cores.

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Radix Sort|Array|Random|1X-4X|3X-5X|1X-2X|User defined class
Radix Sort|List|Random|2X-4X|3X-5X|1X-2X|User defined class
Radix Sort|Array|Presorted|1.2X-1.7X|0.9X-2.5X|0.9X-1.4X|User defined class
Radix Sort|List|Presorted|1.0X-1.2X|1.7X-2.1X|0.7X-1.1X|User defined class
Radix Sort|Array|Constant|3X-4X|4X-5X|2X-3X|User defined class
Radix Sort|List|Constant|2X-4X|3X-4X|1.5X-2X|User defined class

Only slightly slower than Array.Sort and List.Sort for presorted distribution, but faster for all other distributions. Uses a single core and is stable.
Faster than Linq.OrderBy and Linq.OrderBy.AsParallel

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|2X-3X|10X-16X|4X-8X|Stable
Parallel Merge Sort|Array|Presorted|1.2X-2.3X|17X-34X|7X-20X|Stable
Parallel Merge Sort|Array|Constant|2X-3X|13X-20X|7X-13X|Stable

Faster than Array.Sort and List.Sort across all distributions. Substantially faster than Linq.OrderBy and Linq.OrderBy.AsParallel

*36-core AWS c5.18xlarge*

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|5X-14X|19X-90X|7X-47X|Stable
Parallel Merge Sort|Array|Presorted|1X-6X|5X-60X|16X-122X|Stable
Parallel Merge Sort|Array|Constant|||9X-44X|Stable

Parallel Copying:

*Method*|*Collection*|*Parallel*
--- | --- | ---
Parallel CopyTo|List to Array|1.7X-2.5X faster



[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
