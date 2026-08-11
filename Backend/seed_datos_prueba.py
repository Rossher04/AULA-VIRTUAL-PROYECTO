"""
Carga datos de prueba genericos en el Aula Virtual, consumiendo las mismas dos
APIs que usa la app MAUI (no toca la base de datos directamente).

Es idempotente y aditivo: antes de crear cada registro busca si ya existe por su
clave natural (codigo de curso, carne, nombre de facultad, etc.). Se puede correr
las veces que sea necesario sin duplicar ni borrar informacion existente.

Uso:
    # contra las APIs publicadas en Render (por defecto)
    python seed_datos_prueba.py

    # contra backends locales
    python seed_datos_prueba.py --login http://127.0.0.1:8001/api --aula http://127.0.0.1:8002/api
"""
import argparse
import json
import sys
import urllib.error
import urllib.request

LOGIN_URL_DEFAULT = 'https://aulavirtual-umes-login.onrender.com/api'
AULA_URL_DEFAULT = 'https://aulavirtual-umes-academico.onrender.com/api'

# Password unico para todos los usuarios de prueba que crea este script.
PASSWORD_PRUEBA = 'Prueba123!'


def _request(method, url, payload=None):
    data = json.dumps(payload).encode() if payload is not None else None
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header('Content-Type', 'application/json')
    req.add_header('Accept', 'application/json')
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            body = resp.read().decode()
            return json.loads(body) if body else None
    except urllib.error.HTTPError as exc:
        detalle = exc.read().decode()
        raise SystemExit(f'\nERROR {exc.code} en {method} {url}\n{detalle}\n')
    except urllib.error.URLError as exc:
        raise SystemExit(
            f'\nNo se pudo conectar a {url} ({exc.reason}).\n'
            'Si el servicio esta en el plan gratuito de Render puede estar "dormido": '
            'abre la URL en el navegador, espera a que responda y vuelve a correr el script.\n'
        )


def listar(base, endpoint):
    return _request('GET', f'{base}/{endpoint}/') or []


def crear(base, endpoint, payload):
    return _request('POST', f'{base}/{endpoint}/', payload)


def get_or_create(base, endpoint, payload, coincide, etiqueta):
    """Devuelve (registro, fue_creado) buscando primero con el predicado `coincide`."""
    for existente in listar(base, endpoint):
        if coincide(existente):
            print(f'  = ya existia: {etiqueta}')
            return existente, False
    creado = crear(base, endpoint, payload)
    print(f'  + creado:     {etiqueta}')
    return creado, True


