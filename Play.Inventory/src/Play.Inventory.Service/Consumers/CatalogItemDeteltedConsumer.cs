using MassTransit;
using Play.Catalog.Contracts;
using Play.Common.Repositories;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers;

public class CatalogItemDeteltedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> repository;

    public CatalogItemDeteltedConsumer(IRepository<CatalogItem> repository)
    {
        this.repository = repository;
    }
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;

        var item = await repository.GetAsync(message.ItemId);

        if (item == null)
        {
            return;
        }

        await repository.RemoveAsync(item.Id);
    }
}
