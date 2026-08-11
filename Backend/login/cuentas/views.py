from django.core import signing
from django.core.signing import BadSignature, SignatureExpired
from rest_framework import status, viewsets
from rest_framework.decorators import api_view
from rest_framework.response import Response

from .models import Institucion, Rol, Usuario
from .serializers import InstitucionSerializer, RolSerializer, UsuarioSerializer


class RolViewSet(viewsets.ModelViewSet):
    queryset = Rol.objects.all().order_by('id')
    serializer_class = RolSerializer


class InstitucionViewSet(viewsets.ModelViewSet):
    queryset = Institucion.objects.all().order_by('id')
    serializer_class = InstitucionSerializer


class UsuarioViewSet(viewsets.ModelViewSet):
    queryset = Usuario.objects.select_related('institucion', 'rol').all().order_by('id')
    serializer_class = UsuarioSerializer

    def get_queryset(self):
        queryset = super().get_queryset()
        institucion = self.request.query_params.get('institucion_id')
        rol = self.request.query_params.get('rol_id')
        activo = self.request.query_params.get('activo')
        if institucion:
            queryset = queryset.filter(institucion_id=institucion)
        if rol:
            queryset = queryset.filter(rol_id=rol)
        if activo:
            queryset = queryset.filter(activo=activo.lower() == 'true')
        return queryset


def build_auth_payload(usuario):
    return {
        'id_usuario': usuario.id,
        'id_institucion': usuario.institucion_id,
        'institucion': usuario.institucion.nombre,
        'dominio': usuario.institucion.dominio,
        'usuario': usuario.usuario,
        'rol': usuario.rol.tipo_rol,
    }


def extract_bearer_token(request):
    authorization = request.headers.get('Authorization', '')
    if authorization.startswith('Bearer '):
        return authorization.replace('Bearer ', '', 1).strip()
    return request.data.get('token') or request.query_params.get('token')


@api_view(['GET'])
def health_check(request):
    return Response({'status': 'ok', 'api': 'login'})


@api_view(['POST'])
def login(request):
    usuario_login = request.data.get('usuario') or request.data.get('correo')
    contrasena = request.data.get('contrasena') or request.data.get('password')
    dominio = request.data.get('dominio', 'umes')
    institucion_id = request.data.get('institucion') or request.data.get('id_institucion')

    if not usuario_login or not contrasena:
        return Response({'detail': 'Debe enviar usuario y contrasena.'}, status=status.HTTP_400_BAD_REQUEST)

    usuarios = Usuario.objects.select_related('institucion', 'rol').filter(usuario=usuario_login, activo=True)
    if institucion_id:
        usuarios = usuarios.filter(institucion_id=institucion_id)
    else:
        usuarios = usuarios.filter(institucion__dominio=dominio)

    usuario = usuarios.first()
    if not usuario or not usuario.check_password(contrasena):
        return Response({'detail': 'Credenciales invalidas.'}, status=status.HTTP_401_UNAUTHORIZED)

    contexto = build_auth_payload(usuario)
    token = signing.dumps(contexto, salt='aula-virtual-auth')
    return Response({'ok': True, 'token': token, 'usuario': UsuarioSerializer(usuario).data, 'contexto': contexto})


@api_view(['GET', 'POST'])
def validar_token(request):
    token = extract_bearer_token(request)
    if not token:
        return Response({'detail': 'Debe enviar token.'}, status=status.HTTP_400_BAD_REQUEST)

    try:
        contexto = signing.loads(token, salt='aula-virtual-auth', max_age=60 * 60 * 8)
    except SignatureExpired:
        return Response({'detail': 'Token expirado.'}, status=status.HTTP_401_UNAUTHORIZED)
    except BadSignature:
        return Response({'detail': 'Token invalido.'}, status=status.HTTP_401_UNAUTHORIZED)

    return Response({'ok': True, 'contexto': contexto})
