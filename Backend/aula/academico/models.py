from django.db import models


class Institucion(models.Model):
    id_institucion = models.AutoField(primary_key=True, db_column='id_institucion')
    nombre = models.CharField(max_length=150)
    dominio = models.CharField(max_length=100, unique=True)

    class Meta:
        db_table = 'institucion'
        managed = False

    def __str__(self):
        return self.nombre


class Facultad(models.Model):
    id_facultad = models.AutoField(primary_key=True, db_column='id_facultad')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='facultades',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=100)

    class Meta:
        db_table = 'facultad'
        managed = False
        constraints = [
            models.UniqueConstraint(fields=['institucion', 'nombre'], name='academico_facultad_unica')
        ]

    def __str__(self):
        return self.nombre


class Carrera(models.Model):
    id_carrera = models.AutoField(primary_key=True, db_column='id_carrera')
    facultad = models.ForeignKey(
        Facultad,
        on_delete=models.CASCADE,
        related_name='carreras',
        db_column='id_facultad',
    )
    nombre = models.CharField(max_length=100)

    class Meta:
        db_table = 'carrera'
        managed = False

    def __str__(self):
        return self.nombre


class Semestre(models.Model):
    id_semestre = models.AutoField(primary_key=True, db_column='id_semestre')
    numero = models.PositiveIntegerField(unique=True)

    class Meta:
        db_table = 'semestre'
        managed = False

    def __str__(self):
        return f'Semestre {self.numero}'


class CarreraSemestre(models.Model):
    id_carrera_semestre = models.AutoField(primary_key=True, db_column='id_carrera_semestre')
    carrera = models.ForeignKey(
        Carrera,
        on_delete=models.CASCADE,
        related_name='carrera_semestres',
        db_column='id_carrera',
    )
    semestre = models.ForeignKey(
        Semestre,
        on_delete=models.CASCADE,
        related_name='carrera_semestres',
        db_column='id_semestre',
    )

    class Meta:
        db_table = 'carrera_semestre'
        managed = False
        constraints = [
            models.UniqueConstraint(fields=['carrera', 'semestre'], name='academico_semestre_unico_por_carrera')
        ]

    def __str__(self):
        return f'{self.carrera} - {self.semestre}'


class Curso(models.Model):
    id_curso = models.AutoField(primary_key=True, db_column='id_curso')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='cursos',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=100)
    codigo = models.CharField(max_length=20)

    class Meta:
        db_table = 'curso'
        managed = False
        constraints = [
            models.UniqueConstraint(fields=['institucion', 'codigo'], name='academico_curso_codigo_unico')
        ]

    def __str__(self):
        return f'{self.codigo} - {self.nombre}'


class Pensum(models.Model):
    id_pensum = models.AutoField(primary_key=True, db_column='id_pensum')
    carrera_semestre = models.ForeignKey(
        CarreraSemestre,
        on_delete=models.CASCADE,
        related_name='pensum',
        db_column='id_carrera_semestre',
    )
    curso = models.ForeignKey(
        Curso,
        on_delete=models.CASCADE,
        related_name='pensum',
        db_column='id_curso',
    )

    class Meta:
        db_table = 'pensum'
        managed = False
        constraints = [
            models.UniqueConstraint(fields=['carrera_semestre', 'curso'], name='academico_curso_unico_pensum')
        ]


class Docente(models.Model):
    id_docente = models.AutoField(primary_key=True, db_column='id_docente')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='docentes',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=100)
    apellido = models.CharField(max_length=100)
    id_usuario = models.PositiveIntegerField(unique=True)

    class Meta:
        db_table = 'docente'
        managed = False

    def __str__(self):
        return f'{self.nombre} {self.apellido}'