def seed(login_base, aula_base):
    print(f'\nLogin API: {login_base}\nAula API:  {aula_base}\n')

    instituciones = listar(aula_base, 'instituciones')
    if not instituciones:
        raise SystemExit(
            'No hay ninguna institucion en la API de aula. Corre primero '
            '"Base de datos general.sql" contra la base de datos (ver DEPLOY.md).'
        )
    institucion_id = instituciones[0]['id_institucion']
    print(f'Institucion: {instituciones[0]["nombre"]} (id={institucion_id})\n')

    semestres = {s['numero']: s['id_semestre'] for s in listar(aula_base, 'semestres')}
    if not semestres:
        raise SystemExit('No hay semestres cargados; revisa el script SQL inicial.')

    # ---------------------------------------------------------------- facultades
    print('Facultades')
    facultades = {}
    for nombre in ['Facultad de Ingenieria', 'Facultad de Ciencias Economicas', 'Facultad de Humanidades']:
        reg, _ = get_or_create(
            aula_base, 'facultades',
            {'institucion': institucion_id, 'nombre': nombre},
            lambda e, n=nombre: e['nombre'].strip().lower() == n.lower(),
            nombre,
        )
        facultades[nombre] = reg['id_facultad']

    # ------------------------------------------------------------------ carreras
    print('\nCarreras')
    carreras = {}
    catalogo_carreras = [
        ('Ingenieria en Sistemas', 'Facultad de Ingenieria'),
        ('Ingenieria Civil', 'Facultad de Ingenieria'),
        ('Administracion de Empresas', 'Facultad de Ciencias Economicas'),
        ('Psicologia', 'Facultad de Humanidades'),
    ]
    for nombre, facultad in catalogo_carreras:
        reg, _ = get_or_create(
            aula_base, 'carreras',
            {'facultad': facultades[facultad], 'nombre': nombre},
            lambda e, n=nombre: e['nombre'].strip().lower() == n.lower(),
            f'{nombre} ({facultad})',
        )
        carreras[nombre] = reg['id_carrera']

    # ------------------------------------------------- semestres por carrera
    print('\nSemestres por carrera (1 al 4 de cada carrera)')
    carrera_semestres = {}
    for carrera_nombre, carrera_id in carreras.items():
        for numero in range(1, 5):
            reg, _ = get_or_create(
                aula_base, 'carrera-semestres',
                {'carrera': carrera_id, 'semestre': semestres[numero]},
                lambda e, c=carrera_id, n=numero: e['carrera'] == c and e['semestre_numero'] == n,
                f'{carrera_nombre} - Semestre {numero}',
            )
            carrera_semestres[(carrera_nombre, numero)] = reg['id_carrera_semestre']

    # -------------------------------------------------------------------- cursos
    print('\nCursos')
    cursos = {}
    catalogo_cursos = [
        ('MAT101', 'Matematica I'),
        ('FIS101', 'Fisica I'),
        ('PRG101', 'Programacion I'),
        ('PRG201', 'Programacion II'),
        ('BDD201', 'Bases de Datos'),
        ('ADM101', 'Administracion General'),
        ('CON101', 'Contabilidad I'),
        ('PSI101', 'Psicologia General'),
    ]
    for codigo, nombre in catalogo_cursos:
        reg, _ = get_or_create(
            aula_base, 'cursos',
            {'institucion': institucion_id, 'codigo': codigo, 'nombre': nombre},
            lambda e, c=codigo: e['codigo'] == c,
            f'{codigo} - {nombre}',
        )
        cursos[codigo] = reg['id_curso']

    # -------------------------------------------------------------------- pensum
    print('\nPensum (curso asignado a semestre por carrera)')
    pensums = {}
    catalogo_pensum = [
        ('Ingenieria en Sistemas', 1, 'MAT101'),
        ('Ingenieria en Sistemas', 1, 'PRG101'),
        ('Ingenieria en Sistemas', 2, 'PRG201'),
        ('Ingenieria en Sistemas', 3, 'BDD201'),
        ('Ingenieria Civil', 1, 'MAT101'),
        ('Ingenieria Civil', 1, 'FIS101'),
        ('Administracion de Empresas', 1, 'ADM101'),
        ('Administracion de Empresas', 2, 'CON101'),
        ('Psicologia', 1, 'PSI101'),
    ]
    for carrera_nombre, numero, codigo in catalogo_pensum:
        cs_id = carrera_semestres[(carrera_nombre, numero)]
        curso_id = cursos[codigo]
        reg, _ = get_or_create(
            aula_base, 'pensum',
            {'carrera_semestre': cs_id, 'curso': curso_id},
            lambda e, c=cs_id, k=curso_id: e['carrera_semestre'] == c and e['curso'] == k,
            f'{codigo} en {carrera_nombre} Sem {numero}',
        )
        pensums[(carrera_nombre, numero, codigo)] = reg['id_pensum']

    # -------------------------------------------------------------------- ciclos
    print('\nCiclos academicos')
    ciclos = {}
    for nombre, inicio, fin in [
        ('2026 - Ciclo 1', '2026-01-19', '2026-06-05'),
        ('2026 - Ciclo 2', '2026-07-13', '2026-11-27'),
    ]:
        reg, _ = get_or_create(
            aula_base, 'ciclos',
            {'institucion': institucion_id, 'nombre': nombre, 'fecha_inicio': inicio, 'fecha_fin': fin},
            lambda e, n=nombre: e['nombre'].strip().lower() == n.lower(),
            nombre,
        )
        ciclos[nombre] = reg['id_ciclo']

    # --------------------------------------------------------------------- aulas
    print('\nAulas fisicas')
    aulas = {}
    for nombre, capacidad in [('A-101', 40), ('A-102', 35), ('B-201', 30), ('Lab de Computo', 25)]:
        reg, _ = get_or_create(
            aula_base, 'aulas',
            {'institucion': institucion_id, 'nombre': nombre, 'capacidad': capacidad},
            lambda e, n=nombre: e['nombre'].strip().lower() == n.lower(),
            f'{nombre} (capacidad {capacidad})',
        )
        aulas[nombre] = reg['id_aula']

    # ------------------------------------------------------------------- modulos
    print('\nModulos horarios')
    modulos = {}
    for nombre, inicio, fin in [
        ('Modulo 1', '07:00', '08:30'),
        ('Modulo 2', '08:40', '10:10'),
        ('Modulo 3', '10:20', '11:50'),
        ('Modulo 4', '14:00', '15:30'),
    ]:
        reg, _ = get_or_create(
            aula_base, 'modulos',
            {'institucion': institucion_id, 'nombre': nombre, 'hora_inicio': inicio, 'hora_fin': fin},
            lambda e, n=nombre: e['nombre'].strip().lower() == n.lower(),
            f'{nombre} ({inicio}-{fin})',
        )
        modulos[nombre] = reg['id_modulo']

    # ------------------------------------------------------------------ horarios
    print('\nHorarios')
    horarios = {}
    catalogo_horarios = [
        ('Modulo 1', 'A-101', 'LUNES'),
        ('Modulo 2', 'A-102', 'MARTES'),
        ('Modulo 3', 'Lab de Computo', 'MIERCOLES'),
        ('Modulo 4', 'B-201', 'JUEVES'),
        ('Modulo 1', 'Lab de Computo', 'VIERNES'),
    ]
    for modulo, aula, dia in catalogo_horarios:
        m_id, a_id = modulos[modulo], aulas[aula]
        reg, _ = get_or_create(
            aula_base, 'horarios',
            {'modulo': m_id, 'aula': a_id, 'dia': dia},
            lambda e, m=m_id, a=a_id, d=dia: e['modulo'] == m and e['aula'] == a and e['dia'] == d,
            f'{dia} {modulo} en {aula}',
        )
        horarios[(modulo, aula, dia)] = reg['id_horario']

    # ------------------------------------------------- catedraticos + usuarios
    print('\nCatedraticos (con usuario de acceso)')
    usuarios_existentes = {u['usuario']: u['id'] for u in listar(login_base, 'usuarios')}
    docentes_existentes = listar(aula_base, 'docentes')
    docentes = {}
    catalogo_docentes = [
        ('mgarcia', 'Mario', 'Garcia'),
        ('lmendez', 'Lucia', 'Mendez'),
        ('rlopez', 'Rodrigo', 'Lopez'),
    ]
    for usuario, nombre, apellido in catalogo_docentes:
        ya = next((d for d in docentes_existentes
                   if d['nombre'] == nombre and d['apellido'] == apellido), None)
        if ya:
            print(f'  = ya existia: {nombre} {apellido}')
            docentes[usuario] = ya['id_docente']
            continue

        if usuario in usuarios_existentes:
            id_usuario = usuarios_existentes[usuario]
        else:
            nuevo = crear(login_base, 'usuarios', {
                'institucion': institucion_id, 'usuario': usuario,
                'contrasena': PASSWORD_PRUEBA, 'rol_tipo': 'DOCENTE', 'activo': True,
            })
            id_usuario = nuevo['id']

        reg = crear(aula_base, 'docentes', {
            'institucion': institucion_id, 'nombre': nombre,
            'apellido': apellido, 'id_usuario': id_usuario,
        })
        docentes[usuario] = reg['id_docente']
        print(f'  + creado:     {nombre} {apellido} (usuario: {usuario})')

    # ----------------------------------------------------- alumnos + usuarios
    print('\nAlumnos (con usuario de acceso)')
    estudiantes_existentes = listar(aula_base, 'estudiantes')
    estudiantes = {}
    catalogo_estudiantes = [
        ('acastillo', 'Ana', 'Castillo', '202600101', 'Ingenieria en Sistemas'),
        ('jramirez', 'Jorge', 'Ramirez', '202600102', 'Ingenieria en Sistemas'),
        ('mflores', 'Maria', 'Flores', '202600103', 'Ingenieria en Sistemas'),
        ('dperez', 'Diego', 'Perez', '202600201', 'Ingenieria Civil'),
        ('svasquez', 'Sofia', 'Vasquez', '202600301', 'Administracion de Empresas'),
    ]
    for usuario, nombre, apellido, carne, carrera in catalogo_estudiantes:
        ya = next((e for e in estudiantes_existentes if e['carne'] == carne), None)
        if ya:
            print(f'  = ya existia: {nombre} {apellido} ({carne})')
            estudiantes[carne] = ya['id_estudiante']
            continue

        if usuario in usuarios_existentes:
            id_usuario = usuarios_existentes[usuario]
        else:
            nuevo = crear(login_base, 'usuarios', {
                'institucion': institucion_id, 'usuario': usuario,
                'contrasena': PASSWORD_PRUEBA, 'rol_tipo': 'ESTUDIANTE', 'activo': True,
            })
            id_usuario = nuevo['id']

        reg = crear(aula_base, 'estudiantes', {
            'institucion': institucion_id, 'nombre': nombre, 'apellido': apellido,
            'carne': carne, 'carrera': carreras[carrera], 'id_usuario': id_usuario,
        })
        estudiantes[carne] = reg['id_estudiante']
        print(f'  + creado:     {nombre} {apellido} ({carne}) - usuario: {usuario}')

    # ------------------------------------ secciones (catedratico asignado a curso)
    print('\nAsignacion de catedraticos a cursos')
    ciclo_actual = ciclos['2026 - Ciclo 2']
    secciones = {}
    catalogo_secciones = [
        (('Ingenieria en Sistemas', 1, 'PRG101'), 'mgarcia', ('Modulo 3', 'Lab de Computo', 'MIERCOLES')),
        (('Ingenieria en Sistemas', 1, 'MAT101'), 'lmendez', ('Modulo 1', 'A-101', 'LUNES')),
        (('Ingenieria en Sistemas', 3, 'BDD201'), 'mgarcia', ('Modulo 1', 'Lab de Computo', 'VIERNES')),
        (('Ingenieria Civil', 1, 'FIS101'), 'rlopez', ('Modulo 2', 'A-102', 'MARTES')),
        (('Administracion de Empresas', 1, 'ADM101'), 'rlopez', ('Modulo 4', 'B-201', 'JUEVES')),
    ]
    secciones_existentes = listar(aula_base, 'secciones')
    for clave_pensum, docente_usuario, clave_horario in catalogo_secciones:
        p_id = pensums[clave_pensum]
        d_id = docentes[docente_usuario]
        h_id = horarios[clave_horario]
        etiqueta = f'{clave_pensum[2]} ({clave_pensum[0]}) con {docente_usuario}'
        ya = next((s for s in secciones_existentes
                   if s['pensum'] == p_id and s['ciclo'] == ciclo_actual), None)
        if ya:
            print(f'  = ya existia: {etiqueta}')
            secciones[clave_pensum] = ya['id_seccion']
            continue
        reg = crear(aula_base, 'secciones', {
            'pensum': p_id, 'ciclo': ciclo_actual, 'docente': d_id, 'horario': h_id,
        })
        secciones[clave_pensum] = reg['id_seccion']
        print(f'  + creado:     {etiqueta}')

    # ---------------------------------------------- inscripcion de alumnos
    print('\nAsignacion de alumnos a cursos')
    inscripciones_existentes = listar(aula_base, 'estudiante-secciones')
    catalogo_inscripciones = [
        ('202600101', ('Ingenieria en Sistemas', 1, 'PRG101')),
        ('202600101', ('Ingenieria en Sistemas', 1, 'MAT101')),
        ('202600102', ('Ingenieria en Sistemas', 1, 'PRG101')),
        ('202600103', ('Ingenieria en Sistemas', 1, 'MAT101')),
        ('202600201', ('Ingenieria Civil', 1, 'FIS101')),
        ('202600301', ('Administracion de Empresas', 1, 'ADM101')),
    ]
    for carne, clave_pensum in catalogo_inscripciones:
        e_id = estudiantes[carne]
        s_id = secciones[clave_pensum]
        etiqueta = f'{carne} en {clave_pensum[2]}'
        if any(i['estudiante'] == e_id and i['seccion'] == s_id for i in inscripciones_existentes):
            print(f'  = ya existia: {etiqueta}')
            continue
        crear(aula_base, 'estudiante-secciones', {'estudiante': e_id, 'seccion': s_id})
        print(f'  + creado:     {etiqueta}')

    print(f"""
Listo. Usuarios de prueba creados por este script (contrasena: {PASSWORD_PRUEBA}):
  Catedraticos: mgarcia, lmendez, rlopez
  Alumnos:      acastillo, jramirez, mflores, dperez, svasquez

Usuarios que ya venian sembrados en la base:
  admin / Admin123!    docente / Docente123!    estudiante / Estudiante123!
""")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--login', default=LOGIN_URL_DEFAULT, help='URL base de la API de login')
    parser.add_argument('--aula', default=AULA_URL_DEFAULT, help='URL base de la API academica')
    args = parser.parse_args()
    seed(args.login.rstrip('/'), args.aula.rstrip('/'))


if __name__ == '__main__':
    sys.exit(main())
