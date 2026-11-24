using System.Text.Json.Serialization;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Users.UpdateUser;

internal sealed record Request(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    UserRole? Role = null);
