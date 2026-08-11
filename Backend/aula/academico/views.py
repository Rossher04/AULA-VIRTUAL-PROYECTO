from django.core import signing
from django.core.signing import BadSignature, SignatureExpired
from rest_framework import status, viewsets
from rest_framework.decorators import api_view
from rest_framework.response import Response

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
from .serializers import (
    AnuncioSerializer,
    AsistenciaSerializer,
    AulaSerializer,
    CarreraSemestreSerializer,
    CarreraSerializer,
    CicloAcademicoSerializer,
    CursoSerializer,
    DocenteSerializer,
    EntregaSerializer,
    EstudianteSeccionSerializer,
    EstudianteSerializer,
    FacultadSerializer,
    HorarioSerializer,
    InstitucionSerializer,
    ModuloSerializer,
    NotaSerializer,
    PensumSerializer,
    RecursoSerializer,
    SeccionSerializer,
    SemestreSerializer,
    SesionVirtualSerializer,
    TareaSerializer,
)


class FilteredModelViewSet(viewsets.ModelViewSet):
    filter_fields = []

    def get_queryset(self):
        queryset = super().get_queryset()
        filters = {
            field: self.request.query_params.get(field)
            for field in self.filter_fields
            if self.request.query_params.get(field) not in (None, '')
        }
        return queryset.filter(**filters) if filters else queryset


class InstitucionViewSet(FilteredModelViewSet):
    queryset = Institucion.objects.all().order_by('id_institucion')
    serializer_class = InstitucionSerializer


class FacultadViewSet(FilteredModelViewSet):
    queryset = Facultad.objects.select_related('institucion').all().order_by('id_facultad')
    serializer_class = FacultadSerializer
    filter_fields = ['institucion_id']


class CarreraViewSet(FilteredModelViewSet):
    queryset = Carrera.objects.select_related('facultad').all().order_by('id_carrera')
    serializer_class = CarreraSerializer
    filter_fields = ['facultad_id']


class SemestreViewSet(FilteredModelViewSet):
    queryset = Semestre.objects.all().order_by('numero')
    serializer_class = SemestreSerializer


class CarreraSemestreViewSet(FilteredModelViewSet):
    queryset = CarreraSemestre.objects.select_related('carrera', 'semestre').all().order_by('id_carrera_semestre')
    serializer_class = CarreraSemestreSerializer
    filter_fields = ['carrera_id', 'semestre_id']


class CursoViewSet(FilteredModelViewSet):
    queryset = Curso.objects.select_related('institucion').all().order_by('id_curso')
    serializer_class = CursoSerializer
    filter_fields = ['institucion_id']


class PensumViewSet(FilteredModelViewSet):
    queryset = Pensum.objects.select_related('carrera_semestre', 'curso').all().order_by('id_pensum')
    serializer_class = PensumSerializer
    filter_fields = ['carrera_semestre_id', 'curso_id']


class DocenteViewSet(FilteredModelViewSet):
    queryset = Docente.objects.select_related('institucion').all().order_by('id_docente')
    serializer_class = DocenteSerializer
    filter_fields = ['institucion_id', 'id_usuario']


class EstudianteViewSet(FilteredModelViewSet):
    queryset = Estudiante.objects.select_related('institucion', 'carrera').all().order_by('id_estudiante')
    serializer_class = EstudianteSerializer
    filter_fields = ['institucion_id', 'carrera_id', 'id_usuario']


class AulaViewSet(FilteredModelViewSet):
    queryset = Aula.objects.select_related('institucion').all().order_by('id_aula')
    serializer_class = AulaSerializer
    filter_fields = ['institucion_id']


class ModuloViewSet(FilteredModelViewSet):
    queryset = Modulo.objects.select_related('institucion').all().order_by('id_modulo')
    serializer_class = ModuloSerializer
    filter_fields = ['institucion_id']


