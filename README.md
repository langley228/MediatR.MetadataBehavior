# MediatR.MetadataBehavior

MediatR.MetadataBehavior 是一個基於 MediatR 的擴展，用於實現基於 Metadata 的行為處理流程。此專案提供了一種靈活的方式，通過使用屬性來定義和組織行為管道。

## 功能特性

- **Metadata 驅動的行為處理**：使用 `MetadataAttribute` 和 `RequestAttribute` 定義行為。
- **自訂行為工廠**：支持通過 `IMetadataBehaviorFactory` 和 `DefaultMetadataBehaviorFactory` 自訂行為處理邏輯。
- **依賴注入支持**：通過 `ServiceCollectionHelper` 自動註冊行為和工廠。
- **靈活的行為組織**：支持多層行為管道，並可設置默認行為。

## 使用方法

### 1. 定義行為屬性

使用 `RequestAttribute`、`MetadataAttribute`、`DefaultMetadataAttribute` 定義行為。例如：

#### 範例 1：直接定義行為
```csharp
[Request]
[DefaultMetadata(typeof(LoggingBehavior<,>))]
[Metadata(typeof(ValidationBehavior<,>), isDefault: false)]
public class DefaultRequest : IRequest<MyResponse>
{
}
```

#### 範例 2：使用 Metadata 類型
```csharp
[Request(metadataType: typeof(RequestMetadata))]
public class DefaultRequest : IRequest<MyResponse>
{
}

[DefaultMetadata(typeof(LoggingBehavior<,>))]
[Metadata(typeof(ValidationBehavior<,>), isDefault: false)]
public class RequestMetadata
{
}
```

#### 範例 3：自訂行為工廠
```csharp
[Request(
        behaviorFactoryGenericType: typeof(CustomBehaviorFactory<,>),
        metadataType: typeof(RequestMetadata),
        canUseDefault: true)]
public class CustomRequest : IRequest<MyResponse>
{
    public string[] BehaviorNames { get; set; }
}

[DefaultMetadata(typeof(LoggingBehavior<,>))]
[Metadata(typeof(ValidationBehavior<,>), isDefault: false)]
public class RequestMetadata
{
}
```

---

### 2. 註冊行為

使用 `ServiceCollectionHelper` 註冊行為：

```csharp
// 註冊 MediatR  
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 註冊 Metadata 行為  
services.AddMediatRMetadataBehavior(Assembly.GetExecutingAssembly());

```

---

### 3. 實現自訂行為

實現 `IMetadataBehavior<TRequest, TResponse>` 接口來定義行為邏輯：

```csharp
public class LoggingBehavior<TRequest, TResponse> : 
    IMetadataBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Logging] Handling request: {typeof(TRequest).Name}");
        var response = await next();
        Console.WriteLine($"[Logging] Finished handling request: {typeof(TRequest).Name}");
        return response;
    }
}
```

---

### 4. 使用行為工廠

如果需要自訂行為工廠，繼承 `MetadataBehaviorFactory<TRequest, TResponse>` 並實現 `MappingBehaviors` 方法。例如：

```csharp
public class CustomBehaviorFactory<TRequest, TResponse> :
    MetadataBehaviorFactory<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public CustomBehaviorFactory(
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task<TResponse> MapAndExecuteBehaviors(
        TRequest request,
        IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
        Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next, 
        CancellationToken cancellationToken)
    {
        string[] behaviorNames = null;

        // 自訂流程：檢查請求是否為 CustomRequest
        if (request is CustomRequest customRequest)
        {
            behaviorNames = customRequest.BehaviorNames;
        }

        // 根據行為名稱篩選對應的行為
        var findBehaviors = behaviorNames.Select(name => 
            behaviors.FirstOrDefault(b => GetBehaviorName(b.GetType()) == name));

        // 執行篩選後的行為集合
        return next(findBehaviors, cancellationToken);
    }

    private string GetBehaviorName(Type type)
    {
        // 如果是泛型類型，去除泛型參數部分
        if (type.IsGenericType)
            return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}";
        else
            return type.Name;
    }
}
```