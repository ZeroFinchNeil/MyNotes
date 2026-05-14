using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Models;

internal interface IModelFactory<TKey, TModel> where TKey : notnull where TModel : class
{
  public TModel Create(TKey key);
}