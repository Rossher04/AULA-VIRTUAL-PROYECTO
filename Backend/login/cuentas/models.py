from django.contrib.auth.hashers import check_password, make_password
from django.db import models


class Rol(models.Model):
    ADMINISTRADOR = 'ADMINISTRADOR'
    DOCENTE = 'DOCENTE'
    ESTUDIANTE = 'ESTUDIANTE'

    tipo_rol = models.CharField(max_length=20, unique=True)

    def __str__(self):
        return self.tipo_rol


class Institucion(models.Model):
    nombre = models.CharField(max_length=150)
    dominio = models.CharField(max_length=100, unique=True)

    def __str__(self):
        return self.nombre


class Usuario(models.Model):
    institucion = models.ForeignKey(Institucion, on_delete=models.CASCADE, related_name='usuarios')
    usuario = models.CharField(max_length=50)
    contrasena_hash = models.CharField(max_length=255)
    rol = models.ForeignKey(Rol, on_delete=models.PROTECT, related_name='usuarios')
    activo = models.BooleanField(default=True)
    fecha_creacion = models.DateTimeField(auto_now_add=True)

    class Meta:
        constraints = [
            models.UniqueConstraint(fields=['institucion', 'usuario'], name='login_usuario_unico_por_institucion')
        ]

    def set_password(self, raw_password):
        self.contrasena_hash = make_password(raw_password)

    def check_password(self, raw_password):
        return check_password(raw_password, self.contrasena_hash)

    def __str__(self):
        return f'{self.usuario} ({self.rol.tipo_rol})'
