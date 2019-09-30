# Solinosis (Σωλήνωσης)

Solinosis (from Σωλήνωσης, Greek for 'Pipe') aims to be a simple Inter Process Communication library to provide a duplex comunication between processes written in .NET Standard or .NET Core. Under development.


## Server Usage
As it is supposed to be used from .NET Standard or .NET Core projects, it can be used with Dependency Injection.
Eg.:
```csharp
public class TestService: ITestService
{
   public string TestCall(string arg)
   {
      Console.WriteLine($"TestCall called: {arg}");
      return $"TestCall received: {arg}";
   }
}

private static void Main(string[] args)
{
   var servicesCollection = new ServiceCollection();
   servicesCollection.AddNamedPipeServer("KKlapeto")
      .AddService<ITestService, TestService>()
      .AddCallback<ITestCallbackService>();

   var serverHost = new NamedPipeServerHost(servicesCollection.BuildServiceProvider());
   serverHost.Start();
   var reps = serverHost.GetCallbackProxy<ITestCallbackService>().TestCallback("Hello");
   Console.Read();
}
```

## Client Usage
Similar to Server the client can be initialised:
```csharp
public class Callback : ITestCallbackService
{
    public string TestCallback(string arg)
    {
        Console.WriteLine($"Callback: {arg}");
        return $"Callback: {arg}";
    }
}

private static void Main(string[] args)
{
    var servicesCollection = new ServiceCollection();
    servicesCollection.AddNamedPipeClient("KKlapeto")
        .AddService<ITestService>()
        .AddCallbackService<ITestCallbackService>(provider => new Callback());

    var client = new NamedPipeClient(servicesCollection.BuildServiceProvider());
    client.Connect();
    var res = client.GetServiceProxy<ITestService>().TestCall("Hahaha");
    Console.Read();
}
```

## License
MIT
