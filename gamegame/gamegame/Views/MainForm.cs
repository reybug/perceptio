using gamegame.Controllers;
using gamegame.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private int _hitFlashTimer = 0;
        private int _cameraOffset = 0;

        // Цвета для отрисовки (заменить на спрайты)
        private SolidBrush _playerBrush = new SolidBrush(Color.Blue);
        private SolidBrush _enemyBrush = new SolidBrush(Color.Red);
        private SolidBrush _platformBrush = new SolidBrush(Color.Green);
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
            _controller.UpdateGame();
            _cameraOffset = _controller.GetCameraOffset();
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

            // Фоновый градиент
            using (LinearGradientBrush gradient = new LinearGradientBrush(
                new Rectangle(0, 0, 800, 600),
                Color.LightSkyBlue,
                Color.DarkSlateBlue,
                90))
            {
                g.FillRectangle(gradient, 0, 0, 800, 600);
            }

            // ===========================================
            // РИСУЕМ ИГРОВОЙ МИР СО СМЕЩЕНИЕМ КАМЕРЫ
            // ===========================================
            g.TranslateTransform(-_cameraOffset, 0);

            // Рисуем платформы
            foreach (var platform in worldState.Platforms)
            {
                if (platform.Type == PlatformType.Normal)
                {
                    g.FillRectangle(Brushes.Peru, platform.X, platform.Y, platform.Width, platform.Height);
                    g.DrawRectangle(Pens.SaddleBrown, platform.X, platform.Y, platform.Width, platform.Height);
                }
                else if (platform.Type == PlatformType.Wall)
                {
                    g.FillRectangle(Brushes.DarkGray, platform.X, platform.Y, platform.Width, platform.Height);
                    using (Pen darkPen = new Pen(Brushes.Black, 1))
                    {
                        for (int i = 0; i < platform.Height; i += 20)
                        {
                            g.DrawLine(darkPen, platform.X + 5, platform.Y + i,
                                       platform.X + platform.Width - 5, platform.Y + i);
                        }
                    }
                }
            }

            // Точки восстановления
            foreach (var refill in worldState.JumpRefills)
            {
                if (refill.IsActive)
                {
                    Point[] crystal = new Point[]
                    {
                new Point((int)refill.X + 10, (int)refill.Y),
                new Point((int)refill.X + 18, (int)refill.Y + 6),
                new Point((int)refill.X + 18, (int)refill.Y + 14),
                new Point((int)refill.X + 10, (int)refill.Y + 20),
                new Point((int)refill.X + 2, (int)refill.Y + 14),
                new Point((int)refill.X + 2, (int)refill.Y + 6)
                    };

                    for (int i = 3; i > 0; i--)
                    {
                        using (Brush glow = new SolidBrush(Color.FromArgb(40 / i, Color.Cyan)))
                        {
                            g.FillEllipse(glow, refill.X - i * 8, refill.Y - i * 5,
                                          36 + i * 16, 36 + i * 10);
                        }
                    }

                    g.FillPolygon(Brushes.Cyan, crystal);
                    g.DrawPolygon(Pens.White, crystal);
                }
            }

            // Враги
            foreach (var enemy in worldState.Enemies)
            {
                if (enemy.IsAlive)
                {
                    g.FillEllipse(Brushes.DarkRed, enemy.X, enemy.Y, 30, 30);
                    g.FillEllipse(Brushes.Red, enemy.X + 5, enemy.Y + 5, 20, 20);
                    g.DrawLine(Pens.Black, enemy.X + 15, enemy.Y + 10, enemy.X + 15, enemy.Y + 20);
                    g.DrawLine(Pens.Black, enemy.X + 10, enemy.Y + 15, enemy.X + 20, enemy.Y + 15);
                }
            }

            // Игрок
            Brush playerColor = _playerBrush;
            if (_controller.IsPlayerInvincible())
            {
                if ((DateTime.Now.Millisecond / 50) % 2 == 0)
                    playerColor = Brushes.White;
                else
                    playerColor = Brushes.Transparent;
            }
            else if (_controller.IsPlayerGrabbingWall())
                playerColor = Brushes.Gold;
            else if (_controller.IsPlayerOnWall())
                playerColor = Brushes.Orange;
            else if (_controller.CanDoubleJump())
                playerColor = Brushes.LightBlue;

            if (playerColor != Brushes.Transparent)
            {
                g.FillRectangle(playerColor, worldState.PlayerX, worldState.PlayerY, 32, 32);
                g.FillEllipse(Brushes.White, worldState.PlayerX + 22, worldState.PlayerY + 8, 6, 6);
                g.FillEllipse(Brushes.White, worldState.PlayerX + 8, worldState.PlayerY + 8, 6, 6);
                g.FillEllipse(Brushes.Black, worldState.PlayerX + 23, worldState.PlayerY + 9, 3, 3);
                g.FillEllipse(Brushes.Black, worldState.PlayerX + 9, worldState.PlayerY + 9, 3, 3);
            }

            // ФИНИШ (тоже смещается с камерой)
            using (Pen finishPen = new Pen(Brushes.Gold, 8))
            {
                for (int i = 0; i < 5; i++)
                {
                    g.DrawLine(finishPen, 2480 + i * 10, 380, 2480 + i * 10, 430);
                }
            }

            // ===========================================
            // ОТМЕНЯЕМ СМЕЩЕНИЕ КАМЕРЫ ДЛЯ UI
            // ===========================================
            g.ResetTransform();

            // ===========================================
            // РИСУЕМ UI (БЕЗ СМЕЩЕНИЯ)
            // ===========================================
            DrawUI(g, worldState);

            // РИСУЕМ GAME OVER (БЕЗ СМЕЩЕНИЯ)
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
                if (i == 0)
                    g.FillRectangle(Brushes.Red, 10 + i * 30, 10, 25, 22);
                else
                    g.FillRectangle(Brushes.DarkRed, 10 + i * 30, 10, 25, 22);
            }

            // Индикатор двойного прыжка
            if (_controller.CanDoubleJump())
            {
                g.DrawString("★ DOUBLE JUMP READY", new Font("Arial", 10, FontStyle.Bold), Brushes.DarkBlue, 10, 40);
            }

            g.DrawString($"Score: {worldState.Score}", _uiFont, Brushes.Black, 10, 65);

            // Подсказка о стенах
            Font smallFont = new Font("Arial", 9);
            g.DrawString("Tips: | SPACE - jump | A/D - move |", smallFont, Brushes.Gray, 10, 560);
            g.DrawString("| On wall: press SPACE to wall jump | ★ = double jump restore |", smallFont, Brushes.Gray, 10, 575);
        }
        private void DrawGameOver(Graphics g)
        {
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
                catch { }
            }
        }

        // Вызывается контроллером, чтобы запросить перерисовку
        public void RefreshView()
        {
            this.Invalidate();
        }
    }
}
