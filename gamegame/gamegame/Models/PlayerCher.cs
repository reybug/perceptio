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
        private const float Friction = 0.9f;
        private const float MaxFallSpeed = 15f;
        private bool _isTouchingLeftWall;
        private bool _isTouchingRightWall;
        private const float WallSlideSpeed = 2f;
        private const float WallJumpPowerX = 10f;
        private const float WallJumpPowerY = -10f;
        public PlayerCher(int startX, int startY)
        {
            X = startX;
            Y = startY;
            VelocityX = 0;
            VelocityY = 0;
            Health = 3;
            IsOnGround = false;
            JumpCount = 0;
            IsOnWall = false;
            IsWallSliding = false;
        }

        // Для движения (вызывается каждый кадр)
        public void UpdatePhysics()
        {
            // Если на стене - медленное падение
            if (IsOnWall && !IsOnGround)
            {
                IsWallSliding = true;
                if (VelocityY < WallSlideSpeed)
                    VelocityY = WallSlideSpeed;
            }
            else
            {
                IsWallSliding = false;
            }

            // Обычная гравитация если не на стене
            if (!IsWallSliding)
            {
                VelocityY += Gravity;
                if (VelocityY > MaxFallSpeed) VelocityY = MaxFallSpeed;
            }

            // типа трение (только на земле)
            if (IsOnGround)
            {
                VelocityX *= Friction;
                if (Math.Abs(VelocityX) < 0.1f)
                    VelocityX = 0;
                JumpCount = 0;
            }

            // обновляем позицию
            X += VelocityX;
            Y += VelocityY;
        }

        // Действия игрока
        public void MoveLeft()
        {
            VelocityX -= MoveAcceleration;
            if (VelocityX < -MaxSpeed) VelocityX = -MaxSpeed;
        }

        public void MoveRight()
        {
            VelocityX += MoveAcceleration;
            if (VelocityX > MaxSpeed) VelocityX = MaxSpeed;
        }

        public void Jump()
        {
            if (IsOnGround)
            {
                VelocityY = JumpPower;
                IsOnGround = false;
                JumpCount = 1;
            }
            else if (JumpCount < MaxJumps)  // Второй прыжок в воздухе
            {
                VelocityY = JumpPower;
                JumpCount++;
            }
        }

        public void Stop()
        {
            VelocityX = 0;
        }

        // Новый метод для проверки касания стен
        public void CheckWallCollision(bool touchingLeft, bool touchingRight)
        {
            _isTouchingLeftWall = touchingLeft;
            _isTouchingRightWall = touchingRight;
            IsOnWall = (touchingLeft || touchingRight) && !IsOnGround;
        }

        // Метод для прыжка от стены
        public void WallJump()
        {
            if (!IsOnWall) return;

            // Прыжок в противоположную сторону от стены
            if (_isTouchingLeftWall)
            {
                VelocityX = WallJumpPowerX;
            }
            else if (_isTouchingRightWall)
            {
                VelocityX = -WallJumpPowerX;
            }

            VelocityY = WallJumpPowerY;
            IsOnWall = false;
            IsWallSliding = false;
            JumpCount = 0;  // Сбрасываем прыжки после валлджампа
        }

        // Отброс назад при уроне
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            Health -= damage;
            VelocityX = -5 * Math.Sign(VelocityX);
            VelocityY = -8;
            IsOnGround = false;
        }

        /*
        public void Heal(int amount)
        {
            Health += amount;
            if (Health > 5) Health = 5;
        }
        */

        // тест
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