class Estudiante(models.Model):
    id_estudiante = models.AutoField(primary_key=True, db_column='id_estudiante')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='estudiantes',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=100)
    apellido = models.CharField(max_length=100)
    carne = models.CharField(max_length=20)
    carrera = models.ForeignKey(
        Carrera,
        on_delete=models.PROTECT,
        related_name='estudiantes',
        db_column='id_carrera',
    )
    id_usuario = models.PositiveIntegerField(unique=True)

    class Meta:
        db_table = 'estudiante'
        managed = False
        constraints = [
            models.UniqueConstraint(fields=['institucion', 'carne'], name='academico_carne_unico')
        ]

    def __str__(self):
        return f'{self.carne} - {self.nombre} {self.apellido}'


class Aula(models.Model):
    id_aula = models.AutoField(primary_key=True, db_column='id_aula')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='aulas',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=50)
    capacidad = models.PositiveIntegerField(null=True, blank=True)

    class Meta:
        db_table = 'aula'
        managed = False


class Modulo(models.Model):
    id_modulo = models.AutoField(primary_key=True, db_column='id_modulo')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='modulos',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=30)
    hora_inicio = models.TimeField()
    hora_fin = models.TimeField()

    class Meta:
        db_table = 'modulo'
        managed = False


class Horario(models.Model):
    id_horario = models.AutoField(primary_key=True, db_column='id_horario')
    modulo = models.ForeignKey(
        Modulo,
        on_delete=models.CASCADE,
        related_name='horarios',
        db_column='id_modulo',
    )
    aula = models.ForeignKey(
        Aula,
        on_delete=models.CASCADE,
        related_name='horarios',
        db_column='id_aula',
    )
    dia = models.CharField(max_length=10, choices=[
        ('LUNES', 'Lunes'),
        ('MARTES', 'Martes'),
        ('MIERCOLES', 'Miercoles'),
        ('JUEVES', 'Jueves'),
        ('VIERNES', 'Viernes'),
        ('SABADO', 'Sabado'),
    ])

    class Meta:
        db_table = 'horario'
        managed = False


class CicloAcademico(models.Model):
    id_ciclo = models.AutoField(primary_key=True, db_column='id_ciclo')
    institucion = models.ForeignKey(
        Institucion,
        on_delete=models.CASCADE,
        related_name='ciclos',
        db_column='id_institucion',
    )
    nombre = models.CharField(max_length=30)
    fecha_inicio = models.DateField()
    fecha_fin = models.DateField()

    class Meta:
        db_table = 'ciclo_academico'
        managed = False


class Seccion(models.Model):
    id_seccion = models.AutoField(primary_key=True, db_column='id_seccion')
    pensum = models.ForeignKey(
        Pensum,
        on_delete=models.CASCADE,
        related_name='secciones',
        db_column='id_pensum',
    )
    ciclo = models.ForeignKey(
        CicloAcademico,
        on_delete=models.CASCADE,
        related_name='secciones',
        db_column='id_ciclo',
    )
    docente = models.ForeignKey(
        Docente,
        on_delete=models.PROTECT,
        related_name='secciones',
        db_column='id_docente',
    )
    horario = models.ForeignKey(
        Horario,
        on_delete=models.PROTECT,
        related_name='secciones',
        db_column='id_horario',
    )

    class Meta:
        db_table = 'seccion'
        managed = False


class EstudianteSeccion(models.Model):
    id_estudiante_seccion = models.AutoField(primary_key=True, db_column='id_estudiante_seccion')
    estudiante = models.ForeignKey(
        Estudiante,
        on_delete=models.CASCADE,
        related_name='asignaciones',
        db_column='id_estudiante',
    )
    seccion = models.ForeignKey(
        Seccion,
        on_delete=models.CASCADE,
        related_name='estudiantes_asignados',
        db_column='id_seccion',
    )
    fecha_inscripcion = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'estudiante_seccion'
        managed = False
        constraints = [
            models.UniqueConstraint(fields=['estudiante', 'seccion'], name='academico_estudiante_unico_seccion')
        ]


