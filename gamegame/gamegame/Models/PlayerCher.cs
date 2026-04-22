using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace gamegame.Models
{
    public class PlayerCher
    {
        // Состояния игрока
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public int Health { get; private set; }
        public int JumpCount { get; private set; }
        public bool IsOnGround { get; set; }
        public bool IsAlive => Health > 0;
        public bool IsOnWall { get; private set; }
        public bool IsWallSliding { get; private set; }

        // Константы для физики
        private const float Gravity = 0.8f;
        private const int MaxJumps = 2;
        private const float JumpPower = -12f;
        private const float MoveAcceleration = 1.2f;
        private const float MaxSpeed = 8f;
        private const float MoveSpeed = 5f;
        private const float Friction = 0.9f;
        private const float MaxFallSpeed = 15f;
        private bool _isTouchingLeftWall;
        private bool _isTouchingRightWall;
        private const float WallSlideSpeed = 1.5f;
        private const float WallJumpPowerX = 10f;
        private const float WallJumpPowerY = -10f;
        private const float WallGrabOffset = 5f;
        private float _wallGrabX;  // Позиция X, где игрок зацепился за стену
        private bool _isGrabbingWall;
        public bool IsGrabbingWall => _isGrabbingWall;

        // для неуязвимости после удара
        public bool IsInvincible { get; private set; }
        private int _invincibilityTimer = 0;
        private const int InvincibilityDuration = 90;
        public PlayerCher(int startX, int startY)
        {
            X = startX;
            Y = startY;
            VelocityX = 0;
            VelocityY = 0;
            Health = 2;
            IsOnGround = false;
            JumpCount = 0;
            IsOnWall = false;
            IsWallSliding = false;
            _isGrabbingWall = false;
            IsInvincible = false;
            _invincibilityTimer = 0;
        }

        // Для движения (вызывается каждый кадр)
        public void UpdatePhysics()
        {
            if (_invincibilityTimer > 0)
            {
                _invincibilityTimer--;
                if (_invincibilityTimer <= 0)
                {
                    IsInvincible = false;
                }
            }

            // Логика удержания на стене
            if (_isGrabbingWall)
            {
                // Можно медленно скользить вниз если нажать вниз
                VelocityY = Math.Min(VelocityY, WallSlideSpeed);

                // Не даем уйти от стены
                if (_isTouchingLeftWall)
                {
                    X = _wallGrabX;
                }
                else if (_isTouchingRightWall)
                {
                    X = _wallGrabX;
                }

                // Если игрок пытается отойти от стены
                if ((_isTouchingLeftWall && VelocityX > 0) ||
                    (_isTouchingRightWall && VelocityX < 0))
                {
                    ReleaseWall();
                }
            }
            else if (IsOnWall && !IsOnGround)
            {
                // На стене но не зафиксирован - медленное скольжение
                IsWallSliding = true;
                if (VelocityY < WallSlideSpeed)
                    VelocityY = WallSlideSpeed;

                // Автоматический захват стены если игрок не двигается
                if (Math.Abs(VelocityX) < 0.5f && !_isGrabbingWall)
                {
                    GrabWall();
                }
            }
            else
            {
                IsWallSliding = false;
            }

            // Обычная гравитация (если не на стене и не зафиксированы)
            if (!IsOnWall && !_isGrabbingWall)
            {
                VelocityY += Gravity;
                if (VelocityY > MaxFallSpeed) VelocityY = MaxFallSpeed;
            }

            // Трение на земле
            if (IsOnGround)
            {
                VelocityX *= Friction;
                if (Math.Abs(VelocityX) < 0.1f)
                    VelocityX = 0;
                JumpCount = 0;
                _isGrabbingWall = false;  // Сброс захвата при касании земли
            }

            // Движение
            X += VelocityX;
            Y += VelocityY;
        }

        // Захват стены
        private void GrabWall()
        {
            if (!IsOnWall || IsOnGround) return;

            _isGrabbingWall = true;
            _wallGrabX = X;  // Запоминаем позицию

            // Обнуляем горизонтальную скорость
            VelocityX = 0;

            // Немного прижимаем к стене
            if (_isTouchingLeftWall)
            {
                X = _wallGrabX - WallGrabOffset;
            }
            else if (_isTouchingRightWall)
            {
                X = _wallGrabX + WallGrabOffset;
            }
        }

        // Отпускание стены
        private void ReleaseWall()
        {
            _isGrabbingWall = false;
            IsWallSliding = false;
        }

        // Действия игрока
        public void MoveLeft()
        {
            if (_isGrabbingWall)
            {
                // Если зафиксированы, движение в сторону от стены отпускает
                if (_isTouchingRightWall)
                {
                    ReleaseWall();
                    VelocityX = -MoveSpeed;
                }
            }
            else
            {
                VelocityX = -MoveSpeed;
            }
        }

        public void MoveRight()
        {
            if (_isGrabbingWall)
            {
                // Если зафиксированы, движение в сторону от стены отпускает
                if (_isTouchingLeftWall)
                {
                    ReleaseWall();
                    VelocityX = MoveSpeed;
                }
            }
            else
            {
                VelocityX = MoveSpeed;
            }
        }

        public void Jump()
        {
            if (_isGrabbingWall)
            {
                // Прыжок от стены с фиксации
                WallJump();
            }
            else if (IsOnGround)
            {
                // Обычный прыжок
                VelocityY = JumpPower;
                IsOnGround = false;
                JumpCount = 1;
            }
            else if (JumpCount < 2)
            {
                // Двойной прыжок
                VelocityY = JumpPower;
                JumpCount++;
            }
        }

        public void Stop()
        {
            if (!_isGrabbingWall)
            {
                VelocityX = 0;
            }
        }

        // Новый метод для проверки касания стен
        public void CheckWallCollision(bool touchingLeft, bool touchingRight)
        {
            bool wasOnWall = IsOnWall;
            _isTouchingLeftWall = touchingLeft;
            _isTouchingRightWall = touchingRight;
            IsOnWall = (touchingLeft || touchingRight) && !IsOnGround;

            // Если потеряли касание со стеной - отпускаем
            if (!IsOnWall && _isGrabbingWall)
            {
                ReleaseWall();
            }

            // Если только что коснулись стены и не на земле - автоматический захват
            if (!wasOnWall && IsOnWall && !IsOnGround && !_isGrabbingWall)
            {
                GrabWall();
            }
        }

        // Метод для прыжка от стены
        public void WallJump()
        {
            if (!IsOnWall && !_isGrabbingWall) return;

            // Прыжок в противоположную сторону от стены
            if (_isTouchingLeftWall || (_isGrabbingWall && _wallGrabX < X))
            {
                VelocityX = 12f;  // Отталкиваемся вправо
            }
            else if (_isTouchingRightWall || (_isGrabbingWall && _wallGrabX > X))
            {
                VelocityX = -12f;  // Отталкиваемся влево
            }

            VelocityY = -11f;  // Сила прыжка от стены
            _isGrabbingWall = false;
            IsOnWall = false;
            IsWallSliding = false;
            JumpCount = 1;  // Даем возможность сделать двойной прыжок после валлджампа
        }

        // Отброс назад при уроне
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            if (IsInvincible) return;
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                // Смерть
                VelocityX = 0;
                VelocityY = -10;
            }
            else
            {
                // Ранены, но живы - неуязвимость
                IsInvincible = true;
                _invincibilityTimer = InvincibilityDuration;

                // Отбрасывание
                VelocityX = (VelocityX > 0 ? -10 : 10);
                VelocityY = -10;
                IsOnGround = false;
                _isGrabbingWall = false;
            }
        }

        /*
        public void Heal(int amount)
        {
            Health += amount;
            if (Health > 5) Health = 5;
        }
        */

        // сброс состояния при рестарте
        public void Reset()
        {
            Health = 2;
            IsInvincible = false;
            _invincibilityTimer = 0;
            VelocityX = 0;
            VelocityY = 0;
            IsOnGround = false;
            JumpCount = 0;
            IsOnWall = false;
            _isGrabbingWall = false;
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
