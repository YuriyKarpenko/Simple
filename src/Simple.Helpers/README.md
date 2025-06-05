# Simple.Helpers

## Throw

Pierian spring:
https://reubenbond.github.io/posts/dotnet-perf-tuning
or translation
https://www.lshnk.me/2019/01/26/улучшаем-производительность-в-.net-core-по-опыту-бинарного-сериализатора-hagar/

### Using

```cs
using Simple.Helpers;
...
	Throw.Exception(new ...Exception(parameters...));
...
	//	allows optional additional check
	fileName = Throw.IsArgumentNullException(fileName, nameof(fileName));
	or
	fileName = Throw.IsArgumentNullException(fileName, nameof(fileName), i => !string.IsNullOrEmpty(i));


	public Task<TResult> SomeMethodAsync()
	{
		try
		{
			...
			return Task.FromResult(...); 
		}
		catch (Exception ex)
		{
			...
			return Throw.ExceptionAsync<TResult>(ex)
		}
	}
```

## Option (ala Rust)

### Using

```cs
using Simple.Helpers;
class SomeClass
{
...
    private async Task<IOption<IResponse>> ExecStatusAsync(ITestResult tr, IRequest request)
    {
        var resp = await _rest.ExecuteAsync(request);
        return tr.Status(resp, HttpStatusCode.Accepted);
    }
}	
...
static class SomeClassExtensions
{
    public static IOption<IResponse> Status(this ITestResult tr, IResponse resp, HttpStatusCode expected, [CallerMemberName] string? methodName = null)
        => tr.Eq(methodName!, resp.StatusCode, expected)
            ? Option.Value(resp)
            : Option.Error<IResponse>(resp.Content ?? "bad status");
}
```


## HashCoceCombiner (for netstandard2.0 only)
class (from .NetCore v2.2)

## Extensions
some extensions for Stream,
some extensions for collectopns,
some other extensions (string, Type)

##  Release notes
###	8.0.1.6
		update:
			refactoring Options Extensions
		note:
			changed signatures of some methods!
### 8.0.0.5
	append:
		support net8.0
		StrUtil.GetClassName()
		class ExtensionsValidation
		some methods in ExtensionsHex
		some unit-tests
	update:
		refactoring CollectionExtensions class
		refactoring Throw class

### 0.1.1.4
	append:
		support netstandard2.1
		class DicString
		Option (ala Rust)
		class RandomGenerator
		class StrUtil
		some unit-tests
	update:
		CollectionExtensions (Merge IReadOnlyDictionary)
		small fixes (in ExtensionsHex, HashCodeCombiner)

### 0.1.0.3
	update(fixed):
		added Throw.IsArgumentNullException override (for netstandard2.0)
		rename Extensions => ExtensionsHex + refactoring ToHexString()

### 0.1.0.2
	append:
		Stream.WriteAsync()
		Extensions (hex convert, IServiceProvider)
		Throw.Exception as functions
	update:
		added Throw.IsArgumentNullException override

### 0.1.0.1
	append:
		TargetFramework: net6.0
	update:
		IsArgumentNullException() can be raice 2 exeptios (ArgumentNullException & ArgumentException)
