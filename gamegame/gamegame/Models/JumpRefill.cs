using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamegame.Models
{
    public class JumpRefill
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 20;
        public bool IsActive { get; set; } = true;

        public JumpRefill(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool CollidesWith(PlayerCher player, int playerWidth, int playerHeight)
        {
            if (!IsActive) return false;

            return player.X < X + Width &&
                   player.X + playerWidth > X &&
                   player.Y < Y + Height &&
                   player.Y + playerHeight > Y;
        }

        public void Collect()
        {
            IsActive = false;
        }
    }
}
