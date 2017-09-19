﻿

// ------------------------------------------------------------------------------------------------
// This code was generated by EntityFramework Reverse POCO Generator (http://www.reversepoco.com/).
// Created by Simon Hughes (https://about.me/simon.hughes).
//
// Do not make changes directly to this file - edit the template instead.
//
// The following connection settings were used to generate this file:
//     Configuration file:     "ComaModel\App.config"
//     Connection String Name: "ComaContext"
//     Connection String:      "Data Source=brh86eedvq.database.windows.net;Initial Catalog=MetaBank;User ID=merauser;password=**zapped**;"
// ------------------------------------------------------------------------------------------------
// Database Edition       : SQL Azure
// Database Engine Edition: Azure

// <auto-generated>
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable DoNotCallOverridableMethodsInConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantOverridenMember
// ReSharper disable UseNameofExpression
// TargetFrameworkVersion = 4.6
#pragma warning disable 1591    //  Ignore "Missing XML Comment" warning


namespace COMA
{

    #region Unit of work

    public interface IComaContext : System.IDisposable
    {
        System.Data.Entity.DbSet<Todo> Todoes { get; set; } // Todo

        int SaveChanges();
        System.Threading.Tasks.Task<int> SaveChangesAsync();
        System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken);
        System.Data.Entity.Infrastructure.DbChangeTracker ChangeTracker { get; }
        System.Data.Entity.Infrastructure.DbContextConfiguration Configuration { get; }
        System.Data.Entity.Database Database { get; }
        System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity);
        System.Collections.Generic.IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> GetValidationErrors();
        System.Data.Entity.DbSet Set(System.Type entityType);
        System.Data.Entity.DbSet<TEntity> Set<TEntity>() where TEntity : class;
        string ToString();
    }

    #endregion

    #region Database context

    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.32.0.0")]
    public partial class ComaContext : System.Data.Entity.DbContext, IComaContext
    {
        public System.Data.Entity.DbSet<Todo> Todoes { get; set; } // Todo

        static ComaContext()
        {
            System.Data.Entity.Database.SetInitializer<ComaContext>(null);
        }

        public ComaContext()
            : base("Name=ComaContext")
        {
        }

        public ComaContext(string connectionString)
            : base(connectionString)
        {
        }

        public ComaContext(string connectionString, System.Data.Entity.Infrastructure.DbCompiledModel model)
            : base(connectionString, model)
        {
        }

        public ComaContext(System.Data.Common.DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public ComaContext(System.Data.Common.DbConnection existingConnection, System.Data.Entity.Infrastructure.DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public bool IsSqlParameterNull(System.Data.SqlClient.SqlParameter param)
        {
            var sqlValue = param.SqlValue;
            var nullableValue = sqlValue as System.Data.SqlTypes.INullable;
            if (nullableValue != null)
                return nullableValue.IsNull;
            return (sqlValue == null || sqlValue == System.DBNull.Value);
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new TodoConfiguration());
        }

        public static System.Data.Entity.DbModelBuilder CreateModel(System.Data.Entity.DbModelBuilder modelBuilder, string schema)
        {
            modelBuilder.Configurations.Add(new TodoConfiguration(schema));
            return modelBuilder;
        }
    }
    #endregion

    #region POCO classes

    // Todo
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.32.0.0")]
    public class Todo
    {
        public int Id { get; set; } // ID (Primary key)
        public string Action { get; set; } // Action (length: 50)
        public bool IsDone { get; set; } // IsDone
        public System.DateTime DateStamp { get; set; } // DateStamp

        public Todo()
        {
            IsDone = false;
            DateStamp = System.DateTime.Now;
        }
    }

    #endregion

    #region POCO Configuration

    // Todo
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.32.0.0")]
    public class TodoConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Todo>
    {
        public TodoConfiguration()
            : this("dbo")
        {
        }

        public TodoConfiguration(string schema)
        {
            ToTable("Todo", schema);
            HasKey(x => x.Id);

            Property(x => x.Id).HasColumnName(@"ID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            Property(x => x.Action).HasColumnName(@"Action").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(50);
            Property(x => x.IsDone).HasColumnName(@"IsDone").HasColumnType("bit").IsRequired();
            Property(x => x.DateStamp).HasColumnName(@"DateStamp").HasColumnType("datetime2").IsRequired();
        }
    }

    #endregion

}
// </auto-generated>

