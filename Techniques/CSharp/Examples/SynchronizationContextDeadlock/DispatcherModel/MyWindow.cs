namespace DispatcherModel;

public class MyWindow
{
    public void Show()
    {
        OnShow();
    }

    public event EventHandler? Showed;
    protected virtual void OnShow()
    {
        Showed?.Invoke(this, EventArgs.Empty);
    }
}