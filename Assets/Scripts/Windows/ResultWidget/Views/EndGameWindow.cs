using System.Collections;
using Windows.Global;
using Constellation.SceneManagement.Manager;
using Features.Data.Implementation;
using Features.Levels.Implementation;
using GameConstants;
using GameStates;
using Infrastructure.DI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WindowsSystem.Core;
using WindowsSystem.Core.Managers;
using GameUIState = Windows.Global.GameUI.State;

namespace Windows.ResultWidget.Views
{
    /// <summary>
    ///     UI to display the game over screen
    /// </summary>
    public class EndGameWindow : BaseWindow
    {
        /// <summary>
        ///     The containing panel of the End Game UI
        /// </summary>
        public Canvas endGameCanvas;

        /// <summary>
        ///     Reference to the Text object that displays the result message
        /// </summary>
        public TMP_Text endGameMessageText;

        /// <summary>
        ///     Panel that shows final star rating
        /// </summary>
        public ScorePanel scorePanel;

        /// <summary>
        ///     Name of level select screen
        /// </summary>
        public string menuSceneName = "MainMenu";

        /// <summary>
        ///     Text to be displayed on popup
        /// </summary>
        public string levelCompleteText = "{0} COMPLETE!";

        public string levelFailedText = "{0} FAILED!";

        /// <summary>
        ///     Background image
        /// </summary>
        public Image background;

        /// <summary>
        ///     Color to set background
        /// </summary>
        public Color winBackgroundColor;

        public Color loseBackgroundColor;

        /// <summary>
        ///     The Canvas that holds the button to go to the next level
        ///     if the player has beaten the level
        /// </summary>
        public Canvas nextLevelButton;

        [Inject] private readonly IScenesManager _scenesManager;
        [Inject] private readonly ApplicationStateMachine _stateMachine;
        [Inject] private readonly WindowsController _windowsController;

        [Inject] private GameManager _gameManager;
        [Inject] private GameUI _gameUI;
        [Inject] private LevelManager LevelManager { get; }

        /// <summary>
        ///     Hide the panel if it is active at the start.
        ///     Subscribe to the <see cref="LevelManager" /> completed/failed events.
        /// </summary>
        protected void Start()
        {
            endGameCanvas.enabled = false;
            nextLevelButton.enabled = false;
            nextLevelButton.gameObject.SetActive(false);

            LevelManager.levelCompleted += Victory;
            LevelManager.levelFailed += Defeat;
        }

        /// <summary>
        ///     Safely unsubscribes from <see cref="LevelManager" /> events.
        /// </summary>
        protected void OnDestroy()
        {
            SafelyUnsubscribe();
            if (_gameUI) _gameUI.Unpause();
        }

        public void ReturnToMainMenu()
        {
            endGameCanvas.enabled = false;
            _windowsController.TryGetWindowById(WindowsConstants.HUD_WINDOW, out var window);
            window.Close();
            _stateMachine.Enter<MainMenuState>();
        }

        public override void OnOpen()
        {
        }

        /// <summary>
        ///     Safely unsubscribes from <see cref="LevelManager" /> events.
        ///     Go back to the main menu scene
        /// </summary>
        public void GoToMainMenu()
        {
            SafelyUnsubscribe();
            StartCoroutine(LoadSceneAsync(menuSceneName));
        }

        /// <summary>
        ///     Safely unsubscribes from <see cref="LevelManager" /> events.
        ///     Reloads the active scene
        /// </summary>
        public void RestartLevel()
        {
            SafelyUnsubscribe();
            var currentSceneName = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneAsync(currentSceneName));
        }

