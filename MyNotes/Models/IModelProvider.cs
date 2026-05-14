using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Models;

internal interface IModelProvider<TKey, TModel> where TKey : notnull where TModel : class
{
  public TModel? Resolve(TKey key, Func<TModel> modelFactory);

  public bool TryResolve(TKey key, out TModel? model);
}