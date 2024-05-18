using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Dtos;
using System;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    public static readonly List<ItemDto> Items = new()
    {
        new ItemDto(Guid.NewGuid(), "Potion", "Restore a small amount of HP",5,DateTimeOffset.UtcNow),
        new ItemDto(Guid.NewGuid(), "Potion", "Restore a small amount of HP",5,DateTimeOffset.UtcNow),
        new ItemDto(Guid.NewGuid(), "Antidote", "Deals a small amount of damage",20,DateTimeOffset.UtcNow),

    };

    [HttpGet]
    public IEnumerable<ItemDto> Get()
    {
        return Items;
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ItemDto> GetItem(Guid id)
    {
        var item = Items.SingleOrDefault(c => c.Id == id);

        if(item == null)
        {
            return NotFound();
        }

        return Ok(item);

    }

    [HttpPost]
    public ActionResult<ItemDto> Add([FromBody] CreateItemDto item)
    {
        var newItem = new ItemDto(Guid.NewGuid(), item.Name, item.Descrition, item.Price, DateTimeOffset.UtcNow);
        Items.Add(newItem);
        return CreatedAtAction(nameof(GetItem), new { id = newItem.Id },newItem);
    }

    [HttpPut("{id}")]
    public IActionResult Put([FromRoute]Guid id, [FromBody]UpdateItemDto item)
    {
        var existingItem = Items.FirstOrDefault(c => c.Id == id);

        if (existingItem == null)
        {
            var newItem = new ItemDto(id, item.Name, item.Descrition, item.Price, DateTimeOffset.UtcNow);

            Items.Add(newItem);

            return CreatedAtAction(nameof(GetItem), new { id = id},newItem);
        }

        var updatedItem = existingItem with
        {
            Name = item.Name,
            Price = item.Price,
            Descrition = item.Descrition,
        };
        var index = Items.FindIndex(c => c.Id == id);

        Items[index] = updatedItem;

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteItem(Guid id)
    {
        var index = Items.FindIndex(c => c.Id == id);

        if (index == -1)
        {
            return NotFound();
        }

        Items.RemoveAt(index);

        return NoContent();

    }
}