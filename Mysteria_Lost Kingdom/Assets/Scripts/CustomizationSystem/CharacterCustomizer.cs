using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    public bool isRTCharacter = false;

    [Header("Noses")]
    public GameObject[] noses;

    [Header("Hairs (type x color)")]
    public ComplexType[] hairs;

    [Header("FaceHairs (type x color)")]
    public ComplexType[] faceHairs;

    [Header("Eyes (type x color)")]
    public ComplexType[] eyes;

    [Header("EyeBrows (type x color)")]
    public ComplexType[] eyeBrows;

    [Header("Ears")]
    public GameObject[] ears;

    public void ApplyCustomization(CharacterCustomizationData data)
    {
        SetSingle(noses, data.nosesIndex);

        SetWithColor(hairs, data.hairsIndex, data.hairsColorIndex);
        SetWithColor(faceHairs, data.faceHairsIndex, data.faceHairsColorIndex);
        SetWithColor(eyes, data.eyesIndex, data.eyesColorIndex);
        SetWithColor(eyeBrows, data.eyebrowsIndex, data.eyebrowsColorIndex);

        SetSingle(ears, data.earsIndex);
    }

    void SetSingle(GameObject[] array, int index)
    {
        for (int i = 0; i < array.Length; i++)
            array[i].SetActive(i == index);
    }

    void SetWithColor(ComplexType[] array, int typeIndex, int colorIndex)
    {
        for (int t = 0; t < array.Length; t++)
        {
            for (int c = 0; c < array[t].colors.Length; c++)
            {
                bool active = (t == typeIndex && c == colorIndex);
                array[t].colors[c].SetActive(active);
            }
        }
    }

    public CharacterCustomizationData GetCustomizationData()
    {
        CharacterCustomizationData data = new CharacterCustomizationData();

        // Ніс
        data.nosesIndex = GetActiveIndex(noses);

        // Вуха
        data.earsIndex = GetActiveIndex(ears);

        // Складні типи: тип + колір
        GetActiveComplex(hairs, out data.hairsIndex, out data.hairsColorIndex);
        GetActiveComplex(faceHairs, out data.faceHairsIndex, out data.faceHairsColorIndex);
        GetActiveComplex(eyes, out data.eyesIndex, out data.eyesColorIndex);
        GetActiveComplex(eyeBrows, out data.eyebrowsIndex, out data.eyebrowsColorIndex);

        return data;
    }

    private int GetActiveIndex(GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].activeSelf)
                return i;
        }
        return 0;
    }

    private void GetActiveComplex(ComplexType[] array, out int typeIndex, out int colorIndex)
    {
        typeIndex = 0;
        colorIndex = 0;

        for (int t = 0; t < array.Length; t++)
        {
            for (int c = 0; c < array[t].colors.Length; c++)
            {
                if (array[t].colors[c].activeSelf)
                {
                    typeIndex = t;
                    colorIndex = c;
                    return;
                }
            }
        }
    }
}

[System.Serializable]
public class ComplexType
{
    public GameObject[] colors;
}