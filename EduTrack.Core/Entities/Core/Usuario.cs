namespace EduTrack.Core.Entities.Core
{
    public class Usuario
    {
        public Guid id { get; set; }
        public string nombre_usuario { get; set; } = string.Empty;
        public string contrasena_hash { get; set; } = string.Empty;
        public Guid sello_seguridad { get; set; }
        public bool activo { get; set; }

        // Additional profile/contact fields used by the application
        public string? nombre_completo { get; set; }
        public string? email { get; set; }
        public string? telefono { get; set; }
        public string? tipo_documento_id { get; set; }
        public string? numero_documento { get; set; }
        public string? estado { get; set; }

        // Audit fields
        public DateTimeOffset? created_at { get; set; }
        public Guid? created_by { get; set; }
        public DateTimeOffset? updated_at { get; set; }
        public Guid? updated_by { get; set; }
        public DateTimeOffset? deleted_at { get; set; }

        // Roles (used when loading users together with roles)
        public List<Rol> roles { get; set; } = new List<Rol>();

        // Simple helpers (no inheritance). These replicate the behavior previously provided
        // by a base Entity so repositories/controllers can call them directly.
        public void SetId(Guid id)
        {
            this.id = id;
        }

        public void SetCreationAudit(DateTimeOffset? when, Guid? userId)
        {
            created_at = when;
            created_by = userId;
        }

        public void SetUpdateAudit(DateTimeOffset? when, Guid? userId)
        {
            updated_at = when;
            updated_by = userId;
        }

        public void SetDeletedAudit(DateTimeOffset? when)
        {
            deleted_at = when;
        }
    }
}
