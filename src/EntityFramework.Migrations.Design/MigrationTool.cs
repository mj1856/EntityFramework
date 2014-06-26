// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public class MigrationTool
    {
        public void CreateMigration(string[] args)
        {
            var configuration = new Configuration();
            configuration.AddCommandLine(args);

            CreateMigration(configuration.GetSubKey("create"));
        }

        public void CreateMigration(IConfiguration configuration)
        {
            var migrationName = configuration.Get("MigrationName");
            using (var context = CreateContext(configuration))
            {
                var scaffolder = new MigrationScaffolder(
                    context.Configuration,
                    context.Configuration.Services.ServiceProvider.GetService<MigrationAssembly>(),
                    context.Configuration.Services.ServiceProvider.GetService<ModelDiffer>(),
                    new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator()));

                scaffolder.ScaffoldMigration(migrationName);
            }
        }

        public void ListMigrations(IConfiguration configuration)
        {
            using (var context = CreateContext(configuration))
            {
                var migrator = (Migrator)context.Configuration.Services.ServiceProvider.GetService(typeof(Migrator));

                migrator.GetDatabaseMigrations();
            }
        }

        public void UpdateDatabase(IConfiguration configuration)
        {
            using (var context = CreateContext(configuration))
            {
                var migrator = (Migrator)context.Configuration.Services.ServiceProvider.GetService(typeof(Migrator));

                migrator.UpdateDatabase();
            }
            
        }

        public void GenerateSqlToUpdateDatabase(IConfiguration configuration)
        {
            using (var context = CreateContext(configuration))
            {
                var migrator = (Migrator)context.Configuration.Services.ServiceProvider.GetService(typeof(Migrator));

                migrator.GenerateUpdateDatabaseSql();
            }
        }

        public DbContext CreateContext(IConfiguration configuration)
        {
            var assemblyName = configuration.Get("Assembly");
            var contextTypeName = configuration.Get("ContextType");

            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            var type = assembly.GetType(contextTypeName);

            return (DbContext)Activator.CreateInstance(type);
        }
    }
}
