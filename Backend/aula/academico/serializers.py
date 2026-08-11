from rest_framework import serializers

from .models import (
    Anuncio,
    Asistencia,
    Aula,
    Carrera,
    CarreraSemestre,
    CicloAcademico,
    Curso,
    Docente,
    Entrega,
    Estudiante,
    EstudianteSeccion,
    Facultad,
    Horario,
    Institucion,
    Modulo,
    Nota,
    Pensum,
    Recurso,
    Seccion,
    Semestre,
    SesionVirtual,
    Tarea,
)


class InstitucionSerializer(serializers.ModelSerializer):
    class Meta:
        model = Institucion
        fields = '__all__'


class FacultadSerializer(serializers.ModelSerializer):
    class Meta:
        model = Facultad
        fields = '__all__'


class CarreraSerializer(serializers.ModelSerializer):
    facultad_nombre = serializers.CharField(source='facultad.nombre', read_only=True)

    class Meta:
        model = Carrera
        fields = ['id_carrera', 'facultad', 'facultad_nombre', 'nombre']


class SemestreSerializer(serializers.ModelSerializer):
    class Meta:
        model = Semestre
        fields = '__all__'


class CarreraSemestreSerializer(serializers.ModelSerializer):
    carrera_nombre = serializers.CharField(source='carrera.nombre', read_only=True)
    semestre_numero = serializers.IntegerField(source='semestre.numero', read_only=True)

    class Meta:
        model = CarreraSemestre
        fields = ['id_carrera_semestre', 'carrera', 'carrera_nombre', 'semestre', 'semestre_numero']


class CursoSerializer(serializers.ModelSerializer):
    class Meta:
        model = Curso
        fields = '__all__'


class PensumSerializer(serializers.ModelSerializer):
    curso_nombre = serializers.CharField(source='curso.nombre', read_only=True)
    carrera_nombre = serializers.CharField(source='carrera_semestre.carrera.nombre', read_only=True)
    semestre_numero = serializers.IntegerField(source='carrera_semestre.semestre.numero', read_only=True)

    class Meta:
        model = Pensum
        fields = ['id_pensum', 'carrera_semestre', 'curso', 'curso_nombre', 'carrera_nombre', 'semestre_numero']


class DocenteSerializer(serializers.ModelSerializer):
    class Meta:
        model = Docente
        fields = '__all__'


class EstudianteSerializer(serializers.ModelSerializer):
    class Meta:
        model = Estudiante
        fields = '__all__'


class AulaSerializer(serializers.ModelSerializer):
    class Meta:
        model = Aula
        fields = '__all__'


class ModuloSerializer(serializers.ModelSerializer):
    class Meta:
        model = Modulo
        fields = '__all__'


class HorarioSerializer(serializers.ModelSerializer):
    modulo_nombre = serializers.CharField(source='modulo.nombre', read_only=True)
    hora_inicio = serializers.TimeField(source='modulo.hora_inicio', read_only=True, format='%H:%M')
    hora_fin = serializers.TimeField(source='modulo.hora_fin', read_only=True, format='%H:%M')
    aula_nombre = serializers.CharField(source='aula.nombre', read_only=True)

    class Meta:
        model = Horario
        fields = [
            'id_horario',
            'modulo', 'modulo_nombre', 'hora_inicio', 'hora_fin',
            'aula', 'aula_nombre',
            'dia',
        ]


class CicloAcademicoSerializer(serializers.ModelSerializer):
    class Meta:
        model = CicloAcademico
        fields = '__all__'


class SeccionSerializer(serializers.ModelSerializer):
    # Etiquetas legibles para que la app no tenga que mostrar IDs crudos.
    curso_nombre = serializers.CharField(source='pensum.curso.nombre', read_only=True)
    curso_codigo = serializers.CharField(source='pensum.curso.codigo', read_only=True)
    carrera_nombre = serializers.CharField(source='pensum.carrera_semestre.carrera.nombre', read_only=True)
    semestre_numero = serializers.IntegerField(source='pensum.carrera_semestre.semestre.numero', read_only=True)
    ciclo_nombre = serializers.CharField(source='ciclo.nombre', read_only=True)
    docente_nombre = serializers.SerializerMethodField()
    horario_descripcion = serializers.SerializerMethodField()

    class Meta:
        model = Seccion
        fields = [
            'id_seccion',
            'pensum', 'curso_nombre', 'curso_codigo', 'carrera_nombre', 'semestre_numero',
            'ciclo', 'ciclo_nombre',
            'docente', 'docente_nombre',
            'horario', 'horario_descripcion',
        ]

    def get_docente_nombre(self, obj):
        return f'{obj.docente.nombre} {obj.docente.apellido}'

    def get_horario_descripcion(self, obj):
        return f'{obj.horario.dia} {obj.horario.modulo.hora_inicio:%H:%M}-{obj.horario.modulo.hora_fin:%H:%M} ({obj.horario.aula.nombre})'


class EstudianteSeccionSerializer(serializers.ModelSerializer):
    estudiante_nombre = serializers.SerializerMethodField()
    estudiante_carne = serializers.CharField(source='estudiante.carne', read_only=True)
    curso_nombre = serializers.CharField(source='seccion.pensum.curso.nombre', read_only=True)
    ciclo_nombre = serializers.CharField(source='seccion.ciclo.nombre', read_only=True)

    class Meta:
        model = EstudianteSeccion
        fields = [
            'id_estudiante_seccion',
            'estudiante', 'estudiante_nombre', 'estudiante_carne',
            'seccion', 'curso_nombre', 'ciclo_nombre',
            'fecha_inscripcion',
        ]
        read_only_fields = ['fecha_inscripcion']

    def get_estudiante_nombre(self, obj):
        return f'{obj.estudiante.nombre} {obj.estudiante.apellido}'


class SesionVirtualSerializer(serializers.ModelSerializer):
    class Meta:
        model = SesionVirtual
        fields = '__all__'


class AsistenciaSerializer(serializers.ModelSerializer):
    class Meta:
        model = Asistencia
        fields = '__all__'


class RecursoSerializer(serializers.ModelSerializer):
    class Meta:
        model = Recurso
        fields = '__all__'
        read_only_fields = ['fecha_publicacion']


class AnuncioSerializer(serializers.ModelSerializer):
    class Meta:
        model = Anuncio
        fields = '__all__'
        read_only_fields = ['fecha_publicacion']


class TareaSerializer(serializers.ModelSerializer):
    class Meta:
        model = Tarea
        fields = '__all__'
        read_only_fields = ['fecha_publicacion']


class EntregaSerializer(serializers.ModelSerializer):
    class Meta:
        model = Entrega
        fields = '__all__'
        read_only_fields = ['fecha_entrega']


class NotaSerializer(serializers.ModelSerializer):
    class Meta:
        model = Nota
        fields = '__all__'
