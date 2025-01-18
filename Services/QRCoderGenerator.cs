using QRCoder;
using System.Drawing;

namespace TrophiesDisplay.Services
{
    public class QRCoderGenerator : IQRCodeGenerator
    {
        public string GetSVG(string data)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);

            // Define the size of the QR code
            int pixelsPerModule = 20;

            // Generate the SVG with a viewBox for responsive scaling
            string qrCodeAsSvg = qrCode.GetGraphic(
                pixelsPerModule,
                "#ffffff",
                "#00000000",
                drawQuietZones: false,
                sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute
            );

            // Return or use the SVG string as needed
            return qrCodeAsSvg;
        }
    }
}
