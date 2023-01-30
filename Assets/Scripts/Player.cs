using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tetris
{
    public class Player : PlayerInput
    {
        #region Attributes
        [SerializeField] private Board _board;
        [SerializeField] private PiecePool _pool;
        [SerializeField] private PlayerPiece _piece;

        private bool _isDead;

        public delegate void OnGameEvent();
        public OnGameEvent onDeath, onAccelerate, onDeaccelerate;

        public delegate void OnRowsCompleted(int count);

        private InputActionMap map;
        #endregion

        #region PlayerPiece
        [System.Serializable]
        private class PlayerPiece
        {
            [SerializeField] private Color _color;
            [SerializeField] private bool[,] _mask;
            [SerializeField] private Vector2Int _position, _offset, _size;
            [SerializeField] private Vector2Int[] _downPositions;
            [SerializeField] private Vector2Int[] _rightPositions;
            [SerializeField] private Vector2Int[] _leftPositions;
            private OnRowsCompleted onScore;

            public PlayerPiece(OnRowsCompleted onScore)
            {
                this.onScore = onScore;
            }

            public bool SpawnPiece(bool[,] mask, Vector2Int size, Color color, Board board, Vector2Int at)
            {
                _mask = mask;
                _offset = Vector2Int.zero;
                _size = size;
                _downPositions = new Vector2Int[size.x];
                _rightPositions = new Vector2Int[size.y];
                _leftPositions = new Vector2Int[size.y];

                bool canBePlaced = true;
                _position = at;


                for (int y = 0; y < _size.y && canBePlaced; y++)
                {
                    _leftPositions[y] = new Vector2Int(int.MaxValue, -1);
                    for (int x = 0; x < _size.x && canBePlaced; x++)
                    {
                        if (_mask[y, x] == true)
                        {
                            Vector2Int tilePosition = _position + new Vector2Int(x, y);
                            Board.TileData data = board.DataAt(tilePosition);
                            if (data != Board.TileData.UNOCCUPIED && data != Board.TileData.OVER)
                            {
                                canBePlaced = false;
                            }

                            _downPositions[x] = tilePosition - Vector2Int.down;
                            if (tilePosition.x < _leftPositions[y].x) _leftPositions[y] = tilePosition + Vector2Int.left;
                            _rightPositions[y] = tilePosition + Vector2Int.right;
                        }
                    }
                }

                if (!canBePlaced)
                {
                    Debug.Log("Can't be placed");
                    return false;
                }


                _color = color;
                for (int y = 0; y < _size.y; y++)
                    for (int x = 0; x < _size.x; x++)
                    {
                        if (_mask[y, x])
                            board.EnableTile(_color, _position + new Vector2Int(x, y), false);
                    }

                return true;
            }

            public void SettlePiece(Board board)
            {
                Stack<int> rowsCompleted = new Stack<int>();
                if (_mask != null)
                    for (int y = 0; y < _size.y; y++)
                    {
                        for (int x = 0; x < _size.x; x++)
                            if (_mask[y, x])
                                board.EnableTile(_color, _position + new Vector2Int(x, y), true);
                        if (board.CheckRow(_position.y + y))
                            rowsCompleted.Push(_position.y + y);
                    }

                if (rowsCompleted.Count == 0) return;

                int count = rowsCompleted.Count;
                int rowCompleted = rowsCompleted.Pop();
                for (int amount = 1; amount < count; amount++)
                {
                    int nextCompleted = rowsCompleted.Pop();
                    board.RemoveRow(rowCompleted);
                    board.MoveDownRows(rowCompleted - 1, nextCompleted, amount);
                    rowCompleted = nextCompleted;
                }
                board.RemoveRow(rowCompleted);
                Debug.Log(rowCompleted - 1);
                board.MoveDownRows(rowCompleted - 1, 0, count);
                onScore.Invoke(count);
            }

            public enum MoveDirection
            {
                UP = 0,
                RIGHT = 1,
                DOWN = 2,
                LEFT = 3
            }

            private static readonly Vector2Int[] directions = {
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 0)
            };

            private bool Move(Board board, MoveDirection direction, Vector2Int[] checkPositions)
            {
                bool blocked = false;

                foreach (Vector2Int checkPosition in checkPositions)
                {
                    Board.TileData data = board.DataAt(checkPosition + _offset);
                    if (data != Board.TileData.UNOCCUPIED && data != Board.TileData.OVER)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    return true;

                for (int y = 0; y < _size.y; y++)
                    for (int x = 0; x < _size.x; x++)
                        if (_mask[y, x])
                            board.DisableTile(_position + new Vector2Int(x, y));

                _offset += directions[(int)direction];
                _position += directions[(int)direction];

                for (int y = 0; y < _size.y; y++)
                    for (int x = 0; x < _size.x; x++)
                        if (_mask[y, x])
                            board.EnableTile(_color, _position + new Vector2Int(x, y), false);

                return blocked;
            }

            public void MoveRight(Board board)
            {
                Move(board, MoveDirection.RIGHT, _rightPositions);
            }

            public bool MoveDown(Board board)
            {
                return Move(board, MoveDirection.DOWN, _downPositions);
            }

            public void MoveLeft(Board board)
            {
                Move(board, MoveDirection.LEFT, _leftPositions);
            }

            public enum Rotation { LEFT, RIGHT, TWICE };
            public void Rotate(Board board, Rotation rotation)
            {
                // TODO : rotate to edge
                bool[,] newMask = null;

                switch (rotation)
                {
                    case Rotation.LEFT:
                        newMask = RotateMatrixLeft(_mask, _size);
                        break;
                    case Rotation.TWICE:
                        newMask = RotateMatrixTwice(_mask, _size);
                        break;
                    case Rotation.RIGHT:
                        newMask = RotateMatrixRight(_mask, _size);
                        break;
                }

                Vector2Int boundPush = Vector2Int.zero;
                Vector2Int newSize = new Vector2Int(_size.y, _size.x);
                Vector2Int halfSize = (newSize / 2);

                if (_position.x + newSize.x > board.sizeX)
                {
                    boundPush = new Vector2Int(
                        board.sizeX - (_position.x + newSize.x),
                        0
                    );
                }

                Vector2Int newPosition = _position + boundPush;

                bool unoccupied = true;

                for (int y = 0; y < newSize.y && unoccupied; y++)
                    for (int x = 0; x < newSize.x && unoccupied; x++)
                        if (newMask[y, x])
                        {
                            Board.TileData data = board.DataAt(newPosition + new Vector2Int(x, y));

                            if (data != Board.TileData.UNOCCUPIED && data != Board.TileData.OVER)
                            {
                                unoccupied = false;
                            }
                        }

                if (unoccupied)
                {
                    for (int y = 0; y < _size.y; y++)
                        for (int x = 0; x < _size.x; x++)
                            if (_mask[y, x])
                            {
                                board.DisableTile(_position + new Vector2Int(x, y));
                            }

                    SpawnPiece(newMask, newSize, _color, board, newPosition);
                }
            }

            public static bool[,] RotateMatrixRight(bool[,] matrix, Vector2Int size)
            {
                bool[,] result = new bool[size.x, size.y];

                for (int y = 0; y < size.y; y++)
                    for (int x = 0; x < size.x; x++)
                        result[x, y] = matrix[size.y - y - 1, x];

                return result;
            }

            public static bool[,] RotateMatrixTwice(bool[,] matrix, Vector2Int size)
            {
                bool[,] result = new bool[size.y, size.x];

                for (int y = 0; y < size.y; y++)
                    for (int x = 0; x < size.x; x++)
                        result[y, x] = matrix[size.y - y - 1, size.x - x - 1];

                return result;
            }

            public static bool[,] RotateMatrixLeft(bool[,] matrix, Vector2Int size)
            {
                bool[,] result = new bool[size.x, size.y];

                for (int y = 0; y < size.y; y++)
                    for (int x = 0; x < size.x; x++)
                        result[x, y] = matrix[y, size.x - x - 1];

                return result;
            }
        }
        #endregion

        #region MonoBehaviour
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            map = actions.FindActionMap("Game");
            map.Enable();

            SubscribeActions();
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeActions();
            map.Disable();
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's
        /// context menu or when adding the component the first time.
        /// </summary>
        public void Reset()
        {
            _board.Reset();
        }
        #endregion

        #region Input
        private void SubscribeActions()
        {
            map["Down"].performed += DownPress;
            map["Down"].canceled += DownRelease;
            map["Move Left"].performed += MoveLeft;
            map["Move Right"].performed += MoveRight;
            map["Move Left"].performed += MoveLeft;
            map["Rotate Right"].performed += RotateRight;
            map["Rotate Left"].performed += RotateLeft;
        }

        private void UnsubscribeActions()
        {
            map["Down"].performed -= DownPress;
            map["Down"].canceled -= DownRelease;
            map["Move Left"].performed -= MoveLeft;
            map["Move Right"].performed -= MoveRight;
            map["Move Left"].performed -= MoveLeft;
            map["Rotate Right"].performed -= RotateRight;
            map["Rotate Left"].performed -= RotateLeft;
        }
        #endregion

        #region Input callbacks
        private void DownPress(InputAction.CallbackContext ctx)
        {
            onAccelerate.Invoke();
        }

        private void DownRelease(InputAction.CallbackContext ctx)
        {
            onDeaccelerate.Invoke();
        }

        private void MoveRight(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;

            _piece.MoveRight(_board);
        }

        private void MoveLeft(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;

            _piece.MoveLeft(_board);
        }

        private void RotateRight(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;

            _piece.Rotate(_board, PlayerPiece.Rotation.RIGHT);
        }

        private void RotateLeft(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;

            _piece.Rotate(_board, PlayerPiece.Rotation.LEFT);
        }
        #endregion

        #region Player
        private void NextPiece()
        {
            Piece piece = _pool.Pop();
            Color color = piece.color;
            Vector2Int size = piece.size;
            bool[,] mask = new bool[size.y, size.x];
            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                {
                    mask[y, x] = piece.mask[y * piece.size.x + x];
                }

            switch (piece.rotation)
            {
                case 1:
                    mask = PlayerPiece.RotateMatrixRight(mask, size);
                    break;
                case 2:
                    mask = PlayerPiece.RotateMatrixTwice(mask, size);
                    break;
                case 3:
                    mask = PlayerPiece.RotateMatrixLeft(mask, size);
                    break;
            }

            if (!_piece.SpawnPiece(mask, (piece.rotation % 2 == 0) ? size : new Vector2Int(size.y, size.x), color, _board, _board.Spawn - size / 2))
            {
                onDeath.Invoke();
                _isDead = true;
            }
        }

        public void StartPlayer(OnRowsCompleted onScore)
        {
            _piece = new PlayerPiece(onScore);

            NextPiece();
            _isDead = false;
        }

        public void MoveDown()
        {
            if (_piece.MoveDown(_board))
            {
                _piece.SettlePiece(_board);
                NextPiece();
            }
        }
        #endregion
    }
}
