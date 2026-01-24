using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kowin_Login_AlvaroLaguna
{
    // Enum para los resultados del Login
    public enum ResultadoLogin { Exito, UsuarioNoExiste, PasswordIncorrecto, UsuarioBloqueado, UsuarioBaneado, ErrorBaseDatos }

    // Clase Usuario (Con campos de Admin y Config)
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        // Campos Admin
        public string Rol { get; set; } = "Nominal";
        public string Estado { get; set; } = "Activo";

        // Configuración
        public int Volumen { get; set; } = 50;
        public bool Notificaciones { get; set; } = true;
        public string Idioma { get; set; } = "Español";
        public bool IniciarWindows { get; set; } = false;
        public bool MinimizarBandeja { get; set; } = true;
        public bool MostrarFps { get; set; } = false;
        public bool ModoInvisible { get; set; } = false;
        public int LimiteDescarga { get; set; } = 0;
    }

    // Clases auxiliares
    public class MensajeChat { public string Texto { get; set; } public string Hora { get; set; } public string Alineacion { get; set; } public string ColorFondo { get; set; } }

    public class AmigoItem
    {
        public string Nombre { get; set; }
        public string EstadoTexto { get; set; }
        public string EstadoColor { get; set; }
        public int MensajesNoLeidos { get; set; }
        public string VisibilidadNotificacion { get; set; }
    }

    public static class UserManager
    {
        // CADENA DE CONEXIÓN
        private static string connectionString = "Server=localhost;Database=kowin_db;User ID=root;Password=Alvale288;";

        // ==========================================
        //  1. GESTIÓN DE AMIGOS Y CHAT (RESTAURADO)
        // ==========================================

        public static string EnviarSolicitudAmistad(string origen, string destino)
        {
            if (origen == destino) return "No puedes enviarte solicitud a ti mismo.";
            if (!ExisteUsuario(destino)) return "El usuario no existe.";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string check = "SELECT COUNT(*) FROM amistades WHERE (usuario_origen=@u1 AND usuario_destino=@u2) OR (usuario_origen=@u2 AND usuario_destino=@u1)";
                    MySqlCommand cmdCheck = new MySqlCommand(check, conn);
                    cmdCheck.Parameters.AddWithValue("@u1", origen);
                    cmdCheck.Parameters.AddWithValue("@u2", destino);

                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                        return "Ya existe una solicitud o amistad con este usuario.";

                    string insert = "INSERT INTO amistades (usuario_origen, usuario_destino, estado) VALUES (@u1, @u2, 'PENDIENTE')";
                    MySqlCommand cmdInsert = new MySqlCommand(insert, conn);
                    cmdInsert.Parameters.AddWithValue("@u1", origen);
                    cmdInsert.Parameters.AddWithValue("@u2", destino);
                    cmdInsert.ExecuteNonQuery();

                    return "Solicitud enviada correctamente.";
                }
                catch (Exception ex) { return "Error: " + ex.Message; }
            }
        }

        public static List<string> ObtenerSolicitudesPendientes(string usuarioDestino)
        {
            List<string> pendientes = new List<string>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT usuario_origen FROM amistades WHERE usuario_destino = @me AND estado = 'PENDIENTE'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@me", usuarioDestino);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read()) pendientes.Add(r.GetString(0));
                    }
                }
                catch { }
            }
            return pendientes;
        }

        public static int ContarSolicitudesPendientes(string usuarioDestino)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM amistades WHERE usuario_destino = @me AND estado = 'PENDIENTE'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@me", usuarioDestino);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { return 0; }
            }
        }

        public static void AceptarSolicitud(string usuarioActual, string solicitante)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE amistades SET estado = 'ACEPTADA' WHERE usuario_origen = @solicitante AND usuario_destino = @me";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@solicitante", solicitante);
                    cmd.Parameters.AddWithValue("@me", usuarioActual);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        // Obtener lista compleja con estado y notificaciones
        public static List<AmigoItem> ObtenerListaAmigosConEstado(string yo)
        {
            List<AmigoItem> lista = new List<AmigoItem>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // 1. Obtener amigos
                    string qA = "SELECT usuario_origen, usuario_destino FROM amistades WHERE (usuario_origen=@me OR usuario_destino=@me) AND estado='ACEPTADA'";
                    MySqlCommand c = new MySqlCommand(qA, conn); c.Parameters.AddWithValue("@me", yo);

                    List<string> friends = new List<string>();
                    using (MySqlDataReader r = c.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string u1 = r.GetString(0);
                            string u2 = r.GetString(1);
                            friends.Add(u1 == yo ? u2 : u1);
                        }
                    }

                    // 2. Obtener detalles de cada amigo
                    foreach (string f in friends)
                    {
                        bool inv = false;
                        // Chequear si es invisible
                        string qS = "SELECT modo_invisible FROM usuarios WHERE nombre_usuario=@f";
                        MySqlCommand cS = new MySqlCommand(qS, conn); cS.Parameters.AddWithValue("@f", f);
                        object res = cS.ExecuteScalar(); if (res != null) inv = Convert.ToBoolean(res);

                        // Contar mensajes no leídos
                        string qC = "SELECT COUNT(*) FROM chat_mensajes WHERE destinatario=@me AND remitente=@f AND leido=FALSE";
                        MySqlCommand cC = new MySqlCommand(qC, conn); cC.Parameters.AddWithValue("@me", yo); cC.Parameters.AddWithValue("@f", f);
                        int unread = Convert.ToInt32(cC.ExecuteScalar());

                        lista.Add(new AmigoItem
                        {
                            Nombre = f,
                            EstadoTexto = inv ? "Desconectado" : "Conectado",
                            EstadoColor = inv ? "Gray" : "Green",
                            MensajesNoLeidos = unread,
                            VisibilidadNotificacion = unread > 0 ? "Visible" : "Collapsed"
                        });
                    }
                }
                catch { }
            }
            return lista;
        }

        // Chat
        public static void EnviarMensaje(string remitente, string destinatario, string mensaje)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try { conn.Open(); string q = "INSERT INTO chat_mensajes (remitente, destinatario, mensaje) VALUES (@r, @d, @m)"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@r", remitente); c.Parameters.AddWithValue("@d", destinatario); c.Parameters.AddWithValue("@m", mensaje); c.ExecuteNonQuery(); } catch { }
            }
        }

        public static List<MensajeChat> ObtenerHistorialChat(string yo, string amigo)
        {
            List<MensajeChat> h = new List<MensajeChat>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); string q = "SELECT remitente, mensaje, DATE_FORMAT(fecha, '%H:%i') as hora FROM chat_mensajes WHERE (remitente=@y AND destinatario=@a) OR (remitente=@a AND destinatario=@y) ORDER BY fecha ASC"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@y", yo); c.Parameters.AddWithValue("@a", amigo);
                    using (MySqlDataReader r = c.ExecuteReader()) { while (r.Read()) { string rem = r.GetString("remitente"); bool mio = (rem == yo); h.Add(new MensajeChat { Texto = r.GetString("mensaje"), Hora = r.GetString("hora"), Alineacion = mio ? "Right" : "Left", ColorFondo = mio ? "#800000" : "#333333" }); } }
                }
                catch { }
            }
            return h;
        }

        public static void MarcarMensajesLeidos(string yo, string amigo)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try { conn.Open(); string q = "UPDATE chat_mensajes SET leido=TRUE WHERE destinatario=@y AND remitente=@a AND leido=FALSE"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@y", yo); c.Parameters.AddWithValue("@a", amigo); c.ExecuteNonQuery(); } catch { }
            }
        }

        // ==========================================
        //  2. MÉTODOS DE ADMINISTRACIÓN (CRUD)
        // ==========================================

        public static List<Usuario> ObtenerTodosUsuarios(string filtroNombre = "")
        {
            List<Usuario> lista = new List<Usuario>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM usuarios";
                    if (!string.IsNullOrEmpty(filtroNombre)) query += " WHERE nombre_usuario LIKE @filtro";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filtroNombre)) cmd.Parameters.AddWithValue("@filtro", "%" + filtroNombre + "%");

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read()) lista.Add(LeerUsuarioDesdeReader(r));
                    }
                }
                catch { }
            }
            return lista;
        }

        public static bool InsertarUsuarioAdmin(Usuario u, string adminResponsable)
        {
            if (ExisteUsuario(u.NombreUsuario)) return false;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try { conn.Open(); string q = "INSERT INTO usuarios (nombre_usuario, password, email, rol, estado) VALUES (@u, @p, @e, @r, @s)"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@u", u.NombreUsuario); c.Parameters.AddWithValue("@p", u.Password); c.Parameters.AddWithValue("@e", u.Email); c.Parameters.AddWithValue("@r", u.Rol); c.Parameters.AddWithValue("@s", u.Estado); c.ExecuteNonQuery(); RegistrarLog(conn, adminResponsable, "Crear", "Usuario " + u.NombreUsuario); return true; } catch { return false; }
            }
        }

        public static bool ActualizarUsuarioAdmin(Usuario u, string adminResponsable)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try { conn.Open(); string q = "UPDATE usuarios SET email=@e, rol=@r, estado=@s, password=@p WHERE nombre_usuario=@u"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@e", u.Email); c.Parameters.AddWithValue("@r", u.Rol); c.Parameters.AddWithValue("@s", u.Estado); c.Parameters.AddWithValue("@p", u.Password); c.Parameters.AddWithValue("@u", u.NombreUsuario); c.ExecuteNonQuery(); RegistrarLog(conn, adminResponsable, "Editar", "Usuario " + u.NombreUsuario); return true; } catch { return false; }
            }
        }

        public static bool EliminarUsuario(string u, string adminResponsable)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try { conn.Open(); string q = "DELETE FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@u", u); c.ExecuteNonQuery(); RegistrarLog(conn, adminResponsable, "Eliminar", "Usuario " + u); return true; } catch { return false; }
            }
        }

        private static void RegistrarLog(MySqlConnection conn, string admin, string accion, string detalle)
        {
            try { string q = "INSERT INTO log_actividad (admin_responsable, accion, detalle) VALUES (@a, @ac, @d)"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@a", admin); c.Parameters.AddWithValue("@ac", accion); c.Parameters.AddWithValue("@d", detalle); c.ExecuteNonQuery(); } catch { }
        }

        // ==========================================
        //  3. LOGIN, REGISTRO Y SOPORTE
        // ==========================================

        public static ResultadoLogin ValidarUsuario(string usuario, string passwordInput)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT password, intentos_fallidos, fecha_fin_bloqueo, estado FROM usuarios WHERE nombre_usuario = @user";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", usuario);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return ResultadoLogin.UsuarioNoExiste;

                        string dbPass = r.GetString("password");
                        int intentos = r.GetInt32("intentos_fallidos");
                        string estado = ExisteColumna(r, "estado") && !r.IsDBNull(r.GetOrdinal("estado")) ? r.GetString("estado") : "Activo";
                        DateTime? fBloqueo = r.IsDBNull(r.GetOrdinal("fecha_fin_bloqueo")) ? (DateTime?)null : r.GetDateTime("fecha_fin_bloqueo");
                        r.Close();

                        if (estado == "Baneado") return ResultadoLogin.UsuarioBaneado;
                        if (fBloqueo != null && fBloqueo > DateTime.Now) return ResultadoLogin.UsuarioBloqueado;

                        if (dbPass == passwordInput)
                        {
                            new MySqlCommand($"UPDATE usuarios SET intentos_fallidos=0, fecha_fin_bloqueo=NULL WHERE nombre_usuario='{usuario}'", conn).ExecuteNonQuery();
                            return ResultadoLogin.Exito;
                        }
                        else
                        {
                            int ni = intentos + 1;
                            if (ni >= 3)
                            {
                                new MySqlCommand($"UPDATE usuarios SET intentos_fallidos=0, fecha_fin_bloqueo=DATE_ADD(NOW(), INTERVAL 1 MINUTE) WHERE nombre_usuario='{usuario}'", conn).ExecuteNonQuery();
                                return ResultadoLogin.UsuarioBloqueado;
                            }
                            else
                            {
                                new MySqlCommand($"UPDATE usuarios SET intentos_fallidos={ni} WHERE nombre_usuario='{usuario}'", conn).ExecuteNonQuery();
                                return ResultadoLogin.PasswordIncorrecto;
                            }
                        }
                    }
                }
                catch { return ResultadoLogin.ErrorBaseDatos; }
            }
        }

        public static Usuario ObtenerUsuario(string nombreUsuario)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM usuarios WHERE nombre_usuario = @user";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", nombreUsuario);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read()) return LeerUsuarioDesdeReader(r);
                    }
                }
                catch { }
            }
            return null;
        }

        public static bool RegistrarUsuario(Usuario u)
        {
            using (MySqlConnection c = new MySqlConnection(connectionString))
            {
                try { c.Open(); string q = "INSERT INTO usuarios (nombre_usuario, password, email) VALUES (@u, @p, @e)"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u.NombreUsuario); x.Parameters.AddWithValue("@p", u.Password); x.Parameters.AddWithValue("@e", u.Email); x.ExecuteNonQuery(); return true; } catch { return false; }
            }
        }

        public static bool ExisteUsuario(string u)
        {
            using (MySqlConnection c = new MySqlConnection(connectionString))
            {
                try { c.Open(); string q = "SELECT COUNT(*) FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); return Convert.ToInt32(x.ExecuteScalar()) > 0; } catch { return false; }
            }
        }

        public static bool EnviarTicketSoporte(string m, string n, string e, string msg, string u)
        {
            using (MySqlConnection c = new MySqlConnection(connectionString))
            {
                try { c.Open(); string q = "INSERT INTO soporte_tickets (usuario_logueado, motivo, nombre_contacto, email_contacto, mensaje) VALUES (@u, @m, @n, @e, @msg)"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); x.Parameters.AddWithValue("@m", m); x.Parameters.AddWithValue("@n", n); x.Parameters.AddWithValue("@e", e); x.Parameters.AddWithValue("@msg", msg); x.ExecuteNonQuery(); return true; } catch { return false; }
            }
        }

        public static string ActualizarConfiguracion(Usuario u)
        {
            using (MySqlConnection c = new MySqlConnection(connectionString))
            {
                try { c.Open(); string q = "UPDATE usuarios SET volumen=@v, notificaciones=@n, idioma=@i, iniciar_windows=@iw, minimizar_bandeja=@mb, mostrar_fps=@f, modo_invisible=@m, limite_descarga=@l WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@v", u.Volumen); x.Parameters.AddWithValue("@n", u.Notificaciones); x.Parameters.AddWithValue("@i", u.Idioma); x.Parameters.AddWithValue("@iw", u.IniciarWindows); x.Parameters.AddWithValue("@mb", u.MinimizarBandeja); x.Parameters.AddWithValue("@f", u.MostrarFps); x.Parameters.AddWithValue("@m", u.ModoInvisible); x.Parameters.AddWithValue("@l", u.LimiteDescarga); x.Parameters.AddWithValue("@u", u.NombreUsuario); x.ExecuteNonQuery(); return null; } catch (Exception e) { return e.Message; }
            }
        }

        // HELPER PARA LEER USUARIO (Evita duplicados)
        private static Usuario LeerUsuarioDesdeReader(MySqlDataReader r)
        {
            return new Usuario
            {
                Id = r.GetInt32("id"),
                NombreUsuario = r.GetString("nombre_usuario"),
                Email = r.GetString("email"),
                Password = r.GetString("password"),
                Rol = ExisteColumna(r, "rol") && !r.IsDBNull(r.GetOrdinal("rol")) ? r.GetString("rol") : "Nominal",
                Estado = ExisteColumna(r, "estado") && !r.IsDBNull(r.GetOrdinal("estado")) ? r.GetString("estado") : "Activo",
                Volumen = ExisteColumna(r, "volumen") && !r.IsDBNull(r.GetOrdinal("volumen")) ? r.GetInt32("volumen") : 50,
                Notificaciones = ExisteColumna(r, "notificaciones") && !r.IsDBNull(r.GetOrdinal("notificaciones")) && r.GetBoolean("notificaciones"),
                Idioma = ExisteColumna(r, "idioma") && !r.IsDBNull(r.GetOrdinal("idioma")) ? r.GetString("idioma") : "Español",
                ModoInvisible = ExisteColumna(r, "modo_invisible") && !r.IsDBNull(r.GetOrdinal("modo_invisible")) && r.GetBoolean("modo_invisible"),
                // ... resto de campos config opcionales
            };
        }

        private static bool ExisteColumna(MySqlDataReader r, string n) { for (int i = 0; i < r.FieldCount; i++) if (r.GetName(i).Equals(n, StringComparison.OrdinalIgnoreCase)) return true; return false; }
    }
}