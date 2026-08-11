from rest_framework import serializers

from .models import Institucion, Rol, Usuario


class RolSerializer(serializers.ModelSerializer):
    class Meta:
        model = Rol
        fields = '__all__'


class InstitucionSerializer(serializers.ModelSerializer):
    class Meta:
        model = Institucion
        fields = '__all__'


class UsuarioSerializer(serializers.ModelSerializer):
    contrasena = serializers.CharField(write_only=True, required=False)
    rol_nombre = serializers.CharField(source='rol.tipo_rol', read_only=True)
    rol_tipo = serializers.CharField(write_only=True, required=False)
    institucion_nombre = serializers.CharField(source='institucion.nombre', read_only=True)

    class Meta:
        model = Usuario
        fields = [
            'id',
            'institucion',
            'institucion_nombre',
            'usuario',
            'contrasena',
            'rol',
            'rol_nombre',
            'rol_tipo',
            'activo',
            'fecha_creacion',
        ]
        read_only_fields = ['fecha_creacion']

    def create(self, validated_data):
        password = validated_data.pop('contrasena', 'Temporal123!')
        rol_tipo = validated_data.pop('rol_tipo', None)
        if rol_tipo:
            validated_data['rol'] = Rol.objects.get(tipo_rol=rol_tipo)
        usuario = Usuario(**validated_data)
        usuario.set_password(password)
        usuario.save()
        return usuario

    def update(self, instance, validated_data):
        password = validated_data.pop('contrasena', None)
        for field, value in validated_data.items():
            setattr(instance, field, value)
        if password:
            instance.set_password(password)
        instance.save()
        return instance
