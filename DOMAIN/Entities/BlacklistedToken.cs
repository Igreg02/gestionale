using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("blacklistedtoken")]
    public class BlacklistedToken : XPCustomObject
    {
        public BlacklistedToken(Session session) : base(session) { }

        private string _jti = string.Empty;
        [Key]
        [Size(255)]
        [Persistent("jti")]
        public string Jti
        {
            get => _jti;
            set => SetPropertyValue(nameof(Jti), ref _jti, value);
        }

        private DateTime _expiresAt;
        [Persistent("expiresAt")]
        public DateTime ExpiresAt
        {
            get => _expiresAt;
            set => SetPropertyValue(nameof(ExpiresAt), ref _expiresAt, value);
        }
    }
}
