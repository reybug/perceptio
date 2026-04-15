using gamegame.Controllers;
using gamegame.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gamegame.Views
{
    public partial class MainForm : Form
    {
        private Controller _controller;
        private Timer _gameTimer;
        private BufferedGraphicsContext _graphicsContext;
        private BufferedGraphics _graphicsBuffer;

        // Цвета для отрисовки (заменить на спрайты)
        private SolidBrush _playerBrush = new SolidBrush(Color.Blue);
        private SolidBrush _enemyBrush = new SolidBrush(Color.Red);
        private SolidBrush _platformBrush = new SolidBrush(Color.Green);
        private SolidBrush _groundBrush = new SolidBrush(Color.SaddleBrown);
        private Font _uiFont = new Font("Arial", 16);
        public MainForm()
        {
            InitializeComponent();

            // Настройки формы
            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);
            this.Text = "Game test";
            this.KeyPreview = true;
            this.BackColor = Color.LightSkyBlue;

            // Контроллер
            _controller = new Controller(this);

            // Плавная отрисовка
            try
            {
                _graphicsContext = BufferedGraphicsManager.Current;
                _graphicsContext.MaximumBuffer = this.ClientSize;
                _graphicsBuffer = _graphicsContext.Allocate(this.CreateGraphics(), this.ClientRectangle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании буфера: {ex.Message}");
                // Если буфер не создался ->mпростая отрисовка
                _graphicsBuffer = null;
            }

            // Таймер (60 FPS)
            _gameTimer = new Timer();
            _gameTimer.Interval = 16; // ~60 FPS
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();

            // События с клавы
            this.KeyDown += MainForm_KeyDown;
            this.KeyUp += MainForm_KeyUp;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Вызов обновления через контроллер
            _controller.UpdateGame();

            // Перерисовка форму
            this.Invalidate();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    _controller.StartMovingLeft();
                    break;
                case Keys.Right:
                case Keys.D:
                    _controller.StartMovingRight();
                    break;
                case Keys.Space:
                case Keys.Up:
                case Keys.W:
                    _controller.Jump();
                    break;
                case Keys.R:
                    _controller.RestartGame();
                    break;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    _controller.StopMovingLeft();
                    break;
                case Keys.Right:
                case Keys.D:
                    _controller.StopMovingRight();
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_graphicsBuffer != null)
            {
                // Используем буферизованную отрисовку
                Graphics g = _graphicsBuffer.Graphics;
                g.Clear(this.BackColor);
                DrawGame(g);
                _graphicsBuffer.Render(e.Graphics);
            }
            else
            {
                DrawGame(e.Graphics);
            }
        }

        private void DrawGame(Graphics g)
        {
            var worldState = _controller.GetWorldState();

            // Рисуем платформы
            foreach (var platform in worldState.Platforms)
            {
                if (platform.Type == PlatformType.Normal)
                {
                    // Обычные платформы - зеленые
                    g.FillRectangle(Brushes.Green, platform.X, platform.Y, platform.Width, platform.Height);
                }
                else if (platform.Type == PlatformType.Wall)
                {
                    // Стены - коричневые/серые с текстурой
                    g.FillRectangle(Brushes.SaddleBrown, platform.X, platform.Y, platform.Width, platform.Height);
                    // Добавляем полоски для визуального отличия
                    using (Pen darkPen = new Pen(Brushes.Brown, 2))
                    {
                        for (int i = 0; i < platform.Height; i += 15)
                        {
                            g.DrawLine(darkPen, platform.X + 5, platform.Y + i,
                                       platform.X + platform.Width - 5, platform.Y + i);
                        }
                    }
                }
            }

            // Выбираем цвет игрока в зависимости от состояния
            Brush playerColor = _playerBrush;
            if (_controller.IsPlayerGrabbingWall())     // НОВЫЙ метод
                playerColor = Brushes.Gold;              // Золотой - зафиксирован на стене
            else if (_controller.IsPlayerOnWall())
                playerColor = Brushes.Orange;            // Оранжевый - касается стены
            else if (_controller.CanDoubleJump())
                playerColor = Brushes.LightBlue;         // Светло-голубой - есть двойной прыжок

            // Рисуем игрока
            g.FillRectangle(playerColor, worldState.PlayerX, worldState.PlayerY, 32, 32);

            // Рисуем информацию об игре
            DrawUI(g, worldState);

            if (worldState.IsGameOver)
            {
                DrawGameOver(g);
            }
        }

        private void DrawUI(Graphics g, WorldState worldState)
        {
            // Сердечки здоровья
            for (int i = 0; i < worldState.PlayerHealth; i++)
            {
                g.FillRectangle(Brushes.Red, 10 + i * 25, 10, 20, 20);
            }

            g.DrawString($"Score: {worldState.Score}", _uiFont, Brushes.Black, 10, 40);

            Font smallFont = new Font("Arial", 12);

            // Информация о двойном прыжке
            string jumpInfo = _controller.CanDoubleJump() ? "★ Double jump ready!" : "☆ No double jump";
            g.DrawString(jumpInfo, smallFont, Brushes.DarkBlue, 10, 70);

            // Информация о стене
            if (_controller.IsPlayerGrabbingWall())
            {
                string wallInfo = "◆ GRABBED! Press SPACE to wall jump! ◆";
                g.DrawString(wallInfo, smallFont, Brushes.Gold, 10, 90);
            }
            else if (_controller.IsPlayerOnWall())
            {
                string wallInfo = "◆ On wall! Stop moving to grab! ◆";
                g.DrawString(wallInfo, smallFont, Brushes.Orange, 10, 90);
            }
        }

        // Обновление буфера при изменении размера окна
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_graphicsContext != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                try
                {
                    _graphicsBuffer?.Dispose();
                    _graphicsBuffer = _graphicsContext.Allocate(this.CreateGraphics(), this.ClientRectangle);
                    this.Invalidate();
                }
                catch { /* Игнор ошибки изменения размера */ }
            }
        }

        private void DrawGameOver(Graphics g)
        {
            // Полупрозрачный фон
            using (Brush overlay = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
            {
                g.FillRectangle(overlay, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            var gameOverFont = new Font("Arial", 48, FontStyle.Bold);
            var size = g.MeasureString("GAME OVER", gameOverFont);
            g.DrawString("GAME OVER", gameOverFont, Brushes.Red,
                400 - size.Width / 2, 250 - size.Height / 2);

            var restartFont = new Font("Arial", 20);
            var restartText = "Press R to restart";
            var restartSize = g.MeasureString(restartText, restartFont);
            g.DrawString(restartText, restartFont, Brushes.White,
                400 - restartSize.Width / 2, 320 - restartSize.Height / 2);
        }

        // Вызывается контроллером, чтобы запросить перерисовку
        public void RefreshView()
        {
            this.Invalidate();
        }
    }
}
