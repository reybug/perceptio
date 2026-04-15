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
        public PlatformType Type { get; set; }

        // "подушка" для коллизии
        private const float CollisionTolerance = 5f;
        public Platform(float x, float y, int width, int height, PlatformType type = PlatformType.Normal)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Type = type;
        }

        // Проверка, можно ли цепляться за эту платформу
        public bool IsGrabbable => Type == PlatformType.Wall;

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

        public bool CheckWallCollision(PlayerCher player, int playerWidth, int playerHeight,
                                       out bool touchingLeft, out bool touchingRight)
        {
            touchingLeft = false;
            touchingRight = false;

            // Только стены могут быть захвачены
            if (Type != PlatformType.Wall) return false;

            // Проверка касания левой стороны стены
            if (player.X + playerWidth - 5 <= X + Width &&
                player.X + playerWidth - 10 >= X &&
                player.Y + playerHeight - 10 > Y &&
                player.Y + 10 < Y + Height)
            {
                touchingRight = true;
                return true;
            }

            // Проверка касания правой стороны стены
            if (player.X + 5 >= X &&
                player.X + 10 <= X + Width &&
                player.Y + playerHeight - 10 > Y &&
                player.Y + 10 < Y + Height)
            {
                touchingLeft = true;
                return true;
            }

            return false;
        }
    }
}