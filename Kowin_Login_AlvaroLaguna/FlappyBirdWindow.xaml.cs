using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class FlappyBirdWindow : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer();
        double score;
        int gravity = 8;
        bool gameOver;
        Rect birdHitBox;

        public FlappyBirdWindow()
        {
            InitializeComponent();

            // Configurar el bucle del juego (20ms = ~50 FPS)
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);

            StartGame();
        }

        private void StartGame()
        {
            // Poner el foco en el Canvas para recibir teclas
            MyCanvas.Focus();

            // Resetear variables
            score = 0;
            gameOver = false;
            Canvas.SetTop(flappyBird, 190); // Posición inicial pájaro

            // Posición inicial tuberías
            Canvas.SetLeft(pipeTop, 300);
            Canvas.SetLeft(pipeBottom, 300);

            // Ocultar pantalla Game Over
            GameOverBorder.Visibility = Visibility.Collapsed;
            txtStart.Visibility = Visibility.Visible;

            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Actualizar puntuación
            txtScore.Content = "Score: " + (int)score;

            // Mover pájaro (Gravedad)
            // Simula caer sumando a Top
            Canvas.SetTop(flappyBird, Canvas.GetTop(flappyBird) + gravity);

            // Mover tuberías hacia la izquierda (velocidad 8)
            Canvas.SetLeft(pipeTop, Canvas.GetLeft(pipeTop) - 8);
            Canvas.SetLeft(pipeBottom, Canvas.GetLeft(pipeBottom) - 8);

            // --- LÓGICA DE TUBERÍAS ---
            // Si la tubería sale de la pantalla (-50 ancho), la reseteamos a la derecha
            if (Canvas.GetLeft(pipeTop) < -50)
            {
                Canvas.SetLeft(pipeTop, 400); // Volver al inicio
                Canvas.SetLeft(pipeBottom, 400);

                score += 0.5; // Sumamos medio punto por par de tuberías
            }

            // --- COLISIONES ---
            // Creamos rectángulos invisibles (hitboxes) para comprobar choques
            birdHitBox = new Rect(Canvas.GetLeft(flappyBird), Canvas.GetTop(flappyBird), flappyBird.Width, flappyBird.Height);

            Rect pipeTopHitBox = new Rect(Canvas.GetLeft(pipeTop), Canvas.GetTop(pipeTop), pipeTop.Width, pipeTop.Height);
            Rect pipeBottomHitBox = new Rect(Canvas.GetLeft(pipeBottom), Canvas.GetTop(pipeBottom), pipeBottom.Width, pipeBottom.Height);

            // Si choca con el suelo, el techo o las tuberías -> GAME OVER
            if (Canvas.GetTop(flappyBird) < -10 || Canvas.GetTop(flappyBird) > 460 ||
                birdHitBox.IntersectsWith(pipeTopHitBox) || birdHitBox.IntersectsWith(pipeBottomHitBox))
            {
                EndGame();
            }

            // Truco: Si score > 5, aumenta la velocidad (Dificultad)
            if (score > 5)
            {
                // Podrías aumentar velocidad aquí si quisieras
            }
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Saltar con Espacio
            if (e.Key == Key.Space)
            {
                // Invertimos la gravedad para subir
                gravity = -15;
                txtStart.Visibility = Visibility.Collapsed; // Ocultar texto ayuda
            }
            // Tecla Escape para salir rápido
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Canvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Al soltar, la gravedad vuelve a ser positiva (caer)
                gravity = 8;
            }
        }

        private void EndGame()
        {
            gameTimer.Stop();
            gameOver = true;
            txtFinalScore.Text = "Puntuación final: " + (int)score;
            GameOverBorder.Visibility = Visibility.Visible;
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
