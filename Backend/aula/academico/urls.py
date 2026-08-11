from django.urls import include, path
from rest_framework.routers import DefaultRouter

from . import views

router = DefaultRouter()
router.register('instituciones', views.InstitucionViewSet)
router.register('facultades', views.FacultadViewSet)
router.register('carreras', views.CarreraViewSet)
router.register('semestres', views.SemestreViewSet)
router.register('carrera-semestres', views.CarreraSemestreViewSet)
router.register('cursos', views.CursoViewSet)
router.register('pensum', views.PensumViewSet)
router.register('docentes', views.DocenteViewSet)
router.register('estudiantes', views.EstudianteViewSet)
router.register('aulas', views.AulaViewSet)
router.register('modulos', views.ModuloViewSet)
router.register('horarios', views.HorarioViewSet)
router.register('ciclos', views.CicloAcademicoViewSet)
router.register('secciones', views.SeccionViewSet)
router.register('estudiante-secciones', views.EstudianteSeccionViewSet)
router.register('sesiones-virtuales', views.SesionVirtualViewSet)
router.register('asistencias', views.AsistenciaViewSet)
router.register('recursos', views.RecursoViewSet)
router.register('anuncios', views.AnuncioViewSet)
router.register('tareas', views.TareaViewSet)
router.register('entregas', views.EntregaViewSet)
router.register('notas', views.NotaViewSet)

urlpatterns = [
    path('', views.health_check, name='aula-health-check'),
    path('contexto/', views.contexto, name='aula-contexto'),
    path('', include(router.urls)),
]
