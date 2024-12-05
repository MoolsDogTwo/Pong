using Raylib_cs;
using System.Numerics;

namespace Pong
{
    class C
    {
        public const int ScreenWidth = 640;
        public const int ScreenHeight = 480;
        public const int FontSize = 72;
        public const int TextHeightOffset = 20;
        public const int PaddleWidth = 10;
        public const int PaddleHeight= 50;
        public const int BallSize = 10;
        public const int Speed = 5;
        public const int PlayAreaPadding = 15;
        public const int LineThickness = 3;
        public static Sound sndBorder;
        public static Sound sndPaddle;
        public static Sound sndPoint;
    }

    class Program
    {
        static void Main()
        {
            Raylib.InitWindow(C.ScreenWidth, C.ScreenHeight, "Pong");
            Raylib.SetTargetFPS(50);
            
            // Load sound
            Raylib.InitAudioDevice();
            C.sndBorder = Raylib.LoadSound("Border.wav");
            C.sndPaddle = Raylib.LoadSound("Paddle.wav");
            C.sndPoint = Raylib.LoadSound("Point.wav");

            // Init ingame objects
            Paddle Player = new(30 + C.PlayAreaPadding, 30 + C.PlayAreaPadding);
            Paddle Player2 = new(C.ScreenWidth - C.PaddleWidth - C.PlayAreaPadding - 30, 30 + C.PlayAreaPadding);
            Ball ball = new(Player);

            while (!Raylib.WindowShouldClose()) {
                // ==== Controls ====

                ball.Move(Player, Player2);

                // Player control
                if (Raylib.IsKeyDown(KeyboardKey.W)) Player.MoveUp();
                if (Raylib.IsKeyDown(KeyboardKey.S)) Player.MoveDown();
                if (Raylib.IsKeyDown(KeyboardKey.Up)) Player2.MoveUp();
                if (Raylib.IsKeyDown(KeyboardKey.Down)) Player2.MoveDown();

                // ==== Drawing ====
                Raylib.BeginDrawing();

                // UI
                Raylib.ClearBackground(Color.Black);

                Vector2 middleLineStart = new(C.ScreenWidth / 2, C.PlayAreaPadding);
                Vector2 middleLineEnd = new(C.ScreenWidth / 2, C.ScreenHeight - C.PlayAreaPadding);
                Raylib.DrawLineEx(middleLineStart, middleLineEnd, C.LineThickness, Color.White);

                Rectangle playArea = new(C.PlayAreaPadding, C.PlayAreaPadding, C.ScreenWidth - (C.PlayAreaPadding * 2),
                                         C.ScreenHeight - (C.PlayAreaPadding * 2));
                Raylib.DrawRectangleLinesEx(playArea, C.LineThickness, Color.White);

                // Scores
                int PlayerTextWidth = Raylib.MeasureText(Player.Score.ToString(), C.FontSize);
                int Player2TextWidth = Raylib.MeasureText(Player.Score.ToString(), C.FontSize);

                Raylib.DrawText(Player.Score.ToString(), (C.ScreenWidth * 1 / 4) - (PlayerTextWidth / 2),
                                C.TextHeightOffset + C.PlayAreaPadding, C.FontSize, Color.White);

                Raylib.DrawText(Player2.Score.ToString(), (C.ScreenWidth * 3 / 4) - (Player2TextWidth / 2),
                                C.TextHeightOffset + C.PlayAreaPadding, C.FontSize, Color.White);

                // Draw players
                Raylib.DrawRectangle(Player.PosX, Player.PosY, C.PaddleWidth, C.PaddleHeight, Color.White);
                Raylib.DrawRectangle(Player2.PosX, Player2.PosY, C.PaddleWidth, C.PaddleHeight, Color.White);

                // Draw Ball
                Raylib.DrawRectangle(ball.X, ball.Y, C.BallSize, C.BallSize, Color.White);

                Raylib.EndDrawing();
            }

            // ==== De-init ====
            Raylib.UnloadSound(C.sndBorder);
            Raylib.UnloadSound(C.sndPaddle);
            Raylib.UnloadSound(C.sndPoint);
            Raylib.CloseWindow();
        }
    }

    class Ball
    {
        public int X;
        public int Y;
        public int _xVelocity = C.Speed;  // TODO: Change this back as soon as possible
        private int _yVelocity = C.Speed;

        public Ball(Paddle player1)
        {
            ResetPosition(player1);
        }

        private void ResetPosition(Paddle player1)
        {
            X = player1.PosX + C.PaddleWidth;
            Y = player1.PosY + C.PaddleHeight / 2;
            if (_xVelocity < 0) _xVelocity = -_xVelocity;
        }

        public void Move(Paddle player1, Paddle player2)
        {
            // Is the ball outside the screen?
            if (X + C.BallSize > C.ScreenWidth + C.ScreenWidth / 3 || X < -C.ScreenWidth / 3)
            {
                if (X > C.ScreenWidth)
                    ++player1.Score;
                else if (X < 0)
                    ++player2.Score;

                Raylib.PlaySound(C.sndPoint);
                ResetPosition(player1);
            }

            // Paddle collisions
            if (_xVelocity > 0 && Y >= player2.PosY && Y <= player2.PosY + C.PaddleHeight
                               && X >= player2.PosX && X <= player2.PosX + C.PaddleWidth)
            {
                Raylib.PlaySound(C.sndPaddle);
                _xVelocity = -_xVelocity;
            }
            if (_xVelocity < 0 && Y >= player1.PosY && Y <= player1.PosY + C.PaddleHeight
                               && X <= player1.PosX + C.PaddleWidth && X >= player1.PosX)
            {
                Raylib.PlaySound(C.sndPaddle);
                _xVelocity = -_xVelocity;
            }

            // Border collisions
            if (Y + C.BallSize >= C.ScreenHeight - C.PlayAreaPadding && X > 0 && X < C.ScreenWidth)
            {
                _yVelocity = -_yVelocity;
                Raylib.PlaySound(C.sndBorder);
            }
            if (Y <= C.PlayAreaPadding && X > 0 && X < C.ScreenWidth)
            {
                _yVelocity = -_yVelocity;
                Raylib.PlaySound(C.sndBorder);
            }

            X += _xVelocity;
            Y += _yVelocity;
        }
    }

    class Paddle
    {
        public int PosX;
        public int PosY;
        public int Score = 0;
        public bool HitBottom { get; private set; }
        public bool HitTop { get; private set; }

        public Paddle(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public void MoveUp()
        {
            if (PosY > C.PlayAreaPadding)
            {
                PosY -= C.Speed;
                HitTop = false;
                return;
            }
            HitTop = true;
        }

        public void MoveDown()
        {
            if (PosY + C.PaddleHeight < C.ScreenHeight - C.PlayAreaPadding)
            {
                PosY += C.Speed;
                HitBottom = false;
                return;
            }
            HitBottom = true;
        }
    }
}