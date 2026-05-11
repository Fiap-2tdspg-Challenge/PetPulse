namespace PetPulse.Domain.Commom;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    public bool Active { get; set; }
    
    public DateTime CreatedAt{ get; set; } = DateTime.Now;
}