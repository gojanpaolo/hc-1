using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services
            .AddPooledDbContextFactory<ApplicationDbContext>(_ => _.UseSqlite("Data Source=conferences.db"))
            .AddGraphQLServer()
            .AddQueryType<Query>();

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext().Database.EnsureCreated();

            app.UseRouting();

            app.UseEndpoints(_ => _.MapGraphQL());
        }
    }

    public class Query
    {
        [UseApplicationDbContext]
        //public Task<List<Speaker>> GetSpeakers([ScopedService] ApplicationDbContext context) => context.Speakers.ToListAsync();
        public IQueryable<Speaker> GetSpeakers([ScopedService] ApplicationDbContext context) => context.Speakers;
    }

    public static class ObjectFieldDescriptorExtensions
    {
        public static IObjectFieldDescriptor UseDbContext<TDbContext>(
            this IObjectFieldDescriptor descriptor)
            where TDbContext : DbContext =>
            descriptor.UseScopedService(
                create: s => s.GetRequiredService<IDbContextFactory<TDbContext>>().CreateDbContext(),
                disposeAsync: (s, c) => c.DisposeAsync());
    }

    public class UseApplicationDbContextAttribute : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(
            IDescriptorContext context,
            IObjectFieldDescriptor descriptor,
            MemberInfo member) =>
            descriptor.UseDbContext<ApplicationDbContext>();
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Speaker> Speakers { get; set; }
    }

    public class Speaker
    {
        public int Id { get; set; }
    }
}
