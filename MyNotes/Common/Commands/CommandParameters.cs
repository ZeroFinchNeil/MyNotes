namespace MyNotes.Common.Commands;

public record CommandParameters<P1, P2>
{
  public required P1 Parameter1 { get; init; }
  public required P2 Parameter2 { get; init; }
}

public record CommandParameters<P1, P2, P3>
{
  public required P1 Parameter1 { get; init; }
  public required P2 Parameter2 { get; init; }
  public required P3 Parameter3 { get; init; }
}

public record CommandParameters<P1, P2, P3, P4>
{
  public required P1 Parameter1 { get; init; }
  public required P2 Parameter2 { get; init; }
  public required P3 Parameter3 { get; init; }
  public required P4 Parameter4 { get; init; }
}

public record CommandParameters<P1, P2, P3, P4, P5>
{
  public required P1 Parameter1 { get; init; }
  public required P2 Parameter2 { get; init; }
  public required P3 Parameter3 { get; init; }
  public required P4 Parameter4 { get; init; }
  public required P5 Parameter5 { get; init; }
}