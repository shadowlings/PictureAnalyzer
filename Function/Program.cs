using PictureAnalyzer.Common;
using PictureAnalyzer.Data;
using PictureAnalyzer.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        var configuration = s.BuildServiceProvider().GetService<IConfiguration>();

        s.AddDbContext<DBContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        s.AddSingleton<IConfiguration>(configuration);
        s.AddScoped<AnalysisService>();
        s.AddScoped<AzureBlobService>();
    })
    .Build();



host.Run();
