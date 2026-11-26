using System.Text.Json.Serialization;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Users.GetUsers;

internal sealed record Response(
    int Id,
    string GivenName,
    string FamilyName,
    string? SecondFamilyName,
    string? DisplayName,

    string? ControlNumber,
    string? PhoneNumber,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] Gender Gender,
    string Email,
    bool IsEmailVerified,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] UserRole Role,

    bool IsDeleted,
    DateTimeOffset? DeletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);