        /// <summary>
        ///     Safely unsubscribes from <see cref="LevelManager" /> events.
        ///     Goes to the next scene if valid
        /// </summary>
        public void GoToNextLevel()
        {
            SafelyUnsubscribe();
            if (!_gameManager) return;
            var item = _gameManager.GetLevelForCurrentScene();
            var list = _gameManager.levelList;
            var levelCount = list.Count;
            var index = -1;
            for (var i = 0; i < levelCount; i++)
                if (item == list[i])
                {
                    index = i + 1;
                    break;
                }

            if (index < 0 || index >= levelCount) return;
            var nextLevel = _gameManager.levelList[index];
            StartCoroutine(LoadSceneAsync(nextLevel.sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            while (asyncLoad.progress < 0.9f) yield return null;
            asyncLoad.allowSceneActivation = true;
            while (!asyncLoad.isDone) yield return null;
        }

        /// <summary>
        ///     Shows the end game screen
        /// </summary>
        protected void OpenEndGameScreen(string endResultText)
        {
            var level = _gameManager.GetLevelForCurrentScene();
            endGameCanvas.enabled = true;

            var score = CalculateFinalScore();
            scorePanel.SetStars(score);
            if (level != null)
            {
                endGameMessageText.text = string.Format(endResultText, level.name.ToUpper());
                _gameManager.CompleteLevel(level.id, score);
            }
            else
            {
                // If the level is not in LevelList, we should just use the name of the scene. This will not store the level's score.
                var levelName = SceneManager.GetActiveScene().name;
                endGameMessageText.text = string.Format(endResultText, levelName.ToUpper());
            }


            if (!_gameUI) return;
            if (_gameUI.state == GameUIState.Building) _gameUI.CancelGhostPlacement();
            _gameUI.GameOver();
        }

        /// <summary>
        ///     Occurs when the level is sucessfully completed
        /// </summary>
        protected void Victory()
        {
            OpenEndGameScreen(levelCompleteText);
            background.color = winBackgroundColor;

            //first check if there are any more levels after this one
            if (nextLevelButton == null || !_gameManager) return;
            var item = _gameManager.GetLevelForCurrentScene();
            var list = _gameManager.levelList;
            var levelCount = list.Count;
            var index = -1;
            for (var i = 0; i < levelCount; i++)
                if (item == list[i])
                {
                    index = i;
                    break;
                }

            //if the level does not exist or this is the last level
            //hide the next level button
            if (index < 0 || index == levelCount - 1)
            {
                nextLevelButton.enabled = false;
                nextLevelButton.gameObject.SetActive(false);
                return;
            }

            nextLevelButton.enabled = true;
            nextLevelButton.gameObject.SetActive(true);
        }

        /// <summary>
        ///     Occurs when level is failed
        /// </summary>
        protected void Defeat()
        {
            OpenEndGameScreen(levelFailedText);
            if (nextLevelButton != null)
            {
                nextLevelButton.enabled = false;
                nextLevelButton.gameObject.SetActive(false);
            }

            background.color = loseBackgroundColor;
        }

        /// <summary>
        ///     Ensure that <see cref="LevelManager" /> events are unsubscribed from when necessary
        /// </summary>
        protected void SafelyUnsubscribe()
        {
            // LazyLoad();
            LevelManager.levelCompleted -= Victory;
            LevelManager.levelFailed -= Defeat;
        }

        /// <summary>
        ///     Add up the health of all the Home Bases and return a score
        /// </summary>
        /// <returns>Final score</returns>
        protected int CalculateFinalScore()
        {
            var homeBaseCount = LevelManager.numberOfHomeBases;
            var homeBases = LevelManager.playerHomeBases;

            var totalRemainingHealth = 0f;
            var totalBaseHealth = 0f;
            for (var i = 0; i < homeBaseCount; i++)
            {
                var config = homeBases[i].configuration;
                totalRemainingHealth += config.currentHealth;
                totalBaseHealth += config.maxHealth;
            }

            var score = CalculateScore(totalRemainingHealth, totalBaseHealth);
            return score;
        }

        /// <summary>
        ///     Take the final remaining health of all bases and rates them
        /// </summary>
        /// <param name="remainingHealth">the total remaining health of all home bases</param>
        /// <param name="maxHealth">the total maximum health of all home bases</param>
        /// <returns>0 to 3 depending on how much health is remaining</returns>
        protected int CalculateScore(float remainingHealth, float maxHealth)
        {
            var normalizedHealth = remainingHealth / maxHealth;
            if (Mathf.Approximately(normalizedHealth, 1f)) return 3;
            if (normalizedHealth is <= 0.9f and >= 0.5f) return 2;
            if (normalizedHealth is < 0.5f and > 0f) return 1;
            return 0;
        }
    }
}