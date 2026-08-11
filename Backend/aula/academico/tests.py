from django.core import signing
from rest_framework.test import APITestCase


class AulaApiTests(APITestCase):
    def test_contexto_acepta_token_compartido(self):
        token = signing.dumps(
            {
                'id_usuario': 1,
                'id_institucion': 1,
                'institucion': 'UMES',
                'dominio': 'umes',
                'usuario': 'admin',
                'rol': 'ADMINISTRADOR',
            },
            salt='aula-virtual-auth',
        )

        response = self.client.get('/api/contexto/', HTTP_AUTHORIZATION=f'Bearer {token}')

        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.data['contexto']['id_usuario'], 1)
        self.assertEqual(response.data['contexto']['rol'], 'ADMINISTRADOR')

    def test_contexto_rechaza_token_faltante(self):
        response = self.client.get('/api/contexto/')

        self.assertEqual(response.status_code, 400)
