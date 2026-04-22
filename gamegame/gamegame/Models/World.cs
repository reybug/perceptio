using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gamegame.Models;

namespace gamegame.Models
{
    public class World
    {
        public PlayerCher Player { get; private set; }
        public List<Enemy> Enemies { get; private set; }
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
        public void PlayerWallJump() => Player.WallJump();

        public World()
        {
            // + игрок
            Player = new PlayerCher(100, 400);

            // не кенты 
            Enemies = new List<Enemy>
            {
                // Левый
                new Enemy(220, 350, 200, 280),
                
                // Правый
                new Enemy(650, 420, 550, 750)
            };

            // + платформы
            Platforms = new List<Platform>
            {
                // Земля (обычная платформа)
                new Platform(0, 450, 800, 50, PlatformType.Normal),
                
                // Обычные горизонтальные платформы (нельзя цепляться)
                new Platform(200, 380, 100, 20, PlatformType.Normal),
                new Platform(500, 320, 100, 20, PlatformType.Normal),
                
                // СТЕНЫ (вертикальные платформы - можно цепляться!)
                new Platform(680, 280, 20, 100, PlatformType.Wall),   // Правая стена
                new Platform(100, 280, 20, 100, PlatformType.Wall),    // Левая стена
                new Platform(400, 200, 20, 80, PlatformType.Wall),     // Стена в воздухе для тренировки
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
            CheckWallCollisions();

            foreach (var enemy in Enemies)
            {
                enemy.Update();
            }

            HandleEnemyCollisions();

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
                    else if (Player.VelocityY < 0)
                    {
                        // Столкновение снизу или сбоку
                            Player.VelocityY = 0;
                    }
                }
            }

            // Границы мира
            if (Player.X < 0) Player.X = 0;
            if (Player.X > 800 - PlayerWidth) Player.X = 800 - PlayerWidth;
        }

        // Метод в GameWorld для проверки касания стен
        private void CheckWallCollisions()
        {
            bool touchingLeft = false;
            bool touchingRight = false;

            foreach (var platform in Platforms)
            {
                // Только стены (PlatformType.Wall)
                if (platform.Type != PlatformType.Wall) continue;

                // Проверка касания левой стороны стены (игрок справа от стены)
                if (Player.X + 32 > platform.X &&
                    Player.X + 32 < platform.X + platform.Width + 10 &&
                    Player.Y + 25 > platform.Y &&
                    Player.Y + 10 < platform.Y + platform.Height)
                {
                    touchingRight = true;
                }

                // Проверка касания правой стороны стены (игрок слева от стены)
                if (Player.X < platform.X + platform.Width &&
                    Player.X + 10 > platform.X - 10 &&
                    Player.Y + 25 > platform.Y &&
                    Player.Y + 10 < platform.Y + platform.Height)
                {
                    touchingLeft = true;
                }
            }

            Player.CheckWallCollision(touchingLeft, touchingRight);
        }

        private void HandleEnemyCollisions()
        {
            // Обрабатываем каждого врага отдельно
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];
                if (!enemy.IsAlive) continue;

                // Проверяем, наступил ли игрок на врага сверху
                if (enemy.IsPlayerLandingOnTop(Player, PlayerWidth, PlayerHeight))
                {
                    enemy.Die();
                    Score += 100;
                    Player.VelocityY = -10;  // Отскок после убийства врага
                    continue;  // Переходим к следующему врагу
                }

                // Обычное столкновение сбоку - игрок получает урон
                if (enemy.CollidesWith(Player, PlayerWidth, PlayerHeight))
                {
                    Player.TakeDamage(1);

                    // Отбрасываем игрока в сторону от врага
                    if (Player.X < enemy.X)
                    {
                        Player.VelocityX = -8;
                    }
                    else
                    {
                        Player.VelocityX = 8;
                    }
                    Player.VelocityY = -6;

                    break;  // Только один враг может ударить за кадр
                }
            }

            // Удаляем мертвых врагов
            Enemies.RemoveAll(e => !e.IsAlive);
        }
        private void RespawnPlayer()
        {
            Player.SetPosition(100, 400);
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            Player.IsOnGround = true;
        }

        public void Restart()
        {
            Player.Reset();
            Player.SetPosition(100, 400);

            // Восстанавливаем врагов
            Enemies.Clear();
            Enemies.Add(new Enemy(220, 350, 200, 280));
            Enemies.Add(new Enemy(650, 420, 550, 750));

            Score = 0;
            IsGameOver = false;
        }

        // Для тестов
        public int GetPlayerHealth() => Player.Health;
        public (float X, float Y) GetPlayerPosition() => (Player.X, Player.Y);
    }
}
