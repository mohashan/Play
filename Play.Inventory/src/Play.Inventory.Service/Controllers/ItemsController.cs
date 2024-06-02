using Microsoft.AspNetCore.Mvc;
using Play.Common.Repositories;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service;
using Play.Inventory.Service.Clients;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController:ControllerBase
{
    private readonly IRepository<InventoryItem> inventoryItemsRepository;
    private readonly IRepository<CatalogItem> catalogItemsRepsository;

    public ItemsController(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepsository)
    {
        this.inventoryItemsRepository = inventoryItemsRepository;
        this.catalogItemsRepsository = catalogItemsRepsository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if(userId == Guid.Empty)
        {
            return BadRequest();
        }


        var inventoryItemEntities = await inventoryItemsRepository.GetAllAsync(item=>item.UserId == userId);

        var inventoryIds = inventoryItemEntities.Select(c => c.Id);

        var catalogItems = await catalogItemsRepsository.GetAllAsync(c => inventoryIds.Contains(c.Id));

        var inventoryItemDtos = inventoryItemEntities.Select(c => 
        {
            var catalogItem = catalogItems.Single(d => d.Id == c.CatalogItemId);
            return c.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(inventoryItemDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryItemDto>> GetItemAsync(Guid id)
    {
        var item = await inventoryItemsRepository.GetAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);

    }

    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> AddAsync([FromBody] GrantItemsDto item)
    {
        var inventoryItem = await inventoryItemsRepository.GetAsync(
                item => item.UserId == item.UserId && item.CatalogItemId == item.CatalogItemId);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = item.CatalogItemId,
                UserId = item.UserId,
                Quantity = item.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += item.Quantity;
            await inventoryItemsRepository.UpdateAsync(inventoryItem);
        }

        return Ok(inventoryItem);
    }

    //[HttpPut("{id:guid}")]
    //public async Task<IActionResult> PutAsync([FromRoute] Guid id, [FromBody] UpdateItemDto item)
    //{
    //    var existingItem = await itemrepository.GetAsync(id);

    //    if (existingItem == null)
    //    {
    //        var newItem = new Item
    //        {
    //            Price = item.Price,
    //            CreatedDate = DateTimeOffset.UtcNow,
    //            Name = item.Name,
    //            Description = item.Descrition,
    //            Id = Guid.NewGuid(),
    //        };
    //        await itemrepository.CreateAsync(newItem);

    //        return CreatedAtAction(nameof(GetItemAsync), new { id = newItem.Id }, newItem);
    //    }

    //    existingItem.Name = item.Name;
    //    existingItem.Description = item.Descrition;
    //    existingItem.Price = item.Price;

    //    await itemrepository.UpdateAsync(existingItem);
    //    return NoContent();
    //}

    //[HttpDelete("{id:guid}")]
    //public async Task<IActionResult> DeleteAsync(Guid id)
    //{
    //    var item = await itemrepository.GetAsync(id);

    //    if (item == null)
    //    {
    //        return NotFound();
    //    }

    //    await itemrepository.RemoveAsync(id);

    //    return NoContent();

    //}
}
