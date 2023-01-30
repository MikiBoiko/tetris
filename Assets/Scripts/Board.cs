using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris
{
    public class Board : MonoBehaviour
    {
        #region Attributes and properties
        private Tile[,] _tiles;
        [SerializeField] private GameObject _tilePrefab;
        public readonly int sizeX = 10, sizeY = 20;

        public Vector2Int Spawn => new Vector2Int(sizeX / 2, -1);
        #endregion

        #region Tiles
        public enum TileData { OCCUPIED, UNOCCUPIED, OVER, UNDER, LEFT, RIGHT }

        public class Tile
        {
            public SpriteRenderer renderer;
            public Transform transform;

            public bool Ocuppied { private set; get; }
            public Color Color => renderer.color;

            public Tile(GameObject gameObject)
            {
                this.renderer = gameObject.GetComponent<SpriteRenderer>();
                this.transform = gameObject.transform;
            }

            public void Enable(Color color, bool occupy)
            {
                transform.gameObject.SetActive(true);
                renderer.color = color;
                Ocuppied = occupy;
            }

            public void Disable()
            {
                transform.gameObject.SetActive(false);
                Ocuppied = false;
            }
        }

        /// <summary>
        /// Initializes the Tile array instantiating all tile
        /// GameObjects (already positioned) as inactive.
        /// </summary>
        private void InitializeTiles()
        {
            _tiles = new Tile[sizeY, sizeX];

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Tile newTile = new Tile(
                        Instantiate(
                            _tilePrefab,
                            transform
                        )
                    );
                    newTile.transform.localPosition = new Vector2(x, sizeY - y) - new Vector2(sizeX, sizeY) / 2;
                    newTile.Disable();
                    _tiles[y, x] = newTile;
                }
            }
        }
        #endregion

        #region MonoBehaviour
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// 
        /// 1. We instantiate the tiles.
        /// </summary>
        private void Awake()
        {
            InitializeTiles();
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's
        /// context menu or when adding the component the first time.
        /// </summary>
        public void Reset()
        {
            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < sizeX; x++)
                    DisableTile(new Vector2Int(x, y));
        }
        #endregion

        #region Board
        public TileData DataAt(Vector2Int position)
        {
            if (position.x < 0)
                return TileData.LEFT;
            if (position.x >= sizeX)
                return TileData.RIGHT;
            if (position.y < 0)
                return TileData.OVER;
            if (position.y >= sizeY) {
                return TileData.UNDER;
            }
            return _tiles[position.y, position.x].Ocuppied ?
                TileData.OCCUPIED
                :
                TileData.UNOCCUPIED;
        }

        public void DisableTile(Vector2Int position)
        {
            if(position.x >= 0 && position.x < sizeX && position.y >= 0 && position.y < sizeY)
                _tiles[position.y, position.x].Disable();
        }

        public void EnableTile(Color color, Vector2Int position, bool occupy)
        {
            if(position.x >= 0 && position.x < sizeX && position.y >= 0 && position.y < sizeY)
                _tiles[position.y, position.x].Enable(color, occupy);
        }

        public void RemoveRow(int y) {
            for (int x = 0; x < sizeX; x++)
                DisableTile(new Vector2Int(x, y));
        }

        public void MoveDownRows(int initial, int final, int amount)
        {
            for (int y = initial; y > final; y--)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Tile tile = _tiles[y, x];
                    if(tile.Ocuppied)
                        EnableTile(tile.Color, new Vector2Int(x, y + amount), true);
                    DisableTile(new Vector2Int(x, y));
                }
            }
        }

        public bool CheckRow(int y)
        {
            if(y < 0 || y >= sizeY) return false;

            bool occupied = true;
            for (int x = 0; x < sizeX && occupied; x++)
            {
                occupied = _tiles[y, x].Ocuppied;
            }
            return occupied;
        }
        #endregion
    }
}