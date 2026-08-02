using QRCoder;

namespace UrlShortner.Application.Services;

public class QrCodeService
{
    public string GenerateQrCodeBase64(string url, int pixelsPerModule = 10)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);

        return Convert.ToBase64String(qrCodeBytes);
    }
}