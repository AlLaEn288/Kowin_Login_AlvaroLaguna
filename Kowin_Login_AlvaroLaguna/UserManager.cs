using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kowin_Login_AlvaroLaguna
{
    public enum ResultadoLogin { Exito, UsuarioNoExiste, PasswordIncorrecto, UsuarioBloqueado, UsuarioBaneado, ErrorBaseDatos }

    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; } = "Nominal";
        public string Estado { get; set; } = "Activo";
        public int Volumen { get; set; } = 50;
        public bool Notificaciones { get; set; } = true;
        public string Idioma { get; set; } = "Español";
        public bool IniciarWindows { get; set; } = false;
        public bool MinimizarBandeja { get; set; } = true;
        public bool MostrarFps { get; set; } = false;
        public bool ModoInvisible { get; set; } = false;
        public int LimiteDescarga { get; set; } = 0;
    }

    // CLASE JUEGO ACTUALIZADA (CON EDAD Y DESCRIPCIÓN)
    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; } // Ya existía en BD, ahora la usamos
        public string ImagenUrl { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public string Fabricante { get; set; }
        public int Anio { get; set; }
        public int EdadMinima { get; set; } // <--- NUEVO
        public bool Visible { get; set; } = true;
        public bool Adquirido { get; set; }
    }

    public class MensajeChat { public string Texto { get; set; } public string Hora { get; set; } public string Alineacion { get; set; } public string ColorFondo { get; set; } }
    public class AmigoItem { public string Nombre { get; set; } public string EstadoTexto { get; set; } public string EstadoColor { get; set; } public int MensajesNoLeidos { get; set; } public string VisibilidadNotificacion { get; set; } }

    public static class UserManager
    {
        private static string connectionString = "Server=localhost;Database=kowin_db;User ID=root;Password=Alvale288;";

        // === GESTIÓN DE CATÁLOGO ===

        public static List<Juego> ObtenerCatalogo(string categoria = null, bool soloVisibles = true)
        {
            List<Juego> lista = new List<Juego>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM videojuegos";
                    List<string> conds = new List<string>();
                    if (soloVisibles) conds.Add("visible = 1");
                    if (categoria != null) conds.Add("categoria = @cat");
                    if (conds.Count > 0) query += " WHERE " + string.Join(" AND ", conds);

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (categoria != null) cmd.Parameters.AddWithValue("@cat", categoria);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read()) lista.Add(LeerJuego(r));
                    }
                }
                catch { }
            }
            return lista;
        }

        public static List<Juego> ObtenerMisJuegos(int idUsuario)
        {
            List<Juego> lista = new List<Juego>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT v.* FROM videojuegos v INNER JOIN biblioteca b ON v.id = b.id_videojuego WHERE b.id_usuario = @uid";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@uid", idUsuario);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Juego j = LeerJuego(r);
                            j.Adquirido = true;
                            lista.Add(j);
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        // Helper para leer juego con todos los campos nuevos
        private static Juego LeerJuego(MySqlDataReader r)
        {
            return new Juego
            {
                Id = r.GetInt32("id"),
                Titulo = r.GetString("titulo"),
                Descripcion = r.IsDBNull(r.GetOrdinal("descripcion")) ? "Sin descripción disponible." : r.GetString("descripcion"),
                Categoria = r.GetString("categoria"),
                ImagenUrl = r.GetString("imagen_url"),
                Precio = r.GetDecimal("precio"),
                Fabricante = r.IsDBNull(r.GetOrdinal("fabricante")) ? "Desconocido" : r.GetString("fabricante"),
                Visible = r.GetBoolean("visible"),
                Anio = ExisteColumna(r, "anio") && !r.IsDBNull(r.GetOrdinal("anio")) ? r.GetInt32("anio") : 2023,
                EdadMinima = ExisteColumna(r, "edad_minima") && !r.IsDBNull(r.GetOrdinal("edad_minima")) ? r.GetInt32("edad_minima") : 12
            };
        }

        public static bool GuardarJuegoAdmin(Juego j, string admin)
        {
            using (MySqlConnection c = new MySqlConnection(connectionString))
            {
                try
                {
                    c.Open();
                    if (j.Id == 0)
                    {
                        string q = "INSERT INTO videojuegos (titulo, descripcion, categoria, imagen_url, precio, fabricante, anio, edad_minima, visible) VALUES (@t, @d, @c, @i, @p, @f, @a, @em, @v)";
                        MySqlCommand m = new MySqlCommand(q, c); SetJP(m, j); m.ExecuteNonQuery();
                    }
                    else
                    {
                        string q = "UPDATE videojuegos SET titulo=@t, descripcion=@d, categoria=@c, imagen_url=@i, precio=@p, fabricante=@f, anio=@a, edad_minima=@em, visible=@v WHERE id=@id";
                        MySqlCommand m = new MySqlCommand(q, c); SetJP(m, j); m.Parameters.AddWithValue("@id", j.Id); m.ExecuteNonQuery();
                    }
                    return true;
                }
                catch { return false; }
            }
        }

        private static void SetJP(MySqlCommand m, Juego j)
        {
            m.Parameters.AddWithValue("@t", j.Titulo); m.Parameters.AddWithValue("@d", j.Descripcion);
            m.Parameters.AddWithValue("@c", j.Categoria); m.Parameters.AddWithValue("@i", j.ImagenUrl);
            m.Parameters.AddWithValue("@p", j.Precio); m.Parameters.AddWithValue("@f", j.Fabricante);
            m.Parameters.AddWithValue("@a", j.Anio); m.Parameters.AddWithValue("@em", j.EdadMinima);
            m.Parameters.AddWithValue("@v", j.Visible);
        }

        public static bool EliminarJuego(int id) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); new MySqlCommand($"DELETE FROM videojuegos WHERE id={id}", c).ExecuteNonQuery(); return true; } catch { return false; } } }
        public static bool ComprarJuego(int u, int v) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); new MySqlCommand($"INSERT INTO biblioteca (id_usuario, id_videojuego) VALUES ({u}, {v})", c).ExecuteNonQuery(); return true; } catch { return false; } } }

        // === RESTO DE MÉTODOS ===
        public static Usuario ObtenerUsuario(string u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT * FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand m = new MySqlCommand(q, c); m.Parameters.AddWithValue("@u", u); using (MySqlDataReader r = m.ExecuteReader()) { if (r.Read()) return LeerUsuarioDesdeReader(r); } } catch { } } return null; }
        private static Usuario LeerUsuarioDesdeReader(MySqlDataReader r) { return new Usuario { Id = r.GetInt32("id"), NombreUsuario = r.GetString("nombre_usuario"), Email = r.GetString("email"), Password = r.GetString("password"), Rol = ExisteColumna(r, "rol") && !r.IsDBNull(r.GetOrdinal("rol")) ? r.GetString("rol") : "Nominal", Estado = ExisteColumna(r, "estado") && !r.IsDBNull(r.GetOrdinal("estado")) ? r.GetString("estado") : "Activo", Volumen = ExisteColumna(r, "volumen") && !r.IsDBNull(r.GetOrdinal("volumen")) ? r.GetInt32("volumen") : 50, Notificaciones = ExisteColumna(r, "notificaciones") && !r.IsDBNull(r.GetOrdinal("notificaciones")) ? r.GetBoolean("notificaciones") : true, Idioma = ExisteColumna(r, "idioma") && !r.IsDBNull(r.GetOrdinal("idioma")) ? r.GetString("idioma") : "Español", ModoInvisible = ExisteColumna(r, "modo_invisible") && !r.IsDBNull(r.GetOrdinal("modo_invisible")) && r.GetBoolean("modo_invisible") }; }
        private static bool ExisteColumna(MySqlDataReader r, string n) { for (int i = 0; i < r.FieldCount; i++) if (r.GetName(i).Equals(n, StringComparison.OrdinalIgnoreCase)) return true; return false; }
        public static List<Usuario> ObtenerTodosUsuarios(string f = "") { List<Usuario> l = new List<Usuario>(); using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT * FROM usuarios"; if (f != "") q += " WHERE nombre_usuario LIKE @f"; MySqlCommand m = new MySqlCommand(q, c); if (f != "") m.Parameters.AddWithValue("@f", "%" + f + "%"); using (MySqlDataReader r = m.ExecuteReader()) { while (r.Read()) l.Add(LeerUsuarioDesdeReader(r)); } } catch { } } return l; }
        public static bool InsertarUsuarioAdmin(Usuario u, string a) { if (ExisteUsuario(u.NombreUsuario)) return false; using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "INSERT INTO usuarios (nombre_usuario,password,email,rol,estado) VALUES (@u,@p,@e,@r,@s)"; MySqlCommand m = new MySqlCommand(q, c); m.Parameters.AddWithValue("@u", u.NombreUsuario); m.Parameters.AddWithValue("@p", u.Password); m.Parameters.AddWithValue("@e", u.Email); m.Parameters.AddWithValue("@r", u.Rol); m.Parameters.AddWithValue("@s", u.Estado); m.ExecuteNonQuery(); return true; } catch { return false; } } }
        public static bool ActualizarUsuarioAdmin(Usuario u, string a) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "UPDATE usuarios SET email=@e,rol=@r,estado=@s,password=@p WHERE nombre_usuario=@u"; MySqlCommand m = new MySqlCommand(q, c); m.Parameters.AddWithValue("@e", u.Email); m.Parameters.AddWithValue("@r", u.Rol); m.Parameters.AddWithValue("@s", u.Estado); m.Parameters.AddWithValue("@p", u.Password); m.Parameters.AddWithValue("@u", u.NombreUsuario); m.ExecuteNonQuery(); return true; } catch { return false; } } }
        public static bool EliminarUsuario(string u, string a) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "DELETE FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand m = new MySqlCommand(q, c); m.Parameters.AddWithValue("@u", u); m.ExecuteNonQuery(); return true; } catch { return false; } } }
        public static string EnviarSolicitudAmistad(string o, string d) { if (o == d) return "No puedes auto-enviarte."; if (!ExisteUsuario(d)) return "Usuario no existe."; using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string check = "SELECT COUNT(*) FROM amistades WHERE (usuario_origen=@1 AND usuario_destino=@2) OR (usuario_origen=@2 AND usuario_destino=@1)"; MySqlCommand x = new MySqlCommand(check, c); x.Parameters.AddWithValue("@1", o); x.Parameters.AddWithValue("@2", d); if (Convert.ToInt32(x.ExecuteScalar()) > 0) return "Ya existe relación."; string i = "INSERT INTO amistades (usuario_origen, usuario_destino, estado) VALUES (@1, @2, 'PENDIENTE')"; MySqlCommand y = new MySqlCommand(i, c); y.Parameters.AddWithValue("@1", o); y.Parameters.AddWithValue("@2", d); y.ExecuteNonQuery(); return "Solicitud enviada correctamente."; } catch (Exception e) { return "Error: " + e.Message; } } }
        public static List<string> ObtenerSolicitudesPendientes(string d) { List<string> l = new List<string>(); using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT usuario_origen FROM amistades WHERE usuario_destino=@d AND estado='PENDIENTE'"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@d", d); using (MySqlDataReader r = x.ExecuteReader()) { while (r.Read()) l.Add(r.GetString(0)); } } catch { } } return l; }
        public static int ContarSolicitudesPendientes(string u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); return Convert.ToInt32(new MySqlCommand($"SELECT COUNT(*) FROM amistades WHERE usuario_destino='{u}' AND estado='PENDIENTE'", c).ExecuteScalar()); } catch { return 0; } } }
        public static void AceptarSolicitud(string y, string s) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); new MySqlCommand($"UPDATE amistades SET estado='ACEPTADA' WHERE usuario_origen='{s}' AND usuario_destino='{y}'", c).ExecuteNonQuery(); } catch { } } }
        public static List<AmigoItem> ObtenerListaAmigosConEstado(string yo) { List<AmigoItem> l = new List<AmigoItem>(); using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT usuario_origen,usuario_destino FROM amistades WHERE (usuario_origen=@m OR usuario_destino=@m) AND estado='ACEPTADA'"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@m", yo); List<string> fs = new List<string>(); using (MySqlDataReader r = x.ExecuteReader()) { while (r.Read()) { string u1 = r.GetString(0); string u2 = r.GetString(1); fs.Add(u1 == yo ? u2 : u1); } } foreach (string f in fs) { bool inv = false; object res = new MySqlCommand($"SELECT modo_invisible FROM usuarios WHERE nombre_usuario='{f}'", c).ExecuteScalar(); if (res != null) inv = Convert.ToBoolean(res); int un = Convert.ToInt32(new MySqlCommand($"SELECT COUNT(*) FROM chat_mensajes WHERE destinatario='{yo}' AND remitente='{f}' AND leido=0", c).ExecuteScalar()); l.Add(new AmigoItem { Nombre = f, EstadoTexto = inv ? "Desconectado" : "Conectado", EstadoColor = inv ? "Gray" : "Green", MensajesNoLeidos = un, VisibilidadNotificacion = un > 0 ? "Visible" : "Collapsed" }); } } catch { } } return l; }
        public static void EnviarMensaje(string r, string d, string m) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); new MySqlCommand($"INSERT INTO chat_mensajes (remitente,destinatario,mensaje) VALUES ('{r}','{d}','{m}')", c).ExecuteNonQuery(); } catch { } } }
        public static List<MensajeChat> ObtenerHistorialChat(string y, string a) { List<MensajeChat> l = new List<MensajeChat>(); using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = $"SELECT remitente,mensaje,DATE_FORMAT(fecha,'%H:%i') as h FROM chat_mensajes WHERE (remitente='{y}' AND destinatario='{a}') OR (remitente='{a}' AND destinatario='{y}') ORDER BY fecha ASC"; using (MySqlDataReader r = new MySqlCommand(q, c).ExecuteReader()) { while (r.Read()) { string rem = r.GetString(0); bool m = rem == y; l.Add(new MensajeChat { Texto = r.GetString(1), Hora = r.GetString(2), Alineacion = m ? "Right" : "Left", ColorFondo = m ? "#800000" : "#333333" }); } } } catch { } } return l; }
        public static void MarcarMensajesLeidos(string y, string a) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); new MySqlCommand($"UPDATE chat_mensajes SET leido=1 WHERE destinatario='{y}' AND remitente='{a}'", c).ExecuteNonQuery(); } catch { } } }
        public static bool RegistrarUsuario(Usuario u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "INSERT INTO usuarios (nombre_usuario, password, email) VALUES (@u, @p, @e)"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u.NombreUsuario); x.Parameters.AddWithValue("@p", u.Password); x.Parameters.AddWithValue("@e", u.Email); x.ExecuteNonQuery(); return true; } catch { return false; } } }
        public static bool ExisteUsuario(string u) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT COUNT(*) FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand x = new MySqlCommand(q, c); x.Parameters.AddWithValue("@u", u); return Convert.ToInt32(x.ExecuteScalar()) > 0; } catch { return false; } } }
        public static bool EnviarTicketSoporte(string m, string n, string e, string msg, string u) { return true; }
        public static string ActualizarConfiguracion(Usuario u) { return null; }
        public static ResultadoLogin ValidarUsuario(string u, string p) { using (MySqlConnection c = new MySqlConnection(connectionString)) { try { c.Open(); string q = "SELECT password,intentos_fallidos,fecha_fin_bloqueo,estado FROM usuarios WHERE nombre_usuario=@u"; MySqlCommand m = new MySqlCommand(q, c); m.Parameters.AddWithValue("@u", u); using (MySqlDataReader r = m.ExecuteReader()) { if (!r.Read()) return ResultadoLogin.UsuarioNoExiste; string dbP = r.GetString("password"); int i = r.GetInt32("intentos_fallidos"); string s = ExisteColumna(r, "estado") && !r.IsDBNull(r.GetOrdinal("estado")) ? r.GetString("estado") : "Activo"; DateTime? f = r.IsDBNull(r.GetOrdinal("fecha_fin_bloqueo")) ? (DateTime?)null : r.GetDateTime("fecha_fin_bloqueo"); r.Close(); if (s == "Baneado") return ResultadoLogin.UsuarioBaneado; if (f != null && f > DateTime.Now) return ResultadoLogin.UsuarioBloqueado; if (dbP == p) { new MySqlCommand($"UPDATE usuarios SET intentos_fallidos=0 WHERE nombre_usuario='{u}'", c).ExecuteNonQuery(); return ResultadoLogin.Exito; } else { int ni = i + 1; if (ni >= 3) { new MySqlCommand($"UPDATE usuarios SET intentos_fallidos=0, fecha_fin_bloqueo=DATE_ADD(NOW(), INTERVAL 1 MINUTE) WHERE nombre_usuario='{u}'", c).ExecuteNonQuery(); return ResultadoLogin.UsuarioBloqueado; } else { new MySqlCommand($"UPDATE usuarios SET intentos_fallidos={ni} WHERE nombre_usuario='{u}'", c).ExecuteNonQuery(); return ResultadoLogin.PasswordIncorrecto; } } } } catch { return ResultadoLogin.ErrorBaseDatos; } } }
    }
}