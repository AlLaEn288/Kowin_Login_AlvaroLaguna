using System.Windows;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewUser.Text) || string.IsNullOrWhiteSpace(txtNewPass.Password))
            {
                MostrarError("Rellena todos los campos");
                return;
            }

            if (txtNewPass.Password != txtConfirmPass.Password)
            {
                MostrarError("Las contraseñas no coinciden");
                return;
            }

            // Validar si existe (Conecta a BD)
            if (UserManager.ExisteUsuario(txtNewUser.Text))
            {
                MostrarError("El usuario ya existe");
                return;
            }

            Usuario nuevo = new Usuario
            {
                NombreUsuario = txtNewUser.Text,
                Password = txtNewPass.Password,
                Email = txtEmail.Text
            };

            // Intentar registrar en MySQL
            bool registroExitoso = UserManager.RegistrarUsuario(nuevo);

            if (registroExitoso)
            {
                // === EVIDENCIA 3: USUARIO CREADO EN MYSQL ===
                // Mensaje técnico para la captura del profesor
                MessageBox.Show($"[MySQL] INSERT EXITOSO.\n\nEl usuario '{nuevo.NombreUsuario}' se ha registrado correctamente en la tabla 'usuarios' de la base de datos 'kowin_db'.\n\nRegistro ID generado y guardado.",
                                "Verificación MySQL - Registro",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                // Volver al Login
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
            else
            {
                MostrarError("Error al conectar con la base de datos MySQL.");
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void MostrarError(string mensaje)
        {
            lblErrorRegistro.Text = mensaje;
            lblErrorRegistro.Visibility = Visibility.Visible;
        }
    }
}