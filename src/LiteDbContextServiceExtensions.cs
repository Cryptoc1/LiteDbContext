using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0130
namespace LiteDB.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class LiteDbContextServiceExtensions
{
    public static IServiceCollection AddLiteDbContext<[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties )] TContext>( this IServiceCollection services, Action<LiteDbOptions>? configure = default )
        where TContext : LiteDbContext
    {
        ArgumentNullException.ThrowIfNull( services );
        ArgumentNullException.ThrowIfNull( configure );

        var options = services.AddOptions<LiteDbOptions<TContext>>();
        if( configure is not null )
        {
            options.Configure( configure );
        }

        services.TryAddSingleton( serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<LiteDbOptions<TContext>>>();
            return ActivatorUtilities.CreateInstance<TContext>( serviceProvider, options.Value );
        } );

        return services;
    }
}