using UnityEngine;

namespace Tetris
{
    [CreateAssetMenu(fileName = "New Piece", menuName = "Tetris/Piece")]
    public class Piece : ScriptableObject
    {
        public Vector2Int size;
        [HideInInspector] public int rotation = 0;
        public bool[] mask;
        public Color color;
        public Sprite sprite;

        [ContextMenu("Generate Mask")]
        private void GenerateMask() => mask = new bool[size.x * size.y];
    }
}
