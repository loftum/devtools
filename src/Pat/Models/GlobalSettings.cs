namespace Pat.Models
{
    public interface IGlobalSettings
    {
        bool Expect100Continue { get; set; }
        bool CheckCertificateRevocationList { get; set; }
    }

    public class GlobalSettings : IGlobalSettings
    {
        public bool Expect100Continue { get; set; }
        public bool CheckCertificateRevocationList { get; set; }
    }
}