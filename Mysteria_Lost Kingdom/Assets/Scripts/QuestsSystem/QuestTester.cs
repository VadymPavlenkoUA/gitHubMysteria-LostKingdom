using System.Collections.Generic;
using UnityEngine;

public class QuestTester : MonoBehaviour
{
    public QuestData quest;
    void Start()
    {
        // Переконуємось, що є QuestManager у сцені
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager не знайдено в сцені!");
            return;
        }

        // Додаємо в систему
        QuestManager.Instance.StartQuest(quest);

        Debug.Log("Тестовий квест додано до QuestManager");
    }
}