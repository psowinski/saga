using System;
using System.Threading.Tasks;
using Common.Aggregate;
using Persistence;
using Common.General;
using Domain.BusinessLogic.Order;
using DomainOrder = Domain.Model.Order.Order;

namespace App
{
   public class Order
   {
      private readonly IPersistenceClient persistence;

      public Order(IPersistenceClient persistence)
      {
         this.persistence = persistence;
      }

      public async Task AddItem(Optional<string> orderId, string correlationId, string description, decimal cost)
      {
         var orderStreamId = orderId.ValueOr(StreamNumbering.NewStreamId<DomainOrder>);
         var order = await this.persistence.GetState<DomainOrder>(orderStreamId);

         var itemAdded = new AddOrderItem(correlationId, DateTime.Now)
         {
            Description = description,
            Cost = cost
         }.Execute(order);

         await this.persistence.Save(itemAdded);
      }

      public async Task Checkout(string orderId, string correlationId)
      {
         var order = await this.persistence.GetState<DomainOrder>(orderId);
         var orderCheckedOut = new CheckOutOrder(correlationId, DateTime.Now).Execute(order);
         await this.persistence.Save(orderCheckedOut);
      }
   }
}
