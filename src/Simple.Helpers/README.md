# Simple.Helpers

## Throw

Pierian spring:
https://reubenbond.github.io/posts/dotnet-perf-tuning
or translation
https://www.lshnk.me/2019/01/26/улучшаем-производительность-в-.net-core-по-опыту-бинарного-сериализатора-hagar/

### Using

```cs
using Simple;
...
	Throw.Exception(new ...Exception(parameters...));
...
	//	allows optional additional check
	fileName = Throw.IsArgumentNullException(fileName, nameof(fileName), i => !string.IsNullOrEmpty(i));
```

## HashCoceCombiner (for netstandard2.0 only)
class (from MS v2.2)

## Extensions

some extensions for Stream and collectopns
##  Release notes

0.1.0.1
	append:
		TargetFramework: net6.0
	update:
		IsArgumentNullException() can be raice 2 exeptios (ArgumentNullException & ArgumentException)
