# Simple.Helpers

## Throw

Pierian spring:
https://reubenbond.github.io/posts/dotnet-perf-tuning
or translation
https://www.lshnk.me/2019/01/26/улучшаем-производительность-в-.net-core-по-опыту-бинарного-сериализатора-hagar/

## Using

```cs
using Simple;
...
	Throw.Exception(new ...Exception(parameters...));
...
	//	allows optional additional check
	fileName = Throw.IsArgumentNullException(fileName, nameof(fileName));
	or
	fileName = Throw.IsArgumentNullException(fileName, nameof(fileName), i => !string.IsNullOrEmpty(i));
```


### HashCoceCombiner (for netstandard2.0 only)
class (from MS v2.2)

### Extensions
some extensions for Stream
some extensions for collectopns
some other extensions

##  Release notes

0.1.1
	append:
		support netstandard2.1
		DicString
		Option (ala Rust)
		RandomGenerator
		StrUtil
	update:
		CollectionExtensions (Merge IReadOnlyDictionary)
		small fixes (in ExtensionsHex, HashCodeCombiner)

0.1.0.3
	update(fixed):
		added Throw.IsArgumentNullException override (for netstandard2.0)
		rename Extensions => ExtensionsHex + refactoring ToHexString()

0.1.0.2
	append:
		Stream.WriteAsync()
		Extensions (hex convert, IServiceProvider)
		Throw.Exception as functions
	update:
		added Throw.IsArgumentNullException override

0.1.0.1
	append:
		TargetFramework: net6.0
	update:
		IsArgumentNullException() can be raice 2 exeptios (ArgumentNullException & ArgumentException)
