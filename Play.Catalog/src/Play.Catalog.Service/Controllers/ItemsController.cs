using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Extensions;
using Play.Common.Repositories;
using System;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> itemrepository;

    private readonly IPublishEndpoint publishEndpoint;

    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
    {
        this.itemrepository = itemsRepository;
        this.publishEndpoint = publishEndpoint;
    }
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAsync()
    {
        var Items = (await itemrepository.GetAllAsync()).Select(c => c.AsDto());

        return Items;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
    {
        var item = await itemrepository.GetAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);

    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> AddAsync([FromBody] CreateItemDto item)
    {
        var Item = new Item
        {
            CreatedDate = DateTimeOffset.UtcNow,
            Description = item.Description,
            Name = item.Name,
            Price = item.Price,
        };
        await itemrepository.CreateAsync(Item);

        await publishEndpoint.Publish(new CatalogItemCreated(Item.Id,Item.Name,Item.Description));

        return CreatedAtAction(nameof(GetItemAsync), new { id = Item.Id }, Item);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync([FromRoute] Guid id, [FromBody] UpdateItemDto item)
    {
        var existingItem = await itemrepository.GetAsync(id);

        if (existingItem == null)
        {
            var newItem = new Item
            {
                Price = item.Price,
                CreatedDate = DateTimeOffset.UtcNow,
                Name = item.Name,
                Description = item.Description,
                Id = Guid.NewGuid(),
            };
            await itemrepository.CreateAsync(newItem);

            await publishEndpoint.Publish(new CatalogItemCreated(newItem.Id, newItem.Name, newItem.Description));


            return CreatedAtAction(nameof(GetItemAsync), new { id = newItem.Id }, newItem);
        }

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.Price = item.Price;

        await itemrepository.UpdateAsync(existingItem);
        await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await itemrepository.GetAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        await itemrepository.RemoveAsync(id);
        await publishEndpoint.Publish(new CatalogItemDeleted(id));

        return NoContent();

    }
}