class SesionVirtual(models.Model):
    id_sesion_virtual = models.AutoField(primary_key=True, db_column='id_sesion_virtual')
    seccion = models.ForeignKey(
        Seccion,
        on_delete=models.CASCADE,
        related_name='sesiones_virtuales',
        db_column='id_seccion',
    )
    fecha = models.DateField()
    hora = models.TimeField()
    enlace = models.URLField(max_length=255, blank=True)

    class Meta:
        db_table = 'sesion_virtual'
        managed = False


class Asistencia(models.Model):
    id_asistencia = models.AutoField(primary_key=True, db_column='id_asistencia')
    estudiante_seccion = models.ForeignKey(
        EstudianteSeccion,
        on_delete=models.CASCADE,
        related_name='asistencias',
        db_column='id_estudiante_seccion',
    )
    sesion_virtual = models.ForeignKey(
        SesionVirtual,
        on_delete=models.CASCADE,
        related_name='asistencias',
        db_column='id_sesion_virtual',
    )
    estado = models.CharField(max_length=15, choices=[
        ('PRESENTE', 'Presente'),
        ('AUSENTE', 'Ausente'),
        ('JUSTIFICADO', 'Justificado'),
    ])

    class Meta:
        db_table = 'asistencia'
        managed = False


class Recurso(models.Model):
    id_recurso = models.AutoField(primary_key=True, db_column='id_recurso')
    seccion = models.ForeignKey(
        Seccion,
        on_delete=models.CASCADE,
        related_name='recursos',
        db_column='id_seccion',
    )
    titulo = models.CharField(max_length=150)
    tipo = models.CharField(max_length=20, choices=[
        ('DOCUMENTO', 'Documento'),
        ('ENLACE', 'Enlace'),
        ('VIDEO', 'Video'),
    ])
    url_o_archivo = models.CharField(max_length=255)
    fecha_publicacion = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'recurso'
        managed = False


class Anuncio(models.Model):
    id_anuncio = models.AutoField(primary_key=True, db_column='id_anuncio')
    seccion = models.ForeignKey(
        Seccion,
        on_delete=models.CASCADE,
        related_name='anuncios',
        db_column='id_seccion',
    )
    titulo = models.CharField(max_length=150)
    contenido = models.TextField()
    fecha_publicacion = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'anuncio'
        managed = False


class Tarea(models.Model):
    id_tarea = models.AutoField(primary_key=True, db_column='id_tarea')
    seccion = models.ForeignKey(
        Seccion,
        on_delete=models.CASCADE,
        related_name='tareas',
        db_column='id_seccion',
    )
    titulo = models.CharField(max_length=150)
    descripcion = models.TextField(blank=True)
    fecha_publicacion = models.DateTimeField(auto_now_add=True)
    fecha_limite = models.DateTimeField()

    class Meta:
        db_table = 'tarea'
        managed = False


class Entrega(models.Model):
    id_entrega = models.AutoField(primary_key=True, db_column='id_entrega')
    tarea = models.ForeignKey(
        Tarea,
        on_delete=models.CASCADE,
        related_name='entregas',
        db_column='id_tarea',
    )
    estudiante = models.ForeignKey(
        Estudiante,
        on_delete=models.CASCADE,
        related_name='entregas',
        db_column='id_estudiante',
    )
    archivo = models.CharField(max_length=255)
    fecha_entrega = models.DateTimeField(auto_now_add=True)
    nota = models.DecimalField(max_digits=5, decimal_places=2, null=True, blank=True)

    class Meta:
        db_table = 'entrega'
        managed = False


class Nota(models.Model):
    id_nota = models.AutoField(primary_key=True, db_column='id_nota')
    estudiante_seccion = models.ForeignKey(
        EstudianteSeccion,
        on_delete=models.CASCADE,
        related_name='notas',
        db_column='id_estudiante_seccion',
    )
    tipo = models.CharField(max_length=30)
    valor = models.DecimalField(max_digits=5, decimal_places=2)

    class Meta:
        db_table = 'nota'
        managed = False
