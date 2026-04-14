using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamegame.Models
{
    public class Platform
    {
        // Параметры платформы
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // "подушка" для коллизии
        private const float CollisionTolerance = 5f;
        public Platform(float x, float y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        // Взаимодействие игрока с платформой
        public bool CollidesWith(PlayerCher player, int playerWidth, int playerHeight, out float newY, out bool isOnTop)
        {
            newY = player.Y;
            isOnTop = false;

            // чек от коллизию
            // Предыдущая позиция игрока (для проверки, не пролетел ли сквозь)
            float oldY = player.Y - player.VelocityY;
            float oldBottom = oldY + playerHeight;
            float newBottom = player.Y + playerHeight;

            // Проверяем столкновение с учётом движения между кадрами
            bool horizontalCollision = player.X + playerWidth > X + CollisionTolerance &&
                                        player.X < X + Width - CollisionTolerance;

            // Падение сверху (игрок движется вниз)
            if (player.VelocityY >= 0 && horizontalCollision)
            {
                // Была ли платформа над игроком в прошлом кадре?
                if (oldBottom <= Y + CollisionTolerance && newBottom >= Y)
                {
                    newY = Y - playerHeight;
                    isOnTop = true;
                    return true;
                }
            }

            // Прыжок снизу (игрок движется вверх)
            if (player.VelocityY < 0 && horizontalCollision)
            {
                // Была ли платформа под игроком в прошлом кадре?
                if (oldY >= Y + Height - CollisionTolerance && player.Y <= Y + Height)
                {
                    newY = Y + Height;
                    return true;
                }
            }

            return false;
        }
    }
}