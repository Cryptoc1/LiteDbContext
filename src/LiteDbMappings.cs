using System.Text.Json;

namespace LiteDB;

public static class LiteDbMappings
{
    public static BsonMapper ConfigureDates( this BsonMapper mapper )
    {
        ArgumentNullException.ThrowIfNull( mapper );

        mapper.RegisterType(
            date => new BsonValue( date.ToDateTime( TimeOnly.MinValue ) ),
            value => value.IsNull ? default : DateOnly.FromDateTime( value.AsDateTime ) );

        mapper.RegisterType<DateOnly?>(
            date => date.HasValue ? new BsonValue( date.Value.ToDateTime( TimeOnly.MinValue ) ) : BsonValue.Null,
            value => value.IsNull ? default : DateOnly.FromDateTime( value.AsDateTime ) );

        return mapper;
    }

    public static BsonMapper ConfigureJson( this BsonMapper mapper )
    {
        ArgumentNullException.ThrowIfNull( mapper );

        // TODO: translate via DOM api, rather than via serialization?
        mapper.RegisterType(
            element => JsonSerializer.Deserialize( element.GetRawText() ),
            value =>
            {
#pragma warning disable IL2026,IL3050
                return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(
                    JsonSerializer.Serialize( value ) );
#pragma warning restore IL2026,IL3050
            } );

        mapper.RegisterType<JsonElement?>(
            element => element.HasValue ? JsonSerializer.Deserialize( element.Value.GetRawText() ) : BsonValue.Null,
            value =>
            {
                if( value.IsNull )
                {
                    return default;
                }

#pragma warning disable IL2026,IL3050
                return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(
                    JsonSerializer.Serialize( value ) );
#pragma warning restore IL2026,IL3050
            } );

        return mapper;
    }

    public static BsonMapper ConfigureTime( this BsonMapper mapper )
    {
        ArgumentNullException.ThrowIfNull( mapper );

        mapper.RegisterType(
            time => new BsonValue( time.Ticks ),
            value => TimeSpan.FromTicks( value.AsInt64 ) );

        mapper.RegisterType<TimeSpan?>(
            time => time.HasValue ? new( time.Value.Ticks ) : default,
            value => !value.IsNull ? new( value.AsInt64 ) : default );

        return mapper;
    }

    public static BsonMapper ConfigureUris( this BsonMapper mapper )
    {
        ArgumentNullException.ThrowIfNull( mapper );

        mapper.RegisterType(
            url => new( url.OriginalString ),
            value => new UriBuilder( value.AsString ).Uri );

        mapper.RegisterType(
            url => url is not null ? new( url.OriginalString ) : BsonValue.Null,
            value =>
            {
                if( value.IsNull )
                {
                    return default;
                }

                if( string.IsNullOrWhiteSpace( value.AsString ) )
                {
                    return default;
                }

                return new UriBuilder( value.AsString ).Uri;
            } );

        return mapper;
    }
}