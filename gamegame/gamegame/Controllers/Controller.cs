using gamegame.Models;
using gamegame.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gamegame.Controllers
{
    // Для передачи состояния во View
    public class WorldState
    {
        public float PlayerX { get; set; }
        public float PlayerY { get; set; }
        public int PlayerHealth { get; set; }
        public int Score { get; set; }
        public bool IsGameOver { get; set; }
        public List<Platform> Platforms { get; set; }
    }
    public class Controller
    {
        private World _model;
        private MainForm _view;

        // Флаги для движения
        private bool _movingLeft;
        private bool _movingRight;
        private Timer _moveTimer;

        public Controller(MainForm view)
        {
            _model = new World();
            _view = view;

            // Таймер для непрерывного движения
            _moveTimer = new Timer();
            _moveTimer.Interval = 16;
            _moveTimer.Tick += (s, e) => {
                if (_movingLeft) _model.MovePlayerLeft();
                if (_movingRight) _model.MovePlayerRight();
                if (!_movingLeft && !_movingRight) _model.StopPlayer();
            };
            _moveTimer.Start();
        }

        private void MoveTimer_Tick(object sender, System.EventArgs e)
        {
            // Обработка непрерывного движения
            if (_movingLeft) _model.MovePlayerLeft();
            if (_movingRight) _model.MovePlayerRight();
        }

        // Обновление мира (вызывается из таймера)
        public void UpdateGame()
        {
            _model.Update();
        }

        // Действия с клавы
        public void StartMovingLeft()
        {
            _movingLeft = true;
        }

        public void StartMovingRight()
        {
            _movingRight = true;
        }

        public void StopMovingLeft()
        {
            _movingLeft = false;
        }

        public void StopMovingRight()
        {
            _movingRight = false;
        }

        public void Jump()
        {
            // Если на стене - делаем валлджамп
            if (_model.Player.IsOnWall)
            {
                _model.PlayerWallJump();
            }
            else
            {
                _model.PlayerJump();
            }
        }

        public bool IsPlayerGrabbingWall()
        {
            return _model.Player.IsGrabbingWall;
        }

        // Получение состояния для отрисовки
        public WorldState GetWorldState()
        {
            return new WorldState
            {
                PlayerX = _model.Player.X,
                PlayerY = _model.Player.Y,
                PlayerHealth = _model.Player.Health,
                Score = _model.Score,
                IsGameOver = _model.IsGameOver,
                Platforms = _model.Platforms
            };
        }
        public bool CanDoubleJump()
        {
            return _model.Player.JumpCount < 2;
        }

        public bool IsPlayerOnWall()
        {
            return _model.Player.IsOnWall;
        }

        public void RestartGame()
        {
            _model = new World();  // Создаем новый мир
        }
    }
}
