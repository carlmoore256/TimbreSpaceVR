using System;

public class ScheduleCancellationToken
{
    public bool IsCancelled { get; set; } = false;
    public Action OnCancel { get; set; }
    public void Cancel() {
        IsCancelled = true;
        OnCancel?.Invoke();
    }

    public ScheduleCancellationToken() { }
    public ScheduleCancellationToken(Action onCancel) {
        OnCancel = onCancel;
    }
}