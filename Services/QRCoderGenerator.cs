﻿using QRCoder;

namespace TrophiesDisplay.Services
{
    public class QRCoderGenerator : IQRCodeGenerator
    {
        public string GetSVG(string data)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);
            string qrCodeAsSvg = qrCode.GetGraphic(20);
            return qrCodeAsSvg;
        }
    }
}
