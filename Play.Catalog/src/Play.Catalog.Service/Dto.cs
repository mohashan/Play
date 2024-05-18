using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Dtos;

public record ItemDto(Guid Id, string Name, string Descrition, decimal Price, DateTimeOffset CreatedDate);

public record CreateItemDto([Required]string Name, string Descrition, [Range(0, 1000)] decimal Price);

public record UpdateItemDto([Required]string Name, string Descrition,[Range(0,1000)] decimal Price);