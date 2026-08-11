using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AulaVirtualApp.Services;

/// <summary>
/// Resultado generico de una llamada a la API: si fue exitosa, los datos (JsonNode)
/// y, si fallo, un mensaje de error listo para mostrar al usuario.
/// </summary>
public class ApiResult
{
    public bool Ok { get; set; }
    public int StatusCode { get; set; }
    public JsonNode? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Cliente HTTP muy simple, pensado para el patron CRUD generico de la app:
/// todas las entidades (carreras, semestres, cursos, docentes, etc.) se leen/escriben
/// como JsonObject en lugar de clases fuertemente tipadas, porque el shape de cada
/// entidad ya viene definido por los serializers del backend Django.
/// </summary>
public static class ApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private static string BuildUrl(ApiTarget target, string endpoint)
    {
        var baseUrl = ApiConfig.BaseUrlFor(target);
        endpoint = endpoint.TrimStart('/');
        if (!endpoint.EndsWith("/") && !endpoint.Contains("?"))
            endpoint += "/";
        return baseUrl + endpoint;
    }

    private static HttpRequestMessage NewRequest(HttpMethod method, string url, bool authorize)
    {
        var request = new HttpRequestMessage(method, url);
        if (authorize && !string.IsNullOrEmpty(SessionService.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SessionService.Token);
        }
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static async Task<ApiResult> SendAsync(HttpRequestMessage request)
    {
        var result = new ApiResult();
        try
        {
            using var response = await _http.SendAsync(request);
            result.StatusCode = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();

            JsonNode? node = null;
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    node = JsonNode.Parse(body);
                }
                catch (JsonException)
                {
                    // El backend no devolvio JSON (por ejemplo un error 500 con HTML de Django debug)
                    result.Ok = false;
                    result.ErrorMessage = $"Respuesta inesperada del servidor (HTTP {result.StatusCode}).";
                    return result;
                }
            }

            result.Data = node;

            if (response.IsSuccessStatusCode)
            {
                result.Ok = true;
            }
            else
            {
                result.Ok = false;
                result.ErrorMessage = ExtractErrorMessage(node, result.StatusCode);
            }
        }
        catch (TaskCanceledException)
        {
            result.Ok = false;
            result.ErrorMessage = "Tiempo de espera agotado. Verifica la IP/puerto configurados y que el servidor este corriendo.";
        }
        catch (HttpRequestException ex)
        {
            result.Ok = false;
            result.ErrorMessage = $"No se pudo conectar con el servidor. Revisa la configuracion de la API.\n({ex.Message})";
        }
        catch (Exception ex)
        {
            result.Ok = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static string ExtractErrorMessage(JsonNode? node, int statusCode)
    {
        if (node == null)
            return $"Error del servidor (HTTP {statusCode}).";

        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("detail", out var detail) && detail != null)
                return detail.ToString();

            // DRF suele devolver { "campo": ["mensaje"] } en errores de validacion
            var parts = new List<string>();
            foreach (var kv in obj)
            {
                if (kv.Value is JsonArray arr)
                {
                    foreach (var item in arr)
                        parts.Add($"{kv.Key}: {item}");
                }
                else if (kv.Value != null)
                {
                    parts.Add($"{kv.Key}: {kv.Value}");
                }
            }
            if (parts.Count > 0)
                return string.Join("\n", parts);
        }

        return $"Error del servidor (HTTP {statusCode}).";
    }

    public static async Task<ApiResult> GetAsync(ApiTarget target, string endpoint, bool authorize = true)
    {
        var url = BuildUrl(target, endpoint);
        var request = NewRequest(HttpMethod.Get, url, authorize);
        return await SendAsync(request);
    }

    public static async Task<ApiResult> PostAsync(ApiTarget target, string endpoint, JsonObject body, bool authorize = true)
    {
        var url = BuildUrl(target, endpoint);
        var request = NewRequest(HttpMethod.Post, url, authorize);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        return await SendAsync(request);
    }

    public static async Task<ApiResult> PutAsync(ApiTarget target, string endpoint, JsonObject body, bool authorize = true)
    {
        var url = BuildUrl(target, endpoint);
        var request = NewRequest(HttpMethod.Put, url, authorize);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        return await SendAsync(request);
    }

    public static async Task<ApiResult> DeleteAsync(ApiTarget target, string endpoint, bool authorize = true)
    {
        var url = BuildUrl(target, endpoint);
        var request = NewRequest(HttpMethod.Delete, url, authorize);
        return await SendAsync(request);
    }
}
