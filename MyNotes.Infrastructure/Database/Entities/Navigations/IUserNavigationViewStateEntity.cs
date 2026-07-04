using System;

namespace MyNotes.Infrastructure.Database.Entities.Navigations;

internal interface IUserNavigationViewStateEntity
{

}

internal interface IUserNavigationViewStateEntity<out TSelf> : IUserNavigationViewStateEntity where TSelf : IUserNavigationViewStateEntity<TSelf>
{
  public abstract static TSelf CreateDefault(Guid id);

  public Guid Id { get; }
}
