using System;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal interface INavigationViewStateEntity
{

}

internal interface INavigationViewStateEntity<out TSelf> : INavigationViewStateEntity where TSelf : INavigationViewStateEntity<TSelf>
{
  public abstract static TSelf CreateDefault(Guid id);

  public Guid Id { get; }
}
