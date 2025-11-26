using TMPro;
using UnityEngine;

public class EnglishRenderer : MonoBehaviour, ITaskRenderer
{
    public TMP_InputField textInput;
    public MultipleChoiceRenderer choice;

    private TaskRequirement current;

    public void Render(TaskRequirement task)
    {
        current = task;
        Clear();

        if (task.kind == TaskKind.MultipleChoice)
        {
            textInput.gameObject.SetActive(false);
            choice.gameObject.SetActive(true);
            choice.Render(task.choices);
        }
        else
        {
            choice.gameObject.SetActive(false);
            textInput.gameObject.SetActive(true);
            textInput.text = "";
        }
    }

    public string GetAnswer()
    {
        return current.kind == TaskKind.MultipleChoice
            ? choice.GetAnswer()
            : textInput.text;
    }

    public void Clear()
    {
        textInput.gameObject.SetActive(false);
        choice.gameObject.SetActive(false);

        textInput.text = "";
        choice.Clear();
    }

    public void SetInteractable(bool v)
    {
        textInput.interactable = v;
        choice.SetInteractable(v);
    }
}
