namespace StarCorp.Business.Entities;

public record Customer(int Id, string Name, string Document, string Email, bool IsActive, DateTime CreatedAt);
