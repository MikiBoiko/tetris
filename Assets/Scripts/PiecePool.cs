using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris
{
    public class PiecePool : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private int minumumSize = 3;

        [SerializeField]
        private List<Piece> _prototypes, _pool;
        #endregion

        #region MonoBahaviour
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            Initialize();
        }
        #endregion

        #region Pool
        public void Initialize()
        {
            _pool = new();
            Pool();
        }

        private void Pool()
        {
            List<Piece> batch = new();

            foreach (Piece prototype in _prototypes)
            {
                Piece newPiece = Instantiate(prototype);
                newPiece.rotation = Random.Range(0, 4);
                batch.Add(newPiece);
            }

            // Fisher-Yates shuffle
            int n = batch.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                Piece aux = batch[k];
                batch[k] = batch[n];
                batch[n] = aux;
            }

            _pool.AddRange(batch);
        }

        public Piece Pop()
        {
            Piece piece = _pool[0];
            _pool.RemoveAt(0);

            if(_pool.Count <= minumumSize)
                Pool();

            return piece;
        }
        #endregion
    }
}