class HorarioViewSet(FilteredModelViewSet):
    queryset = Horario.objects.select_related('modulo', 'aula').all().order_by('id_horario')
    serializer_class = HorarioSerializer
    filter_fields = ['modulo_id', 'aula_id', 'dia']


class CicloAcademicoViewSet(FilteredModelViewSet):
    queryset = CicloAcademico.objects.select_related('institucion').all().order_by('id_ciclo')
    serializer_class = CicloAcademicoSerializer
    filter_fields = ['institucion_id']


class SeccionViewSet(FilteredModelViewSet):
    queryset = Seccion.objects.select_related('pensum', 'ciclo', 'docente', 'horario').all().order_by('id_seccion')
    serializer_class = SeccionSerializer
    filter_fields = ['pensum_id', 'ciclo_id', 'docente_id', 'horario_id']


class EstudianteSeccionViewSet(FilteredModelViewSet):
    queryset = EstudianteSeccion.objects.select_related('estudiante', 'seccion').all().order_by('id_estudiante_seccion')
    serializer_class = EstudianteSeccionSerializer
    filter_fields = ['estudiante_id', 'seccion_id']


class SesionVirtualViewSet(FilteredModelViewSet):
    queryset = SesionVirtual.objects.select_related('seccion').all().order_by('id_sesion_virtual')
    serializer_class = SesionVirtualSerializer
    filter_fields = ['seccion_id']


class AsistenciaViewSet(FilteredModelViewSet):
    queryset = Asistencia.objects.select_related('estudiante_seccion', 'sesion_virtual').all().order_by('id_asistencia')
    serializer_class = AsistenciaSerializer
    filter_fields = ['estudiante_seccion_id', 'sesion_virtual_id', 'estado']


class RecursoViewSet(FilteredModelViewSet):
    queryset = Recurso.objects.select_related('seccion').all().order_by('-fecha_publicacion')
    serializer_class = RecursoSerializer
    filter_fields = ['seccion_id', 'tipo']


class AnuncioViewSet(FilteredModelViewSet):
    queryset = Anuncio.objects.select_related('seccion').all().order_by('-fecha_publicacion')
    serializer_class = AnuncioSerializer
    filter_fields = ['seccion_id']


class TareaViewSet(FilteredModelViewSet):
    queryset = Tarea.objects.select_related('seccion').all().order_by('-fecha_publicacion')
    serializer_class = TareaSerializer
    filter_fields = ['seccion_id']


class EntregaViewSet(FilteredModelViewSet):
    queryset = Entrega.objects.select_related('tarea', 'estudiante').all().order_by('-fecha_entrega')
    serializer_class = EntregaSerializer
    filter_fields = ['tarea_id', 'estudiante_id']


class NotaViewSet(FilteredModelViewSet):
    queryset = Nota.objects.select_related('estudiante_seccion').all().order_by('id_nota')
    serializer_class = NotaSerializer
    filter_fields = ['estudiante_seccion_id', 'tipo']


def extract_bearer_token(request):
    authorization = request.headers.get('Authorization', '')
    if authorization.startswith('Bearer '):
        return authorization.replace('Bearer ', '', 1).strip()
    return request.data.get('token') or request.query_params.get('token')


@api_view(['GET'])
def health_check(request):
    return Response({'status': 'ok', 'api': 'aula'})


@api_view(['GET', 'POST'])
def contexto(request):
    token = extract_bearer_token(request)
    if not token:
        return Response({'detail': 'Debe enviar token.'}, status=status.HTTP_400_BAD_REQUEST)

    try:
        payload = signing.loads(token, salt='aula-virtual-auth', max_age=60 * 60 * 8)
    except SignatureExpired:
        return Response({'detail': 'Token expirado.'}, status=status.HTTP_401_UNAUTHORIZED)
    except BadSignature:
        return Response({'detail': 'Token invalido.'}, status=status.HTTP_401_UNAUTHORIZED)

    return Response({'ok': True, 'contexto': payload})
