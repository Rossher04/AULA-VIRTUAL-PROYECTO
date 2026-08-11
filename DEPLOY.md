# Despliegue en la nube (Render)

Guía paso a paso para publicar las dos APIs (`login` y `aula`) en Render, usando
el blueprint [`render.yaml`](render.yaml) de la raíz del proyecto.

## 0. Prerrequisitos

- Cuenta de [GitHub](https://github.com) (gratis).
- Cuenta de [Render](https://render.com) (gratis, se puede crear con la cuenta de GitHub).
- `psql` instalado en tu máquina (viene con PostgreSQL) o usar el botón **"Connect > PSQL"**
  que Render muestra en el dashboard de cada base de datos (abre una terminal en el navegador,
  no necesitas instalar nada).

> **Nota sobre el plan gratuito de Render:** los servicios web "free" se duermen tras ~15
> minutos sin tráfico y tardan unos 30-50 segundos en despertar en la siguiente petición
> (normal, no es un error). Las bases de datos Postgres "free" de Render tienen fecha de
> expiración (revísala en el dashboard al crearlas) — si tu entrega es en varias semanas,
> considera anotar la fecha o pasar esa base a un plan pago pequeño antes de que expire.

## 1. Subir el proyecto a GitHub

El repo local ya está inicializado y con el primer commit hecho. Crea un repositorio
vacío en GitHub (sin README, sin .gitignore — ya los tenemos) y luego:

```bash
git remote add origin https://github.com/<tu-usuario>/<tu-repo>.git
git branch -M main
git push -u origin main
```

## 2. Crear el Blueprint en Render

1. Entra a [dashboard.render.com](https://dashboard.render.com) → **New** → **Blueprint**.
2. Conecta tu cuenta de GitHub y selecciona el repositorio que acabas de subir.
3. Render detecta `render.yaml` automáticamente y te muestra el plan: 2 bases de datos
   (`aulavirtual-umes-aula-db`, `aulavirtual-umes-login-db`) + 2 web services
   (`aulavirtual-umes-academico`, `aulavirtual-umes-login`).
4. Dale **Apply**. Render crea todo y arranca el primer deploy (tarda unos minutos).

> Si algún nombre de servicio ya está tomado por otro usuario de Render, te lo va a
> marcar en este paso. Cambia el nombre en `render.yaml` (services → `name`) y también
> en `AulaVirtualApp/Services/ApiConfig.cs` (las URLs por defecto) antes de aplicar de nuevo.

En este punto:
- La API `login` va a funcionar completa (sus tablas las crea Django solo, vía migraciones).
- La API `aula` va a **responder pero fallar en casi todo** (error 500 "relation does not
  exist") porque sus tablas no las crea Django — las crea el script SQL. Eso se resuelve
  en el siguiente paso.

## 3. Crear el esquema de la API "aula" (una sola vez)

Las tablas de `aula` (`institucion`, `carrera`, `curso`, etc.) están modeladas como
`managed=False` en Django porque el diseño de base de datos vive en
[`Base de datos general.sql`](Base%20de%20datos%20general.sql) — Django solo las lee,
no las crea. Hay que correr ese script una vez contra la base de Render:

1. En el dashboard de Render, entra a la base `aulavirtual-umes-aula-db`.
2. Copia la **External Database URL** (botón "Connect").
3. Desde tu máquina, con `psql` instalado:

```bash
psql "<External Database URL que copiaste>" -f "Base de datos general.sql"
```

   (O usa el botón "Connect > PSQL" de Render, que abre una terminal en el navegador ya
   conectada, y pega ahí el contenido del archivo.)

Esto crea el schema `aula_virtual_academica`, las ~20 tablas, y siembra la institución
"UMES" + los semestres 1 al 10. A partir de aquí, la API `aula` queda funcional.

> No hace falta correr `base de datos login.sql` — esa API es 100% manejada por Django
> (las migraciones ya crean y siembran sus tablas solas). Ese archivo es solo el diseño
> de referencia para la rúbrica.

## 4. Verificar que todo responde

Sustituye por tus URLs reales si cambiaste los nombres de servicio:

```bash
curl https://aulavirtual-umes-login.onrender.com/api/
curl https://aulavirtual-umes-academico.onrender.com/api/

curl -X POST https://aulavirtual-umes-login.onrender.com/api/login/ \
  -H "Content-Type: application/json" \
  -d '{"usuario":"admin","contrasena":"Admin123!","dominio":"umes"}'
```

El último debe devolver `"ok": true` y un `token`. Si da 500, revisa los logs del
servicio en el dashboard de Render (pestaña **Logs**) — casi siempre es el paso 3
pendiente o una variable de entorno mal copiada.

## 5. Apuntar la app MAUI

`AulaVirtualApp/Services/ApiConfig.cs` ya trae por defecto las URLs de Render
(`https://aulavirtual-umes-login.onrender.com/api/` y `.../aulavirtual-umes-academico.../api/`).
Si cambiaste los nombres de servicio en el paso 2, actualízalas ahí. Vuelve a compilar
la app y prueba el login con el usuario de prueba (`admin` / `Admin123!`).

Si necesitas volver a apuntar a un backend local durante desarrollo, usa la pantalla
de **Configuración** dentro de la app (ahora sí guarda el valor, ver commit de limpieza).

## 6. Cómo se actualiza después

El Blueprint deja auto-deploy activado: cada `git push` a `main` vuelve a construir y
desplegar ambos servicios automáticamente. Los cambios de esquema de la API `aula`
(nuevas tablas/columnas) no se aplican solos — hay que correr el SQL correspondiente a
mano contra la base, igual que en el paso 3.
