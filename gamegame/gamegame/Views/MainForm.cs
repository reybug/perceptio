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

            // ========== РИСУЕМ МИР СО СМЕЩЕНИЕМ КАМЕРЫ ==========
            g.TranslateTransform(-_cameraOffset, 0);

            // Платформы
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

            // Точки восстановления (кристаллы)
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

            // РИСУЕМ ВЕСЫ
            g.ResetTransform();

            int scaleX = 2500 - _cameraOffset;  // Левая граница весов

            // ПОЛ под весами
            g.FillRectangle(Brushes.SaddleBrown, scaleX, 550, 500, 30);
            g.DrawRectangle(Pens.Black, scaleX, 550, 500, 30);

            // ЦЕНТРАЛЬНАЯ СТОЙКА (огромная) - X = 2720-2760
            int columnX = scaleX + 220;
            g.FillRectangle(Brushes.SaddleBrown, columnX, 250, 40, 300);
            g.DrawRectangle(Pens.Black, columnX, 250, 40, 300);

            // ВЕРХНЯЯ ПЕРЕКЛАДИНА (широкое коромысло) - X = 2500-3000
            g.FillRectangle(Brushes.Peru, scaleX, 250, 500, 25);
            g.DrawRectangle(Pens.Black, scaleX, 250, 500, 25);

            // ВЕРЁВКИ/ЦЕПИ для левой чаши
            using (Pen chainPen = new Pen(Brushes.Gold, 6))
            {
                // Левая чаша: верёвки от перекладины (X=2560-2680, Y=275) до чаши (Y=400)
                g.DrawLine(chainPen, scaleX + 60, 275, scaleX + 70, 400);
                g.DrawLine(chainPen, scaleX + 170, 275, scaleX + 170, 400);
            }

            // ВЕРЁВКИ/ЦЕПИ для правой чаши
            using (Pen chainPen = new Pen(Brushes.Gold, 6))
            {
                g.DrawLine(chainPen, scaleX + 280, 275, scaleX + 290, 400);
                g.DrawLine(chainPen, scaleX + 390, 275, scaleX + 390, 400);
            }

            // ЛЕВАЯ ЧАША (ОГРОМНАЯ, ФИНИШ)
            g.FillRectangle(Brushes.Gold, scaleX + 60, 400, 120, 25);
            g.DrawRectangle(Pens.DarkGoldenrod, scaleX + 60, 400, 120, 25);

            // БОЛЬШАЯ ЗВЕЗДА на левой чаше
            g.DrawString("★", new Font("Arial", 28, FontStyle.Bold), Brushes.Red, scaleX + 100, 396);

            // ПРАВАЯ ЧАША (ОГРОМНАЯ, декоративная)
            g.FillRectangle(Brushes.Gold, scaleX + 300, 400, 120, 25);
            g.DrawRectangle(Pens.DarkGoldenrod, scaleX + 300, 400, 120, 25);
            // ========== ЭКРАНЫ GAME OVER ИЛИ ПОБЕДА ==========
            if (worldState.IsGameOver)
            {
                if (worldState.IsVictory)
                    DrawVictory(g);
                else
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

        private void DrawVictory(Graphics g)
        {
            // Полупрозрачный фон
            using (Brush overlay = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
            {
                g.FillRectangle(overlay, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            var victoryFont = new Font("Arial", 48, FontStyle.Bold);
            var size = g.MeasureString("VICTORY!", victoryFont);
            g.DrawString("VICTORY!", victoryFont, Brushes.Gold,
                400 - size.Width / 2, 200 - size.Height / 2);

            var scoreFont = new Font("Arial", 24);
            var scoreText = $"Final Score: {_controller.GetScore()}";
            var scoreSize = g.MeasureString(scoreText, scoreFont);
            g.DrawString(scoreText, scoreFont, Brushes.White,
                400 - scoreSize.Width / 2, 270 - scoreSize.Height / 2);

            var restartFont = new Font("Arial", 20);
            var restartText = "Press R to play again";
            var restartSize = g.MeasureString(restartText, restartFont);
            g.DrawString(restartText, restartFont, Brushes.LightBlue,
                400 - restartSize.Width / 2, 350 - restartSize.Height / 2);
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
