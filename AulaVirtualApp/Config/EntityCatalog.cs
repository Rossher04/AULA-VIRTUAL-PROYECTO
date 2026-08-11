using AulaVirtualApp.Models;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Config;

/// <summary>
/// Aqui se define, en un solo lugar, la forma de cada entidad academica que el
/// administrador puede administrar. Cada EntityConfig alimenta tanto la pantalla
/// de lista (EntityListPage) como el formulario generico de alta/edicion (EntityFormPage).
/// </summary>
public static class EntityCatalog
{
    public static EntityConfig Instituciones() => new()
    {
        Title = "Instituciones",
        Endpoint = "instituciones/",
        ApiTarget = ApiTarget.Aula,
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
        DisplayTitle = o => o.GetStr("nombre"),
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
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
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => o.GetStr("facultad_nombre"),
        Fields = new()
        {
            new FieldConfig
            {
                Key = "facultad", Label = "Facultad", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "facultades/", ValueField = "id",
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
        DisplayTitle = o => $"{o.GetStr("carrera_nombre")} - Semestre {o.GetStr("semestre_numero")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "carrera", Label = "Carrera", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "carreras/", ValueField = "id",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "semestre", Label = "Semestre", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "semestres/", ValueField = "id",
                OptionLabel = o => $"Semestre {o.GetStr("numero")}"
            },
        }
    };

    public static EntityConfig Cursos() => new()
    {
        Title = "Cursos",
        Endpoint = "cursos/",
        ApiTarget = ApiTarget.Aula,
        DisplayTitle = o => $"{o.GetStr("codigo")} - {o.GetStr("nombre")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
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
        DisplayTitle = o => o.GetStr("curso_nombre"),
        DisplaySubtitle = o => $"{o.GetStr("carrera_nombre")} - Semestre {o.GetStr("semestre_numero")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "carrera_semestre", Label = "Carrera / Semestre", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "carrera-semestres/", ValueField = "id",
                OptionLabel = o => $"{o.GetStr("carrera_nombre")} - Semestre {o.GetStr("semestre_numero")}"
            },
            new FieldConfig
            {
                Key = "curso", Label = "Curso", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "cursos/", ValueField = "id",
                OptionLabel = o => $"{o.GetStr("codigo")} - {o.GetStr("nombre")}"
            },
        }
    };

    public static EntityConfig Docentes() => new()
    {
        Title = "Catedraticos",
        Endpoint = "docentes/",
        ApiTarget = ApiTarget.Aula,
        DisplayTitle = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")}",
        DisplaySubtitle = o => $"id_usuario: {o.GetStr("id_usuario")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
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
        DisplayTitle = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")}",
        DisplaySubtitle = o => $"Carne: {o.GetStr("carne")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig { Key = "nombre", Label = "Nombre", Type = FieldType.Text },
            new FieldConfig { Key = "apellido", Label = "Apellido", Type = FieldType.Text },
            new FieldConfig { Key = "carne", Label = "Carne", Type = FieldType.Text },
            new FieldConfig
            {
                Key = "carrera", Label = "Carrera", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "carreras/", ValueField = "id",
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
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => $"{o.GetStr("fecha_inicio")} a {o.GetStr("fecha_fin")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
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
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => $"Capacidad: {o.GetStr("capacidad")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
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
        DisplayTitle = o => o.GetStr("nombre"),
        DisplaySubtitle = o => $"{o.GetStr("hora_inicio")} - {o.GetStr("hora_fin")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "institucion", Label = "Institucion", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "instituciones/", ValueField = "id",
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
        DisplayTitle = o => $"Horario #{o.GetStr("id")} - {o.GetStr("dia")}",
        DisplaySubtitle = o => $"modulo: {o.GetStr("modulo")}  aula: {o.GetStr("aula")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "modulo", Label = "Modulo", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "modulos/", ValueField = "id",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "aula", Label = "Aula", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "aulas/", ValueField = "id",
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
        DisplayTitle = o => $"Seccion #{o.GetStr("id")}",
        DisplaySubtitle = o => $"pensum:{o.GetStr("pensum")}  docente:{o.GetStr("docente")}  horario:{o.GetStr("horario")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "pensum", Label = "Curso (pensum)", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "pensum/", ValueField = "id",
                OptionLabel = o => $"{o.GetStr("curso_nombre")} ({o.GetStr("carrera_nombre")} - Sem {o.GetStr("semestre_numero")})"
            },
            new FieldConfig
            {
                Key = "ciclo", Label = "Ciclo academico", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "ciclos/", ValueField = "id",
                OptionLabel = o => o.GetStr("nombre")
            },
            new FieldConfig
            {
                Key = "docente", Label = "Catedratico", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "docentes/", ValueField = "id",
                OptionLabel = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")}"
            },
            new FieldConfig
            {
                Key = "horario", Label = "Horario", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "horarios/", ValueField = "id",
                OptionLabel = o => $"#{o.GetStr("id")} - {o.GetStr("dia")}"
            },
        }
    };

    public static EntityConfig EstudianteSecciones() => new()
    {
        Title = "Asignar alumno a curso",
        Endpoint = "estudiante-secciones/",
        ApiTarget = ApiTarget.Aula,
        DisplayTitle = o => $"Inscripcion #{o.GetStr("id")}",
        DisplaySubtitle = o => $"estudiante:{o.GetStr("estudiante")}  seccion:{o.GetStr("seccion")}",
        Fields = new()
        {
            new FieldConfig
            {
                Key = "estudiante", Label = "Alumno", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "estudiantes/", ValueField = "id",
                OptionLabel = o => $"{o.GetStr("nombre")} {o.GetStr("apellido")} ({o.GetStr("carne")})"
            },
            new FieldConfig
            {
                Key = "seccion", Label = "Seccion (curso asignado a catedratico)", Type = FieldType.Picker,
                SourceApiTarget = ApiTarget.Aula, SourceEndpoint = "secciones/", ValueField = "id",
                OptionLabel = o => $"Seccion #{o.GetStr("id")}"
            },
        }
    };
}
