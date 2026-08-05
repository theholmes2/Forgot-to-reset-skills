using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    public Button newGameButton; // 새 게임 버튼
    public Button continueButton; // 이어하기 버튼
    public Button quitButton; // 종료 버튼

    private void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnClickNewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnClickContinue);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnClickQuit);

        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave(); // 저장 파일이 있을 때만 이어하기 가능
    }

    private void OnClickNewGame()
    {
        if (GameManager.Instance == null)
            return;

        if (GameSoundController.Instance != null)
            GameSoundController.Instance.PlayPersistentButtonClick();

        GameManager.Instance.StartNewGame();
    }

    private void OnClickContinue()
    {
        if (GameManager.Instance == null)
            return;

        if (GameSoundController.Instance != null)
            GameSoundController.Instance.PlayPersistentButtonClick();

        GameManager.Instance.ContinueGame();
    }

    private void OnClickQuit()
    {
        if (GameSoundController.Instance != null)
            GameSoundController.Instance.PlayButtonClick();

        Application.Quit(); // 빌드된 게임 종료
    }
}
