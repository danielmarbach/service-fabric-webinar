using System;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;

public class ServiceUriBuilder
{
    public ServiceUriBuilder(string serviceInstance)
    {
        this.ServiceName = serviceInstance;
    }

    public ServiceUriBuilder(string applicationInstance, string serviceName)
    {
        this.ApplicationInstance = !applicationInstance.StartsWith("fabric:/")
            ? "fabric:/" + applicationInstance
            : applicationInstance;

        this.ServiceName = serviceName;
    }

    /// <summary>
    /// The name of the application instance that contains he service.
    /// </summary>
    public string ApplicationInstance { get; set; }

    /// <summary>
    /// The name of the service instance.
    /// </summary>
    public string ServiceName { get; set; }

    public Uri Build()
    {
        string applicationInstance = this.ApplicationInstance;

        if (String.IsNullOrEmpty(applicationInstance))
        {
            try
            {
                // the ApplicationName property here automatically prepends "fabric:/" for us
                applicationInstance = FabricRuntime.GetActivationContext().ApplicationName;
            }
            catch (InvalidOperationException)
            {
                // FabricRuntime is not available. 
                // This indicates that this is being called from somewhere outside the Service Fabric cluster.
                applicationInstance = "test";
            }
        }

        return new Uri(applicationInstance.TrimEnd('/') + "/" + this.ServiceName);
    }
}

/// <summary>
/// http://fabric/app/service/#/partitionkey/any|primary|secondary/endpoint-name/api-path
/// </summary>
public class HttpServiceUriBuilder
{
    private const short FabricSchemeLength = 8;

    public HttpServiceUriBuilder()
    {
        this.Scheme = "http";
        this.Host = "fabric";
        this.Target = HttpServiceUriTarget.Default;
    }

    public HttpServiceUriBuilder(string uri)
        : this(new Uri(uri, UriKind.Absolute))
    {
    }

    public HttpServiceUriBuilder(Uri uri)
    {
        this.Scheme = uri.Scheme;
        this.Host = uri.Host;
        this.ServiceName = new Uri("fabric:" + uri.AbsolutePath.TrimEnd('/'));

        string path = uri.Fragment.Remove(0, 2);
        string[] segments = path.Split('/');

        long int64PartitionKey;
        this.PartitionKey = Int64.TryParse(segments[0], out int64PartitionKey)
            ? new ServicePartitionKey(int64PartitionKey)
            : String.IsNullOrEmpty(segments[0])
                ? new ServicePartitionKey()
                : new ServicePartitionKey(segments[0]);

        HttpServiceUriTarget target;
        if (!Enum.TryParse<HttpServiceUriTarget>(segments[1], true, out target))
        {
            throw new ArgumentException();
        }

        this.Target = target;
        this.EndpointName = segments[2];
        this.ServicePathAndQuery = String.Join("/", segments.Skip(3));
    }

    public string Scheme { get; private set; }

    public string Host { get; private set; }

    public Uri ServiceName { get; private set; }

    public ServicePartitionKey PartitionKey { get; private set; }

    public HttpServiceUriTarget Target { get; private set; }

    public string EndpointName { get; private set; }

    public string ServicePathAndQuery { get; private set; }

    public override string ToString()
    {
        return base.ToString();
    }

    public Uri Build()
    {
        if (this.ServiceName == null)
        {
            throw new UriFormatException("Service name is null.");
        }

        UriBuilder builder = new UriBuilder();
        builder.Scheme = String.IsNullOrEmpty(this.Scheme) ? "http" : this.Scheme;
        builder.Host = String.IsNullOrEmpty(this.Host) ? "fabric" : this.Host;
        builder.Path = this.ServiceName.AbsolutePath.Trim('/') + '/';
        ;
        string partitionKey = this.PartitionKey == null || this.PartitionKey.Kind == ServicePartitionKind.Singleton
            ? String.Empty
            : this.PartitionKey.Value.ToString();

        builder.Fragment = $"/{partitionKey}/{this.Target.ToString()}/{this.EndpointName ?? String.Empty}/{this.ServicePathAndQuery ?? String.Empty}";

        return builder.Uri;
    }

    public HttpServiceUriBuilder SetHost(string host)
    {
        this.Host = host?.ToLowerInvariant();
        return this;
    }

    public HttpServiceUriBuilder SetScheme(string scheme)
    {
        this.Scheme = scheme?.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Fully-qualified service name URI: fabric:/name/of/service
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public HttpServiceUriBuilder SetServiceName(Uri serviceName)
    {
        if (serviceName != null)
        {
            if (!serviceName.IsAbsoluteUri)
            {
                throw new UriFormatException("Service URI must be an absolute URI in the form 'fabric:/name/of/service");
            }

            if (!String.Equals(serviceName.Scheme, "fabric", StringComparison.OrdinalIgnoreCase))
            {
                throw new UriFormatException("Scheme must be 'fabric'.");
            }
        }

        this.ServiceName = serviceName;

        return this;
    }

    /// <summary>
    /// Fully-qualified service name URI: fabric:/name/of/service
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public HttpServiceUriBuilder SetServiceName(string serviceName)
    {
        return this.SetServiceName(new Uri(serviceName, UriKind.Absolute));
    }

    public HttpServiceUriBuilder SetPartitionKey(string namedPartitionKey)
    {
        this.PartitionKey = new ServicePartitionKey(namedPartitionKey);
        return this;
    }

    public HttpServiceUriBuilder SetPartitionKey(long int64PartitionKey)
    {
        this.PartitionKey = new ServicePartitionKey(int64PartitionKey);
        return this;
    }

    public HttpServiceUriBuilder SetTarget(HttpServiceUriTarget target)
    {
        this.Target = target;
        return this;
    }

    public HttpServiceUriBuilder SetEndpointName(string endpointName)
    {
        this.EndpointName = endpointName;
        return this;
    }

    public HttpServiceUriBuilder SetServicePathAndQuery(string servicePathAndQuery)
    {
        this.ServicePathAndQuery = servicePathAndQuery;
        return this;
    }
}

public enum HttpServiceUriTarget
{
    /// <summary>
    /// Primary for stateful, Any for stateless.
    /// </summary>
    Default,

