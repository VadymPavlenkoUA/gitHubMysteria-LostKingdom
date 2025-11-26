using UnityEngine.UI;

public interface ITaskRenderer
{
    void Render(TaskRequirement task);
    string GetAnswer();
    void Clear();
    void SetInteractable(bool v);
}
