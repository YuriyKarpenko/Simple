# Simple.Configuration

Easy way to use JSON and Envirement parameters, which used like a MS in "Hosting" flow

##	Using
```cs
    var builder = new ConfigurationBuilder()
        .SetBasePath(context.HostingEnvironment.ContentRootPath)
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

    var configuration = builder.Build();

	Locator.Setup().AddConst<IConfiguration>(configuration);
...
    var configuration = Locator.Current.GetService<IConfiguration>();
	var myConfig = configuration.Json.GetValue("some section").ToObject<MyConfig>();
```

##  Release notes
0.1.0	
	start
