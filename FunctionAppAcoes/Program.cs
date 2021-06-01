using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Worker.Configuration;
using FunctionAppAcoes.Data;

namespace FunctionAppAcoes
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<AcoesContext>(
                        options => options.UseSqlServer(
                            Environment.GetEnvironmentVariable("BaseAcoes_Connection")));
                })
                .Build();

            host.Run();
        }
    }
}