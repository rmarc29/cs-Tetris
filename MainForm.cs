using System;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisGameWinForms
{
    public class MainForm : Form
    {
        private System.Windows.Forms.Timer gameTimer;
        private int gridWidth = 10, gridHeight = 20, cellSize = 30;
        private char[,] grid;
        private int blockX = 4, blockY = 0;
        private char[,] block = {
            { 'O', 'O' },
            { 'O', 'O' }
        };
        private int score = 0;

        public MainForm()
        {
            // Set up the form
            this.Text = "Tetris Game";
            this.ClientSize = new Size(gridWidth * cellSize, gridHeight * cellSize + 50); // Extra space for score
            this.DoubleBuffered = true;

            // Initialize grid
            grid = new char[gridHeight, gridWidth];
            InitializeGrid();

            // Set up timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 500; // 500ms
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            // Paint event
            this.Paint += DrawGrid;
        }

        private void InitializeGrid()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[y, x] = '.';
                }
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            MoveBlockDown();
            this.Invalidate(); // Redraw the form
        }

        private void MoveBlockDown()
        {
            ClearBlock();
            if (CanMove(blockX, blockY + 1))
            {
                blockY++;
            }
            else
            {
                PlaceBlock();
                LockBlock();
                CheckForLines();
                SpawnNewBlock();
            }
            PlaceBlock();
        }

        private bool CanMove(int newX, int newY)
        {
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    if (block[y, x] == 'O')
                    {
                        int targetX = newX + x;
                        int targetY = newY + y;

                        if (targetX < 0 || targetX >= gridWidth || targetY >= gridHeight ||
                            (targetY >= 0 && grid[targetY, targetX] != '.'))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void ClearBlock()
        {
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    if (block[y, x] == 'O' && blockY + y < gridHeight && blockX + x < gridWidth)
                    {
                        grid[blockY + y, blockX + x] = '.';
                    }
                }
            }
        }

        private void PlaceBlock()
        {
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    if (block[y, x] == 'O' && blockY + y < gridHeight && blockX + x < gridWidth)
                    {
                        grid[blockY + y, blockX + x] = block[y, x];
                    }
                }
            }
        }

        private void LockBlock()
        {
            PlaceBlock();
        }

        private void CheckForLines()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                bool fullLine = true;
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[y, x] == '.')
                    {
                        fullLine = false;
                        break;
                    }
                }

                if (fullLine)
                {
                    ClearLine(y);
                    ShiftLinesDown(y);
                    score += 100; // Increment score for each cleared line
                }
            }
        }

        private void ClearLine(int y)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[y, x] = '.';
            }
        }

        private void ShiftLinesDown(int fromRow)
        {
            for (int y = fromRow; y > 0; y--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[y, x] = grid[y - 1, x];
                }
            }

            for (int x = 0; x < gridWidth; x++)
            {
                grid[0, x] = '.';
            }
        }

        private void SpawnNewBlock()
        {
            blockX = 4;
            blockY = 0;
            block = new char[,] {
                { 'O', 'O' },
                { 'O', 'O' }
            };

            if (!CanMove(blockX, blockY))
            {
                gameTimer.Stop();
                MessageBox.Show("Game Over! Final Score: " + score, "Tetris");
                Application.Exit();
            }
        }

        private void RotateBlock()
        {
            int size = block.GetLength(0);
            char[,] rotatedBlock = new char[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    rotatedBlock[x, size - 1 - y] = block[y, x];
                }
            }

            if (CanMove(blockX, blockY)) // Ensure the rotation is valid
            {
                block = rotatedBlock;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            ClearBlock();
            if (keyData == Keys.Left && CanMove(blockX - 1, blockY))
            {
                blockX--;
            }
            else if (keyData == Keys.Right && CanMove(blockX + 1, blockY))
            {
                blockX++;
            }
            else if (keyData == Keys.Down)
            {
                MoveBlockDown();
            }
            else if (keyData == Keys.Up)
            {
                RotateBlock();
            }
            PlaceBlock();
            this.Invalidate();
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DrawGrid(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Draw the grid
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Rectangle cell = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                    g.FillRectangle(grid[y, x] == '.' ? Brushes.White : Brushes.Blue, cell);
                    g.DrawRectangle(Pens.Black, cell);
                }
            }

            // Draw the score
            g.DrawString($"Score: {score}", new Font("Arial", 16), Brushes.Black, 5, gridHeight * cellSize + 5);
        }
    }
}
