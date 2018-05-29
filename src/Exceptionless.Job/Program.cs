﻿using System;
using Foundatio.Jobs.Commands;
using System.Collections.Generic;
using Serilog;
using Exceptionless.Insulation.Jobs;
using Foundatio.Jobs;

namespace Exceptionless.Job {
    public class Program {
        public static int Main(string[] args) {
            IServiceProvider serviceProvider = null;

            try {
                var cancellationToken = JobRunner.GetShutdownCancellationToken();
                serviceProvider = JobServiceProvider.GetServiceProvider(cancellationToken);

                int result = JobCommands.Run(args, serviceProvider, app => {
                    app.JobConfiguration.Assemblies = new List<string> { "Exceptionless.Core", "Foundatio" };
                });

                return result;
            } catch (Exception ex) {
                Log.Fatal(ex, "Job terminated unexpectedly");
                return 1;
            } finally {
                Log.CloseAndFlush();
                if (serviceProvider is IDisposable disposable)
                    disposable.Dispose();

                ExceptionlessClient.Default.ProcessQueue();
            }
        }
    }
}
