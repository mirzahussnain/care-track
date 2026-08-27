using CareTrack.DemoSeeder;

return await DemoSeederApplication.RunAsync(
    args,
    () => Environment.GetEnvironmentVariable(
        DemoSeederApplication.ConnectionStringEnvironmentVariable),
    Console.In,
    Console.Out,
    Console.Error);
