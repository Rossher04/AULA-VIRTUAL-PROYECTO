from django.urls import include, path
from rest_framework.routers import DefaultRouter

from . import views

router = DefaultRouter()
router.register('roles', views.RolViewSet)
router.register('instituciones', views.InstitucionViewSet)
router.register('usuarios', views.UsuarioViewSet)

urlpatterns = [
    path('', views.health_check, name='login-health-check'),
    path('login/', views.login, name='login'),
    path('validar-token/', views.validar_token, name='validar-token'),
    path('', include(router.urls)),
]
