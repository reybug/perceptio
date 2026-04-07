using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamegame.Models
{
    public class World
    {
        public PlayerCher Player { get; private set; }
        public List<Platform> Platforms { get; private set; }
        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

        // Размеры игрока для коллизий
        private const int PlayerWidth = 32;
        private const int PlayerHeight = 32;

        // Действия, которые вызываются из контроллера
        public void MovePlayerLeft() => Player.MoveLeft();
        public void MovePlayerRight() => Player.MoveRight();
        public void PlayerJump() => Player.Jump();
        public void StopPlayer() => Player.Stop();

        public World()
        {
            // + игрок
            Player = new PlayerCher(100, 400);

            // + платформы
            Platforms = new List<Platform>
            {
                new Platform(0, 450, 800, 50),  // Земля
                new Platform(200, 380, 100, 20), // Средняя
                new Platform(500, 320, 100, 20), // Высокая
                new Platform(50, 300, 80, 20)    // Левая
            };

            Score = 0;
            IsGameOver = false;
        }

        // Главный метод обновления мира (каждый кадр из таймера)
        public void Update()
        {
            if (IsGameOver) return;

            // Обновление физики
            Player.UpdatePhysics();

            // Обрабатываем коллизии
            HandlePlatformCollisions();

            // Проверка на смерть от падения
            if (Player.Y > 600)
            {
                Player.TakeDamage(3);
                RespawnPlayer();
            }

            // Проверяем game over
            if (!Player.IsAlive)
            {
                IsGameOver = true;
            }
        }

        private void HandlePlatformCollisions()
        {
            Player.IsOnGround = false;

            foreach (var platform in Platforms)
            {
                if (platform.CollidesWith(Player, PlayerWidth, PlayerHeight, out float newY, out bool isOnTop))
                {
                    Player.Y = newY;
                    if (isOnTop)
                    {
                        Player.VelocityY = 0;
                        Player.IsOnGround = true;
                    }
                    else
                    {
                        // Столкновение снизу или сбоку
                        if (Player.VelocityY < 0)
                            Player.VelocityY = 0;
                    }
                }
            }

            // Границы мира
            if (Player.X < 0) Player.X = 0;
            if (Player.X > 800 - PlayerWidth) Player.X = 800 - PlayerWidth;
        }

        private void RespawnPlayer()
        {
            Player.SetPosition(100, 400);
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            Player.IsOnGround = true;
        }

        // Для тестов
        public int GetPlayerHealth() => Player.Health;
        public (float X, float Y) GetPlayerPosition() => (Player.X, Player.Y);
    }
}
