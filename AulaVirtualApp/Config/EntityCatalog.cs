using AulaVirtualApp.Models;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Config;

/// <summary>
/// Aqui se define, en un solo lugar, la forma de cada entidad academica que el
/// administrador puede administrar. Cada EntityConfig alimenta tanto la pantalla
/// de lista (EntityListPage) como el formulario generico de alta/edicion (EntityFormPage).
///
/// Importante: los modelos de la API "aula" usan nombres de llave primaria propios
/// (ej. "id_facultad", "id_carrera") en vez de "id", porque el diseno de base de datos
/// (ver Base de datos general.sql) define esos nombres de columna explicitamente. Por
/// eso cada EntityConfig fija IdField, y cada FieldConfig de tipo Picker fija ValueField
/// con el nombre de PK real de la entidad de la que carga sus opciones.
/// </summary>
public static class EntityCatalog
{
    public static EntityConfig Instituciones() => new()
    {
        Title = "Instituciones",
        Endpoint = "instituciones/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_institucion",
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => o.GetStr("dominio"),
        Fields = new()
        {
            new FieldConfig { Key = "nombre", Label = "Nombre", Type = FieldType.Text },
            new FieldConfig { Key = "dominio", Label = "Dominio", Type = FieldType.Text },
        }
    };

    public static EntityConfig Facultades() => new()
    {
        Title = "Facultades",
        Endpoint = "facultades/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_facultad",
        DisplayTitle = o => o.GetStr("nombre"),
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre de la facultad", Type = FieldType.Text },
        }
    };

    public static EntityConfig Carreras() => new()
    {
        Title = "Carreras",
        Endpoint = "carreras/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_carrera",
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => o.GetStr("facultad_nombre"),
        Fields = new()
        {
            new FieldConfig
            {
                Key = "facultad", Label = "Facultad", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "facultades/", ValueField = "id_facultad",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre de la carrera", Type = FieldType.Text },
        }
    };

    public static EntityConfig Semestres() => new()
    {
        Title = "Semestres",
        Endpoint = "semestres/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_semestre",
        DisplayTitle = o => $"Semestre {o.GetStr("numero")}",
        Fields = new()
        {
            new FieldConfig { Key = "numero", Label = "Numero de semestre", Type = FieldType.Number },
        }
    };

    public static EntityConfig CarreraSemestres() => new()
    {
        Title = "Asignar semestre a carrera",
        Endpoint = "carrera-semestres/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_carrera_semestre",
        DisplayTitle = o => $"{o.GetStr("carrera_nombre")} - Semestre {o.GetStr("semestre_numero")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "carrera", Label = "Carrera", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "carreras/", ValueField = "id_carrera",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "semestre", Label = "Semestre", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "semestres/", ValueField = "id_semestre",
                OptionLabel = o => $"Semestre {o.GetStr("numero")}"
            },
        }
    };

    public static EntityConfig Cursos() => new()
    {
        Title = "Cursos",
        Endpoint = "cursos/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_curso",
        DisplayTitle = o => $"{o.GetStr("codigo")} - {o.GetStr("nombre")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "codigo", Label = "Codigo del curso", Type = FieldType.Text },
            new FieldConfig { Key = "nombre", Label = "Nombre del curso", Type = FieldType.Text },
        }
    };

    public static EntityConfig Pensum() => new()
    {
        Title = "Asignar curso a semestre por carrera",
        Endpoint = "pensum/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_pensum",
        DisplayTitle = o => o.GetStr("curso_nombre"),
        DisplaySubtitle = o => $"{o.GetStr("carrera_nombre")} - Semestre {o.GetStr("semestre_numero")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "carrera_semestre", Label = "Carrera / Semestre", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "carrera-semestres/", ValueField = "id_carrera_semestre",
                OptionLabel = o => $"{o.GetStr("carrera_nombre")} - Semestre {o.GetStr("semestre_numero")}"
            },
            new FieldConfig
            {
                Key = "curso", Label = "Curso", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "cursos/", ValueField = "id_curso",
                OptionLabel = o => $"{o.GetStr("codigo")} - {o.GetStr("nombre")}"
            },
        }
    };

    public static EntityConfig Docentes() => new()
    {
        Title = "Catedraticos",
        Endpoint = "docentes/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_docente",
        DisplayTitle = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")}",
        DisplaySubtitle = o => $"id_usuario: {o.GetStr("id_usuario")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre", Type = FieldType.Text },
            new FieldConfig { Key = "apellido", Label = "Apellido", Type = FieldType.Text },
            new FieldConfig
            {
                Key = "id_usuario", Label = "ID de usuario (login)", Type = FieldType.Number,
                OnlyOnCreate = true
            },
        }
    };

    public static EntityConfig Estudiantes() => new()
    {
        Title = "Alumnos",
        Endpoint = "estudiantes/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_estudiante",
        DisplayTitle = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")}",
        DisplaySubtitle = o => $"Carne: {o.GetStr("carne")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre", Type = FieldType.Text },
            new FieldConfig { Key = "apellido", Label = "Apellido", Type = FieldType.Text },
            new FieldConfig { Key = "carne", Label = "Carne", Type = FieldType.Text },
            new FieldConfig
            {
                Key = "carrera", Label = "Carrera", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "carreras/", ValueField = "id_carrera",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "id_usuario", Label = "ID de usuario (login)", Type = FieldType.Number,
                OnlyOnCreate = true
            },
        }
    };

    public static EntityConfig Ciclos() => new()
    {
        Title = "Ciclos academicos",
        Endpoint = "ciclos/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_ciclo",
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => $"{o.GetStr("fecha_inicio")} a {o.GetStr("fecha_fin")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre del ciclo (ej. 2026-2)", Type = FieldType.Text },
            new FieldConfig { Key = "fecha_inicio", Label = "Fecha de inicio", Type = FieldType.Date },
            new FieldConfig { Key = "fecha_fin", Label = "Fecha de fin", Type = FieldType.Date },
        }
    };

    public static EntityConfig Aulas() => new()
    {
        Title = "Aulas fisicas",
        Endpoint = "aulas/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_aula",
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => $"Capacidad: {o.GetStr("capacidad")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre / codigo del aula", Type = FieldType.Text },
            new FieldConfig { Key = "capacidad", Label = "Capacidad", Type = FieldType.Number, Required = false },
        }
    };

    public static EntityConfig Modulos() => new()
    {
        Title = "Modulos horarios",
        Endpoint = "modulos/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_modulo",
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => $"{o.GetStr("hora_inicio")} - {o.GetStr("hora_fin")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id_institucion",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre del modulo", Type = FieldType.Text },
            new FieldConfig { Key = "hora_inicio", Label = "Hora de inicio (HH:MM)", Type = FieldType.Time },
            new FieldConfig { Key = "hora_fin", Label = "Hora de fin (HH:MM)", Type = FieldType.Time },
        }
    };

    public static EntityConfig Horarios() => new()
    {
        Title = "Horarios",
        Endpoint = "horarios/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_horario",
        DisplayTitle = o => $"Horario #{o.GetStr("id_horario")} - {o.GetStr("dia")}",
        DisplaySubtitle = o => $"modulo: {o.GetStr("modulo")}  aula: {o.GetStr("aula")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "modulo", Label = "Modulo", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "modulos/", ValueField = "id_modulo",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "aula", Label = "Aula", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "aulas/", ValueField = "id_aula",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "dia", Label = "Dia", Type = FieldType.StaticChoice,
                StaticOptions = new()
                {
                    ("LUNES", "Lunes"), ("MARTES", "Martes"), ("MIERCOLES", "Miercoles"),
                    ("JUEVES", "Jueves"), ("VIERNES", "Viernes"), ("SABADO", "Sabado")
                }
            },
        }
    };

    public static EntityConfig Secciones() => new()
    {
        Title = "Asignar catedratico a curso",
        Endpoint = "secciones/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_seccion",
        DisplayTitle = o => $"Seccion #{o.GetStr("id_seccion")}",
        DisplaySubtitle = o => $"pensum:{o.GetStr("pensum")}  docente:{o.GetStr("docente")}  horario:{o.GetStr("horario")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "pensum", Label = "Curso (pensum)", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "pensum/", ValueField = "id_pensum",
                OptionLabel = o => $"{o.GetStr("curso_nombre")} ({o.GetStr("carrera_nombre")} - Sem {o.GetStr("semestre_numero")})"
            },
            new FieldConfig
            {
                Key = "ciclo", Label = "Ciclo academico", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "ciclos/", ValueField = "id_ciclo",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "docente", Label = "Catedratico", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "docentes/", ValueField = "id_docente",
                OptionLabel = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")}"
            },
            new FieldConfig
            {
                Key = "horario", Label = "Horario", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "horarios/", ValueField = "id_horario",
                OptionLabel = o => $"#{o.GetStr("id_horario")} - {o.GetStr("dia")}"
            },
        }
    };

    public static EntityConfig EstudianteSecciones() => new()
    {
        Title = "Asignar alumno a curso",
        Endpoint = "estudiante-secciones/",
        ApiTarget = ApiTarget.Aula,
        IdField = "id_estudiante_seccion",
        DisplayTitle = o => $"Inscripcion #{o.GetStr("id_estudiante_seccion")}",
        DisplaySubtitle = o => $"estudiante:{o.GetStr("estudiante")}  seccion:{o.GetStr("seccion")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "estudiante", Label = "Alumno", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "estudiantes/", ValueField = "id_estudiante",
                OptionLabel = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")} ({o.GetStr("carne")})"
            },
            new FieldConfig
            {
                Key = "seccion", Label = "Seccion (curso asignado a catedratico)", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "secciones/", ValueField = "id_seccion",
                OptionLabel = o => $"Seccion #{o.GetStr("id_seccion")}"
            },
        }
    };
}
