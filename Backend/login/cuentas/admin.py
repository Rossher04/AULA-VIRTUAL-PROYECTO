from django.contrib import admin

from .models import Institucion, Rol, Usuario

admin.site.register(Rol)
admin.site.register(Institucion)
admin.site.register(Usuario)
