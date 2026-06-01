using StarCorp.Business.Notifications;
using StarCorp.Business.Notifications.Abstractions;

namespace StarCorp.UnitTests.Notifications;

public class NotificationContextTests
{
    private readonly NotificationContext _sut = new();

    [Fact]
    public void New_Context_Should_Be_Empty()
    {
        Assert.False(_sut.HasNotifications);
        Assert.Equal(NotificationType.None, _sut.Type);
    }

    [Fact]
    public void AddNotification_Should_Set_Validation_Type()
    {
        _sut.AddNotification("field", "invalido");

        Assert.True(_sut.HasNotifications);
        Assert.Equal(NotificationType.Validation, _sut.Type);
        var notification = Assert.Single(_sut.Notifications);
        Assert.Equal("field", notification.Key);
        Assert.Equal("invalido", notification.Message);
    }

    [Fact]
    public void AddNotFound_Should_Set_NotFound_Type()
    {
        _sut.AddNotFound("booking", "nao encontrada");
        Assert.Equal(NotificationType.NotFound, _sut.Type);
    }

    [Fact]
    public void AddConflict_Should_Set_Conflict_Type()
    {
        _sut.AddConflict("flight", "sem assentos");
        Assert.Equal(NotificationType.Conflict, _sut.Type);
    }

    [Fact]
    public void AddUnprocessable_Should_Set_UnprocessableEntity_Type()
    {
        _sut.AddUnprocessable("customer", "inativo");
        Assert.Equal(NotificationType.UnprocessableEntity, _sut.Type);
    }

    [Fact]
    public void Specific_Error_Should_Not_Be_Downgraded_By_Later_Validation()
    {
        _sut.AddNotFound("booking", "nao encontrada");
        _sut.AddNotification("field", "invalido");

        Assert.Equal(NotificationType.NotFound, _sut.Type);
    }
}
