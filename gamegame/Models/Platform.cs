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
            if (player.X + playerWidth > X && player.X < X + Width &&
                player.Y + playerHeight > Y && player.Y < Y + Height)
            {
                // Сверху платформы
                if (player.VelocityY >= 0 && player.Y + playerHeight - player.VelocityY <= Y)
                {
                    newY = Y - playerHeight;
                    isOnTop = true;
                    return true;
                }
                // Снизу платформы
                else if (player.VelocityY < 0 && player.Y - player.VelocityY >= Y + Height)
                {
                    newY = Y + Height;
                    return true;
                }
            }
            return false;
        }
    }
}