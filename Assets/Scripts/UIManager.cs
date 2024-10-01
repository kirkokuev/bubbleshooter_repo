using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BubbleShooter
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance= null;
        public UIState currentUIState = UIState.Main;

        public GameObject mainSceneUIPrefab;
        public GameObject gameplaySceneUIPrefab;
        public GameObject aboutSceneUIPrefab;

        private Text scoreValue;
        private Text remainingBalls;

        private GameObject winPanel;
        private GameObject losePanel;

        private GameObject mainUI;
        private GameObject gameplayUI;
        private GameObject aboutUI;


        public enum UIState
        {
            Main,
            Init,
            Gameplay,
            Losing,
            Winning,
            About
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);

            }
            else 
            {
                Destroy(gameObject);
            }
        }

        public void ExitButtonPress()
        {

        }


        private void Update()
        {
            if (currentUIState == UIState.About)
            {
                InitAboutUI();
            }
            if (currentUIState == UIState.Main)
            {
                InitMainUI();
            }
            if (currentUIState == UIState.Init)
            {
                InitGameplayUI();
            }
            if (currentUIState == UIState.Gameplay)
            {
                UpdateGameplayUI();
            }
            if (currentUIState == UIState.Losing)
            {
                losePanel.SetActive(true);
            }
            if (currentUIState == UIState.Winning)
            {
                winPanel.SetActive(true);
            }

        }

        void StartGame()
        {
            if (mainUI != null)
            {
                Destroy(mainUI);
                Debug.Log("Main UI " + mainUI);
            }
            StartCoroutine(AsyncSceneLoad(1));

        }
        void OpenAbout()
        {
            if (mainUI != null ) Destroy(mainUI);
            if (gameplayUI != null) Destroy(gameplayUI);
            StartCoroutine(AsyncSceneLoad(2));
        }

        void ReturnToMain()
        {

            if (gameplayUI != null) Destroy(gameplayUI);
            if (aboutUI != null) Destroy(aboutUI);
            StartCoroutine(AsyncSceneLoad(0));
        }

        void OpenSocialLink()
        {
            Application.OpenURL("https://vk.com/id79048");
        }

        void ExitGame()
        {
            Application.Quit();
        }

        private void InitAboutUI()
        {
            if (aboutUI == null)
            {
                if (mainUI != null) { Destroy(mainUI); }
                aboutUI = Instantiate(aboutSceneUIPrefab);
                GameObject.Find("BackButton").gameObject.GetComponent<Button>().onClick.AddListener(() => ReturnToMain());
                GameObject.Find("SocialButton").gameObject.GetComponent<Button>().onClick.AddListener(() => OpenSocialLink());
            }
        }


        IEnumerator AsyncSceneLoad(int sceneIndex)
        {
            AsyncOperation asyncload = SceneManager.LoadSceneAsync(sceneIndex);
            while (!asyncload.isDone)
            {
                yield return null;

            }

            if (sceneIndex == 1)
            {
                currentUIState = UIState.Init;
                
            }
            else if (sceneIndex == 0) { currentUIState = UIState.Main; }
            else if (sceneIndex == 2) { currentUIState = UIState.About; }
        }
        private void InitMainUI()
        {
            if (mainUI == null)
            {
                if (aboutUI != null) { Destroy(aboutUI); }
                mainUI = Instantiate(mainSceneUIPrefab);
                GetComponent<ScoreManager>().LoadScore();
                GameObject.Find("HighscoreValue").gameObject.GetComponent<Text>().text = DataObject.instance.highScore.ToString();
                GameObject.Find("StartButton").gameObject.GetComponent<Button>().onClick.AddListener(() => StartGame());
                GameObject.Find("AboutButton").gameObject.GetComponent<Button>().onClick.AddListener(() => OpenAbout());
                GameObject.Find("ExitButton").gameObject.GetComponent<Button>().onClick.AddListener(() => ExitGame());
                DataObject.instance.ResetData();
            }
        }
        private void InitGameplayUI()
        {
            if (gameplayUI == null)
            {
                if (mainUI != null) { Destroy(mainUI); }
                gameplayUI = Instantiate(gameplaySceneUIPrefab);
                scoreValue = GameObject.Find("ScoreValue").gameObject.GetComponent<Text>();
                remainingBalls = GameObject.Find("RemainingBallsValue").gameObject.GetComponent<Text>();
                winPanel = GameObject.Find("WinPanel");
                losePanel = GameObject.Find("LosePanel");
                GameObject.Find("BackButton").gameObject.GetComponent<Button>().onClick.AddListener(() => ReturnToMain());
                GameObject.Find("WinExitButton").gameObject.GetComponent<Button>().onClick.AddListener(() => ReturnToMain());
                GameObject.Find("LostExitButton").gameObject.GetComponent<Button>().onClick.AddListener(() => ReturnToMain());
                currentUIState = UIState.Gameplay;
                Debug.Log("Current UI state " + currentUIState);

            }

        }

        private void UpdateGameplayUI()
        {
            scoreValue.text = DataObject.instance.currentScore.ToString();
            remainingBalls.text = DataObject.instance.remainingBalls.ToString();
            winPanel.SetActive(false);
            losePanel.SetActive(false);
            if (GameLoopManager.instance.currentGameState == GameLoopManager.GameStates.Finish)
            {
                if (GameLoopManager.instance.isWinning)
                {
                    currentUIState = UIState.Winning;
                    
                }
                else
                {
                    currentUIState = UIState.Losing;
                }
            }
        }
    }

}
