using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace gamegame.Models
{
    public class World
    {
        public PlayerCher Player { get; private set; }
        public List<Enemy> Enemies { get; private set; }
        public List<Platform> Platforms { get; private set; }
        public List<JumpRefill> JumpRefills { get; private set; }
        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsVictory { get; private set; }

        private const int PlayerWidth = 32;
        private const int PlayerHeight = 32;

        public World()
        {
            Player = new PlayerCher(100, 400);
            Enemies = new List<Enemy>();
            Platforms = new List<Platform>();
            JumpRefills = new List<JumpRefill>();
            IsVictory = false;

            BuildLevel();
        }

        private void BuildLevel()
        {
            // ===========================================
            // СЕКЦИЯ 1: СТАРТОВЫЙ БЛОК + ЛЕСЕНКА (2 пути)
            // ===========================================

            // Стартовая платформа (ДЛИННАЯ)
            Platforms.Add(new Platform(0, 450, 300, 30, PlatformType.Normal));

            // Лесенка вверх (путь 1 - для новичков) - РАСТЯНУТА
            Platforms.Add(new Platform(150, 420, 40, 15, PlatformType.Normal));
            Platforms.Add(new Platform(190, 390, 40, 15, PlatformType.Normal));
            Platforms.Add(new Platform(230, 360, 40, 15, PlatformType.Normal));
            Platforms.Add(new Platform(270, 330, 40, 15, PlatformType.Normal));
            Platforms.Add(new Platform(310, 300, 40, 15, PlatformType.Normal));
            Platforms.Add(new Platform(350, 270, 40, 15, PlatformType.Normal));

            // Вертикальный блок в конце стартовой платформы (путь 2 - для опытных)
            Platforms.Add(new Platform(250, 370, 20, 100, PlatformType.Wall));

            // Верхняя горизонтальная платформа (общая цель)
            Platforms.Add(new Platform(400, 260, 100, 20, PlatformType.Normal));

            // Точка восстановления
            JumpRefills.Add(new JumpRefill(450, 240));

            // ===========================================
            // СЕКЦИЯ 2: ВЕРТИКАЛЬНЫЕ ПЛАТФОРМЫ + СТЕНЫ + ВРАГ
            // ===========================================

            // Разрыв
            Platforms.Add(new Platform(550, 320, 80, 20, PlatformType.Normal));

            // Стены для карабканья (зигзаг) - РАСТЯНУТ
            Platforms.Add(new Platform(650, 380, 25, 120, PlatformType.Wall));   // Стена 1
            Platforms.Add(new Platform(750, 300, 25, 120, PlatformType.Wall));   // Стена 2
            Platforms.Add(new Platform(650, 220, 25, 120, PlatformType.Wall));   // Стена 3
            Platforms.Add(new Platform(750, 140, 25, 120, PlatformType.Wall));   // Стена 4

            // Платформы для отдыха
            Platforms.Add(new Platform(700, 260, 50, 15, PlatformType.Normal));
            Platforms.Add(new Platform(700, 180, 50, 15, PlatformType.Normal));

            // Враг
            Platforms.Add(new Platform(850, 280, 80, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(880, 250, 830, 920));

            // Выход
            Platforms.Add(new Platform(950, 130, 100, 20, PlatformType.Normal));

            JumpRefills.Add(new JumpRefill(1000, 110));

            // ===========================================
            // СЕКЦИЯ 3: ОГРОМНАЯ ПРОПАСТЬ + КРИСТАЛЛ В ВОЗДУХЕ
            // ===========================================

            // Большой разрыв - РАСТЯНУТ
            Platforms.Add(new Platform(1150, 300, 60, 20, PlatformType.Normal));
            Platforms.Add(new Platform(1350, 280, 60, 20, PlatformType.Normal));

            // Кристалл В ВОЗДУХЕ между ними!
            JumpRefills.Add(new JumpRefill(1400, 250));

            Platforms.Add(new Platform(1550, 260, 60, 20, PlatformType.Normal));
            Platforms.Add(new Platform(1750, 240, 60, 20, PlatformType.Normal));

            JumpRefills.Add(new JumpRefill(1800, 220));

            // ===========================================
            // СЕКЦИЯ 4: ВРАГИ КАК ПЛАТФОРМЫ
            // ===========================================

            Platforms.Add(new Platform(1950, 280, 80, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(1980, 250, 1940, 2020));

            Platforms.Add(new Platform(2150, 240, 80, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(2180, 210, 2140, 2220));

            Platforms.Add(new Platform(2350, 200, 60, 20, PlatformType.Normal));

            JumpRefills.Add(new JumpRefill(2400, 180));

            // ===========================================
            // СЕКЦИЯ 5: ОГРОМНЫЕ ВЕСЫ (ФИНАЛ)
            // ===========================================

            // ПОЛ под весами (большая платформа)
            Platforms.Add(new Platform(2500, 550, 500, 30, PlatformType.Normal));

            // ЦЕНТРАЛЬНАЯ СТОЙКА (ОГРОМНАЯ, от пола до верха)
            Platforms.Add(new Platform(2720, 250, 40, 300, PlatformType.Wall));

            // ВЕРХНЯЯ ПЕРЕКЛАДИНА (широкая, как коромысло)
            Platforms.Add(new Platform(2500, 250, 500, 25, PlatformType.Normal));

            // ЛЕВАЯ ЧАША (ОГРОМНАЯ) - ФИНИШ
            Platforms.Add(new Platform(2560, 400, 120, 25, PlatformType.Normal));

            // ПРАВАЯ ЧАША (ОГРОМНАЯ, для красоты)
            Platforms.Add(new Platform(2800, 400, 120, 25, PlatformType.Normal));
        }

        public void Update()
        {
            if (IsGameOver) return;

            Player.UpdatePhysics();
            HandlePlatformCollisions();
            CheckWallCollisions();
            HandleJumpRefills();

            foreach (var enemy in Enemies)
            {
                enemy.Update();
            }

            HandleEnemyCollisions();

            // Проверка победы - встать на левую чашу (X от 2560 до 2680)
            foreach (var platform in Platforms)
            {
                if (platform.X >= 2560 && platform.X <= 2680 && platform.Type == PlatformType.Normal && platform.Y == 400)
                {
                    if (Player.IsOnGround &&
                        Math.Abs(Player.Y + PlayerHeight - platform.Y) <= 5 &&
                        Player.X + PlayerWidth > platform.X &&
                        Player.X < platform.X + platform.Width)
                    {
                        IsVictory = true;
                        IsGameOver = true;
                        Score += 1000;
                        break;
                    }
                }
            }

            // Падение в пропасть
            if (Player.Y > 600)
            {
                Player.TakeDamage(1);
                RespawnPlayer();
            }

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
                        Player.VelocityY = 0;
                    }
                }
            }

            // Границы мира по бокам
            if (Player.X < 0) Player.X = 0;
            if (Player.X > 3100 - PlayerWidth) Player.X = 3100 - PlayerWidth;
        }

        private void CheckWallCollisions()
        {
            bool touchingLeft = false;
            bool touchingRight = false;

            foreach (var platform in Platforms)
            {
                if (platform.Type != PlatformType.Wall) continue;

                if (platform.CheckWallCollision(Player, PlayerWidth, PlayerHeight,
                    out bool left, out bool right))
                {
                    touchingLeft = touchingLeft || left;
                    touchingRight = touchingRight || right;
                }
            }

            Player.CheckWallCollision(touchingLeft, touchingRight);
        }

        private void HandleJumpRefills()
        {
            for (int i = 0; i < JumpRefills.Count; i++)
            {
                var refill = JumpRefills[i];
                if (refill.IsActive && refill.CollidesWith(Player, PlayerWidth, PlayerHeight))
                {
                    refill.Collect();
                    Player.ResetJumps();
                    Score += 50;
                }
            }
        }

        private void HandleEnemyCollisions()
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];
                if (!enemy.IsAlive) continue;

                if (enemy.CollidesWith(Player, PlayerWidth, PlayerHeight))
                {
                    // Падение сверху = убийство врага
                    if (Player.VelocityY > 0 && Player.Y + PlayerHeight - Player.VelocityY <= enemy.Y + 10)
                    {
                        enemy.Die();
                        Score += 100;
                        Player.VelocityY = -10;
                    }
                    else if (!Player.IsInvincible)
                    {
                        Player.TakeDamage(1);

                        // Отбрасывание
                        if (Player.X < enemy.X)
                            Player.VelocityX = -10;
                        else
                            Player.VelocityX = 10;
                        Player.VelocityY = -8;
                        break;
                    }
                }
            }

            Enemies.RemoveAll(e => !e.IsAlive);
        }

        private void RespawnPlayer()
        {
            Player.SetPosition(100, 400);
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            Player.IsOnGround = true;
        }

        // Методы для Controller
        public void MovePlayerLeft() => Player.MoveLeft();
        public void MovePlayerRight() => Player.MoveRight();
        public void PlayerJump() => Player.Jump();
        public void StopPlayer() => Player.Stop();
        public void PlayerWallJump() => Player.WallJump();

        // Для камеры
        public int GetCameraOffset()
        {
            float offset = Player.X - 400;
            if (offset < 0) offset = 0;
            if (offset > 2500) offset = 2500;
            return (int)offset;
        }

        // Рестарт игры
        public void Restart()
        {
            Player.Reset();
            Player.SetPosition(100, 400);
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            Player.IsOnGround = true;

            Enemies.Clear();
            Platforms.Clear();
            JumpRefills.Clear();

            Score = 0;
            IsGameOver = false;
            IsVictory = false;

            BuildLevel();
        }
    }
}