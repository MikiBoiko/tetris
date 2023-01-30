using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris
{
    public class Game : MonoBehaviour
    {
        #region Attributes, properties and fields
        [Header("Components")]
        [SerializeField] private Player _player;
        [SerializeField] private GameCanvas _canvas;

        [Header("Tick and score")]
        [SerializeField] private int _score, _highscore;
        [SerializeField] private float _initialTick, _tickDecay, _minimumTick;
        public float TickTime => _initialTick / Mathf.Pow(_score + 1, _tickDecay) + _minimumTick;
        private IEnumerator _tick;
        [SerializeField] private int scoreAcelerating = 1, scoreRow = 100, scoreRowPow = 2;
        #endregion

        #region MonoBehaviour
        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            StartGame();

            _highscore = PlayerPrefs.HasKey("Highscore") ? PlayerPrefs.GetInt("Highscore") : 0;
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's
        /// context menu or when adding the component the first time.
        /// </summary>
        private void Reset()
        {
            _score = 0;

            _player.Reset();
            _canvas.Reset();
            
            StartGame();
        }
        #endregion

        #region Game
        private void DoTick()
        {
            _player.MoveDown();
            _canvas.SetScore(_score, _score > _highscore);

        }

        private void StartTick(float tickTime, bool accelerating)
        {
            StopTick();
            _tick = ITick(tickTime, accelerating);
            StartCoroutine(_tick);
        }

        private void StopTick()
        {
            if (_tick != null)
                StopCoroutine(_tick);
        }

        private IEnumerator ITick(float tickTime, bool accelerating)
        {
            while (true)
            {
                yield return new WaitForSeconds(tickTime);
                DoTick();
                if (accelerating)
                    _score += scoreAcelerating;
            }
        }

        private void StartGame()
        {
            _player.StartPlayer((int count) => {
                _score += (int)Mathf.Pow(count, scoreRowPow) * scoreRow;
            });
            _player.onDeath = StopGame;

            _player.onAccelerate = Accelerate;
            _player.onDeaccelerate = Deaccelerate;

            StartTick(TickTime, false);

            Debug.Log("Game started!");
        }

        private void StopGame()
        {
            StopTick();

            _player.onAccelerate = () => { Reset(); };
            _player.onDeaccelerate = () => { };

            if(_score > _highscore)
                _highscore = _score;
            PlayerPrefs.SetInt("Highscore", _highscore);

            _canvas.SetGameOver();

            Debug.Log("Game ended!");
        }

        private void Accelerate()
        {
            Debug.Log("Accelerating!");
            DoTick();
            StartTick(_minimumTick, true);
        }

        private void Deaccelerate()
        {
            StartTick(TickTime, false);
        }
        #endregion
    }
}
