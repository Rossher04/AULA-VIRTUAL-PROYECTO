using System.Text.Json.Nodes;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Models;

public enum FieldType
{
    Text,
    Number,
    Decimal,
    Password,
    Picker,        // Picker cargado desde otro endpoint (relacion / llave foranea)
    StaticChoice,  // Picker con opciones fijas en codigo (ej. dia de la semana)
    Bool,
    Date,
    Time
}

/// <summary>
/// Describe un campo del formulario generico de alta/edicion para una entidad.
/// </summary>
public class FieldConfig
{
    /// <summary>Nombre del campo tal como lo espera el JSON del backend (ej. "facultad", "nombre").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Etiqueta visible para el usuario.</summary>
    public string Label { get; set; } = string.Empty;

    public FieldType Type { get; set; } = FieldType.Text;

    public bool Required { get; set; } = true;

    /// <summary>Si es true, el campo no se muestra al editar (solo al crear). Ej: contrasena inicial.</summary>
    public bool OnlyOnCreate { get; set; } = false;

    // --- Para Type == Picker (opciones cargadas desde otro endpoint) ---
    public ApiTarget SourceApiTarget { get; set; }
    public string SourceEndpoint { get; set; } = string.Empty;
    public Func<JsonObject, string>? OptionLabel { get; set; }
    public string ValueField { get; set; } = "id";

    // --- Para Type == StaticChoice (opciones fijas) ---
    public List<(string Value, string Label)>? StaticOptions { get; set; }

    public string? DefaultTextValue { get; set; }
}

/// <summary>
/// Describe una entidad completa (endpoint + campos) para reutilizar una sola
/// pantalla de lista y una sola pantalla de formulario en toda la app.
/// </summary>
public class EntityConfig
{
    public string Title { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public ApiTarget ApiTarget { get; set; }
    public List<FieldConfig> Fields { get; set; } = new();

    /// <summary>Texto principal a mostrar por cada fila de la lista.</summary>
    public Func<JsonObject, string> DisplayTitle { get; set; } = _ => "(sin nombre)";

    /// <summary>Texto secundario/detalle opcional por fila.</summary>
    public Func<JsonObject, string>? DisplaySubtitle { get; set; }

    /// <summary>
    /// Si se define, el boton "+" de la lista abre esta pantalla en lugar del
    /// formulario generico. Se usa para Docentes/Estudiantes, cuya alta requiere
    /// primero crear el usuario en la API de Login y luego el registro en Aula.
    /// </summary>
    public Func<Page>? CustomCreatePage { get; set; }
}
