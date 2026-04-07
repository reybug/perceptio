using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using gamegame.Controllers;

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
            // Нажатия клавиш -> контроллер
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
                g.FillRectangle(_platformBrush, platform.X, platform.Y, platform.Width, platform.Height);
            }

            // Рисуем игрока
            g.FillRectangle(_playerBrush, worldState.PlayerX, worldState.PlayerY, 32, 32);

            // UI (типа)
            g.DrawString($"Health: {worldState.PlayerHealth}", _uiFont, Brushes.Black, 10, 10);
            g.DrawString($"Score: {worldState.Score}", _uiFont, Brushes.Black, 10, 40);

            if (worldState.IsGameOver)
            {
                var gameOverFont = new Font("Arial", 32, FontStyle.Bold);
                var size = g.MeasureString("GAME OVER", gameOverFont);
                g.DrawString("GAME OVER", gameOverFont, Brushes.Red,
                    400 - size.Width / 2, 300 - size.Height / 2);
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

        // Вызывается контроллером, чтобы запросить перерисовку
        public void RefreshView()
        {
            this.Invalidate();
        }
    }
}
