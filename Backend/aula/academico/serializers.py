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
        fields = ['id', 'facultad', 'facultad_nombre', 'nombre']


class SemestreSerializer(serializers.ModelSerializer):
    class Meta:
        model = Semestre
        fields = '__all__'


class CarreraSemestreSerializer(serializers.ModelSerializer):
    carrera_nombre = serializers.CharField(source='carrera.nombre', read_only=True)
    semestre_numero = serializers.IntegerField(source='semestre.numero', read_only=True)

    class Meta:
        model = CarreraSemestre
        fields = ['id', 'carrera', 'carrera_nombre', 'semestre', 'semestre_numero']


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
        fields = ['id', 'carrera_semestre', 'curso', 'curso_nombre', 'carrera_nombre', 'semestre_numero']


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
    class Meta:
        model = Horario
        fields = '__all__'


class CicloAcademicoSerializer(serializers.ModelSerializer):
    class Meta:
        model = CicloAcademico
        fields = '__all__'


class SeccionSerializer(serializers.ModelSerializer):
    class Meta:
        model = Seccion
        fields = '__all__'


class EstudianteSeccionSerializer(serializers.ModelSerializer):
    class Meta:
        model = EstudianteSeccion
        fields = '__all__'
        read_only_fields = ['fecha_inscripcion']


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
