namespace Blazor.Midi.GoKeys.Pages;

public partial class Home
{
    private bool _connected = true;
    private string? _connectionStatus = "disconnect";

    private void PowerClick()
    {
        _connected = !_connected;

        _connectionStatus = _connected ? "disconnect" : "connected";
    }
}
