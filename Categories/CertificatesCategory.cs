namespace UnsortedOrderer.Categories;

public sealed class CertificatesCategory : FileCategory
{
    private static readonly string[] CertificateExtensions =
    [
        ".cer", ".crt", ".pfx", ".p12", ".pem"
    ];

    public CertificatesCategory()
        : base("Certificates", "Certificates", CertificateExtensions)
    {
    }
}
