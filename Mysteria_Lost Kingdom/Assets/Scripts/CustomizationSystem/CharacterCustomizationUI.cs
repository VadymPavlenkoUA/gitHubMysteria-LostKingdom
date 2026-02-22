using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCustomizationUI : MonoBehaviour
{
    public CharacterCustomizer customizer;

    public CharacterCustomizationData defaultData;

    private CharacterCustomizationData data;

    [Header("Nickname Panel")]
    public GameObject nicknamePanel;
    public TMP_InputField nicknameInput;
    public Button startGameButton;
    public Button backButton;

    void Start()
    {
        data = CloneData(defaultData);
        UpdatePreview();
    }

    #region HAIR
    public void NextHair()
    {
        data.hairsIndex = LoopNext(data.hairsIndex, customizer.hairs.Length);

        int maxColors = customizer.hairs[data.hairsIndex].colors.Length;
        if (data.hairsColorIndex >= maxColors)
            data.hairsColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void PrevHair()
    {
        data.hairsIndex = LoopPrev(data.hairsIndex, customizer.hairs.Length);

        int maxColors = customizer.hairs[data.hairsIndex].colors.Length;
        if (data.hairsColorIndex >= maxColors)
            data.hairsColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void SetHairColor(int colorIndex)
    {
        data.hairsColorIndex = colorIndex;
        UpdatePreview();
    }
    #endregion

    #region FACE HAIR
    public void NextFaceHair()
    {
        data.faceHairsIndex = LoopNext(data.faceHairsIndex, customizer.faceHairs.Length);

        int maxColors = customizer.faceHairs[data.faceHairsIndex].colors.Length;
        if (data.faceHairsColorIndex >= maxColors)
            data.faceHairsColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void PrevFaceHair()
    {
        data.faceHairsIndex = LoopPrev(data.faceHairsIndex, customizer.faceHairs.Length);

        int maxColors = customizer.faceHairs[data.faceHairsIndex].colors.Length;
        if (data.faceHairsColorIndex >= maxColors)
            data.faceHairsColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void SetFaceHairColor(int colorIndex)
    {
        data.faceHairsColorIndex = colorIndex;
        UpdatePreview();
    }
    #endregion

    #region EYES
    public void NextEyes()
    {
        data.eyesIndex = LoopNext(data.eyesIndex, customizer.eyes.Length);

        int maxColors = customizer.eyes[data.eyesIndex].colors.Length;
        if (data.eyesColorIndex >= maxColors)
            data.eyesColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void PrevEyes()
    {
        data.eyesIndex = LoopPrev(data.eyesIndex, customizer.eyes.Length);

        int maxColors = customizer.eyes[data.eyesIndex].colors.Length;
        if (data.eyesColorIndex >= maxColors)
            data.eyesColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void SetEyesColor(int colorIndex)
    {
        data.eyesColorIndex = colorIndex;
        UpdatePreview();
    }
    #endregion

    #region EYEBROWS
    public void NextEyebrows()
    {
        data.eyebrowsIndex = LoopNext(data.eyebrowsIndex, customizer.eyeBrows.Length);

        int maxColors = customizer.eyeBrows[data.eyebrowsIndex].colors.Length;
        if (data.eyebrowsColorIndex >= maxColors)
            data.eyebrowsColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void PrevEyebrows()
    {
        data.eyebrowsIndex = LoopPrev(data.eyebrowsIndex, customizer.eyeBrows.Length);

        int maxColors = customizer.eyeBrows[data.eyebrowsIndex].colors.Length;
        if (data.eyebrowsColorIndex >= maxColors)
            data.eyebrowsColorIndex = maxColors - 1;

        UpdatePreview();
    }

    public void SetEyebrowColor(int colorIndex)
    {
        data.eyebrowsColorIndex = colorIndex;
        UpdatePreview();
    }
    #endregion

    #region NOSE
    public void NextNose()
    {
        data.nosesIndex = LoopNext(data.nosesIndex, customizer.noses.Length);
        UpdatePreview();
    }

    public void PrevNose()
    {
        data.nosesIndex = LoopPrev(data.nosesIndex, customizer.noses.Length);
        UpdatePreview();
    }
    #endregion

    #region EARS
    public void NextEars()
    {
        data.earsIndex = LoopNext(data.earsIndex, customizer.ears.Length);
        UpdatePreview();
    }

    public void PrevEars()
    {
        data.earsIndex = LoopPrev(data.earsIndex, customizer.ears.Length);
        UpdatePreview();
    }
    #endregion

    int LoopNext(int current, int max)
    {
        if (max == 0) return 0;
        current++;
        if (current >= max) current = 0;
        return current;
    }

    int LoopPrev(int current, int max)
    {
        if (max == 0) return 0;
        current--;
        if (current < 0) current = max - 1;
        return current;
    }

    void UpdatePreview()
    {
        customizer.ApplyCustomization(data);
    }

    public void Confirm()
    {
        //SceneLoader.isNewGame = true;
        //SceneLoader.newGameCustomization = CloneData(data);
        //SceneLoader.LoadScene("MainScene");

        nicknamePanel.SetActive(true);
        startGameButton.interactable = false;

        nicknameInput.onValueChanged.RemoveAllListeners();
        nicknameInput.onValueChanged.AddListener(OnNicknameChanged);
    }
        private void OnNicknameChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            startGameButton.interactable = false;
        else
            startGameButton.interactable = true;
    }

    public void StartGameWithNickname()
    {
        string finalNick = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(finalNick)) return;

        SceneLoader.isNewGame = true;
        SceneLoader.newGameCustomization = CloneData(data);

        SceneLoader.newGameNickName = finalNick;

        SceneLoader.LoadScene("MainScene");
    }

    public void BackToCustomization()
    {
        nicknamePanel.SetActive(false);
    }

    public void ResetCustomization()
    {
        data = CloneData(defaultData);
        UpdatePreview();
    }

    CharacterCustomizationData CloneData(CharacterCustomizationData source)
    {
        return new CharacterCustomizationData
        {
            nosesIndex = source.nosesIndex,
            hairsIndex = source.hairsIndex,
            hairsColorIndex = source.hairsColorIndex,
            faceHairsIndex = source.faceHairsIndex,
            faceHairsColorIndex = source.faceHairsColorIndex,
            eyesIndex = source.eyesIndex,
            eyesColorIndex = source.eyesColorIndex,
            eyebrowsIndex = source.eyebrowsIndex,
            eyebrowsColorIndex = source.eyebrowsColorIndex,
            earsIndex = source.earsIndex
        };
    }

    public void Return()
    {
        SceneLoader.LoadScene("MainMenu");
    }
}