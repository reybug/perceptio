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
        public List<JumpRefill> JumpRefills { get; private set; }
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
            Enemies = new List<Enemy>();

            // + платформы
            Platforms = new List<Platform>();

            JumpRefills = new List<JumpRefill>();

            BuildLevel();
        }

        private void BuildLevel()
        {
            // ===========================================
            // УРОВЕНЬ 1: ПЛАТФОРМЫ + ДВОЙНОЙ ПРЫЖОК
            // ===========================================

            // Земля
            Platforms.Add(new Platform(0, 450, 300, 50, PlatformType.Normal));

            // Простой разрыв - обычный прыжок
            Platforms.Add(new Platform(350, 450, 80, 20, PlatformType.Normal));

            // Разрыв побольше - нужен двойной прыжок
            Platforms.Add(new Platform(500, 420, 60, 20, PlatformType.Normal));
            Platforms.Add(new Platform(620, 390, 60, 20, PlatformType.Normal));
            Platforms.Add(new Platform(740, 360, 60, 20, PlatformType.Normal));

            // Точка восстановления (видимый кристалл)
            JumpRefills.Add(new JumpRefill(770, 340));

            // ===========================================
            // УРОВЕНЬ 2: СТЕНЫ (обязательное карабканье)
            // ===========================================

            // Вертикальная стена - нужно залезть вверх
            Platforms.Add(new Platform(850, 450, 30, 200, PlatformType.Wall));

            // Платформа сверху, куда можно попасть только через стену
            Platforms.Add(new Platform(820, 250, 60, 20, PlatformType.Normal));

            // Ещё одна стена выше
            Platforms.Add(new Platform(880, 200, 30, 150, PlatformType.Wall));
            Platforms.Add(new Platform(850, 120, 80, 20, PlatformType.Normal));

            JumpRefills.Add(new JumpRefill(890, 100));

            // ===========================================
            // УРОВЕНЬ 3: ПРЫЖКИ МЕЖДУ СТЕНАМИ
            // ===========================================

            // Стена слева
            Platforms.Add(new Platform(950, 400, 25, 150, PlatformType.Wall));
            // Стена справа (выше)
            Platforms.Add(new Platform(1020, 320, 25, 150, PlatformType.Wall));
            // Стена слева (ещё выше)
            Platforms.Add(new Platform(950, 240, 25, 150, PlatformType.Wall));
            // Стена справа (финал)
            Platforms.Add(new Platform(1020, 160, 25, 150, PlatformType.Wall));

            // Выход после серии wall jump
            Platforms.Add(new Platform(990, 100, 80, 20, PlatformType.Normal));

            // ===========================================
            // УРОВЕНЬ 4: ВРАГИ (нужно убивать сверху)
            // ===========================================

            // Платформа с врагом
            Platforms.Add(new Platform(1150, 450, 100, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(1170, 420, 1150, 1250));

            // Нужно подпрыгнуть и приземлиться на врага
            Platforms.Add(new Platform(1300, 400, 80, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(1320, 370, 1300, 1380));

            JumpRefills.Add(new JumpRefill(1380, 380));

            // ===========================================
            // УРОВЕНЬ 5: ВСЁ ВМЕСТЕ
            // ===========================================

            // Стена
            Platforms.Add(new Platform(1450, 400, 25, 150, PlatformType.Wall));
            // Платформа с врагом
            Platforms.Add(new Platform(1530, 350, 80, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(1550, 320, 1530, 1610));
            // Стена после врага
            Platforms.Add(new Platform(1650, 300, 25, 150, PlatformType.Wall));
            // Платформа вверху
            Platforms.Add(new Platform(1620, 200, 80, 20, PlatformType.Normal));

            // ===========================================
            // УРОВЕНЬ 6: ДВОЙНОЙ ПРЫЖОК НАД ПРОПАСТЬЮ
            // ===========================================

            // Огромный разрыв - только двойной прыжок
            Platforms.Add(new Platform(1780, 450, 60, 20, PlatformType.Normal));
            Platforms.Add(new Platform(1920, 400, 60, 20, PlatformType.Normal));
            Platforms.Add(new Platform(2060, 350, 60, 20, PlatformType.Normal));

            JumpRefills.Add(new JumpRefill(2090, 330));

            // ===========================================
            // УРОВЕНЬ 7: ФИНАЛ
            // ===========================================

            // Сложная комбинация: стена -> враг -> платформа -> стена
            Platforms.Add(new Platform(2200, 450, 25, 150, PlatformType.Wall));
            Platforms.Add(new Platform(2280, 400, 80, 20, PlatformType.Normal));
            Enemies.Add(new Enemy(2300, 370, 2280, 2360));
            Platforms.Add(new Platform(2380, 320, 25, 200, PlatformType.Wall));
            Platforms.Add(new Platform(2350, 220, 80, 20, PlatformType.Normal));

            // ФИНИШ
            Platforms.Add(new Platform(2480, 380, 100, 20, PlatformType.Normal));

            // Бонусные точки восстановления в сложных местах
            JumpRefills.Add(new JumpRefill(1650, 280));
            JumpRefills.Add(new JumpRefill(2380, 300));
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
            HandleJumpRefills();

            foreach (var enemy in Enemies)
            {
                enemy.Update();
            }

            HandleEnemyCollisions();

            // Проверка победы
            if (Player.X >= 2480 && Player.Y >= 360 && Player.Y <= 430)
            {
                IsGameOver = true;
                Score += 1000;
            }

            // Проверка на смерть от падения
            if (Player.Y > 600)
            {
                Player.TakeDamage(1);
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
                        Player.VelocityY = 0;
                    }
                }
            }
        }

        // Метод в GameWorld для проверки касания стен
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

        private void RespawnPlayer()
        {
            Player.SetPosition(100, 400);
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            Player.IsOnGround = true;
        }

        public int GetCameraOffset()
        {
            // Возвращаем смещение камеры (центрируем игрока)
            float offset = Player.X - 400;
            if (offset < 0) offset = 0;
            if (offset > 2150) offset = 2150;  // Максимум для уровня 2450
            return (int)offset;
        }

        public void Restart()
        {
            Player.Reset();
            Player.SetPosition(100, 400);
            Player.VelocityX = 0;
            Player.VelocityY = 0;
            Player.IsOnGround = true;

            // Очищаем и пересоздаём уровень
            Enemies.Clear();
            Platforms.Clear();
            JumpRefills.Clear();

            Score = 0;
            IsGameOver = false;

            BuildLevel();
        }

        public (float X, float Y) GetPlayerPosition() => (Player.X, Player.Y);
    }
}
