using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Http;

public interface IHttpService
{
    public Task<Reply<T>> TryGetObjRequest<T>( string apiPath, Dictionary<string, object>? parameters = null, string? authToken = null );
    public Task<Reply<T>> TryPostObjRequest<T>( string apiPath, object? body = null, string? authToken = null );
    public Task<Reply<T>> TryPutObjRequest<T>( string apiPath, object? body = null, string? authToken = null );
    public Task<Reply<T>> TryDeleteObjRequest<T>( string apiPath, Dictionary<string, object>? parameters = null, string? authToken = null );

    public static Dictionary<string, object> QueryParameters( string n1, object o1 ) => new() {
        { n1, o1 }
    };
    public static Dictionary<string, object> QueryParameters( string n1, string n2, object o1, object o2 ) => new() {
        { n1, o1 },
        { n2, o2 }
    };
    public static Dictionary<string, object> QueryParameters( string n1, string n2, string n3, object o1, object o2, object o3 ) => new() {
        { n1, o1 },
        { n2, o2 },
        { n3, o3 }
    };
    public static Dictionary<string, object> QueryParameters( string n1, string n2, string n3, string n4, object o1, object o2, object o3, object o4 ) => new() {
        { n1, o1 },
        { n2, o2 },
        { n3, o3 },
        { n4, o4 }
    };
    public static Dictionary<string, object> QueryParameters( string n1, string n2, string n3, string n4, string n5, object o1, object o2, object o3, object o4, object o5 ) => new() {
        { n1, o1 },
        { n2, o2 },
        { n3, o3 },
        { n4, o4 },
        { n5, o5 }
    };
    public static Dictionary<string, object> QueryParameters( string n1, string n2, string n3, string n4, string n5, string n6, object o1, object o2, object o3, object o4, object o5, object o6  ) => new() {
        { n1, o1 },
        { n2, o2 },
        { n3, o3 },
        { n4, o4 },
        { n5, o5 },
        { n6, o6 }
    };
}