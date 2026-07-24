using System;

namespace MyNotes.Domain.ValueObjects;

public readonly record struct ImageId
{
  public static ImageId NewId() => new(Guid.NewGuid());

  public static ImageId Create(Guid id) => new(id);
  public static ImageId Create(string id) => Create(Guid.Parse(id));

  public Guid Value { get; init; }

  private ImageId(Guid id) => Value = id;
  public ImageId() => throw new InvalidOperationException("ImageId has not been properly initialized.");
}