using System;
using System.Collections.Generic;
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

        public MainWindow(string nombreUsuario)
        {
            InitializeComponent();
            if (TxtUsuario != null) TxtUsuario.Text = nombreUsuario;

            CargarJuegos();
            AplicarIdioma();
            CargarDatosAmigos();

            // Cargar posts de comunidad
            CargarComunidad();

            chatTimer = new DispatcherTimer();
            chatTimer.Interval = TimeSpan.FromSeconds(2);
            chatTimer.Tick += ChatTimer_Tick;
            chatTimer.Start();
        }

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

        // --- SOPORTE TÉCNICO ---
        private void BtnSoporte_Click(object sender, MouseButtonEventArgs e)
        {
            OcultarTodasVistas();
            SideMenuBorder.Visibility = Visibility.Collapsed;
            PanelChat.Visibility = Visibility.Collapsed;

            VistaSoporte.Visibility = Visibility.Visible;

            TxtTienda.Foreground = Brushes.Gray;
            TxtComunidad.Foreground = Brushes.Gray;
            TxtSoporte.Foreground = Brushes.White;
        }

        private void BtnEnviarTicket_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMotivoSoporte.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtNombreSoporte.Text) ||
                string.IsNullOrWhiteSpace(txtEmailSoporte.Text) ||
                string.IsNullOrWhiteSpace(txtMensajeSoporte.Text))
            {
                lblEstadoSoporte.Text = "Por favor, rellena todos los campos.";
                lblEstadoSoporte.Foreground = Brushes.Red;
                lblEstadoSoporte.Visibility = Visibility.Visible;
                return;
            }

            string motivo = (cmbMotivoSoporte.SelectedItem as ComboBoxItem).Content.ToString();
            bool exito = UserManager.EnviarTicketSoporte(motivo, txtNombreSoporte.Text, txtEmailSoporte.Text, txtMensajeSoporte.Text, TxtUsuario.Text);

            if (exito)
            {
                lblEstadoSoporte.Text = "Ticket enviado correctamente.";
                lblEstadoSoporte.Foreground = Brushes.Green;
                lblEstadoSoporte.Visibility = Visibility.Visible;

                cmbMotivoSoporte.SelectedIndex = -1;
                txtNombreSoporte.Text = "";
                txtEmailSoporte.Text = "";
                txtMensajeSoporte.Text = "";
            }
            else
            {
                lblEstadoSoporte.Text = "Error al conectar con el servidor.";
                lblEstadoSoporte.Foreground = Brushes.Red;
                lblEstadoSoporte.Visibility = Visibility.Visible;
            }
        }

        // --- COMUNIDAD ---
        private void BtnComunidad_Click(object sender, MouseButtonEventArgs e)
        {
            OcultarTodasVistas();
            SideMenuBorder.Visibility = Visibility.Collapsed;
            PanelChat.Visibility = Visibility.Collapsed;
            VistaComunidad.Visibility = Visibility.Visible;
            TxtTienda.Foreground = Brushes.Gray;
            TxtComunidad.Foreground = Brushes.White;
            TxtSoporte.Foreground = Brushes.Gray;
        }

        private void CargarComunidad()
        {
            List<PostComunidad> posts = new List<PostComunidad>();

            // RUTAS DE IMÁGENES: Usa URLs de internet para los memes para asegurar que se vean

            posts.Add(new PostComunidad
            {
                Usuario = "NeonSamurai99",
                AvatarColor = "#00FFFF",
                LetraInicial = "N",
                EsReview = true,
                ImagenUrl = "pack://application:,,,/Imagenes/Cyberpunk2077.jpg",
                TituloJuego = "Cyberpunk 2077",
                Estrellas = "★★★★★",
                EsPositiva = true,
                ContenidoTexto = "Después de las actualizaciones, este juego es una obra maestra."
            });

            posts.Add(new PostComunidad
            {
                Usuario = "SpaceExplorer",
                AvatarColor = "#FFA500",
                LetraInicial = "S",
                EsReview = true,
                ImagenUrl = "pack://application:,,,/Imagenes/Starfield.jpg",
                TituloJuego = "Starfield",
                Estrellas = "★★★☆☆",
                EsPositiva = false,
                ContenidoTexto = "Un poco vacío. Esperaba más exploración real."
            });

            posts.Add(new PostComunidad
            {
                Usuario = "TrollMaster",
                AvatarColor = "#00FF00",
                LetraInicial = "T",
                EsReview = false,
                ImagenUrl = "https://i.ytimg.com/vi/9G3j-1rwEak/hq720.jpg?sqp=-oaymwEXCK4FEIIDSFryq4qpAwkIARUAAIhCGAE=&rs=AOn4CLBc9Zg5tZ0qjIHj4X2_EQieNBOy9A",
                ContenidoTexto = "Cuando intentas correr Cyberpunk en una tostadora:"
            });

            posts.Add(new PostComunidad
            {
                Usuario = "LeonFanboy",
                AvatarColor = "#FF0000",
                LetraInicial = "L",
                EsReview = true,
                ImagenUrl = "pack://application:,,,/Imagenes/Re4.jpg",
                TituloJuego = "Resident Evil 4 Remake",
                Estrellas = "★★★★★",
                EsPositiva = true,
                ContenidoTexto = "Capcom lo ha vuelto a hacer. Increíble."
            });

            posts.Add(new PostComunidad
            {
                Usuario = "GamerCat",
                AvatarColor = "#FF00FF",
                LetraInicial = "G",
                EsReview = false,
                ImagenUrl = "https://i.ytimg.com/vi/hlnpkrJs6wM/maxresdefault.jpg",
                ContenidoTexto = "Yo a las 3 AM diciendo 'una partida más':"
            });

            ListaComunidad.ItemsSource = posts;
        }

        // --- NAVEGACIÓN ---

        private void BtnVolverTienda_Click(object sender, MouseButtonEventArgs e)
        {
            OcultarTodasVistas();
            VistaComunidad.Visibility = Visibility.Collapsed;
            VistaSoporte.Visibility = Visibility.Collapsed;
            SideMenuBorder.Visibility = Visibility.Visible;
            MainScroll.Visibility = Visibility.Visible;

            TxtTienda.Foreground = Brushes.White;
            TxtComunidad.Foreground = Brushes.Gray;
            TxtSoporte.Foreground = Brushes.Gray;
        }

        private void BtnAmigos_Click(object sender, MouseButtonEventArgs e)
        {
            OcultarTodasVistas();
            SideMenuBorder.Visibility = Visibility.Visible;
            VistaAmigos.Visibility = Visibility.Visible;
            CargarDatosAmigos();
        }

        private void BtnMisJuegos_Click(object sender, MouseButtonEventArgs e)
        {
            OcultarTodasVistas();
            SideMenuBorder.Visibility = Visibility.Visible;
            VistaMisJuegos.Visibility = Visibility.Visible;
        }

        private void OcultarTodasVistas()
        {
            MainScroll.Visibility = Visibility.Collapsed;
            VistaAmigos.Visibility = Visibility.Collapsed;
            VistaMisJuegos.Visibility = Visibility.Collapsed;
            VistaComunidad.Visibility = Visibility.Collapsed;
            VistaSoporte.Visibility = Visibility.Collapsed;
        }

        // --- JUEGO ---
        private void BtnJugarFlappy_Click(object sender, RoutedEventArgs e)
        {
            FlappyBirdWindow juego = new FlappyBirdWindow();
            juego.Owner = this;
            juego.ShowDialog();
        }

        // --- AMIGOS ---
        private void CargarDatosAmigos() { ActualizarContadorSolicitudes(); RefrescarListaAmigos(); }
        private void ActualizarContadorSolicitudes()
        {
            string usuario = TxtUsuario.Text;
            int numSolicitudes = UserManager.ContarSolicitudesPendientes(usuario);
            List<string> solicitudes = UserManager.ObtenerSolicitudesPendientes(usuario);
            ListaSolicitudes.ItemsSource = solicitudes;
            if (numSolicitudes > 0) { NotificacionAmigos.Visibility = Visibility.Visible; NumSolicitudes.Text = numSolicitudes.ToString(); }
            else { NotificacionAmigos.Visibility = Visibility.Collapsed; }
        }
        private void RefrescarListaAmigos() { List<AmigoItem> amigos = UserManager.ObtenerListaAmigosConEstado(TxtUsuario.Text); ListaAmigos.ItemsSource = amigos; }
        private void BtnEnviarSolicitud_Click(object sender, RoutedEventArgs e)
        {
            string destino = txtBusquedaAmigo.Text;
            if (string.IsNullOrWhiteSpace(destino)) return;
            string resultado = UserManager.EnviarSolicitudAmistad(TxtUsuario.Text, destino);
            lblMensajeAmigo.Text = resultado; lblMensajeAmigo.Visibility = Visibility.Visible;
            if (resultado.Contains("correctamente")) txtBusquedaAmigo.Text = "";
        }
        private void BtnAceptarSolicitud_Click(object sender, RoutedEventArgs e) { Button btn = sender as Button; UserManager.AceptarSolicitud(TxtUsuario.Text, btn.Tag.ToString()); CargarDatosAmigos(); }

        // --- CHAT ---
        private void BtnAbrirChat_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button; string amigo = btn.Tag.ToString();
            amigoChatActual = amigo; TxtNombreChat.Text = amigo.ToUpper(); PanelChat.Visibility = Visibility.Visible;
            UserManager.MarcarMensajesLeidos(TxtUsuario.Text, amigo); CargarMensajesChat(); RefrescarListaAmigos();
        }
        private void BtnCerrarChat_Click(object sender, RoutedEventArgs e) { PanelChat.Visibility = Visibility.Collapsed; amigoChatActual = null; }
        private void BtnEnviarMensaje_Click(object sender, RoutedEventArgs e) { EnviarMensaje(); }
        private void TxtMensaje_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) EnviarMensaje(); }
        private void EnviarMensaje()
        {
            string texto = txtMensaje.Text;
            if (string.IsNullOrWhiteSpace(texto) || amigoChatActual == null) return;
            UserManager.EnviarMensaje(TxtUsuario.Text, amigoChatActual, texto);
            txtMensaje.Text = ""; CargarMensajesChat();
        }
        private void CargarMensajesChat()
        {
            var historial = UserManager.ObtenerHistorialChat(TxtUsuario.Text, amigoChatActual);
            ListaMensajes.ItemsSource = historial;
            if (ListaMensajes.Items.Count > 0) ScrollChat.ScrollToBottom();
        }

        // --- OTROS ---
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e) { if (MainScroll.Visibility == Visibility.Visible) { MainScroll.ScrollToVerticalOffset(MainScroll.VerticalOffset - e.Delta); e.Handled = true; } }
        private void BtnCerrarSesion_Click(object sender, MouseButtonEventArgs e) { chatTimer.Stop(); LoginWindow login = new LoginWindow(); login.Show(); this.Close(); }
        private void BtnConfiguracion_Click(object sender, MouseButtonEventArgs e) { SettingsWindow s = new SettingsWindow(TxtUsuario.Text); s.Owner = this; s.ShowDialog(); AplicarIdioma(); }

        // --- TRADUCCIÓN COMPLETA DE TODAS LAS SECCIONES ---
        private void AplicarIdioma()
        {
            Usuario u = UserManager.ObtenerUsuario(TxtUsuario.Text);
            if (u == null) return;
            string idioma = u.Idioma;

            if (idioma == "English")
            {
                // Barra Superior
                TxtTienda.Text = "STORE"; TxtComunidad.Text = "COMMUNITY"; TxtSoporte.Text = "SUPPORT"; TxtCerrarSesion.Text = "sign out";
                // Menú Lateral
                TxtDestacadosMenu.Text = "Featured"; TxtMisJuegos.Text = "My Games"; TxtConfiguracion.Text = "Settings"; TxtAmigos.Text = "Friends";
                // Tienda
                TxtHeaderDestacados.Text = "Featured"; TxtHeaderLanzamientos.Text = "Latest Releases"; TxtHeaderTerror.Text = "Horror & Suspense";
                TxtHeaderSupervivencia.Text = "Survival"; TxtHeaderRPG.Text = "RPG & Adventure";
                // Gestión Amigos
                TxtTituloGestionAmigos.Text = "FRIENDS MANAGEMENT"; TxtLabelAddAmigo.Text = "Add Friend:"; BtnEnviarSolTexto.Content = "SEND";
                TxtLabelSolicitudes.Text = "Pending Requests"; TxtLabelMisAmigos.Text = "My Friends";
                // Mis Juegos
                TxtTituloBiblioteca.Text = "MY LIBRARY"; TxtSubtituloBiblioteca.Text = "Games installed and ready to play"; BtnTextoJugar.Content = "▶ PLAY NOW";
                // Comunidad
                TxtTituloComunidad.Text = "KOWIN COMMUNITY";
                // Soporte
                TxtTituloSoporte.Text = "CUSTOMER SUPPORT"; TxtSubtituloSoporte.Text = "Fill out all fields to send your inquiry.";
                LblMotivo.Text = "Reason for Contact:"; LblNombre.Text = "Your Name:"; LblEmail.Text = "Contact Email:"; LblMensaje.Text = "Issue Description:"; BtnTextoEnviarTicket.Content = "SEND TICKET";
            }
            else if (idioma == "Français")
            {
                TxtTienda.Text = "BOUTIQUE"; TxtComunidad.Text = "COMMUNAUTÉ"; TxtSoporte.Text = "SUPPORT"; TxtCerrarSesion.Text = "déconnexion";
                TxtDestacadosMenu.Text = "En Vedette"; TxtMisJuegos.Text = "Mes Jeux"; TxtConfiguracion.Text = "Paramètres"; TxtAmigos.Text = "Amis";
                TxtHeaderDestacados.Text = "En Vedette"; TxtHeaderLanzamientos.Text = "Dernières Sorties"; TxtHeaderTerror.Text = "Horreur et Suspense";
                TxtHeaderSupervivencia.Text = "Survie"; TxtHeaderRPG.Text = "RPG et Aventure";
                TxtTituloGestionAmigos.Text = "GESTION DES AMIS"; TxtLabelAddAmigo.Text = "Ajouter un ami:"; BtnEnviarSolTexto.Content = "ENVOYER";
                TxtLabelSolicitudes.Text = "Demandes en Attente"; TxtLabelMisAmigos.Text = "Mes Amis";
                TxtTituloBiblioteca.Text = "MA BIBLIOTHÈQUE"; TxtSubtituloBiblioteca.Text = "Jeux installés et prêts à jouer"; BtnTextoJugar.Content = "▶ JOUER";
                TxtTituloComunidad.Text = "COMMUNAUTÉ KOWIN";
                TxtTituloSoporte.Text = "SERVICE CLIENT"; TxtSubtituloSoporte.Text = "Remplissez tous les champs.";
                LblMotivo.Text = "Motif:"; LblNombre.Text = "Votre Nom:"; LblEmail.Text = "Email:"; LblMensaje.Text = "Description:"; BtnTextoEnviarTicket.Content = "ENVOYER";
            }
            else if (idioma == "Deutsch")
            {
                TxtTienda.Text = "SHOP"; TxtComunidad.Text = "COMMUNITY"; TxtSoporte.Text = "HILFE"; TxtCerrarSesion.Text = "abmelden";
                TxtDestacadosMenu.Text = "Vorgestellt"; TxtMisJuegos.Text = "Meine Spiele"; TxtConfiguracion.Text = "Einstellungen"; TxtAmigos.Text = "Freunde";
                TxtHeaderDestacados.Text = "Vorgestellt"; TxtHeaderLanzamientos.Text = "Neuerscheinungen"; TxtHeaderTerror.Text = "Horror & Spannung";
                TxtHeaderSupervivencia.Text = "Überleben"; TxtHeaderRPG.Text = "Rollenspiel & Abenteuer";
                TxtTituloGestionAmigos.Text = "FREUNDE VERWALTEN"; TxtLabelAddAmigo.Text = "Freund hinzufügen:"; BtnEnviarSolTexto.Content = "SENDEN";
                TxtLabelSolicitudes.Text = "Ausstehende Anfragen"; TxtLabelMisAmigos.Text = "Meine Freunde";
                TxtTituloBiblioteca.Text = "MEINE BIBLIOTHEK"; TxtSubtituloBiblioteca.Text = "Installierte Spiele bereit zum Spielen"; BtnTextoJugar.Content = "▶ SPIELEN";
                TxtTituloComunidad.Text = "KOWIN COMMUNITY";
                TxtTituloSoporte.Text = "KUNDENDIENST"; TxtSubtituloSoporte.Text = "Füllen Sie alle Felder aus.";
                LblMotivo.Text = "Grund:"; LblNombre.Text = "Ihr Name:"; LblEmail.Text = "Email:"; LblMensaje.Text = "Beschreibung:"; BtnTextoEnviarTicket.Content = "SENDEN";
            }
            else // Español
            {
                TxtTienda.Text = "TIENDA"; TxtComunidad.Text = "COMUNIDAD"; TxtSoporte.Text = "SOPORTE"; TxtCerrarSesion.Text = "cerrar sesión";
                TxtDestacadosMenu.Text = "Destacados"; TxtMisJuegos.Text = "Mis juegos"; TxtConfiguracion.Text = "Configuración"; TxtAmigos.Text = "Amigos";
                TxtHeaderDestacados.Text = "Destacados"; TxtHeaderLanzamientos.Text = "Últimos Lanzamientos"; TxtHeaderTerror.Text = "Terror y Suspense";
                TxtHeaderSupervivencia.Text = "Supervivencia"; TxtHeaderRPG.Text = "RPG y Aventura";
                TxtTituloGestionAmigos.Text = "GESTIÓN DE AMIGOS"; TxtLabelAddAmigo.Text = "Añadir amigo:"; BtnEnviarSolTexto.Content = "ENVIAR";
                TxtLabelSolicitudes.Text = "Solicitudes Pendientes"; TxtLabelMisAmigos.Text = "Mis Amigos";
                TxtTituloBiblioteca.Text = "MI BIBLIOTECA"; TxtSubtituloBiblioteca.Text = "Juegos instalados y listos para jugar"; BtnTextoJugar.Content = "▶ JUGAR AHORA";
                TxtTituloComunidad.Text = "COMUNIDAD KOWIN";
                TxtTituloSoporte.Text = "ATENCIÓN AL CLIENTE"; TxtSubtituloSoporte.Text = "Rellena todos los campos para enviarnos tu consulta.";
                LblMotivo.Text = "Motivo de contacto:"; LblNombre.Text = "Tu Nombre:"; LblEmail.Text = "Email de Contacto:"; LblMensaje.Text = "Descripción del Problema:"; BtnTextoEnviarTicket.Content = "ENVIAR TICKET";
            }
        }

        private void CargarJuegos()
        {
            // Rutas absolutas para evitar problemas
            List<Juego> juegosDestacados = new List<Juego>();
            juegosDestacados.Add(new Juego { Titulo = "Cyberpunk 2077", ImagenUrl = "pack://application:,,,/Imagenes/Cyberpunk2077.jpg" });
            juegosDestacados.Add(new Juego { Titulo = "Grand Theft Auto VI", ImagenUrl = "pack://application:,,,/Imagenes/GtaV.jpg" });
            juegosDestacados.Add(new Juego { Titulo = "Starfield", ImagenUrl = "pack://application:,,,/Imagenes/Starfield.jpg" });
            ListaDestacados.ItemsSource = juegosDestacados;

            List<Juego> juegosLanzamientos = new List<Juego>();
            juegosLanzamientos.Add(new Juego { Titulo = "Call of Duty: MW3", ImagenUrl = "pack://application:,,,/Imagenes/Mdw3.png" });
            juegosLanzamientos.Add(new Juego { Titulo = "Spider-Man 2", ImagenUrl = "pack://application:,,,/Imagenes/Spiderman.jpg" });
            juegosLanzamientos.Add(new Juego { Titulo = "Assassin's Creed Mirage", ImagenUrl = "pack://application:,,,/Imagenes/Assassins.png" });
            juegosLanzamientos.Add(new Juego { Titulo = "Alan Wake 2", ImagenUrl = "pack://application:,,,/Imagenes/Alan.jfif" });
            ListaLanzamientos.ItemsSource = juegosLanzamientos;

            List<Juego> juegosTerror = new List<Juego>();
            juegosTerror.Add(new Juego { Titulo = "Resident Evil 4", ImagenUrl = "pack://application:,,,/Imagenes/Re4.jpg" });
            juegosTerror.Add(new Juego { Titulo = "Silent Hill 2 Remake", ImagenUrl = "pack://application:,,,/Imagenes/Silent.png" });
            juegosTerror.Add(new Juego { Titulo = "Outlast Trials", ImagenUrl = "pack://application:,,,/Imagenes/Outlast.jpg" });
            juegosTerror.Add(new Juego { Titulo = "Amnesia: The Bunker", ImagenUrl = "pack://application:,,,/Imagenes/Amnesia.jpg" });
            juegosTerror.Add(new Juego { Titulo = "Alien Isolation", ImagenUrl = "pack://application:,,,/Imagenes/Alien.jfif" });
            ListaTerror.ItemsSource = juegosTerror;

            List<Juego> juegosSupervivencia = new List<Juego>();
            juegosSupervivencia.Add(new Juego { Titulo = "Rust", ImagenUrl = "pack://application:,,,/Imagenes/Rust.jpg" });
            juegosSupervivencia.Add(new Juego { Titulo = "Ark: Survival Ascended", ImagenUrl = "pack://application:,,,/Imagenes/Ark.jfif" });
            juegosSupervivencia.Add(new Juego { Titulo = "Sons of The Forest", ImagenUrl = "pack://application:,,,/Imagenes/Sons.jpg" });
            juegosSupervivencia.Add(new Juego { Titulo = "Raft", ImagenUrl = "pack://application:,,,/Imagenes/Raft.jpg" });
            juegosSupervivencia.Add(new Juego { Titulo = "Subnautica 2", ImagenUrl = "pack://application:,,,/Imagenes/Subnautica.jpg" });
            ListaSupervivencia.ItemsSource = juegosSupervivencia;

            List<Juego> juegosRPG = new List<Juego>();
            juegosRPG.Add(new Juego { Titulo = "Elden Ring", ImagenUrl = "pack://application:,,,/Imagenes/Elden.jpg" });
            juegosRPG.Add(new Juego { Titulo = "Baldur's Gate 3", ImagenUrl = "pack://application:,,,/Imagenes/Bladurs.jpg" });
            juegosRPG.Add(new Juego { Titulo = "Final Fantasy XVI", ImagenUrl = "pack://application:,,,/Imagenes/FinalF.jpg" });
            juegosRPG.Add(new Juego { Titulo = "The Witcher 3", ImagenUrl = "pack://application:,,,/Imagenes/Witcher.jfif" });
            juegosRPG.Add(new Juego { Titulo = "Diablo IV", ImagenUrl = "pack://application:,,,/Imagenes/Diablo.jpg" });
            ListaRPG.ItemsSource = juegosRPG;
        }
    }

    public class Juego { public string Titulo { get; set; } public string ImagenUrl { get; set; } }

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