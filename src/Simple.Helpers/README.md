#	Simple.Helpers

##	Throw

Pierian spring:
https://reubenbond.github.io/posts/dotnet-perf-tuning
or translation
https://www.lshnk.me/2019/01/26/улучшаем-производительность-в-.net-core-по-опыту-бинарного-сериализатора-hagar/

###	Using

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
			return Throw.ExceptionAsync<TResult>(ex);
		}
	}
```

##	HashCodeCombiner (for netstandard2.x only)

class (from .NetCore v2.2)

##	Extensions

some extensions for Stream,
some extensions for collections,
some other extensions (string, Type)

##	Release notes

###	8.1.0.8
		use AI analysis

	append:
		JsonMerge: string-overload contract (empty master => {}, one-side-missing passthrough), depth guard 64, ArrayBufferWriter pre-size, unknown MergeBehavior => ArgumentOutOfRangeException
		JsonConvertMs: FindDiff arrays O(n+m) via JSON-string keys (caveat: property order inside array items matters), depth guard 64
	update:
		many fixes/improvements from AI
		Throw.Exception/Exception<T>: rethrow via ExceptionDispatchInfo (keeps original stack)
		RandomGenerator (netstandard2.0): crypto RNG (RandomNumberGenerator), unbiased ranges
		HexConverter: allocation-free AsBytes(char*), int overload range validation (HiByte/LoByte/ToHexChar)
		ExtensionsValidation.IsIp: \z anchor (no trailing-newline bypass); IsIp is now an extension method
		ExtensionsValidation.IsEmail: strict ASCII (Unicode \w/\d removed — Arabic-Indic digits, CJK no longer match)
		RandomGenerator.Shuffle: XML-doc updated (crypto RNG on all target frameworks)
		DicString.ToEnumKey: where TKey : struct, Enum
		DicString: default comparer InvariantCultureIgnoreCase => OrdinalIgnoreCase (faster, culture-safe for keys)
		Throw.Exception/Exception<T>: marked [DoesNotReturn] (+ netstandard2.0 polyfill); dead code removed in byte-hex/base64 converters and JsonMerge default-cases
		JsonConverterBytesHex/Base64: catch narrowed to FormatException; dead Null-token guards removed (serializer handles null via HandleNull=false); escaped payloads documented (not decoded); hex: segmented payload (HasValueSequence) via ArrayPool rent instead of ToArray
		JsonConvertMs.ParseDoc(string): null => ArgumentNullException; DeserializeObject(string): IsNullOrEmpty (whitespace throws, consistent with span overloads); Merge self-merge (same reference) => no-op (was Collection-was-modified); dead commented ParseObj stub removed
		HexConverter.AsBytes(string): null => ArgumentNullException (was silent empty array); ExtensionsHex.GetServiceRequired: null provider/type => ArgumentNullException; ToHexString separator capacity: long math + clamp (no int-overflow from StringBuilder ctor)
		CollectionExtensions: AsString param typo souce => source; ToNameValueCollection skips null values (was NRE); Merge overloads: null args => ArgumentNullException; HashCodeCombiner (netstandard2.x): struct => sealed class (copy-mutation traps removed); Add(value, comparer): null comparer falls back to EqualityComparer<T>.Default
		RandomGenerator.GetItems: length > Int32.MaxValue => ArgumentOutOfRangeException (was OverflowException/OOM); GetItemsCore batches RNG fills (256 items per fill, unbiased rejection preserved, fast-path for single choice); HashCodeCombiner: generic Add(IEnumerable<T>) overload (no boxing through the non-generic enumerator)
		StreamExtensions: ReadToEndAsync signature Stream? => Stream (null still => ArgumentNullException, redundant ?-chain removed); all awaits use ConfigureAwait(false); WriteAsync(string): guards moved before the empty-content early return (null stream/encoding now throw even for empty content)
		JsonConvertMs.JsonOptions: frozen (MakeReadOnly) at type init — late mutations throw at the call site (was: at first serializer use); new CreateOptions() factory returns a mutable copy for custom converters
		JsonConvertMs.FindDiff: property-name comparison follows JsonNodeOptions.PropertyNameCaseInsensitive (OrdinalIgnoreCase when enabled; default unchanged); ToWrite: SkipValidation=true removed — writer validates output (valid JSON over micro-perf)
		unit-tests: 271 => 316
	removed:
		IOption, Option
	note:
		breaking:
			RandomGenerator.Random property removed (netstandard2.0)
			StreamExtensions.GetBufferSegment removed (use MemoryStream.TryGetBuffer(out ArraySegment<byte>) directly)
			JsonConvertMs CreateObject/FromObject/ParseObj throw JsonException on non-object input (empty/null input still => empty object)
			HashCodeCombiner (netstandard2.x) is now a sealed class (was mutable struct); CollectionExtensions.AsString parameter renamed souce => source; CollectionExtensions.Merge null args now throw ArgumentNullException (was NRE)

###	8.0.2.7

	append:
		json helpers [JsonConvert, JsonConverterBytesBase64, JsonConverterBytesHex, JsonMerge]
	update:
		refactoring Hex Extensions
		refactoring Options Extensions
	note:
		changed signatures of some methods!

###	8.0.1.6

	update:
		refactoring Options Extensions
	note:
		changed signatures of some methods!

###	8.0.0.5

	append:
		support net8.0
		StrUtil.GetClassName()
		class ExtensionsValidation
		some methods in ExtensionsHex
		some unit-tests
	update:
		refactoring CollectionExtensions class
		refactoring Throw class

###	0.1.1.4

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

###	0.1.0.3

	update(fixed):
		added Throw.IsArgumentNullException override (for netstandard2.0)
		rename Extensions => ExtensionsHex + refactoring ToHexString()

###	0.1.0.2

	append:
		Stream.WriteAsync()
		Extensions (hex convert, IServiceProvider)
		Throw.Exception as functions
	update:
		added Throw.IsArgumentNullException override

###	0.1.0.1

	append:
		TargetFramework: net6.0
	update:
		IsArgumentNullException() can be raice 2 exeptios (ArgumentNullException & ArgumentException)
