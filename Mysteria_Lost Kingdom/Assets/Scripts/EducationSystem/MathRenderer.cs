using TMPro;
using UnityEngine;

public class MathRenderer : MonoBehaviour, ITaskRenderer
{
    public TMP_InputField numericInput;
    public MultipleChoiceRenderer choice;

    private TaskRequirement current;

    public void Render(TaskRequirement task)
    {
        current = task;
        Clear();

        if (task.kind == TaskKind.MultipleChoice)
        {
            numericInput.gameObject.SetActive(false);
            choice.gameObject.SetActive(true);
            choice.Render(task.choices);
        }
        else
        {
            choice.gameObject.SetActive(false);
            numericInput.gameObject.SetActive(true);
            numericInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            numericInput.text = "";
        }
    }

    public string GetAnswer()
    {
        return current.kind == TaskKind.MultipleChoice
            ? choice.GetAnswer()
            : numericInput.text;
    }

    public void Clear()
    {
        numericInput.gameObject.SetActive(false);
        choice.gameObject.SetActive(false);

        numericInput.text = "";
        choice.Clear();
    }

    public void SetInteractable(bool v)
    {
        numericInput.interactable = v;
        choice.SetInteractable(v);
    }
}
