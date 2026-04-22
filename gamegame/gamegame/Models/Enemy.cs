using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamegame.Models
{
    public class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public int Width { get; } = 30;
        public int Height { get; } = 30;
        public bool IsAlive { get; set; } = true;

        private float _moveDirection = 1;
        private float _leftBoundary;
        private float _rightBoundary;

        public Enemy(float x, float y, float patrolLeft, float patrolRight)
        {
            X = x;
            Y = y;
            _leftBoundary = patrolLeft;
            _rightBoundary = patrolRight;
            VelocityX = 1.5f;
        }

        public void Update()
        {
            if (!IsAlive) return;

            X += VelocityX * _moveDirection;

            if (X <= _leftBoundary)
            {
                X = _leftBoundary;
                _moveDirection = 1;
            }
            else if (X >= _rightBoundary)
            {
                X = _rightBoundary;
                _moveDirection = -1;
            }
        }

        public void Die()
        {
            IsAlive = false;
        }

        public bool CollidesWith(PlayerCher player, int playerWidth, int playerHeight)
        {
            if (!IsAlive) return false;

            // Проверка пересечения прямоугольников
            bool collision = player.X < X + Width &&
                             player.X + playerWidth > X &&
                             player.Y < Y + Height &&
                             player.Y + playerHeight > Y;

            return collision;
        }

        // проверка падения на врага сверху
        public bool IsPlayerLandingOnTop(PlayerCher player, int playerWidth, int playerHeight)
        {
            if (!IsAlive) return false;

            // Игрок падает сверху на врага
            bool isFalling = player.VelocityY > 0;
            bool isAbove = player.Y + playerHeight - player.VelocityY <= Y + 10;
            bool horizontalOverlap = player.X + playerWidth > X + 5 && player.X < X + Width - 5;

            return isFalling && isAbove && horizontalOverlap && CollidesWith(player, playerWidth, playerHeight);
        }
    }
}