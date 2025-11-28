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

            if (UserManager.ExisteUsuario(txtNewUser.Text))
            {
                MostrarError("El usuario ya existe");
                return;
            }

            // CREAR USUARIO
            Usuario nuevo = new Usuario
            {
                NombreUsuario = txtNewUser.Text,
                Password = txtNewPass.Password,
                Email = txtEmail.Text
            };

            UserManager.RegistrarUsuario(nuevo);

            MessageBox.Show("Usuario creado con éxito. Ahora inicia sesión.");

            // Volver al Login
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
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