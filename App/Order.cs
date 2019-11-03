using System;
using System.Threading.Tasks;
using Domain.Order;
using Persistence;
using Common.General;

namespace App
{
   public class Order
   {
      IPersistenceClient persistence;

      public Order(IPersistenceClient persistence)
      {
         this.persistence = persistence;
      }

      public async Task AddItem(Optional<string> orderId, string correlationId, string description, decimal cost)
      {
         var orderStreamId = orderId.ValueOr(() => StreamNumbering.NewStreamId<Domain.Order.Order>());
         var order = await this.persistence.GetState<Domain.Order.Order>(orderStreamId);

         var itemAdded = new AddOrderItem(correlationId, DateTime.Now)
         {
            Description = description,
            Cost = cost
         }.Execute(order);

         await this.persistence.Save(itemAdded);
      }

      public async Task Checkout(string orderId, string correlationId)
      {
         var order = await this.persistence.GetState<Domain.Order.Order>(orderId);
         var orderCheckedOut = new CheckOutOrder(correlationId, DateTime.Now).Execute(order);
         await this.persistence.Save(orderCheckedOut);
      }
   }
}
