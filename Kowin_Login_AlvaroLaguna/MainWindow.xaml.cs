using System;
using System.Collections.Generic;
using System.Linq; // Necesario para .Where y .ToList()
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer chatTimer;
        private string amigoChatActual = null;
        private Usuario _usuarioActual;

        public MainWindow(string nombreUsuario)
        {
            InitializeComponent();
            if (TxtUsuario != null) TxtUsuario.Text = nombreUsuario;

            // 1. Obtener usuario y comprobar Admin
            _usuarioActual = UserManager.ObtenerUsuario(nombreUsuario);
            if (_usuarioActual != null && _usuarioActual.Rol == "Admin")
            {
                if (BtnAdminPanel != null) BtnAdminPanel.Visibility = Visibility.Visible;
            }

            // 2. Cargas Iniciales
            CargarJuegosDesdeBD();
            AplicarIdioma();
            CargarDatosAmigos();
            CargarComunidad();

            // 3. Timer para Chat
            chatTimer = new DispatcherTimer();
            chatTimer.Interval = TimeSpan.FromSeconds(2);
            chatTimer.Tick += ChatTimer_Tick;
            chatTimer.Start();
        }

        // --- CARGA DE JUEGOS (REFRESCO) ---
        private void CargarJuegosDesdeBD()
        {
            // UserManager.ObtenerCatalogo() por defecto trae soloVisibles=true
            // Así que si el admin ocultó uno, aquí ya no vendrá.
            List<Juego> todos = UserManager.ObtenerCatalogo();

            // Filtramos y asignamos a las listas visuales
            ListaDestacados.ItemsSource = todos.Where(j => j.Categoria == "Destacados").ToList();
            ListaLanzamientos.ItemsSource = todos.Where(j => j.Categoria == "Lanzamientos").ToList();
            ListaTerror.ItemsSource = todos.Where(j => j.Categoria == "Terror").ToList();
            ListaSupervivencia.ItemsSource = todos.Where(j => j.Categoria == "Supervivencia").ToList();
            ListaRPG.ItemsSource = todos.Where(j => j.Categoria == "RPG").ToList();
        }

        // --- BOTÓN ADMIN (CLAVE DE LA FUNCIONALIDAD) ---
        private void BtnAdminPanel_Click(object sender, MouseButtonEventArgs e)
        {
            AdminWindow admin = new AdminWindow(TxtUsuario.Text);

            // ShowDialog() bloquea esta ventana hasta que cierras la de Admin.
            admin.ShowDialog();

            // AL CERRAR ADMIN, SE EJECUTA ESTO AUTOMÁTICAMENTE:
            CargarJuegosDesdeBD(); // ¡Aquí desaparecen los juegos ocultos!
        }

        // --- EVENTO CLIC EN JUEGO (COMPRA) ---
        private void TarjetaJuego_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.Tag is Juego j)
            {
                PurchaseWindow pw = new PurchaseWindow(j, _usuarioActual.Id);
                pw.Owner = this;
                pw.ShowDialog();
                // Opcional: Podrías recargar 'Mis Juegos' aquí si la compra fue exitosa
            }
        }

        // --- MIS JUEGOS ---
        private void BtnMisJuegos_Click(object sender, MouseButtonEventArgs e)
        {
            OcultarTodasVistas();
            VistaMisJuegos.Visibility = Visibility.Visible;
            SideMenuBorder.Visibility = Visibility.Visible;

            List<Juego> misJuegos = UserManager.ObtenerMisJuegos(_usuarioActual.Id);
            if (ListaMisJuegos != null) ListaMisJuegos.ItemsSource = misJuegos;

            if (misJuegos.Count > 0 && TxtSubtituloBiblioteca != null)
                TxtSubtituloBiblioteca.Text = $"Tienes {misJuegos.Count} juegos listos para jugar.";
            else if (TxtSubtituloBiblioteca != null)
                TxtSubtituloBiblioteca.Text = "Aún no has comprado ningún juego.";
        }

        // --- RESTO DE LÓGICA (CHAT, AMIGOS, SOPORTE...) ---
        private void ChatTimer_Tick(object sender, EventArgs e)
        {
            if (PanelChat.Visibility == Visibility.Visible && amigoChatActual != null)
            {
                CargarMensajesChat();
                UserManager.MarcarMensajesLeidos(TxtUsuario.Text, amigoChatActual);
            }
            if (VistaAmigos.Visibility == Visibility.Visible) RefrescarListaAmigos();
            ActualizarContadorSolicitudes();
        }

        private void BtnSoporte_Click(object sender, MouseButtonEventArgs e) { OcultarTodasVistas(); SideMenuBorder.Visibility = Visibility.Collapsed; PanelChat.Visibility = Visibility.Collapsed; VistaSoporte.Visibility = Visibility.Visible; TxtTienda.Foreground = Brushes.Gray; TxtComunidad.Foreground = Brushes.Gray; TxtSoporte.Foreground = Brushes.White; }
        private void BtnEnviarTicket_Click(object sender, RoutedEventArgs e) { if (cmbMotivoSoporte.SelectedItem == null || string.IsNullOrWhiteSpace(txtNombreSoporte.Text) || string.IsNullOrWhiteSpace(txtEmailSoporte.Text) || string.IsNullOrWhiteSpace(txtMensajeSoporte.Text)) { lblEstadoSoporte.Text = "Rellena todo."; lblEstadoSoporte.Foreground = Brushes.Red; lblEstadoSoporte.Visibility = Visibility.Visible; return; } string m = (cmbMotivoSoporte.SelectedItem as ComboBoxItem).Content.ToString(); if (UserManager.EnviarTicketSoporte(m, txtNombreSoporte.Text, txtEmailSoporte.Text, txtMensajeSoporte.Text, TxtUsuario.Text)) { lblEstadoSoporte.Text = "Enviado."; lblEstadoSoporte.Foreground = Brushes.Green; lblEstadoSoporte.Visibility = Visibility.Visible; txtNombreSoporte.Text = ""; txtEmailSoporte.Text = ""; txtMensajeSoporte.Text = ""; } else { lblEstadoSoporte.Text = "Error."; lblEstadoSoporte.Foreground = Brushes.Red; lblEstadoSoporte.Visibility = Visibility.Visible; } }
        private void BtnComunidad_Click(object sender, MouseButtonEventArgs e) { OcultarTodasVistas(); SideMenuBorder.Visibility = Visibility.Collapsed; PanelChat.Visibility = Visibility.Collapsed; VistaComunidad.Visibility = Visibility.Visible; TxtTienda.Foreground = Brushes.Gray; TxtComunidad.Foreground = Brushes.White; TxtSoporte.Foreground = Brushes.Gray; }

        private void CargarComunidad()
        {
            List<PostComunidad> posts = new List<PostComunidad>();
            posts.Add(new PostComunidad { Usuario = "NeonSamurai99", AvatarColor = "#00FFFF", LetraInicial = "N", EsReview = true, ImagenUrl = "pack://application:,,,/Imagenes/Cyberpunk2077.jpg", TituloJuego = "Cyberpunk 2077", Estrellas = "★★★★★", EsPositiva = true, ContenidoTexto = "Obra maestra." });
            posts.Add(new PostComunidad { Usuario = "TrollMaster", AvatarColor = "#00FF00", LetraInicial = "T", EsReview = false, ImagenUrl = "https://i.imgflip.com/4/30b1gx.jpg", ContenidoTexto = "Cuando corres Cyberpunk en una tostadora:" });
            if (ListaComunidad != null) ListaComunidad.ItemsSource = posts;
        }

        private void BtnVolverTienda_Click(object sender, MouseButtonEventArgs e) { OcultarTodasVistas(); SideMenuBorder.Visibility = Visibility.Visible; MainScroll.Visibility = Visibility.Visible; TxtTienda.Foreground = Brushes.White; TxtComunidad.Foreground = Brushes.Gray; TxtSoporte.Foreground = Brushes.Gray; }
        private void BtnAmigos_Click(object sender, MouseButtonEventArgs e) { OcultarTodasVistas(); SideMenuBorder.Visibility = Visibility.Visible; VistaAmigos.Visibility = Visibility.Visible; CargarDatosAmigos(); }
        private void OcultarTodasVistas() { MainScroll.Visibility = Visibility.Collapsed; VistaAmigos.Visibility = Visibility.Collapsed; VistaMisJuegos.Visibility = Visibility.Collapsed; VistaComunidad.Visibility = Visibility.Collapsed; VistaSoporte.Visibility = Visibility.Collapsed; }
        private void BtnJugarFlappy_Click(object sender, RoutedEventArgs e) { FlappyBirdWindow juego = new FlappyBirdWindow(); juego.Owner = this; juego.ShowDialog(); }
        private void CargarDatosAmigos() { ActualizarContadorSolicitudes(); RefrescarListaAmigos(); }
        private void ActualizarContadorSolicitudes() { string u = TxtUsuario.Text; int n = UserManager.ContarSolicitudesPendientes(u); ListaSolicitudes.ItemsSource = UserManager.ObtenerSolicitudesPendientes(u); if (n > 0) { NotificacionAmigos.Visibility = Visibility.Visible; NumSolicitudes.Text = n.ToString(); } else NotificacionAmigos.Visibility = Visibility.Collapsed; }
        private void RefrescarListaAmigos() { ListaAmigos.ItemsSource = UserManager.ObtenerListaAmigosConEstado(TxtUsuario.Text); }
        private void BtnEnviarSolicitud_Click(object sender, RoutedEventArgs e) { string d = txtBusquedaAmigo.Text; if (string.IsNullOrWhiteSpace(d)) return; lblMensajeAmigo.Text = UserManager.EnviarSolicitudAmistad(TxtUsuario.Text, d); lblMensajeAmigo.Visibility = Visibility.Visible; if (lblMensajeAmigo.Text.Contains("correctamente")) txtBusquedaAmigo.Text = ""; }
        private void BtnAceptarSolicitud_Click(object sender, RoutedEventArgs e) { Button btn = sender as Button; UserManager.AceptarSolicitud(TxtUsuario.Text, btn.Tag.ToString()); CargarDatosAmigos(); }
        private void BtnAbrirChat_Click(object sender, RoutedEventArgs e) { Button btn = sender as Button; string a = btn.Tag.ToString(); amigoChatActual = a; TxtNombreChat.Text = a.ToUpper(); PanelChat.Visibility = Visibility.Visible; UserManager.MarcarMensajesLeidos(TxtUsuario.Text, a); CargarMensajesChat(); RefrescarListaAmigos(); }
        private void BtnCerrarChat_Click(object sender, RoutedEventArgs e) { PanelChat.Visibility = Visibility.Collapsed; amigoChatActual = null; }
        private void BtnEnviarMensaje_Click(object sender, RoutedEventArgs e) { EnviarMensaje(); }
        private void TxtMensaje_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) EnviarMensaje(); }
        private void EnviarMensaje() { string t = txtMensaje.Text; if (string.IsNullOrWhiteSpace(t) || amigoChatActual == null) return; UserManager.EnviarMensaje(TxtUsuario.Text, amigoChatActual, t); txtMensaje.Text = ""; CargarMensajesChat(); }
        private void CargarMensajesChat() { var h = UserManager.ObtenerHistorialChat(TxtUsuario.Text, amigoChatActual); ListaMensajes.ItemsSource = h; if (ListaMensajes.Items.Count > 0) ScrollChat.ScrollToBottom(); }
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e) { if (MainScroll.Visibility == Visibility.Visible) { MainScroll.ScrollToVerticalOffset(MainScroll.VerticalOffset - e.Delta); e.Handled = true; } }
        private void BtnCerrarSesion_Click(object sender, MouseButtonEventArgs e) { chatTimer.Stop(); LoginWindow login = new LoginWindow(); login.Show(); this.Close(); }
        private void BtnConfiguracion_Click(object sender, MouseButtonEventArgs e) { SettingsWindow s = new SettingsWindow(TxtUsuario.Text); s.Owner = this; s.ShowDialog(); AplicarIdioma(); }
        private void AplicarIdioma() { if (_usuarioActual == null) return; string i = _usuarioActual.Idioma; if (i == "English") { TxtTienda.Text = "STORE"; TxtDestacadosMenu.Text = "Featured"; TxtMisJuegos.Text = "My Games"; TxtHeaderDestacados.Text = "Featured"; TxtHeaderLanzamientos.Text = "Latest Releases"; TxtTituloBiblioteca.Text = "MY LIBRARY"; if (BtnTextoJugar != null) BtnTextoJugar.Content = "PLAY NOW"; } }
    }

// CLASE POSTCOMUNIDAD (AQUÍ ESTÁ LA SOLUCIÓN AL ERROR CS0246)
public class PostComunidad
    {
        public string Usuario { get; set; }
        public string AvatarColor { get; set; }
        public string LetraInicial { get; set; }
        public bool EsReview { get; set; }
        public string ImagenUrl { get; set; }
        public string TituloJuego { get; set; }
        public string Estrellas { get; set; }
        public bool EsPositiva { get; set; }
        public string ContenidoTexto { get; set; }
        public string VisibilidadReview => EsReview ? "Visible" : "Collapsed";
        public string VisibilidadMeme => EsReview ? "Collapsed" : "Visible";
        public string ColorRecomendacion => EsPositiva ? "Green" : "Red";
        public string TextoRecomendacion => EsPositiva ? "RECOMENDADO" : "NO RECOMENDADO";
    }
}