﻿using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Amazon.S3;
using EMG.Tools.EnsureUnique.Concurrency;
using EMG.Tools.EnsureUnique.ProcessRunners;
using EMG.Tools.EnsureUnique.TokenGenerators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMG.Tools.EnsureUnique
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand("Execute a program and ensure there are no concurrent executions")
            {
                new RunDotNetCommand(),
                new RunExeCommand()
            };

            rootCommand.AddGlobalOption(CommonOptions.VerboseOption);

            await new CommandLineBuilder(rootCommand)
                        .UseHost(_ => Host.CreateDefaultBuilder(), ConfigureHost)
                        .UseDefaults()
                        .Build()
                        .InvokeAsync(args)
                        .ConfigureAwait(false);
        }

        private static void ConfigureHost(IHostBuilder host)
        {
            host.ConfigureLogging(ConfigureLogging);

            host.ConfigureServices(ConfigureServices);
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
        {
            if (context.TryGetOptionValue(CommonOptions.VerboseOption, out LogLevel logLevel))
            {
                logging.SetMinimumLevel(logLevel);
            }
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<TokenOptions>(options =>
            {
                if (context.TryGetOptionValue(CommonOptions.TokenOption, out string? token))
                {
                    options.Token = token;
                }
            });

            services.Configure<S3ConcurrencyServiceOptions>(context.Configuration.GetSection("DotNetEnsureUnique").GetSection("S3"));

            services.Configure<S3ConcurrencyServiceOptions>(options =>
            {
                if (context.TryGetOptionValue(CommonOptions.BucketNameOption, out string? bucketName))
                {
                    options.BucketName = bucketName;
                }

                if (context.TryGetOptionValue(CommonOptions.FilePrefixOption, out string? filePrefix))
                {
                    options.FilePrefix = filePrefix;
                }
            });

            services.AddSingleton<IProcessExecutor, DefaultProcessExecutor>();

            services.AddSingleton<IProcessRunner, ProcessRunner>();

            services.AddSingleton<IExecutionTokenGenerator>(sp =>
            {
                var md5 = sp.GetRequiredService<MD5ExecutionTokenGenerator>();

                var options = sp.GetRequiredService<IOptions<TokenOptions>>();

                return new FixedTokenExecutionTokenGeneratorAdapter(md5, options);
            });

            services.AddSingleton<MD5ExecutionTokenGenerator>();

            services.AddSingleton<IConcurrencyService, S3ConcurrencyService>();

            services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());

            services.AddAWSService<IAmazonS3>();
        }
    }
}
