using UnityEngine;

public class TestEdu : MonoBehaviour
{
    public TaskDatabase db;
    public TaskUIController ui;

    public void Start()
    {
        var t = db.GetRandomTask();
        ui.ShowTask(t);
        ui.OnTaskComplete += OnCompleted;
    }

    private void OnCompleted(TaskResult res)
    {
        Debug.Log("Result: " + res.correct);
    }
}