    /// <summary>
    /// Selects the primary replica of a stateful service.
    /// </summary>
    Primary,

    /// <summary>
    /// Selects a random secondary replica of a stateful service.
    /// </summary>
    Secondary,

    /// <summary>
    /// Selects a random replica of a stateful service or a random instance of a stateless service.
    /// </summary>
    Any
}

public class HttpServiceClientHandler : HttpClientHandler
{
    private const int MaxRetries = 5;
    private const int InitialRetryDelayMs = 25;
    private readonly Random random = new Random();

    public HttpServiceClientHandler()
    {
    }

    /// <summary>
    /// http://fabric/app/service/#/partitionkey/any|primary|secondary/endpoint-name/api-path
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();
        ResolvedServicePartition partition = null;
        HttpServiceUriBuilder uriBuilder = new HttpServiceUriBuilder(request.RequestUri);

        int retries = MaxRetries;
        int retryDelay = InitialRetryDelayMs;
        bool resolveAddress = true;

        HttpResponseMessage lastResponse = null;
        Exception lastException = null;

        while (retries-- > 0)
        {
            lastResponse = null;
            cancellationToken.ThrowIfCancellationRequested();

            if (resolveAddress)
            {
                partition = partition != null
                    ? await resolver.ResolveAsync(partition, cancellationToken)
                    : await resolver.ResolveAsync(uriBuilder.ServiceName, uriBuilder.PartitionKey, cancellationToken);

                string serviceEndpointJson;

                switch (uriBuilder.Target)
                {
                    case HttpServiceUriTarget.Default:
                    case HttpServiceUriTarget.Primary:
                        serviceEndpointJson = partition.GetEndpoint().Address;
                        break;
                    case HttpServiceUriTarget.Secondary:
                        serviceEndpointJson = partition.Endpoints.ElementAt(this.random.Next(1, partition.Endpoints.Count)).Address;
                        break;
                    case HttpServiceUriTarget.Any:
                    default:
                        serviceEndpointJson = partition.Endpoints.ElementAt(this.random.Next(0, partition.Endpoints.Count)).Address;
                        break;
                }

                string endpointUrl = JObject.Parse(serviceEndpointJson)["Endpoints"][uriBuilder.EndpointName].Value<string>();

                request.RequestUri = new Uri($"{endpointUrl.TrimEnd('/')}/{uriBuilder.ServicePathAndQuery.TrimStart('/')}", UriKind.Absolute);
            }

            try
            {
                lastResponse = await base.SendAsync(request, cancellationToken);

                if (lastResponse.StatusCode == HttpStatusCode.NotFound ||
                    lastResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    resolveAddress = true;
                }
                else
                {
                    return lastResponse;
                }
            }
            catch (TimeoutException te)
            {
                lastException = te;
                resolveAddress = true;
            }
            catch (SocketException se)
            {
                lastException = se;
                resolveAddress = true;
            }
            catch (HttpRequestException hre)
            {
                lastException = hre;
                resolveAddress = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                WebException we = ex as WebException;

                if (we == null)
                {
                    we = ex.InnerException as WebException;
                }

                if (we != null)
                {
                    HttpWebResponse errorResponse = we.Response as HttpWebResponse;

                    // the following assumes port sharing
                    // where a port is shared by multiple replicas within a host process using a single web host (e.g., http.sys).
                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (errorResponse.StatusCode == HttpStatusCode.NotFound ||
                            errorResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            // This could either mean we requested an endpoint that does not exist in the service API (a user error)
                            // or the address that was resolved by fabric client is stale (transient runtime error) in which we should re-resolve.
                            resolveAddress = true;
                        }

                        // On any other HTTP status codes, re-throw the exception to the caller.
                        throw;
                    }

                    if (we.Status == WebExceptionStatus.Timeout ||
                        we.Status == WebExceptionStatus.RequestCanceled ||
                        we.Status == WebExceptionStatus.ConnectionClosed ||
                        we.Status == WebExceptionStatus.ConnectFailure)
                    {
                        resolveAddress = true;
                    }
                }
                else
                {
                    throw;
                }
            }

            await Task.Delay(retryDelay);

            retryDelay += retryDelay;
        }

        if (lastResponse != null)
        {
            return lastResponse;
        }
        else
        {
            throw lastException;
        }
    }
}