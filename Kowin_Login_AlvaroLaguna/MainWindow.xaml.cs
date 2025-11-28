using System.Collections.Generic;
using System.Windows;
using System.Windows.Input; // Necesario para MouseButtonEventArgs

namespace Kowin_Login_AlvaroLaguna
{
    public partial class MainWindow : Window
    {
        // Constructor que recibe el nombre del usuario logueado
        public MainWindow(string nombreUsuario)
        {
            InitializeComponent();

            // Asignamos el nombre al TextBlock de arriba a la derecha (asegúrate de que en el XAML se llame x:Name="TxtUsuario")
            if (TxtUsuario != null)
            {
                TxtUsuario.Text = nombreUsuario;
            }

            CargarJuegos();
        }

        // TC 05: Cerrar Sesión y volver al Login
        private void BtnCerrarSesion_Click(object sender, MouseButtonEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close(); // Cierra el Home actual
        }

        private void CargarJuegos()
        {
            // === 1. JUEGOS DESTACADOS ===
            List<Juego> juegosDestacados = new List<Juego>();

            // Rutas de ejemplo según tu estructura
            juegosDestacados.Add(new Juego { Titulo = "Cyberpunk 2077", ImagenUrl = "/Imagenes/Cyberpunk2077.jpg" });
            juegosDestacados.Add(new Juego { Titulo = "Grand Theft Auto VI", ImagenUrl = "/Imagenes/GtaV.jpg" });
            juegosDestacados.Add(new Juego { Titulo = "Starfield", ImagenUrl = "/Imagenes/Starfield.jpg" });

            ListaDestacados.ItemsSource = juegosDestacados;


            // === 2. ÚLTIMOS LANZAMIENTOS ===
            List<Juego> juegosLanzamientos = new List<Juego>();

            juegosLanzamientos.Add(new Juego { Titulo = "Call of Duty: MW3", ImagenUrl = "/Imagenes/Mdw3.png" });
            juegosLanzamientos.Add(new Juego { Titulo = "Spider-Man 2", ImagenUrl = "/Imagenes/Spiderman.jpg" });
            juegosLanzamientos.Add(new Juego { Titulo = "Assassin's Creed Mirage", ImagenUrl = "/Imagenes/Assassins.png" });
            juegosLanzamientos.Add(new Juego { Titulo = "Alan Wake 2", ImagenUrl = "/Imagenes/Alan.jfif" });

            ListaLanzamientos.ItemsSource = juegosLanzamientos;


            // === 3. TERROR ===
            List<Juego> juegosTerror = new List<Juego>();

            juegosTerror.Add(new Juego { Titulo = "Resident Evil 4", ImagenUrl = "/Imagenes/Re4.jpg" });
            juegosTerror.Add(new Juego { Titulo = "Silent Hill 2 Remake", ImagenUrl = "/Imagenes/Silent.png" });
            juegosTerror.Add(new Juego { Titulo = "Outlast Trials", ImagenUrl = "/Imagenes/Outlast.jpg" });
            juegosTerror.Add(new Juego { Titulo = "Amnesia: The Bunker", ImagenUrl = "/Imagenes/Amnesia.jpg" });
            juegosTerror.Add(new Juego { Titulo = "Alien Isolation", ImagenUrl = "/Imagenes/Alien.jfif" });

            ListaTerror.ItemsSource = juegosTerror;


            // === 4. SUPERVIVENCIA ===
            List<Juego> juegosSupervivencia = new List<Juego>();

            juegosSupervivencia.Add(new Juego { Titulo = "Rust", ImagenUrl = "/Imagenes/Rust.jpg" });
            juegosSupervivencia.Add(new Juego { Titulo = "Ark: Survival Ascended", ImagenUrl = "/Imagenes/Ark.jfif" });
            juegosSupervivencia.Add(new Juego { Titulo = "Sons of The Forest", ImagenUrl = "/Imagenes/Sons.jpg" });
            juegosSupervivencia.Add(new Juego { Titulo = "Raft", ImagenUrl = "/Imagenes/Raft.jpg" });
            juegosSupervivencia.Add(new Juego { Titulo = "Subnautica 2", ImagenUrl = "/Imagenes/Subnautica.jpg" });

            ListaSupervivencia.ItemsSource = juegosSupervivencia;


            // === 5. RPG ===
            List<Juego> juegosRPG = new List<Juego>();

            juegosRPG.Add(new Juego { Titulo = "Elden Ring", ImagenUrl = "/Imagenes/Elden.jpg" });
            juegosRPG.Add(new Juego { Titulo = "Baldur's Gate 3", ImagenUrl = "/Imagenes/Bladurs.jpg" });
            juegosRPG.Add(new Juego { Titulo = "Final Fantasy XVI", ImagenUrl = "/Imagenes/FinalF.jpg" });
            juegosRPG.Add(new Juego { Titulo = "The Witcher 3", ImagenUrl = "/Imagenes/Witcher.jfif" });
            juegosRPG.Add(new Juego { Titulo = "Diablo IV", ImagenUrl = "/Imagenes/Diablo.jpg" });

            ListaRPG.ItemsSource = juegosRPG;
        }
    }

    public class Juego
    {
        public string Titulo { get; set; }
        public string ImagenUrl { get; set; }
    }
}