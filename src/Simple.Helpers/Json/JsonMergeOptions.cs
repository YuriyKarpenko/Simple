namespace Simple.Helpers;

public enum MergeBehavior
{
    /// <summary> Try to merge. </summary>
    Merge,

    /// <summary> Replace from addon. </summary>
    Replace,

    /// <summary> Leave as master. </summary>
    Ignore,
}

/*
I: какой был смысл "MergeBehavior ArrayBehavior" (в классе JsonMergeOptions) переделывать в свойство ?
AI: Это и есть суть дефекта #10. Причина:

1. Field initializer (= MergeBehavior.Replace) выполняется только в конструкторе. new JsonMergeOptions() → Replace ✓, но default(JsonMergeOptions) (а также new JsonMergeOptions[n], default<T> в дженериках, незаполненный stackalloc/struct-copy нулей) конструктор не вызывает → все поля обнуляются → ArrayBehavior = 0 = MergeBehavior.Merge → тихо «Append» вместо «Replace». Это глушило ошибку потребителя: неинициализированные опции вели себя иначе, чем Default.
2. У обычного поля в C# нет состояния «не задано» — нулевое значение и есть значение. Отличить «пользователь явно выбрал Merge (=0)» от «структура обнулена» невозможно: биты одинаковые.
3. Свойство с MergeBehavior?-бэкендом создаёт третий смысл: null = «не задано» → геттер подставляет Replace. Обнуленное состояние default(T) читается как intended default, а явный opts.ArrayBehavior = MergeBehavior.Merge — сохраняется и уважается.

 Альтернативы отклонены:
Порядок enum (Replace=0) — сломал бы ObjectBehavior: оба поля делят один enum, но им нужны противоположные нулевые дефолты (массив → Replace, объект → Merge).
Нормализация при чтении в HandleArray — убила бы явный Merge.
 Цена:
структура +8 байт (MergeBehavior? = nullable int), геттер/сеттер вместо прямого поля (для options-структуры не hot-path), публичная сигнатура для потребителей не изменилась — source-compatible. Тест Default_Equals_DefaultStruct закрепляет равенство default == Default.
*/
public struct JsonMergeOptions()
{
    public static readonly JsonMergeOptions Default = new();

    private MergeBehavior? _arrayBehavior;

    /// <summary> Array handling; default is <see cref="MergeBehavior.Replace"/> — same for <c>default(JsonMergeOptions)</c>. </summary>
    public MergeBehavior ArrayBehavior
    {
        get => _arrayBehavior ?? MergeBehavior.Replace;
        set => _arrayBehavior = value;
    }

    /// <summary> false = exclude property, true = keep property (default = false). </summary>
    public bool KeepNullValue;

    public MergeBehavior ObjectBehavior;
}