# Aula Virtual UMES - Frontend .NET MAUI (Fase 1)

App de escritorio Windows (.NET 10 MAUI, target Windows) que consume las dos APIs Django
del backend (`login` y `aula`), publicadas en la nube (Render, ver `/DEPLOY.md` en la raiz
del proyecto), para implementar la Fase 1 de la hoja de trabajo: Login con 3 roles +
funcionalidad completa del rol Administrador.

## Como abrir el proyecto

1. Instala **Visual Studio 2022** (17.10+) con la carga de trabajo
   **".NET Multi-platform App UI development"**.
2. Abre `AulaVirtualApp.csproj` directamente (o crea una solucion `.sln` y agrega
   el proyecto).
3. Selecciona el target `net10.0-windows10.0.19041.0` y ejecuta en Windows Machine
   como destino de depuracion.

Por defecto la app apunta a las APIs publicadas en Render (ver `Services/ApiConfig.cs`),
asi que no necesitas levantar nada localmente para probarla.

## Desarrollo local (opcional): levantar los dos backends

Si quieres correr los backends en tu propia maquina en vez de usar la nube:

```powershell
cd Backend\login
..\.venv\Scripts\python.exe manage.py runserver 0.0.0.0:8001

cd Backend\aula
..\.venv\Scripts\python.exe manage.py runserver 0.0.0.0:8002
```

Usa `0.0.0.0` (no `127.0.0.1`) para que el emulador/dispositivo pueda conectarse
a tu PC. Luego, desde la app, entra a **Configuracion** (`SettingsPage`) y cambia
las URLs a `http://127.0.0.1:8001/api/` y `http://127.0.0.1:8002/api/` (o la IP de
tu PC en la red si usas un dispositivo fisico) — el boton **Guardar** persiste el
cambio entre sesiones.

## Usuario de prueba

```
Usuario:    admin
Contrasena: Admin123!
Dominio:    umes
```

## Que se implemento (Fase 1)

- **Login** contra `POST /api/login/` de la API de Login. El token firmado que
  regresa se guarda y se manda como `Authorization: Bearer <token>` en cada
  llamada a la API de Aula.
- **3 roles**: Administrador (funcional completo), Docente y Estudiante
  (pantalla de bienvenida placeholder; se implementaran en fases siguientes).
- **Rol Administrador**, con pantallas de lista + alta + edicion + eliminacion
  para:
  - Facultades, Carreras, Semestres, Asignar semestre a carrera
  - Cursos, Asignar curso a semestre por carrera (Pensum)
  - Catedraticos (flujo especial: crea el usuario en la API de Login con rol
    `DOCENTE` y luego el registro en la API de Aula con el `id_usuario`
    devuelto; si el segundo paso falla, se revierte el usuario creado)
  - Alumnos (mismo flujo especial, con rol `ESTUDIANTE` y carrera)
  - Asignar catedratico a curso (Seccion: combina pensum + ciclo + docente +
    horario, tal como lo requiere el modelo de datos del backend)
  - Asignar alumno a curso (Estudiante-Seccion)
  - Datos de apoyo requeridos por el modelo de datos para poder armar las
    asignaciones anteriores: Ciclos academicos, Aulas fisicas, Modulos
    horarios, Horarios, Instituciones.

## Arquitectura del codigo

- `Services/ApiConfig.cs` - guarda las URLs base de ambas APIs.
- `Services/SessionService.cs` - guarda el token y el contexto de sesion
  (usuario, rol, id_usuario, id_institucion).
- `Services/ApiClient.cs` - cliente HTTP generico (GET/POST/PUT/DELETE) que
  adjunta el token y traduce errores de Django REST Framework a mensajes
  legibles.
- `Models/EntityConfig.cs` - describe una entidad (endpoint + lista de campos)
  para poder reutilizar una sola pantalla de lista (`Pages/EntityListPage`) y
  una sola pantalla de formulario dinamico (`Pages/EntityFormPage`) en lugar
  de repetir XAML para cada una de las ~14 entidades academicas.
- `Config/EntityCatalog.cs` - la definicion concreta de cada entidad (que
  campos tiene, de que picker depende, como se muestra en la lista).
- `Pages/CrearDocentePage.xaml(.cs)` y `Pages/CrearEstudiantePage.xaml(.cs)` -
  los dos flujos especiales de 2 pasos (Login API + Aula API) que no encajan
  en el formulario generico.

## Notas / siguientes pasos

- Las pantallas de "Datos de apoyo" (Ciclos, Aulas, Modulos, Horarios) no las
  pide literalmente el enunciado de la Fase 1, pero son necesarias porque el
  modelo `Seccion` del backend (usado para "asignar catedratico a curso")
  requiere pensum + ciclo + docente + horario, y `Horario` a su vez requiere
  modulo + aula + dia.
- Las capturas de pantalla para el PDF de entrega deben incluir: login con los
  3 roles, y el funcionamiento de cada punto del rol administrador.


