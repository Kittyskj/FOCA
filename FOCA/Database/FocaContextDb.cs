using FOCA.Database.Entities;
using FOCA.Plugins;
using System;
using System.Configuration;
using System.Data.Entity;

namespace FOCA.Database
{
    public class FocaContextDb : DbContext
    {
        static FocaContextDb()
        {
            string connString = ConfigurationManager.ConnectionStrings[nameof(FocaContextDb)]?.ConnectionString;
            if (connString != null && (connString.Contains("Data Source") || connString.Contains("sqlite") || connString.Contains(".db")))
            {
                System.Data.Entity.Database.SetInitializer<FocaContextDb>(new CreateDatabaseIfNotExists<FocaContextDb>());
            }
            else
            {
                System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<FocaContextDb, Migrations.Configuration>());
            }
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<FilesItem> Files { get; set; }
        public DbSet<DomainsItem> Domains { get; set; }
        public DbSet<ComputersItem> Computers { get; set; }
        public DbSet<ComputerIPsItem> ComputerIps { get; set; }
        public DbSet<ComputerDomainsItem> ComputerDomain { get; set; }
        public DbSet<Limits> Limits { get; set; }
        public DbSet<RelationsItem> Relations { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<IPsItem> Ips { get; set; }
        public DbSet<HttpMapTypesFiles> HttpMapTypesFiles { get; set; }
        public DbSet<Plugin> Plugins { get; set; }

        public FocaContextDb() : base("name=FocaContextDb")
        { }

        public FocaContextDb(string connectionString) : base(connectionString)
        { }

        public FocaContextDb(System.Data.Common.DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        { }

        public static bool IsDatabaseAvailable(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            try
            {
                bool isSQLite = connectionString.ToLower().Contains("data source") && (connectionString.ToLower().Contains(".db") || connectionString.ToLower().Contains("sqlite"));
                if (isSQLite)
                {
                    string dsPath = null;
                    var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
                    if (builder.TryGetValue("Data Source", out object dsPathObj))
                    {
                        dsPath = dsPathObj.ToString();
                        if (!System.IO.Path.IsPathRooted(dsPath))
                        {
                            dsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dsPath);
                        }
                    }

                    bool needCreate = true;
                    if (dsPath != null && System.IO.File.Exists(dsPath))
                    {
                        var fileInfo = new System.IO.FileInfo(dsPath);
                        if (fileInfo.Length > 0)
                        {
                            needCreate = false;
                        }
                        else
                        {
                            System.IO.File.Delete(dsPath);
                        }
                    }

                    using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                    using (var context = new FocaContextDb(conn, true))
                    {
                        if (needCreate)
                        {
                            context.Database.Initialize(true);
                        }
                        else
                        {
                            context.Database.Connection.Open();
                            context.Database.Connection.Close();
                        }
                    }
                }
                else
                {
                    using (FocaContextDb context = new FocaContextDb(connectionString))
                    {
                        context.Database.CreateIfNotExists();
                        context.Database.Connection.Open();
                        context.Database.Connection.Close();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            string connString = this.Database.Connection.ConnectionString;
            if (connString != null && (connString.ToLower().Contains("data source") || connString.ToLower().Contains("sqlite") || connString.ToLower().Contains(".db")))
            {
                var sqliteConnectionInitializer = new SQLite.CodeFirst.SqliteCreateDatabaseIfNotExists<FocaContextDb>(modelBuilder);
                System.Data.Entity.Database.SetInitializer(sqliteConnectionInitializer);
            }
            base.OnModelCreating(modelBuilder);
        }
    }
}
