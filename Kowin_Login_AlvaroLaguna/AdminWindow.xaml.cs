using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class AdminWindow : Window
    {
        private string _adminResponsable;
        private bool _modoEdicionUsuario = false;
        private bool _modoEdicionJuego = false;
        private int _idJuegoSeleccionado = 0;

        public AdminWindow(string adminName)
        {
            InitializeComponent();
            _adminResponsable = adminName;
            CargarUsuarios();
            CargarCatalogo();
            LimpiarFormularioUsuario();
            LimpiarFormularioJuego();
        }

        // ==========================================
        //  PESTAÑA 1: GESTIÓN DE USUARIOS
        // ==========================================
        private void CargarUsuarios(string filtro = "") { GridUsuarios.ItemsSource = UserManager.ObtenerTodosUsuarios(filtro); }
        private void GridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridUsuarios.SelectedItem is Usuario u)
            {
                txtNombre.Text = u.NombreUsuario; txtNombre.IsEnabled = false;
                txtPass.Text = u.Password; txtEmail.Text = u.Email;
                cmbRol.Text = u.Rol; cmbEstado.Text = u.Estado;
                BtnAccion.Content = "GUARDAR CAMBIOS"; _modoEdicionUsuario = true;
            }
        }
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) return;
            Usuario u = new Usuario { NombreUsuario = txtNombre.Text, Password = txtPass.Text, Email = txtEmail.Text, Rol = cmbRol.Text, Estado = cmbEstado.Text };
            if (_modoEdicionUsuario) UserManager.ActualizarUsuarioAdmin(u, _adminResponsable);
            else UserManager.InsertarUsuarioAdmin(u, _adminResponsable);
            CargarUsuarios(); LimpiarFormularioUsuario();
        }
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) return;
            UserManager.EliminarUsuario(txtNombre.Text, _adminResponsable); CargarUsuarios(); LimpiarFormularioUsuario();
        }
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e) { LimpiarFormularioUsuario(); }
        private void LimpiarFormularioUsuario() { txtNombre.Text = ""; txtNombre.IsEnabled = true; txtPass.Text = ""; txtEmail.Text = ""; cmbRol.SelectedIndex = 0; cmbEstado.SelectedIndex = 0; GridUsuarios.SelectedItem = null; BtnAccion.Content = "CREAR USUARIO"; _modoEdicionUsuario = false; }
        private void TxtBuscar_KeyUp(object sender, KeyEventArgs e) { CargarUsuarios(txtBuscar.Text); }
        private void BtnRefrescar_Click(object sender, RoutedEventArgs e) { txtBuscar.Text = ""; CargarUsuarios(); }
        private void BtnSalir_Click(object sender, RoutedEventArgs e) { this.Close(); }

        // ==========================================
        //  PESTAÑA 2: GESTIÓN DE VIDEOJUEGOS
        // ==========================================

        private void CargarCatalogo()
        {
            // Cargar TODOS los juegos (incluidos los no visibles)
            GridJuegos.ItemsSource = UserManager.ObtenerCatalogo(null, false);
        }

        private void GridJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridJuegos.SelectedItem is Juego j)
            {
                _idJuegoSeleccionado = j.Id;
                txtJuegoTitulo.Text = j.Titulo;
                cmbJuegoCategoria.Text = j.Categoria;
                txtJuegoPrecio.Text = j.Precio.ToString();
                txtJuegoFab.Text = j.Fabricante;
                txtJuegoImg.Text = j.ImagenUrl;
                txtJuegoAnio.Text = j.Anio.ToString(); // Cargar Año
                chkJuegoVisible.IsChecked = j.Visible;

                try { imgPreview.Source = new BitmapImage(new Uri(j.ImagenUrl, UriKind.RelativeOrAbsolute)); } catch { }

                BtnJuegoAccion.Content = "GUARDAR CAMBIOS";
                _modoEdicionJuego = true;
            }
        }

        private void BtnGuardarJuego_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtJuegoTitulo.Text)) return;
            decimal.TryParse(txtJuegoPrecio.Text, out decimal precio);
            int.TryParse(txtJuegoAnio.Text, out int anio); // Parsear Año

            Juego j = new Juego
            {
                Id = _modoEdicionJuego ? _idJuegoSeleccionado : 0,
                Titulo = txtJuegoTitulo.Text,
                Categoria = cmbJuegoCategoria.Text,
                ImagenUrl = txtJuegoImg.Text,
                Precio = precio,
                Fabricante = txtJuegoFab.Text,
                Anio = anio, // Guardar Año
                Visible = chkJuegoVisible.IsChecked == true
            };

            UserManager.GuardarJuegoAdmin(j, _adminResponsable);
            CargarCatalogo();
            LimpiarFormularioJuego();
            MessageBox.Show("Juego guardado correctamente.");
        }

        private void BtnEliminarJuego_Click(object sender, RoutedEventArgs e)
        {
            if (_idJuegoSeleccionado == 0) return;
            UserManager.EliminarJuego(_idJuegoSeleccionado);
            CargarCatalogo();
            LimpiarFormularioJuego();
        }

        private void BtnLimpiarJuego_Click(object sender, RoutedEventArgs e) { LimpiarFormularioJuego(); }

        private void LimpiarFormularioJuego()
        {
            txtJuegoTitulo.Text = ""; txtJuegoPrecio.Text = ""; txtJuegoFab.Text = ""; txtJuegoImg.Text = ""; txtJuegoAnio.Text = "";
            chkJuegoVisible.IsChecked = true; cmbJuegoCategoria.SelectedIndex = 0;
            imgPreview.Source = null;
            GridJuegos.SelectedItem = null;
            BtnJuegoAccion.Content = "AÑADIR JUEGO";
            _modoEdicionJuego = false;
            _idJuegoSeleccionado = 0;
        }
    }
}