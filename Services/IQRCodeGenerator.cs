namespace TrophiesDisplay.Services
{
    public interface IQRCodeGenerator
    {
        string GetSVG(string data);
    }
}
