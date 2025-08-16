using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Reflection;
using ExpenseTracker;
using ExpenseTracker.Source;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpensesTracker
{
	class Program
	{
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });



            services.AddSingleton<IConfiguration>(_ =>
            new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory().Split("\\bin")[0], "ExpenseTracker/Source/appsettings.json"), optional: false)
            .AddEnvironmentVariables()
            .Build());

            void Configure<TConfig>(string sectionName) where TConfig : class
            {
                services
                    .AddSingleton(p => p.GetRequiredService<IOptions<TConfig>>().Value)
                    .AddOptionsWithValidateOnStart<TConfig>()
                    .BindConfiguration(sectionName)
                    .Validate(options =>
                    {
                        var results = new List<ValidationResult>();
                        var context = new ValidationContext(options);
                        if (!Validator.TryValidateObject(options, context, results, validateAllProperties: true))
                        {
                            throw new OptionsValidationException(
                                sectionName,
                                typeof(TConfig),
                                results.Select(r => $"[{sectionName}] {r.ErrorMessage}")
                            );
                        }
                        return true;
                    });
            }

            Configure<ExpensesTrackerOptions>(ExpensesTrackerOptions.SectionName);

            services.AddTransient<IExpenseTrackerActions,ExpenseTrackerActions>();

            services.AddTransient<App>();

		}

		public static async Task Main(string[] args)
		{
            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);

                using var serviceProvider = services.BuildServiceProvider();

                // entry to run app
                await serviceProvider.GetRequiredService<App>().Execute(args);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
			
		}
	}
}