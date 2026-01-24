using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class AdminWindow : Window
    {
        private string _adminResponsable;
        private bool _modoEdicion = false; // False = CREAR NUEVO, True = EDITAR EXISTENTE

        public AdminWindow(string adminName)
        {
            InitializeComponent();
            _adminResponsable = adminName;
            CargarUsuarios();
            LimpiarFormulario(); // Empezar en modo crear
        }

        private void CargarUsuarios(string filtro = "")
        {
            List<Usuario> lista = UserManager.ObtenerTodosUsuarios(filtro);
            GridUsuarios.ItemsSource = lista;
        }

        // Al seleccionar un usuario de la tabla -> MODO EDICIÓN
        private void GridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridUsuarios.SelectedItem is Usuario u)
            {
                // Rellenamos los campos con los datos del usuario seleccionado
                txtNombre.Text = u.NombreUsuario;
                txtPass.Text = u.Password;
                txtEmail.Text = u.Email;
                cmbRol.Text = u.Rol;
                cmbEstado.Text = u.Estado;

                // Bloqueamos el nombre (clave primaria lógica) y cambiamos el botón
                txtNombre.IsEnabled = false;
                BtnAccion.Content = "GUARDAR CAMBIOS"; // Feedback visual
                _modoEdicion = true;
            }
        }

        // Botón Verde (Sirve para Crear o Guardar según el modo)
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
            {
                MessageBox.Show("El nombre y la contraseña son obligatorios."); return;
            }

            // Creamos el objeto usuario con los datos del formulario
            Usuario u = new Usuario
            {
                NombreUsuario = txtNombre.Text,
                Password = txtPass.Text,
                Email = txtEmail.Text,
                Rol = cmbRol.Text,
                Estado = cmbEstado.Text
            };

            if (_modoEdicion)
            {
                // --- MODO EDICIÓN ---
                if (UserManager.ActualizarUsuarioAdmin(u, _adminResponsable))
                    MessageBox.Show($"Usuario '{u.NombreUsuario}' actualizado correctamente.");
                else
                    MessageBox.Show("Error al actualizar. Verifica la conexión.");
            }
            else
            {
                // --- MODO CREACIÓN (NUEVO) ---
                // Aquí el ID se genera solo en la base de datos (Auto Increment)
                if (UserManager.InsertarUsuarioAdmin(u, _adminResponsable))
                    MessageBox.Show($"Usuario '{u.NombreUsuario}' creado con éxito.");
                else
                    MessageBox.Show("Error: Ya existe un usuario con ese nombre.");
            }

            // Recargamos la tabla para ver los cambios
            CargarUsuarios();
            LimpiarFormulario();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) return;

            if (MessageBox.Show($"¿Seguro que quieres eliminar permanentemente a {txtNombre.Text}?", "Confirmar Borrado", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                UserManager.EliminarUsuario(txtNombre.Text, _adminResponsable);
                CargarUsuarios();
                LimpiarFormulario();
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        // Prepara el formulario para un NUEVO usuario
        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtNombre.IsEnabled = true; // Habilitamos nombre porque es nuevo
            txtPass.Text = "";
            txtEmail.Text = "";

            // Valores por defecto
            cmbRol.SelectedIndex = 0; // Nominal
            cmbEstado.SelectedIndex = 0; // Activo

            GridUsuarios.SelectedItem = null;

            // Cambiamos el texto del botón para que el admin sepa que está creando
            BtnAccion.Content = "CREAR USUARIO";
            _modoEdicion = false;
        }

        private void TxtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            CargarUsuarios(txtBuscar.Text);
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            CargarUsuarios();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}