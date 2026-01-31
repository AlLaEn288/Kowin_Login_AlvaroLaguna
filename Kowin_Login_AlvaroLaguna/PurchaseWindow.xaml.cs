using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class PurchaseWindow : Window
    {
        private Juego _juego;
        private int _idUsuario;

        public PurchaseWindow(Juego juego, int idUsuario)
        {
            InitializeComponent();
            _juego = juego;
            _idUsuario = idUsuario;
            CargarDatos();
        }

        private void CargarDatos()
        {
            txtTitulo.Text = _juego.Titulo;
            txtFabricante.Text = _juego.Fabricante;
            txtPrecio.Text = _juego.Precio + " €";

            // Campos nuevos
            txtDescripcion.Text = string.IsNullOrEmpty(_juego.Descripcion) ? "Sin descripción disponible." : _juego.Descripcion;
            txtAnio.Text = _juego.Anio > 0 ? _juego.Anio.ToString() : "N/A";
            txtEdad.Text = _juego.EdadMinima + "+";

            // Color del PEGI según edad
            if (_juego.EdadMinima >= 18) txtEdad.Foreground = System.Windows.Media.Brushes.Red;
            else if (_juego.EdadMinima >= 12) txtEdad.Foreground = System.Windows.Media.Brushes.Yellow;
            else txtEdad.Foreground = System.Windows.Media.Brushes.LightGreen;

            if (!string.IsNullOrEmpty(_juego.ImagenUrl))
            {
                try { imgJuego.Source = new BitmapImage(new Uri(_juego.ImagenUrl, UriKind.RelativeOrAbsolute)); } catch { }
            }
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (UserManager.ComprarJuego(_idUsuario, _juego.Id))
            {
                MessageBox.Show($"¡Gracias por tu compra!\n{_juego.Titulo} se ha añadido a tu biblioteca.", "Compra Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Error: Ya tienes este juego o hubo un problema de conexión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}