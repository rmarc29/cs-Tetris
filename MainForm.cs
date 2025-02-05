using System;
using System.Drawing;
using System.Windows.Forms;
using TetrisGame;

namespace TetrisGameWinForms
{
    public class MainForm : Form
    {
        class FullScreen
        {
            public void EnterFullScreenMode(Form targetForm)
            {
                targetForm.WindowState = FormWindowState.Normal;
                targetForm.FormBorderStyle = FormBorderStyle.None;
                targetForm.WindowState = FormWindowState.Maximized;
            }

            public void LeaveFullScreenMode(Form targetForm)
            {
                targetForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                targetForm.WindowState = FormWindowState.Normal;
            }
        }

        private System.Windows.Forms.Timer gameTimer;
        private int gridWidth = 10,
            gridHeight = 20,
            cellSize = 30;
        private char[,] grid;
        private Brush[,] gridColors;
        private int blockX,
            blockY;
        private char[,] block;
        private Brush blockColor;
        private int score = 0;
        private readonly Random random = new Random();

        private readonly char[][,] tetrominoes = new char[][,]
        {
            new char[,]
            {
                { '.', '.', '.', '.' },
                { 'O', 'O', 'O', 'O' },
                { '.', '.', '.', '.' },
                { '.', '.', '.', '.' }
            },
            new char[,] { { 'O', 'O' }, { 'O', 'O' } },
            new char[,] { { '.', 'O', '.' }, { 'O', 'O', 'O' }, { '.', '.', '.' } },
            new char[,] { { '.', '.', 'O' }, { 'O', 'O', 'O' }, { '.', '.', '.' } },
            new char[,] { { 'O', '.', '.' }, { 'O', 'O', 'O' }, { '.', '.', '.' } },
            new char[,] { { '.', 'O', 'O' }, { 'O', 'O', '.' }, { '.', '.', '.' } },
            new char[,] { { 'O', 'O', '.' }, { '.', 'O', 'O' }, { '.', '.', '.' } }
        };

        private readonly Brush[] blockColors = new Brush[]
        {
            Brushes.Cyan,
            Brushes.Yellow,
            Brushes.Purple,
            Brushes.Orange,
            Brushes.Blue,
            Brushes.Green,
            Brushes.Red
        };

        public MainForm()
        {
            this.Text = "Tetris Game";
            this.ClientSize = new Size(gridWidth * cellSize, gridHeight * cellSize + 50);
            this.DoubleBuffered = true;

            grid = new char[gridHeight, gridWidth];
            gridColors = new Brush[gridHeight, gridWidth];
            InitializeGrid();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 500;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.Paint += DrawGrid;

            SpawnNewBlock();
        }

        private void InitializeGrid()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[y, x] = '.';
                    gridColors[y, x] = Brushes.White;
                }
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            MoveBlockDown();
            this.Invalidate();
        }

        private void SpawnNewBlock()
        {
            int index = random.Next(tetrominoes.Length);
            block = tetrominoes[index];
            blockColor = blockColors[index];
            blockX = gridWidth / 2 - block.GetLength(1) / 2;
            blockY = 0;

            if (!CanMove(blockX, blockY))
            {
                gameTimer.Stop();
                MessageBox.Show("Game Over! Final Score: " + score, "Tetris");
                /* Application.Exit(); */
                Application.Restart();
            }
        }

        private void MoveBlockDown()
        {
            ClearBlockFromGrid();
            if (CanMove(blockX, blockY + 1))
            {
                blockY++;
            }
            else
            {
                PlaceBlockOnGrid();
                CheckForLines();
                SpawnNewBlock();
            }
            PlaceBlockOnGrid();
        }

        private bool CanMove(int newX, int newY)
        {
            return IsValidPosition(block, newX, newY);
        }

        private bool IsValidPosition(char[,] testBlock, int newX, int newY)
        {
            for (int y = 0; y < testBlock.GetLength(0); y++)
            {
                for (int x = 0; x < testBlock.GetLength(1); x++)
                {
                    if (testBlock[y, x] == 'O')
                    {
                        int targetX = newX + x;
                        int targetY = newY + y;

                        // Check for boundaries
                        if (targetX < 0 || targetX >= gridWidth || targetY >= gridHeight)
                        {
                            return false;
                        }

                        // Check for collisions with existing blocks
                        if (targetY >= 0 && grid[targetY, targetX] != '.')
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void ClearBlockFromGrid()
        {
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    if (
                        block[y, x] == 'O'
                        && blockY + y >= 0
                        && blockY + y < gridHeight
                        && blockX + x < gridWidth
                    )
                    {
                        grid[blockY + y, blockX + x] = '.';
                        gridColors[blockY + y, blockX + x] = Brushes.White;
                    }
                }
            }
        }

        private void PlaceBlockOnGrid()
        {
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    if (
                        block[y, x] == 'O'
                        && blockY + y >= 0
                        && blockY + y < gridHeight
                        && blockX + x < gridWidth
                    )
                    {
                        grid[blockY + y, blockX + x] = block[y, x];
                        gridColors[blockY + y, blockX + x] = blockColor;
                    }
                }
            }
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
                    score += 100;
                }
            }
        }

        private void ClearLine(int y)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[y, x] = '.';
                gridColors[y, x] = Brushes.White;
            }
        }

        private void ShiftLinesDown(int fromRow)
        {
            for (int y = fromRow; y > 0; y--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[y, x] = grid[y - 1, x];
                    gridColors[y, x] = gridColors[y - 1, x];
                }
            }

            for (int x = 0; x < gridWidth; x++)
            {
                grid[0, x] = '.';
                gridColors[0, x] = Brushes.White;
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

            if (IsValidPosition(rotatedBlock, blockX, blockY))
            {
                block = rotatedBlock;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            ClearBlockFromGrid();
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
            PlaceBlockOnGrid();
            this.Invalidate();
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DrawGrid(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Rectangle cell = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                    g.FillRectangle(gridColors[y, x], cell);
                    g.DrawRectangle(Pens.Black, cell);
                }
            }

            g.DrawString(
                $"Score: {score}",
                new Font("Arial", 16),
                Brushes.Black,
                5,
                gridHeight * cellSize + 5
            );
        }
    }
}
