using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Http;

internal sealed class HttpService( IConfiguration configuration, ILogger<HttpService> logger ) : InfrastructureService<HttpService>( logger ), IHttpService
{
    const string ErrorMessage = "An exception occurred while trying to make an http request.";
    readonly HttpClient http = GetHttpClient( configuration );

    public async Task<Reply<T>> TryGetObjRequest<T>( string apiPath, Dictionary<string, object>? parameters = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            string url = GetQueryParameters( apiPath, parameters );
            HttpResponseMessage httpResponse = await http.GetAsync( url );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    public async Task<Reply<T>> TryPostObjRequest<T>( string apiPath, object? body = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            HttpResponseMessage httpResponse = await http.PostAsJsonAsync( apiPath, body );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    public async Task<Reply<T>> TryPutObjRequest<T>( string apiPath, object? body = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            HttpResponseMessage httpResponse = await http.PutAsJsonAsync( apiPath, body );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    public async Task<Reply<T>> TryDeleteObjRequest<T>( string apiPath, Dictionary<string, object>? parameters = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            string url = GetQueryParameters( apiPath, parameters );
            HttpResponseMessage httpResponse = await http.DeleteAsync( url );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    
    static string GetQueryParameters( string apiPath, Dictionary<string, object>? parameters )
    {
        if (parameters is null)
            return apiPath;

        NameValueCollection query = HttpUtility.ParseQueryString( string.Empty );

        foreach ( KeyValuePair<string, object> param in parameters ) 
            query[param.Key] = param.Value.ToString();

        return $"{apiPath}?{query}";
    }
    static async Task<Reply<T>> HandleHttpObjResponse<T>( HttpResponseMessage httpResponse )
    {
        if (httpResponse.IsSuccessStatusCode) {
            var httpContent = await httpResponse.Content.ReadFromJsonAsync<T>();
            return httpContent is not null
                ? Reply<T>.With( httpContent )
                : Reply<T>.None( "No data returned from http request." );
        }

        string errorContent = await httpResponse.Content.ReadAsStringAsync();
        Console.WriteLine( $"An exception was thrown during an http request : {errorContent}" );
        return Reply<T>.None( $"An exception was thrown during an http request : {errorContent}" );
    }
    
    Reply<T> HandleHttpObjException<T>( Exception e, string requestUrl )
    {
        Logger.LogError( e, e.Message );
        return Reply<T>.None( $"{ErrorMessage} : {requestUrl}" );
    }
    void SetAuthHttpHeader( string? token )
    {
        http.DefaultRequestHeaders.Authorization = !string.IsNullOrWhiteSpace( token )
            ? new System.Net.Http.Headers.AuthenticationHeaderValue( "Bearer", token )
            : null;
    }

    static HttpClient GetHttpClient( IConfiguration config ) =>
        new( new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes( 3 ) } ) {
            BaseAddress = new Uri( config["BaseUrl"] ?? throw new Exception( "Failed to get BaseUrl from IConfiguration in HttpService!" ) )
        };
}