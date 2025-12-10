using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kowin_Login_AlvaroLaguna
{
    public enum ResultadoLogin { Exito, UsuarioNoExiste, PasswordIncorrecto, UsuarioBloqueado, ErrorBaseDatos }

    public class Usuario
    {
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Volumen { get; set; } = 50;
        public bool Notificaciones { get; set; } = true;
        public string Idioma { get; set; } = "Español";
        public bool IniciarWindows { get; set; } = false;
        public bool MinimizarBandeja { get; set; } = true;
        public bool MostrarFps { get; set; } = false;
        public bool ModoInvisible { get; set; } = false;
        public int LimiteDescarga { get; set; } = 0;
    }

    public class MensajeChat
    {
        public string Texto { get; set; }
        public string Hora { get; set; }
        public string Alineacion { get; set; }
        public string ColorFondo { get; set; }
    }

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
        private static string connectionString = "Server=localhost;Database=kowin_db;User ID=root;Password=Alvale288;";

        // === SOPORTE TÉCNICO ===
        public static bool EnviarTicketSoporte(string motivo, string nombre, string email, string mensaje, string usuarioActual)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO soporte_tickets (usuario_logueado, motivo, nombre_contacto, email_contacto, mensaje) VALUES (@u, @mot, @nom, @em, @msg)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuarioActual);
                    cmd.Parameters.AddWithValue("@mot", motivo);
                    cmd.Parameters.AddWithValue("@nom", nombre);
                    cmd.Parameters.AddWithValue("@em", email);
                    cmd.Parameters.AddWithValue("@msg", mensaje);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        // === NOTIFICACIONES Y CHAT ===
        public static void MarcarMensajesLeidos(string yo, string amigo)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try { conn.Open(); string q = "UPDATE chat_mensajes SET leido = TRUE WHERE destinatario = @yo AND remitente = @amigo AND leido = FALSE"; MySqlCommand c = new MySqlCommand(q, conn); c.Parameters.AddWithValue("@yo", yo); c.Parameters.AddWithValue("@amigo", amigo); c.ExecuteNonQuery(); } catch { }
            }
        }

        public static List<AmigoItem> ObtenerListaAmigosConEstado(string yo)
        {
            List<AmigoItem> lista = new List<AmigoItem>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string qA = "SELECT usuario_origen, usuario_destino FROM amistades WHERE (usuario_origen=@me OR usuario_destino=@me) AND estado='ACEPTADA'";
                    MySqlCommand c = new MySqlCommand(qA, conn); c.Parameters.AddWithValue("@me", yo);
                    List<string> friends = new List<string>();
                    using (MySqlDataReader r = c.ExecuteReader()) { while (r.Read()) { string u1 = r.GetString(0); string u2 = r.GetString(1); friends.Add(u1 == yo ? u2 : u1); } }

                    foreach (string f in friends)
                    {
                        bool inv = false;
                        string qS = "SELECT modo_invisible FROM usuarios WHERE nombre_usuario=@f";
                        MySqlCommand cS = new MySqlCommand(qS, conn); cS.Parameters.AddWithValue("@f", f);
                        object res = cS.ExecuteScalar(); if (res != null) inv = Convert.ToBoolean(res);

                        string qC = "SELECT COUNT(*) FROM chat_mensajes WHERE destinatario=@me AND remitente=@f AND leido=FALSE";
                        MySqlCommand cC = new MySqlCommand(qC, conn); cC.Parameters.AddWithValue("@me", yo); cC.Parameters.AddWithValue("@f", f);
                        int unread = Convert.ToInt32(cC.ExecuteScalar());

                        lista.Add(new AmigoItem { Nombre = f, EstadoTexto = inv ? "Desconectado" : "Conectado", EstadoColor = inv ? "Gray" : "Green", MensajesNoLeidos = unread, VisibilidadNotificacion = unread > 0 ? "Visible" : "Collapsed" });
                    }
                }
                catch { }
            }
            return lista;
        }

        public static int ContarSolicitudesPendientes(string yo) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT COUNT(*) FROM amistades WHERE usuario_destino=@me AND estado='PENDIENTE'"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@me", yo); return Convert.ToInt32(x.ExecuteScalar()); } catch { return 0; } } }

        public static void EnviarMensaje(string r, string d, string m) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "INSERT INTO chat_mensajes (remitente, destinatario, mensaje) VALUES (@r, @d, @m)"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@r", r); x.Parameters.AddWithValue("@d", d); x.Parameters.AddWithValue("@m", m); x.ExecuteNonQuery(); } catch { } } }

        public static List<MensajeChat> ObtenerHistorialChat(string yo, string amigo)
        {
            List<MensajeChat> h = new List<MensajeChat>();
            using (MySqlConnection c = new MySqlConnection(connectionString))
            {
                try
                {
                    c.Open(); string q = "SELECT remitente, mensaje, DATE_FORMAT(fecha, '%H:%i') as hora FROM chat_mensajes WHERE (remitente=@y AND destinatario=@a) OR (remitente=@a AND destinatario=@y) ORDER BY fecha ASC"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@y", yo); x.Parameters.AddWithValue("@a", amigo);
                    using (MySqlDataReader r = x.ExecuteReader()) { while (r.Read()) { string rem = r.GetString("remitente"); bool mio = (rem == yo); h.Add(new MensajeChat { Texto = r.GetString("mensaje"), Hora = r.GetString("hora"), Alineacion = mio ? "Right" : "Left", ColorFondo = mio ? "#800000" : "#333333" }); } }
                }
                catch { }
            }
            return h;
        }

        // === RESTO BASE ===
        public static string EnviarSolicitudAmistad(string o, string d) { if (o == d) return "No puedes auto-enviarte."; if (!ExisteUsuario(d)) return "Usuario no existe."; using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT COUNT(*) FROM amistades WHERE (usuario_origen=@1 AND usuario_destino=@2) OR (usuario_origen=@2 AND usuario_destino=@1)"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@1", o); x.Parameters.AddWithValue("@2", d); if (Convert.ToInt32(x.ExecuteScalar()) > 0) return "Ya existe relación."; string i = "INSERT INTO amistades (usuario_origen, usuario_destino, estado) VALUES (@1, @2, 'PENDIENTE')"; MySqlCommand y = new MySqlCommand(i, c); y.Parameters.AddWithValue("@1", o); y.Parameters.AddWithValue("@2", d); y.ExecuteNonQuery(); return "Solicitud enviada correctamente."; } catch (Exception e) { return "Error: " + e.Message; } } }
        public static List<string> ObtenerSolicitudesPendientes(string d) { List<string> l = new List<string>(); using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT usuario_origen FROM amistades WHERE usuario_destino=@d AND estado='PENDIENTE'"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@d", d); using (MySqlDataReader r = x.ExecuteReader()) { while (r.Read()) l.Add(r.GetString(0)); } } catch { } } return l; }
        public static void AceptarSolicitud(string y, string s) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "UPDATE amistades SET estado='ACEPTADA' WHERE usuario_origen=@s AND usuario_destino=@y"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@s", s); x.Parameters.AddWithValue("@y", y); x.ExecuteNonQuery(); } catch { } } }
        public static List<string> ObtenerAmigos(string u) { List<string> l = new List<string>(); using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT usuario_origen, usuario_destino FROM amistades WHERE (usuario_origen=@u OR usuario_destino=@u) AND estado='ACEPTADA'"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); using (MySqlDataReader r = x.ExecuteReader()) { while (r.Read()) { string u1 = r.GetString(0); string u2 = r.GetString(1); l.Add(u1 == u ? u2 : u1); } } } catch { } } return l; }

        public static Usuario ObtenerUsuario(string u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT * FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); using (MySqlDataReader r = x.ExecuteReader()) { if (r.Read()) return new Usuario { NombreUsuario = r.GetString("nombre_usuario"), Email = r.GetString("email"), Volumen = r.IsDBNull(r.GetOrdinal("volumen")) ? 50 : r.GetInt32("volumen"), Notificaciones = r.IsDBNull(r.GetOrdinal("notificaciones")) ? true : r.GetBoolean("notificaciones"), Idioma = ExisteColumna(r, "idioma") && !r.IsDBNull(r.GetOrdinal("idioma")) ? r.GetString("idioma") : "Español" }; } } catch { } } return null; }
        private static bool ExisteColumna(MySqlDataReader r, string n) { for (int i = 0; i < r.FieldCount; i++) if (r.GetName(i).Equals(n, StringComparison.OrdinalIgnoreCase)) return true; return false; }

        public static string ActualizarConfiguracion(Usuario u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "UPDATE usuarios SET volumen=@v, notificaciones=@n, idioma=@i, iniciar_windows=@iw, minimizar_bandeja=@mb, mostrar_fps=@fp, modo_invisible=@mi, limite_descarga=@ld WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@v", u.Volumen); x.Parameters.AddWithValue("@n", u.Notificaciones); x.Parameters.AddWithValue("@i", u.Idioma); x.Parameters.AddWithValue("@iw", u.IniciarWindows); x.Parameters.AddWithValue("@mb", u.MinimizarBandeja); x.Parameters.AddWithValue("@fp", u.MostrarFps); x.Parameters.AddWithValue("@mi", u.ModoInvisible); x.Parameters.AddWithValue("@ld", u.LimiteDescarga); x.Parameters.AddWithValue("@u", u.NombreUsuario); x.ExecuteNonQuery(); return null; } catch (Exception e) { return e.Message; } } }
        public static bool RegistrarUsuario(Usuario u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "INSERT INTO usuarios (nombre_usuario, password, email) VALUES (@u, @p, @e)"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u.NombreUsuario); x.Parameters.AddWithValue("@p", u.Password); x.Parameters.AddWithValue("@e", u.Email); x.ExecuteNonQuery(); return true; } catch { return false; } } }
        public static bool ExisteUsuario(string u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT COUNT(*) FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); return Convert.ToInt32(x.ExecuteScalar()) > 0; } catch { return false; } } }
        public static ResultadoLogin ValidarUsuario(string u, string p) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT password, intentos_fallidos, fecha_fin_bloqueo FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); using (MySqlDataReader r = x.ExecuteReader()) { if (!r.Read()) return ResultadoLogin.UsuarioNoExiste; string dbP = r.GetString("password"); int i = r.GetInt32("intentos_fallidos"); DateTime? f = r.IsDBNull(r.GetOrdinal("fecha_fin_bloqueo")) ? (DateTime?)null : r.GetDateTime("fecha_fin_bloqueo"); r.Close(); if (f != null && f > DateTime.Now) return ResultadoLogin.UsuarioBloqueado; if (dbP == p) { ResetearIntentos(c, u); return ResultadoLogin.Exito; } else return ManejarFalloLogin(c, u, i); } } catch { return ResultadoLogin.ErrorBaseDatos; } } }
        private static void ResetearIntentos(MySqlConnection c, string u) { string q = "UPDATE usuarios SET intentos_fallidos=0, fecha_fin_bloqueo=NULL WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); x.ExecuteNonQuery(); }
        private static ResultadoLogin ManejarFalloLogin(MySqlConnection c, string u, int i) { int ni = i + 1; if (ni >= 3) { string q = "UPDATE usuarios SET intentos_fallidos=0, fecha_fin_bloqueo=DATE_ADD(NOW(), INTERVAL 1 MINUTE) WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); x.ExecuteNonQuery(); return ResultadoLogin.UsuarioBloqueado; } else { string q = "UPDATE usuarios SET intentos_fallidos=@i WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); x.Parameters.AddWithValue("@i", ni); x.ExecuteNonQuery(); return ResultadoLogin.PasswordIncorrecto; } }
    }
}