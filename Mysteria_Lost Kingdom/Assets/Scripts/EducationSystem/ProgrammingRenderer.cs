using TMPro;
using UnityEngine;

public class ProgrammingRenderer : MonoBehaviour, ITaskRenderer
{
    public TMP_InputField codeInput;
    public MultipleChoiceRenderer choice;

    private TaskRequirement current;

    public void Render(TaskRequirement task)
    {
        current = task;
        Clear();

        if (task.kind == TaskKind.MultipleChoice)
        {
            codeInput.gameObject.SetActive(false);
            choice.gameObject.SetActive(true);
            choice.Render(task.choices);
        }
        else
        {
            choice.gameObject.SetActive(false);
            codeInput.gameObject.SetActive(true);

            codeInput.contentType = TMP_InputField.ContentType.Standard;
            codeInput.lineType = TMP_InputField.LineType.MultiLineNewline;

            codeInput.text =
                string.IsNullOrEmpty(task.codeTemplate) ? "" : task.codeTemplate;
        }
    }

    public string GetAnswer()
    {
        return current.kind == TaskKind.MultipleChoice
            ? choice.GetAnswer()
            : codeInput.text;
    }

    public void Clear()
    {
        codeInput.gameObject.SetActive(false);
        choice.gameObject.SetActive(false);

        codeInput.text = "";
        choice.Clear();
    }

    public void SetInteractable(bool v)
    {
        codeInput.interactable = v;
        choice.SetInteractable(v);
    }
}
