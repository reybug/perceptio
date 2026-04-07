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
        public bool IsOnGround { get; set; }
        public bool IsAlive => Health > 0;

        // Константы для физики
        private const float Gravity = 0.8f;
        private const float JumpPower = -12f;
        private const float MoveAcceleration = 1.2f;
        private const float MaxSpeed = 8f;
        private const float Friction = 0.9f;

        public PlayerCher(int startX, int startY)
        {
            X = startX;
            Y = startY;
            VelocityX = 0;
            VelocityY = 0;
            Health = 3;
            IsOnGround = false;
        }

        // Для движения (вызывается каждый кадр)
        public void UpdatePhysics()
        {
            // + гравитация
            VelocityY += Gravity;

            // типа трение (только на земле)
            if (IsOnGround)
            {
                VelocityX *= Friction;
                if (Math.Abs(VelocityX) < 0.1f) VelocityX = 0;
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
            }
        }

        public void Stop()
        {
            VelocityX = 0;
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
