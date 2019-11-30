using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Aggregate;
using Common.General;
using Microsoft.Extensions.Logging;

namespace Saga
{
   public class CategoryState : IEnumerable<(string Category, int Version)>
   {
      private readonly HashSet<string> damageCategories = new HashSet<string>();
      private readonly ILogger logger;
      private List<(string Category, int Version)> state;

      public CategoryState(ILogger logger, IEnumerable<string> categories)
      {
         this.logger = logger;
         this.state = categories.Select(category => (category, 0)).ToList();
      }

      public IEnumerator<(string Category, int Version)> GetEnumerator()
         => this.state.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      public void MarkCategoryAsDamage(string category) => this.damageCategories.Add(category);
      public void ClearDamageCategories() => this.damageCategories.Clear();

      public void Update(List<Indexed> indexed)
      {
         var newState = CalculateNewState(indexed);
         PrintStateDifference(newState);
         this.state = newState;
      }

      public bool IsInScope(Indexed evn)
         => evn.Version <= this.state.First(x => x.Category == evn.Category).Version;

      private List<(string Category, int Version)> CalculateNewState(IEnumerable<Indexed> indexed)
      {
         var indexState = indexed
            .GroupBy(x => x.Category)
            .Where(g => !this.damageCategories.Contains(g.Key))
            .Select(g => (Category: g.Key, Version: g.Max(evn => evn.Version)));

         var newState = this.state.LeftOuterJoin(indexState,
            left => left.Category,
            right => right.Category,
            (left, right) => (left.Category, Version: Math.Max(left.Version, right.Version)));

         return newState.ToList();
      }

      private void PrintStateDifference(List<(string Category, int Version)> newState)
      {
         var mix = this.state.Join(newState,
            left => left.Category,
            right => right.Category,
            (left, right) => (left.Category, prev: left.Version, next: right.Version))
            .Where(x => x.prev < x.next);

         foreach (var (category, prev, next) in mix)
         {
            this.logger.LogInformation(
               $"Processing {next - prev} event(s) of category [{category}] (ver. {prev + 1}-{next}).");
         }
      }
   }
}