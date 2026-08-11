from rest_framework.test import APITestCase

from .models import Rol


class LoginApiTests(APITestCase):
    def test_login_correcto_devuelve_token(self):
        response = self.client.post(
            '/api/login/',
            {'usuario': 'admin', 'contrasena': 'Admin123!', 'dominio': 'umes'},
            format='json',
        )

        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.data['ok'])
        self.assertIn('token', response.data)
        self.assertEqual(response.data['contexto']['rol'], Rol.ADMINISTRADOR)

    def test_login_invalido_devuelve_401(self):
        response = self.client.post(
            '/api/login/',
            {'usuario': 'admin', 'contrasena': 'incorrecta', 'dominio': 'umes'},
            format='json',
        )

        self.assertEqual(response.status_code, 401)
