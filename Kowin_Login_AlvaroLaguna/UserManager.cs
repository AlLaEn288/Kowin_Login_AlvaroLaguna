using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Kowin_Login_AlvaroLaguna
{
    // Usamos un Enum para saber EXACTAMENTE qué pasó en el login
    public enum ResultadoLogin
    {
        Exito,
        UsuarioNoExiste,      // TC 03
        PasswordIncorrecto,   // TC 02
        UsuarioBloqueado,     // TC 07
        ErrorGenerico
    }

    public class Usuario
    {
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        // Nuevos campos para el bloqueo (TC 07)
        public int IntentosFallidos { get; set; } = 0;
        public DateTime FechaFinBloqueo { get; set; } = DateTime.MinValue;
    }

    public static class UserManager
    {
        private static string rutaArchivo = "usuarios.xml";

        public static List<Usuario> CargarUsuarios()
        {
            if (!File.Exists(rutaArchivo))
                return new List<Usuario>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<Usuario>));
            using (FileStream stream = new FileStream(rutaArchivo, FileMode.Open))
            {
                return (List<Usuario>)serializer.Deserialize(stream);
            }
        }

        public static void GuardarTodos(List<Usuario> usuarios)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Usuario>));
            using (FileStream stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                serializer.Serialize(stream, usuarios);
            }
        }

        public static void RegistrarUsuario(Usuario nuevoUsuario)
        {
            var usuarios = CargarUsuarios();
            usuarios.Add(nuevoUsuario);
            GuardarTodos(usuarios);
        }

        public static ResultadoLogin ValidarUsuario(string nombreUser, string passwordInput)
        {
            var usuarios = CargarUsuarios();
            var usuario = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUser);

            // TC 03: El usuario no existe
            if (usuario == null)
            {
                return ResultadoLogin.UsuarioNoExiste;
            }

            // TC 07: Comprobar si está bloqueado
            if (usuario.FechaFinBloqueo > DateTime.Now)
            {
                return ResultadoLogin.UsuarioBloqueado;
            }

            // Comprobar contraseña
            if (usuario.Password == passwordInput)
            {
                // TC 01: Éxito -> Reseteamos intentos y guardamos
                usuario.IntentosFallidos = 0;
                usuario.FechaFinBloqueo = DateTime.MinValue;
                GuardarTodos(usuarios);
                return ResultadoLogin.Exito;
            }
            else
            {
                // TC 02 y TC 07: Fallo -> Sumamos intentos
                usuario.IntentosFallidos++;

                // Si llega a 3 fallos, bloqueamos 1 minuto
                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.FechaFinBloqueo = DateTime.Now.AddMinutes(1); // Bloqueo de 1 minuto
                    usuario.IntentosFallidos = 0; // Opcional: resetear contador tras bloquear
                    GuardarTodos(usuarios);
                    return ResultadoLogin.UsuarioBloqueado;
                }

                GuardarTodos(usuarios);
                return ResultadoLogin.PasswordIncorrecto;
            }
        }

        public static bool ExisteUsuario(string usuario)
        {
            var usuarios = CargarUsuarios();
            return usuarios.Any(u => u.NombreUsuario == usuario);
        }
    }
}