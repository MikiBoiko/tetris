using UnityEngine;
using UnityEngine.UI;

namespace Tetris
{
    public class GameCanvas : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _gameOverText;
        private bool _isHighscore;

        public void SetScore(int score, bool highscore)
        {
            _scoreText.text = highscore ?
                "¡" + score + "!"
                :
                score.ToString();

            _isHighscore = highscore;
        }

        public void SetGameOver()
        {
            _gameOverText.enabled = true;
            _gameOverText.text = _isHighscore ?
                "GAME OVER HIGHSCORE"
                :
                "GAME OVER";
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            Reset();
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's
        /// context menu or when adding the component the first time.
        /// </summary>
        public void Reset()
        {
            SetScore(0, false);
            _gameOverText.enabled = false;
        }
    }
}
